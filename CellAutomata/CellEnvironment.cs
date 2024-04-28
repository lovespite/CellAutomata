using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CellAutomata;

public class CellEnvironment
{
    private readonly int _colWidth;
    private readonly int _rowCount;

    private readonly byte[] _cells;
    private readonly IBitMap _bitmap;

    public IBitMap BitMap => _bitmap;
    public IPositionConvert Bpc => _bitmap.Bpc;

    public int Width => _colWidth;
    public int Height => _rowCount;

    private readonly object _lock = new();

    public CellEnvironment(IBitMap bitmap)
    {
        _bitmap = bitmap;
        _cells = bitmap.Bytes;
        _colWidth = bitmap.Bpc.Width;
        _rowCount = bitmap.Bpc.Height;
    }

    public void Lock(Action<IBitMap> action)
    {
        lock (_lock)
        {
            action(_bitmap);
        }
    }

    public long Population { get; private set; } = 0;
    public long Generation { get; private set; } = 0;

    public int MsCPUTime { get; private set; } = 0; // milliseconds

    public bool IsAlive(int row, int col)
    {
        lock (_lock)
        {
            var bPos = Bpc.Transform(row, col);
            return _bitmap.Get(ref bPos);
        }
    }

    public IReadOnlyCollection<Point> GetRegionAliveCells(Rectangle rect)
    {
        lock (_lock)
        {
            return GetRegionAliveCells(_bitmap, rect);
        }
    }

    public void ActivateCell(int row, int col)
    {
        lock (_lock)
        {
            ActivateCellInternal(row, col);
        }
    }

    public void DeactivateCell(int row, int col)
    {
        lock (_lock)
        {
            DeactivateCellInternal(row, col);
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            ClearInternal();
        }
    }


    public IBitMap CreateSnapshot()
    {
        lock (_lock)
        {
            return CreateSnapshotInternal();
        }
    }

    public async Task SaveTo(string file)
    {
        var cells = GetRegionAliveCells(new Rectangle(0, 0, _colWidth, _rowCount)).ToArray();
        using var fs = File.Create(file);
        using var bw = new BinaryWriter(fs);

        var buffer = new byte[cells.Length * Marshal.SizeOf<int>() * 2];
        unsafe
        {
            fixed (byte* ptr = buffer)
            {
                var p = (int*)ptr;
                for (int i = 0; i < cells.Length; i++)
                {
                    var cell = cells[i];
                    p[i * 2] = cell.X;
                    p[i * 2 + 1] = cell.Y;
                }
            }
        }

        await fs.WriteAsync(buffer);
    }

    public async Task LoadFrom(string file)
    {
        var buffer = new byte[Marshal.SizeOf<int>() * 2];
        using var fs = File.OpenRead(file);
        using var br = new BinaryReader(fs);

        while (fs.Position < fs.Length)
        {
            await fs.ReadAsync(buffer);
            unsafe
            {
                fixed (byte* ptr = buffer)
                {
                    var p = (int*)ptr;

                    ActivateCell( p[1], p[0]);
                }
            }
        }
    }

    public void EvolveMultiThread(int threadCount)
    {
        var ws = Stopwatch.StartNew();

        var blocks = new List<Rectangle>();
        var blockHeight = _rowCount / threadCount;
        for (int i = 0; i < threadCount; i++)
        {
            var block = new Rectangle(0, i * blockHeight, _colWidth, blockHeight);
            if (i == threadCount - 1)
            {
                // last block, adjust height to the end
                block.Height = _rowCount - block.Top;
            }
            blocks.Add(block);
        }

        using var sharedBitmap = CreateSnapshot();
        Parallel.ForEach(blocks, block =>
        {
            EvolvePartialInternal(sharedBitmap, block);
        });

        ws.Stop();
        MsCPUTime = (int)ws.ElapsedMilliseconds;
        Generation++;
    }

    #region Internal Methods 

    private void ActivateCellInternal(int row, int col)
    {
        var bPos = Bpc.Transform(row, col);
        _bitmap.Set(ref bPos, true);
    }

    private void DeactivateCellInternal(int row, int col)
    {
        var bPos = Bpc.Transform(row, col);
        _bitmap.Set(ref bPos, false);
    }

    private void ClearInternal()
    {
        Array.Clear(_cells, 0, _cells.Length);
        Generation = 0;
    }
    private IBitMap CreateSnapshotInternal()
    {
        return _bitmap.CreateSnapshot();
    }

    private void EvolvePartialInternal(IBitMap sharedBitMap, Rectangle block)
    {
        byte n;
        var snapshot = sharedBitMap;

        for (int r = block.Top; r < block.Bottom; r++)
        {
            for (int c = block.Left; c < block.Right; c++)
            {
                n = CountAliveNeighbors(snapshot, r, c);

                var bPos = Bpc.Transform(r, c);
                if (snapshot.Get(ref bPos))
                {
                    if (n < 2 || n > 3)
                    {
                        _bitmap.Set(ref bPos, false);
                    }
                }
                else
                {
                    if (n == 3)
                    {
                        _bitmap.Set(ref bPos, true);
                    }
                }
            }
        }
    }

    private static byte CountAliveNeighbors(IBitMap src, int row, int col)
    {
        byte count = 0;
        var cvt = src.Bpc;
        var width = cvt.Width;
        var height = cvt.Height;

        for (int r = row - 1; r <= row + 1; r++)
        {
            for (int c = col - 1; c <= col + 1; c++)
            {
                if (r == row && c == col)
                {
                    continue;
                }

                if (r < 0 || r >= height || c < 0 || c >= width)
                {
                    continue;
                }

                var bPos = cvt.Transform(r, c);
                if (src.Get(ref bPos))
                {
                    count++;
                }
            }
        }

        return count;
    }

    private static IReadOnlyCollection<Point> GetRegionAliveCells(IBitMap src, Rectangle rect)
    {
        var list = new List<Point>();
        var cvt = src.Bpc;

        for (int r = rect.Top; r < rect.Bottom; r++)
        {
            for (int c = rect.Left; c < rect.Right; c++)
            {
                var bPos = cvt.Transform(r, c);
                if (src.Get(ref bPos))
                {
                    list.Add(new Point(c, r));
                }
            }
        }

        return list;
    }

    #endregion
}
