using System.Buffers;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace CellAutomata;

public class Array2LifeMap : ILifeMap
{
    public long Population => QueryRegionCount(true, Rectangle.Empty);
    public long Generation { get; private set; } = 0;

    public long MsCPUTime { get; private set; } = 0; // milliseconds
    public long MsMemoryCopyTime { get; private set; } = 0; // milliseconds
    public long MsGenerationTime { get; private set; } = 0; // milliseconds

    private int _threadCount = 16;
    public int ThreadCount
    {
        get => _threadCount;
        set
        {
            if (value < 1 || value > 64)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Thread count must be between 1 and 64.");
            }


            _threadCount = value;
        }
    }

    public byte[] Bytes { get; } = null!;

    private readonly ArrayCell2.Factory _cellFactory;

    public Array2LifeMap()
    {
        Bytes = [];
        _cellFactory = new ArrayCell2.Factory(1000);
    }

    public void BlockCopy(ILifeMap source, Size srcSize, Point dstLocation, CopyMode mode = CopyMode.Overwrite)
    {
        for (int row = 0; row < srcSize.Height; row++)
        {
            for (int col = 0; col < srcSize.Width; col++)
            {
                var srcPos = new Point(col, row);
                var dstPos = new Point(dstLocation.X + col, dstLocation.Y + row);

                var srcVal = source.Get(ref srcPos);
                var dstVal = Get(ref dstPos);

                switch (mode)
                {
                    case CopyMode.Overwrite:
                        Set(ref dstPos, srcVal);
                        break;
                    case CopyMode.Or:
                        Set(ref dstPos, srcVal || dstVal);
                        break;
                    case CopyMode.And:
                        Set(ref dstPos, srcVal && dstVal);
                        break;
                    case CopyMode.Xor:
                        Set(ref dstPos, srcVal ^ dstVal);
                        break;
                }
            }
        }
    }

    public void Clear()
    {
        _cellFactory.ReturnAll();
        Generation = 0;
    }

    public ILifeMap CreateRegionSnapshot(Rectangle rect)
    {
        var snapshot = new Array2LifeMap();

        snapshot._cellFactory.Copy(_cellFactory, rect);

        return snapshot;
    }

    public ILifeMap CreateSnapshot()
    {
        return CreateRegionSnapshot(Rectangle.Empty);
    }

    public void Dispose()
    {
        _cellFactory.Dispose();
        GC.SuppressFinalize(this);
    }

    public bool Get(int row, int col)
    {
        var p = new Point(col, row);
        return _cellFactory.IsAlive(ref p);
    }
    public bool Get(ref Point point)
    {
        return _cellFactory.IsAlive(ref point);
    }

    public void Set(int row, int col, bool value)
    {
        var p = new Point(col, row);
        Set(ref p, value);
    }
    public void Set(ref Point point, bool value)
    {
        if (value)
        {
            _cellFactory.Get(ref point);
        }
        else
        {
            _cellFactory.Return(ref point);
        }
    }

    public Point[] QueryRegion(bool val, Rectangle rect)
    {
        var cells = _cellFactory.GetLocations();

        var list = new List<Point>(Math.Min(50000, rect.Width * rect.Height));

        for (long i = 0; i < cells.Length; ++i)
            if (rect.Contains(cells[i]))
            {
                list.Add(cells[i]);
            }

        return [.. list];
    }

    public long QueryRegionCount(bool val, Rectangle rect)
    {
        var cells = _cellFactory.GetLocations();
        if (rect.IsEmpty)
        {
            // if the rectangle is empty, return the count of all cells
            return cells.Length;
        }

        long count = 0;

        for (long i = 0; i < cells.Length; ++i)
            if (rect.Contains(cells[i]))
            {
                count++;
            }

        return count;
    }

    public Point[] GetLocations(bool val)
    {
        if (val)
        {
            return _cellFactory.GetLocations();
        }
        else
        {
            return [];
        }
    }

    public void NextGeneration()
    {
        if (_threadCount == 1)
        {
            NextGenerationInternal();
        }
        else
        {
            NextGenerationInternalMultiThread(_threadCount);
        }
    }

    private void NextGenerationInternal()
    {
        var cells = _cellFactory.GetLocations();

        var nCollector = new HashSet<Point>(cells.Length); // neighbors collector

        for (int i = 0; i < cells.Length; i++)
        {
            var loc = cells[i];
            var n = CountAliveNeighbors(loc, nCollector);

            if (_cellFactory.IsAlive(ref loc))
            {
                if (n < 2 || n > 3)
                {
                    _cellFactory.Return(ref loc);
                }
            }
            else
            {
                if (n == 3)
                {
                    _cellFactory.Get(ref loc);
                }
            }
        }
    }

    private int CountAliveNeighbors(Point loc, HashSet<Point> nCollector)
    {
        int n = 0;
        var row = loc.Y;
        var col = loc.X;
        for (int r = row - 1; r <= row + 1; r++)
        {
            for (int c = col - 1; c <= col + 1; c++)
            {
                if (r == row && c == col)
                {
                    continue;
                }

                var p = new Point(c, r);
                if (_cellFactory.IsAlive(ref p))
                {
                    n++;
                }
                else
                {
                    nCollector.Add(p);
                }
            }
        }

        return n;
    }

    private void NextGenerationInternalMultiThread(int threadCount)
    {
        var blocks = new List<Point[]>(threadCount);

        using var sharedBitmap = CreateSnapshot();
        var cells = sharedBitmap.GetLocations(true);
        var blockSize = cells.Length / threadCount;

        int start;
        int end;

        var sw2 = Stopwatch.StartNew();
        for (var i = 0; i < threadCount; i++)
        {
            start = i * blockSize;
            end = (i + 1) * blockSize;
            if (i == threadCount - 1)
            {
                // last block
                end = cells.Length;
            }
            blocks.Add(cells[start..end]);
        }
        sw2.Stop();
        MsMemoryCopyTime = sw2.ElapsedMilliseconds;

        sw2.Restart();
        Parallel.ForEach(blocks, block =>
        {
            if (block.Length == 0) return;
            PartialNextGenerationInternal(sharedBitmap, block);
        });
        sw2.Stop();
        MsGenerationTime = sw2.ElapsedMilliseconds;

        MsCPUTime = MsMemoryCopyTime + MsGenerationTime;
        Generation++;
    }

    private void PartialNextGenerationInternal(ILifeMap sharedBitMap, Point[] locations)
    {
        byte n;
        Point loc;
        var snapshot = sharedBitMap;

        var nCollector = new HashSet<Point>(locations.Length); // neighbors collector

        for (int i = 0; i < locations.Length; i++)
        {
            loc = locations[i];
            n = CountAliveNeighbors2(snapshot, ref loc, nCollector);

            if (snapshot.Get(ref loc))
            {
                if (n < 2 || n > 3)
                {
                    Set(ref loc, false);
                }
            }
            else
            {
                if (n == 3)
                {
                    Set(ref loc, true);
                }
            }
        }

        locations = [.. nCollector];
        for (int i = 0; i < locations.Length; i++)
        {
            loc = locations[i];
            n = CountAliveNeighbors2(snapshot, ref loc, null);

            if (snapshot.Get(ref loc))
            {
                if (n < 2 || n > 3)
                {
                    Set(ref loc, false);
                }
            }
            else
            {
                if (n == 3)
                {
                    Set(ref loc, true);
                }
            }
        }
    }

    private static byte CountAliveNeighbors2(ILifeMap src, ref Point loc, HashSet<Point>? nCollector)
    {
        byte n = 0;
        var row = loc.Y;
        var col = loc.X;
        for (int r = row - 1; r <= row + 1; r++)
        {
            for (int c = col - 1; c <= col + 1; c++)
            {
                if (r == row && c == col)
                {
                    continue;
                }

                var p = new Point(c, r);
                if (src.Get(ref p))
                {
                    n++;
                }
                else
                {
                    nCollector?.Add(p);
                }
            }
        }

        return n;
    }

    public void SaveRle(Stream stream)
    {
        var locations = _cellFactory.GetLocations();
        var minX = locations.Min(p => p.X);
        var minY = locations.Min(p => p.Y);

        var maxX = locations.Max(p => p.X);
        var maxY = locations.Max(p => p.Y);

        var width = maxX - minX + 1;
        var height = maxY - minY + 1;

        var writer = new StreamWriter(stream);
        writer.WriteLine($"x = {width}, y = {height}, rule = B3/S23");



    }

    public Bitmap DrawRegionBitmap(Rectangle rect)
    {
        // 创建指定尺寸的1bpp位图
        Bitmap bmp = new Bitmap(rect.Width, rect.Height, PixelFormat.Format1bppIndexed);

        // 锁定位图数据
        BitmapData data = bmp.LockBits(new Rectangle(0, 0, rect.Width, rect.Height),
                                       ImageLockMode.WriteOnly, bmp.PixelFormat);

        int stride = data.Stride;
        IntPtr scan0 = data.Scan0;

        // 创建空的位图数据缓冲区
        byte[] bytes = new byte[stride * rect.Height];

        // 获取指定区域内的活细胞
        Point[] cells = QueryRegion(true, rect);

        // 将生命游戏数据转换为位图数据
        for (int i = 0; i < cells.Length; i++)
        {
            Point p = cells[i];
            if (rect.Contains(p))
            {
                int x = p.X - rect.Left;
                int y = p.Y - rect.Top;
                int index = y * stride + x / 8;
                int bit = 7 - x % 8;
                bytes[index] |= (byte)(1 << bit);
            }
        }

        // 复制缓冲区数据到位图
        Marshal.Copy(bytes, 0, scan0, bytes.Length);

        // 解锁位图
        bmp.UnlockBits(data);

        // 设置1bpp位图的调色板（黑白）
        ColorPalette palette = bmp.Palette;
        palette.Entries[0] = Color.Black;
        palette.Entries[1] = Color.White;
        bmp.Palette = palette;

        return bmp;
    }

    public byte[] DrawRegionBitmapBGRA(Rectangle rect)
    {
        throw new NotSupportedException();
    }

    public RectangleL GetBounds()
    {
        var locs = _cellFactory.GetLocations();
        if (locs.Length == 0)
        {
            return new RectangleL(0, 0, 0, 0);
        }

        var minX = locs.Min(p => p.X);
        var minY = locs.Min(p => p.Y);
        var maxX = locs.Max(p => p.X);
        var maxY = locs.Max(p => p.Y);

        return new RectangleL(minY, minX, maxY, maxX);
    }

    public void ReadRle(Stream stream)
    {
        throw new NotImplementedException();
    }

    public PointL At(int x, int y)
    {
        throw new NotImplementedException();
    }
}