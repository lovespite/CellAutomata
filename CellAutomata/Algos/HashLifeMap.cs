using System.Buffers;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using CellAutomata.Render;
using CellAutomata.Util;

namespace CellAutomata.Algos;

public class HashLifeMap : ILifeMap, IDcRender
{
    public int GenInterval { get; set; } = 10;

    private int _renderContextId = -1;
    private Size _vwSize;

    public PointL At(int x, int y)
    {
        long row = 0, col = 0;
        HashLifeMapStatic.AtViewport(_renderContextId, x, y, ref row, ref col);
        return new PointL(col, row);
    }

    public IDcRender GetDcRender()
    {
        return this;
    }

    private bool _isSuspended = false;
    public bool IsSuspended => _isSuspended;

    public void Suspend()
    {
        _isSuspended = true;
        HashLifeMapStatic.SuspendRender(_renderContextId);
    }

    public void Resume()
    {
        _isSuspended = false;
        HashLifeMapStatic.ResumeRender(_renderContextId);
    }

    public void DrawViewportDc(nint hWndCanvas, int mag, Size vwSize, PointL center, ViewInfo selection, string text)
    {
        if (_isSuspended) return;

        if (_renderContextId < 0)
        {
            _renderContextId = HashLifeMapStatic.CreateRender(vwSize.Width, vwSize.Height, hWndCanvas, 1);
            _vwSize = vwSize;
        }
        else if (_vwSize != vwSize)
        {
            HashLifeMapStatic.ResizeViewport(_renderContextId, vwSize.Width, vwSize.Height); // Resize render context

            _vwSize = vwSize;

            Debug.WriteLine($"Resize render to: {_vwSize}");
        }

        HashLifeMapStatic.DrawViewport(
            _renderContextId,
            _index, mag, (int)center.X, (int)center.Y, vwSize.Width, vwSize.Height,
            ref selection,
            text
        );
    }

    private int _index;
    public int AlgoIndex => _index;

    private string _rule;

    public string Rule
    {
        get
        {
            if (string.IsNullOrEmpty(_rule))
            {
                try
                {
                    _rule = GetRule();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    _rule = string.Empty;
                }
            }

            return _rule;
        }
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("Rule cannot be null or empty.");
            }

            if (_rule == value) return;
            if (_index < 0) return;

            var ret = HashLifeMapStatic.SetUniverseRule(_index, value);
            if (ret != 0)
            {
                throw new InvalidOperationException(
                    $"Failed to set HashLife universe rule: {value}, code: {ret}");
            }

