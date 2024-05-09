using SharpDX.Direct2D1;
using SharpDX;
using System.Buffers;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace CellAutomata;

public partial class HashLifeMap : ILifeMap
{
    const string HashLifeLib = "Hashlifelib/hashlife.dll";

    [LibraryImport(HashLifeLib, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int CreateNewUniverse(string rule);

    [LibraryImport(HashLifeLib)]
    internal static partial void DestroyUniverse(int index);

    [LibraryImport(HashLifeLib)]
    internal static partial void SetCell(int index, int x, int y, [MarshalAs(UnmanagedType.Bool)] bool value);

    [LibraryImport(HashLifeLib)]
    internal static partial int GetCell(int index, int x, int y);

    [LibraryImport(HashLifeLib)]
    internal static partial void NextStep(int index, ref ulong pop);

    [LibraryImport(HashLifeLib)]
    internal static partial void SetThreadCount(int index, int count);

    [LibraryImport(HashLifeLib)]
    internal static partial void GetPopulation(int index, ref ulong pop);

    [LibraryImport(HashLifeLib)]
    internal static partial ulong GetRegion(int index, int x, int y, int w, int h, [In, Out] byte[] buffer, int bufferSize);

    [LibraryImport(HashLifeLib)]
    internal static partial void SetRegion(int index, int x, int y, int w, int h, [In] byte[] buffer, int bufferSize);

    [LibraryImport(HashLifeLib)]
    internal static partial void Version([In, Out] byte[] buffer, int bufferSize);

    [LibraryImport(HashLifeLib)]
    internal static partial void FindEdges(int index, ref Int64 top, ref Int64 left, ref Int64 bottom, ref Int64 right);

    [LibraryImport(HashLifeLib)]
    internal static partial void DrawRegionBitmap(int index, IntPtr bitmapBuffer, long stride, int x, int y, int w, int h);

    [LibraryImport(HashLifeLib)]
    internal static partial void DrawRegionBitmapBGRA(int index, IntPtr bitmapBuffer, long stride, int x, int y, int w, int h);

    [LibraryImport(HashLifeLib)]
    internal static partial void DrawRegionBitmapBGRA2(int index, IntPtr bitmapBuffer, long stride, int x, int y, int w, int h);

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

        // 3. 调用C++ DLL函数
        DrawRegionBitmapBGRA(_index, bitmapPtr, stride, rect.X, rect.Y, width, height);

        // 4. 释放数据指针
        handle.Free();

        return bitmapData;
    }

    public byte[] DrawRegionBitmapBGRA(Rectangle rect, int vw, int vh)
    {
        // viewport size must be equal to rect size at this time, which means cell size = 1
        Debug.Assert(vw == rect.Width && vh == rect.Height, "vw == rect.Width && vh == rect.Height");

        // 1. 创建BGRA的位图缓冲区
        int stride = vw * 4;
        byte[] bitmapData = ArrayPool<byte>.Shared.Rent(stride * vh);

        // 2. 创建Bitmap对象并锁定位图数据
        GCHandle handle = GCHandle.Alloc(bitmapData, GCHandleType.Pinned);
        IntPtr bitmapPtr = handle.AddrOfPinnedObject();

        // 3. 调用C++ DLL函数
        DrawRegionBitmapBGRA2(_index, bitmapPtr, stride, rect.X, rect.Y, vw, vh); // !

        // 4. 释放数据指针
        handle.Free();

        return bitmapData;
    }

    private static readonly string _version;
    private int _index = int.MinValue;
    public int AlgoIndex => _index;

    static HashLifeMap()
    {
        var buffer = new byte[256];
        Version(buffer, buffer.Length);
        _version = Encoding.UTF8.GetString(buffer).TrimEnd('\0');
        Debug.WriteLine($"Hashlifelib Version: {_version}");
    }

    private readonly string Rule;
    public HashLifeMap(string rule = "B3/S23")
    {
        Rule = rule;
        _index = CreateNewUniverse(rule);
        if (_index < 0)
        {
            throw new InvalidOperationException($"Failed to create HashLife universe: {rule}, code: {_index}");
        }
    }

    ~HashLifeMap()
    {
        DestroyUniverse(_index);
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
                GetPopulation(_index, ref _population);
                _isPopulationDirty = false;
            }

            return (long)_population;
        }
    }

    private bool _isPopulationDirty = true;

    public bool Get(int row, int col)
    {
        return GetCell(_index, col, row) != 0;
    }

    public void Set(int row, int col, bool value)
    {
        SetCell(_index, col, row, value);
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
        DestroyUniverse(_index);
        _index = CreateNewUniverse(Rule);
        _population = 0;
        Generation = 0;
    }

    public ILifeMap CreateSnapshot()
    {
        var bounds = GetBounds();

        return CreateRegionSnapshot(bounds);
    }

    public RectangleL GetBounds()
    {
        Int64 top = 0, left = 0, bottom = 0, right = 0;
        FindEdges(_index, ref top, ref left, ref bottom, ref right);

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
            GetRegion(_index, rect.Left, rect.Top, rect.Width, rect.Height, buffer, buffer.Length);
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

    private static void CollectBitMap(Rectangle rect, List<Point> points, byte[] buffer)
    {
        int byteIndex = 0;
        int bitIndex = 0;
        for (int row = 0; row < rect.Height; row++)
        {
            for (int col = 0; col < rect.Width; col++)
            {
                if (buffer[byteIndex] != 0)
                {
                    if ((buffer[byteIndex] & (1 << bitIndex)) != 0)
                    {
                        points.Add(new Point(col + rect.Left, row + rect.Top));
                    }
                }

                if (++bitIndex == 8)
                {
                    bitIndex = 0;
                    ++byteIndex;
                }
            }
        }
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
        NextStep(_index, ref _population);
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
        throw new NotImplementedException();
    }

    public void ReadRle(Stream stream)
    {
        throw new NotImplementedException();
    }
}

