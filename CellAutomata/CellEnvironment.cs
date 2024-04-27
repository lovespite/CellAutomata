using System.Diagnostics;

namespace CellAutomata;

public class CellEnvironment
{
    private readonly int _colWidth;
    private readonly int _rowCount;

    private readonly byte[] _cells;
    private readonly IBitMap _bitmap;

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

    public long Generation { get; private set; } = 0;
    public int MsTimeUsed { get; private set; } = 0;

    public void ToggleCell(int row, int col)
    {
        lock (_lock)
        {
            ToggleCellInternal(row, col);
        }
    }

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
            var result = new List<Point>();
            var bpc = Bpc;

            for (int r = rect.Top; r < rect.Bottom; r++)
            {
                for (int c = rect.Left; c < rect.Right; c++)
                {
                    var bPos = bpc.Transform(r, c);
                    if (_bitmap.Get(ref bPos))
                    {
                        result.Add(new Point(c, r));
                    }
                }
            }

            return result;
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
        var snapshot = CreateSnapshot();
        using var fs = File.Create(file);
        await fs.WriteAsync(snapshot.Bytes);
    }

    public async Task LoadFrom(string file)
    {
        var buffer = new byte[_cells.Length];
        using var fs = File.OpenRead(file);
        await fs.ReadAsync(buffer);

        lock (_lock)
        {
            Buffer.BlockCopy(buffer, 0, _cells, 0, buffer.Length);
            Generation = 0;
        }
    }

    public void Evolve()
    {
        var ws = Stopwatch.StartNew();
        lock (_lock)
        {
            EvolveInternal();
        }
        ws.Stop();
        MsTimeUsed = (int)ws.ElapsedMilliseconds;
        Generation++;
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
        MsTimeUsed = (int)ws.ElapsedMilliseconds;
        Generation++;
    }

    #region Internal Methods

    private void ToggleCellInternal(int row, int col)
    {
        var bPos = Bpc.Transform(row, col);
        _bitmap.Toggle(ref bPos);
    }

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

    private void EvolveInternal()
    {
        byte n;
        var snapshot = _bitmap.CreateSnapshot();

        for (int r = 0; r < _rowCount; r++)
        {
            for (int c = 0; c < _colWidth; c++)
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
    #endregion
}
