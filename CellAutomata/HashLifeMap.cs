using SharpDX.Direct2D1;
using SharpDX;
using System.Buffers;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System;

namespace CellAutomata;

[StructLayout(LayoutKind.Sequential)]
public struct VIEWINFO
{
    public Int32 EMPTY;
    public Int32 psl_x1; // selection rect point 1
    public Int32 psl_y1; // selection rect point 1
    public Int32 psl_x2; // selection rect point 2
    public Int32 psl_y2; // selection rect point 2 
}

public partial class HashLifeMap : ILifeMap
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

    public void DrawViewportDC(nint hWndCanvas, int mag, Size vwSize, Point center, ref VIEWINFO selection, string text)
    {
        if (_renderContextId < 0)
        {
            _renderContextId = HashLifeMapStatic.CreateRender(vwSize.Width, vwSize.Height, hWndCanvas, Use3dRender);
            _vwSize = vwSize;
        }
        else if (_vwSize != vwSize)
        {
            HashLifeMapStatic.
                        // Resize render context
                        DestroyRender(_renderContextId);
            _renderContextId = HashLifeMapStatic.CreateRender(vwSize.Width, vwSize.Height, hWndCanvas, Use3dRender);
            _vwSize = vwSize;

            Debug.WriteLine("Resize render context: " + _renderContextId);
        }

        HashLifeMapStatic.
                DrawViewport(
            _renderContextId,
            _index, mag, center.X, center.Y, vwSize.Width, vwSize.Height,
            ref selection,
            text + "\nD3D Render: " + (Use3dRender == 1 ? "On" : "Off"));
    }

    private static readonly string _version;
    private int _index = int.MinValue;
    public int AlgoIndex => _index;

    static HashLifeMap()
    {
        var buffer = new byte[256];
        HashLifeMapStatic.Version(buffer, buffer.Length);
        _version = Encoding.UTF8.GetString(buffer).TrimEnd('\0');
        Debug.WriteLine($"Hashlifelib Version: {_version}");
    }

    private readonly string Rule;
    private readonly int Use3dRender;
    public HashLifeMap(string rule = "B3/S23", bool use3dRender = false)
    {
        Use3dRender = use3dRender ? 1 : 0;
        Rule = rule;
        _index = HashLifeMapStatic.CreateNewUniverse(rule);
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
    public long MsMemoryCopyTime { get; private set; }
    public long MsCPUTime => MsGenerationTime;
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

    private bool _isPopulationDirty = true;

    public bool Get(int row, int col)
    {
        return HashLifeMapStatic.GetCell(_index, col, row) != 0;
    }

    public void Set(int row, int col, bool value)
    {
        HashLifeMapStatic.SetCell(_index, col, row, value);
        _isPopulationDirty = true;
    }

    public bool Get(ref Point point)
    {
        return Get(point.Y, point.X);
    }

    public void Set(ref Point point, bool value)
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

    public void ClearRect(Rectangle rect)
    {
        for (int row = rect.Top; row < rect.Bottom; row++)
        {
            for (int col = rect.Left; col < rect.Right; col++)
            {
                Set(row, col, false);
            }
        }
    }

    public ILifeMap CreateSnapshot()
    {
        var bounds = GetBounds();

        return CreateRegionSnapshot(bounds);
    }

    public RectangleL GetBounds()
    {
        Int64 top = 0, left = 0, bottom = 0, right = 0;
        HashLifeMapStatic.FindEdges(_index, ref top, ref left, ref bottom, ref right);

        return new RectangleL(top, left, bottom, right);
    }


    public ILifeMap CreateRegionSnapshot(Rectangle rect)
    {
        var snapshot = new HashLifeMap(Rule);

        for (int row = rect.Top; row < rect.Bottom; row++)
        {
            for (int col = rect.Left; col < rect.Right; col++)
            {
                snapshot.Set(row - rect.Top, col - rect.Left, Get(row, col));
            }
        }

        return snapshot;
    }

    public void BlockCopy(ILifeMap source, Size srcSize, Point dstLocation, CopyMode mode = CopyMode.Overwrite)
    {
        for (int row = 0; row < srcSize.Height; row++)
        {
            for (int col = 0; col < srcSize.Width; col++)
            {
                var srcPoint = new Point(col, row);
                var dstPoint = new Point(col + dstLocation.X, row + dstLocation.Y);

                bool srcValue = source.Get(ref srcPoint);
                bool dstValue = Get(ref dstPoint);

                switch (mode)
                {
                    case CopyMode.Overwrite:
                        Set(ref dstPoint, srcValue);
                        break;
                    case CopyMode.Or:
                        Set(ref dstPoint, srcValue || dstValue);
                        break;
                    case CopyMode.And:
                        Set(ref dstPoint, srcValue && dstValue);
                        break;
                    case CopyMode.Xor:
                        Set(ref dstPoint, srcValue ^ dstValue);
                        break;
                }
            }
        }
    }

    public Point[] QueryRegion(bool val, Rectangle rect)
    {
        if (!val) throw new NotSupportedException();

        var bufferLen = rect.Width * rect.Height / 8;

        if (bufferLen == 0)
        {
            return [];
        }

        List<Point> points = new(Math.Min(10_000, bufferLen)); // initial capacity   

        var buffer = ArrayPool<byte>.Shared.Rent(bufferLen);
        try
        {
            var sw = Stopwatch.StartNew();
            HashLifeMapStatic.GetRegion(_index, rect.Left, rect.Top, rect.Width, rect.Height, buffer, buffer.Length);
            var msGetRegion = sw.ElapsedMilliseconds;

            sw.Restart();
            // CollectBitMap(rect, points, buffer); 
            CollectBitMap2(rect, points, buffer, bufferLen);
            var msCollect = sw.ElapsedMilliseconds;

            sw.Stop();
            // Debug.WriteLine($"> GetRegion: {msGetRegion} ms, Collect: {msCollect} ms");
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }


        return [.. points];
    }

    private static void CollectBitMap2(Rectangle rect, List<Point> points, byte[] buffer, int bufferLen)
    {
        int byteIndex;
        int bitIndex;
        for (byteIndex = 0; byteIndex < bufferLen; byteIndex++)
        {
            byte b = buffer[byteIndex];
            if (b == 0) continue;

            for (bitIndex = 0; bitIndex < 8; bitIndex++)
            {
                if ((b & (1 << bitIndex)) != 0)
                {
                    points.Add(new Point((byteIndex * 8 + bitIndex) % rect.Width + rect.Left, (byteIndex * 8 + bitIndex) / rect.Width + rect.Top));
                }
            }
        }
    }

    public long QueryRegionCount(bool val, Rectangle rect)
    {
        long count = 0;
        for (int row = rect.Top; row < rect.Bottom; row++)
        {
            for (int col = rect.Left; col < rect.Right; col++)
            {
                if (Get(row, col) == val)
                {
                    count++;
                }
            }
        }
        return count;
    }

    public Point[] GetLocations(bool val)
    {
        var snapshot = CreateSnapshot();
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

    public void SaveRle(Stream stream)
    {
    }

    public void ReadRle(Stream stream)
    {
    }


    // ======================

    #region Drawing

    public System.Drawing.Bitmap DrawRegionBitmap(Rectangle rect)
    {
        System.Drawing.Bitmap bitmap = new(rect.Width, rect.Height, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);

        // 锁定位图的像素数据
        BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                             ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);

        // 获取位图缓冲区的指针
        IntPtr bitmapPtr = bmpData.Scan0;

        // 获取每行字节跨度
        long stride = bmpData.Stride;
        HashLifeMapStatic.

                // 调用C++ DLL函数
                DrawRegionBitmap(_index, bitmapPtr, stride, rect.X, rect.Y, rect.Width, rect.Height);

        // 解锁位图
        bitmap.UnlockBits(bmpData);

        // 设置1bpp位图的调色板（黑白）
        ColorPalette palette = bitmap.Palette;
        palette.Entries[0] = Color.Black;
        palette.Entries[1] = Color.White;
        bitmap.Palette = palette;

        return bitmap;
    }

    public byte[] DrawRegionBitmapBGRA(Rectangle rect)
    {
        // 1. 创建BGRA的位图缓冲区
        int width = rect.Width;
        int height = rect.Height;
        int stride = width * 4;
        byte[] bitmapData = ArrayPool<byte>.Shared.Rent(stride * height);

        // 2. 创建Bitmap对象并锁定位图数据
        GCHandle handle = GCHandle.Alloc(bitmapData, GCHandleType.Pinned);
        IntPtr bitmapPtr = handle.AddrOfPinnedObject();
        HashLifeMapStatic.

                // 3. 调用C++ DLL函数
                DrawRegionBitmapBGRA(_index, bitmapPtr, stride, rect.X, rect.Y, width, height);

        // 4. 释放数据指针
        handle.Free();

        return bitmapData;
    }

    #endregion
}