            _rule = GetRule();
        }
    }

    const int BufferSize = 256;

    private string GetRule()
    {
        nint bufferPtr = Marshal.AllocHGlobal(BufferSize);
        try
        {
            HashLifeMapStatic.GetUniverseRule(_index, bufferPtr, BufferSize);
            var rule = Marshal.PtrToStringAnsi(bufferPtr);

            return rule ?? string.Empty;
        }
        finally
        {
            Marshal.FreeHGlobal(bufferPtr);
        }
    }

    public HashLifeMap(string rule = "B3/S23")
    {
        Debug.WriteLine(HashLifeMapStatic.LibVersion);
        _index = HashLifeMapStatic.CreateNewUniverse(rule);

        if (_index < 0)
        {
            throw new InvalidOperationException($"Failed to create HashLife universe: {rule}, code: {_index}");
        }

        _rule = GetRule();
        if (_index < 0)
        {
            throw new InvalidOperationException($"Failed to create HashLife universe: {rule}, code: {_index}");
        }
    }

    ~HashLifeMap()
    {
        HashLifeMapStatic.DestroyRender(_renderContextId);
        HashLifeMapStatic.DestroyUniverse(_index);
    }


    public int ThreadCount { get; set; }

    public byte[] Bytes => [];

    private ulong _population = 0;

    public long MsGenerationTime { get; private set; }

    // ReSharper disable once UnusedAutoPropertyAccessor.Local
    public long MsMemoryCopyTime { get; private set; }
    public long MsCpuTime => MsGenerationTime;
    public long Generation { get; private set; }

    public long Population
    {
        get
        {
            if (_isPopulationDirty)
            {
                HashLifeMapStatic.GetPopulation(_index, ref _population);
                _isPopulationDirty = false;
            }

            return (long)_population;
        }
    }

    public byte this[long row, long col]
    {
        get => Get(row, col) ? (byte)1 : (byte)0;
        set => Set(row, col, value != 0);
    }

    private bool _isPopulationDirty = true;

    public bool Get(long row, long col)
    {
        return HashLifeMapStatic.GetCell(_index, (int)col, (int)row) != 0;
    }

    public void Set(long row, long col, bool value)
    {
        HashLifeMapStatic.SetCell(_index, (int)col, (int)row, value);
        _isPopulationDirty = true;
    }

    public bool Get(PointL point)
    {
        return Get(point.Y, point.X);
    }

    public void Set(PointL point, bool value)
    {
        Set(point.Y, point.X, value);
    }

    public void Clear()
    {
        HashLifeMapStatic.DestroyUniverse(_index);
        _index = HashLifeMapStatic.CreateNewUniverse(Rule);
        _population = 0;
        Generation = 0;
    }

    public void ClearRegion(RectangleL rect)
    {
        for (var row = rect.Top; row < rect.Bottom; row++)
        {
            for (var col = rect.Left; col < rect.Right; col++)
            {
                Set(row, col, false);
            }
        }
    }

    public async Task ClearRegionAsync(RectangleL rect, IProgressReporter? reporter = null)
    {
        double total = rect.Width * rect.Height;
        double count = 0;
        for (var row = rect.Top; row < rect.Bottom; row++)
        {
            for (var col = rect.Left; col < rect.Right; col++)
            {
                if (reporter?.IsAborted ?? false) return;

                Set(row, col, false);

                if (reporter is null) continue;

                if ((++count) % 100_000 == 0)
                {
                    reporter.ReportProgress((float)(count / total));
                    await Task.Delay(1);
                }
            }
        }
    }

    public ILifeMap CreateSnapshot()
    {
        return CreateRegionSnapshot(GetBounds());
    }

    public Task<ILifeMap> CreateSnapshotAsync(IProgressReporter? reporter)
    {
        return CreateRegionSnapshotAsync(GetBounds(), reporter);
    }

    public RectangleL GetBounds()
    {
        long top = 0, left = 0, bottom = 0, right = 0;
        HashLifeMapStatic.FindEdges(_index, ref top, ref left, ref bottom, ref right);

        return new RectangleL(top, left, bottom, right);
    }

    public ILifeMap CreateRegionSnapshot(RectangleL rect)
    {
        var snapshot = new HashLifeMap(Rule);

        for (var row = rect.Top; row < rect.Bottom; row++)
        for (var col = rect.Left; col < rect.Right; col++)
            snapshot.Set(row - rect.Top, col - rect.Left, Get(row, col));

        return snapshot;
    }

    public async Task<ILifeMap> CreateRegionSnapshotAsync(RectangleL rect, IProgressReporter? reporter = null)
    {
        var snapshot = new HashLifeMap(Rule);

        double total = rect.Area();
        double count = 0;

        for (var row = rect.Top; row < rect.Bottom; row++)
        for (var col = rect.Left; col < rect.Right; col++)
        {
            snapshot.Set(row - rect.Top, col - rect.Left, Get(row, col));

            if (reporter is null) continue;
            if (reporter.IsAborted) return snapshot;
            if ((++count) % 100_000 != 0) continue;

            reporter.ReportProgress((float)(count / total), "Copying...", TimeSpan.Zero);
            await Task.Delay(1);
        }

        return snapshot;
    }

    public void BlockCopy(ILifeMap source, SizeL srcSize, PointL dstLocation, CopyMode mode = CopyMode.Overwrite)
    {
        for (var row = 0; row < srcSize.Height; row++)
        {
            for (var col = 0; col < srcSize.Width; col++)
            {
                var srcPoint = new PointL(col, row);
                var dstPoint = new PointL(col + dstLocation.X, row + dstLocation.Y);

                bool srcValue = source.Get(srcPoint);
                bool dstValue = Get(dstPoint);

                switch (mode)
                {
                    case CopyMode.Overwrite:
                        Set(dstPoint, srcValue);
                        break;
                    case CopyMode.Or:
                        Set(dstPoint, srcValue || dstValue);
                        break;
                    case CopyMode.And:
                        Set(dstPoint, srcValue && dstValue);
                        break;
                    case CopyMode.Xor:
                        Set(dstPoint, srcValue ^ dstValue);
                        break;
                }
            }
        }
    }

    public async Task BlockCopyAsync(ILifeMap source, SizeL srcSize, PointL dstLocation,
        CopyMode mode = CopyMode.Overwrite, IProgressReporter? reporter = null)
    {
        double total = srcSize.Width * srcSize.Height;
        double count = 0;

        for (var row = 0; row < srcSize.Height; row++)
        {
            for (var col = 0; col < srcSize.Width; col++)
            {
                var srcPoint = new PointL(col, row);
                var dstPoint = new PointL(col + dstLocation.X, row + dstLocation.Y);

                bool srcValue = source.Get(srcPoint);
                bool dstValue = Get(dstPoint);

                switch (mode)
                {
                    case CopyMode.Overwrite:
                        Set(dstPoint, srcValue);
                        break;
                    case CopyMode.Or:
                        Set(dstPoint, srcValue || dstValue);
                        break;
                    case CopyMode.And:
                        Set(dstPoint, srcValue && dstValue);
                        break;
                    case CopyMode.Xor:
                        Set(dstPoint, srcValue ^ dstValue);
                        break;
                }

                if (reporter is null) continue;

                if (reporter.IsAborted) return;

                if ((++count) % 100_000 == 0)
                {
                    reporter.ReportProgress((float)(count / total));
                    await Task.Delay(1);
                }
            }
        }
    }

    public PointL[] QueryRegion(bool val, RectangleL rect)
    {
        if (!val) throw new NotSupportedException();

        var bufferLen = (int)(rect.Width * rect.Height / 8);

        if (bufferLen == 0) return [];

        List<PointL> points = new(Math.Min(10_000, bufferLen)); // initial capacity   

        var buffer = ArrayPool<byte>.Shared.Rent(bufferLen);
        try
        {
            HashLifeMapStatic.GetRegion(
                _index,
                (int)rect.Left,
                (int)rect.Top,
                (int)rect.Width,
                (int)rect.Height,
                buffer, buffer.Length
            );

            CollectBitMap2(rect, points, buffer, bufferLen);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }

        return [.. points];
    }

    public async Task<PointL[]> QueryRegionAsync(bool val, RectangleL rect, IProgressReporter? reporter = null)
    {
        return await Task.Run(() => QueryRegion(val, rect), reporter?.CancelToken ?? CancellationToken.None);
    }

    private static void CollectBitMap2(RectangleL rect, List<PointL> points, byte[] buffer, int bufferLen)
    {
        int byteIndex;
        for (byteIndex = 0; byteIndex < bufferLen; byteIndex++)
        {
            byte b = buffer[byteIndex];
            if (b == 0) continue;

            int bitIndex;
            for (bitIndex = 0; bitIndex < 8; bitIndex++)
            {
                if ((b & 1 << bitIndex) != 0)
                {
                    points.Add(new((byteIndex * 8 + bitIndex) % rect.Width + rect.Left,
                        (byteIndex * 8 + bitIndex) / rect.Width + rect.Top));
                }
            }
        }
    }

    public long QueryRegionCount(bool val, RectangleL rect)
    {
        long count = 0;
        for (var row = rect.Top; row < rect.Bottom; row++)
        {
            for (var col = rect.Left; col < rect.Right; col++)
            {
                if (Get(row, col) == val)
                {
                    count++;
                }
            }
        }

        return count;
    }

    public async Task<long> QueryRegionCountAsync(bool val, RectangleL rect, IProgressReporter? reporter = null)
    {
        return await Task.Run(() => QueryRegionCount(val, rect), reporter?.CancelToken ?? CancellationToken.None);
    }

    public PointL[] GetLocations(bool val)
    {
        var snapshot = CreateSnapshot();
        var bounds = snapshot.GetBounds();

        return snapshot.QueryRegion(val, bounds);
    }

    public async Task<PointL[]> GetLocationsAsync(bool val, IProgressReporter? reporter = null)
    {
        var snapshot = await CreateSnapshotAsync(reporter);
        var bounds = snapshot.GetBounds();

        return snapshot.QueryRegion(val, bounds);
    }

    private readonly Stopwatch _sw = new();

    public void NextGeneration()
    {
        _sw.Restart();
        HashLifeMapStatic.NextStep(_index, ref _population);
        _sw.Stop();
        MsGenerationTime = _sw.ElapsedMilliseconds;
        _isPopulationDirty = false;
        Generation++;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }


    public void ReadRle(string filename)
    {
        nint errBufferPtr = 0;
        try
        {
            errBufferPtr = Marshal.AllocHGlobal(BufferSize);
            var ret = HashLifeMapStatic.ReadRleFile(_index, filename, errBufferPtr, BufferSize);

            if (ret != 0)
            {
                var errMsg = Marshal.PtrToStringAnsi(errBufferPtr);
                throw new InvalidOperationException("File not supported." + errMsg);
            }

            _rule = GetRule();
        }
        finally
        {
            if (errBufferPtr != 0) Marshal.FreeHGlobal(errBufferPtr);
        }
    }
    // ======================

    #region Drawing

    public Bitmap DrawRegionBitmap(RectangleL rect)
    {
        Bitmap bitmap = new((int)rect.Width, (int)rect.Height, PixelFormat.Format1bppIndexed);

        // 锁定位图的像素数据
        BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
            ImageLockMode.WriteOnly, PixelFormat.Format1bppIndexed);

        // 获取位图缓冲区的指针
        nint bitmapPtr = bmpData.Scan0;

        // 获取每行字节跨度
        long stride = bmpData.Stride;
        HashLifeMapStatic.

            // 调用C++ DLL函数
            DrawRegionBitmap(_index, bitmapPtr, stride, (int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);

        // 解锁位图
        bitmap.UnlockBits(bmpData);

        // 设置1bpp位图的调色板（黑白）
        ColorPalette palette = bitmap.Palette;
        palette.Entries[0] = Color.Black;
        palette.Entries[1] = Color.White;
        bitmap.Palette = palette;

        return bitmap;
    }

    public byte[] DrawRegionBitmapBgra(RectangleL rect)
    {
        // 1. 创建BGRA的位图缓冲区
        var width = (int)rect.Width;
        var height = (int)rect.Height;
        var stride = width * 4;
        byte[] bitmapData = ArrayPool<byte>.Shared.Rent(stride * height);

        // 2. 创建Bitmap对象并锁定位图数据
        GCHandle handle = GCHandle.Alloc(bitmapData, GCHandleType.Pinned);
        nint bitmapPtr = handle.AddrOfPinnedObject();
        HashLifeMapStatic.

            // 3. 调用C++ DLL函数
            DrawRegionBitmapBGRA(_index, bitmapPtr, stride, (int)rect.X, (int)rect.Y, width, height);

        // 4. 释放数据指针
        handle.Free();

        return bitmapData;
    }

    #endregion
}