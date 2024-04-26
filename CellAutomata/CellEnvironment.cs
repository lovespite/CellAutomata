using System.Diagnostics;

namespace CellAutomata;

public class CellEnvironment
{
    private readonly int _colWidth;
    private readonly int _rowCount;
    private readonly IList<(int, int)> _rCells = new List<(int, int)>();

    private readonly byte[] _cells;
    private readonly IByteArrayBitOperator _bitmap;

    public IPositionConvert Bpc => _bitmap.Bpc;

    public int Width => _colWidth;
    public int Height => _rowCount;

    private readonly object _lock = new();

    public CellEnvironment(IByteArrayBitOperator bitmap)
    {
        _bitmap = bitmap;
        _cells = bitmap.Bytes;
        _colWidth = bitmap.Bpc.Width;
        _rowCount = bitmap.Bpc.Height;

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

    public void Clear()
    {
        lock (_lock)
        {
            ClearInternal();
        }
    }


    public IByteArrayBitOperator CreateSnapshot()
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
    }

    #region Internal Methods

    private void ToggleCellInternal(int row, int col)
    {
        var bPos = Bpc.Transform(row, col);
        _bitmap.Toggle(ref bPos);
        _rCells.Add((row, col));
    }
    private void ClearInternal()
    {
        Array.Clear(_cells, 0, _cells.Length);
        Generation = 0;
    }
    private IByteArrayBitOperator CreateSnapshotInternal()
    { 
        return _bitmap.Clone();
    }
    private void EvolveInternal()
    {
        byte n;
        var snapshot = _bitmap.Clone();

        for (int r = 0; r < _rowCount; r++)
        {
            for (int c = 0; c < _colWidth; c++)
            {
                n = CountAliveNeighbors(snapshot, Bpc, r, c);

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

        Generation++;
    }

    private static byte CountAliveNeighbors(IByteArrayBitOperator src, IPositionConvert cvt, int row, int col)
    {
        byte count = 0;

        for (int r = row - 1; r <= row + 1; r++)
        {
            for (int c = col - 1; c <= col + 1; c++)
            {
                if (r == row && c == col)
                {
                    continue;
                }

                if (r < 0 || r >= cvt.Height || c < 0 || c >= cvt.Width)
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
