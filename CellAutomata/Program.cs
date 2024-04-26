using Accessibility;
using System.Runtime.InteropServices;

namespace CellAutomata;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        Application.Run(new Form1());
    }
}


public struct BitPosition
{
    public long Index;
    public long ByteArrayIndex;
    public byte BitIndex;
}

public class BitPositionConvert(int colWidth, int rowCount)
{
    private readonly int _colWidth = colWidth;
    private readonly int _rowCount = rowCount;

    public int Width => _colWidth;
    public int Height => _rowCount;

    public BitPosition Transform(int row, int column)
    {
        var index = row * _colWidth + column;

        BitPosition bitPosition = new()
        {
            Index = index,
            ByteArrayIndex = index / 8,
            BitIndex = (byte)(index % 8)
        };

        return bitPosition;
    }
}

public class CellEnvironment
{
    private readonly int _colWidth;
    private readonly int _rowCount;
    private readonly IList<(int, int)> _rCells = new List<(int, int)>();

    private readonly byte[] _cells;
    private readonly BitPositionConvert _bpc;
    private readonly ByteArrayBitOperator _bitOperator;

    public BitPositionConvert Bpc => _bpc;

    public int Width => _colWidth;
    public int Height => _rowCount;

    private readonly object _lock = new();

    public CellEnvironment(int colWidth, int rowCount)
    {
        _colWidth = colWidth;
        _rowCount = rowCount;

        _bpc = new BitPositionConvert(colWidth, rowCount);
        _cells = new byte[(colWidth * rowCount) / 8];
        _bitOperator = new ByteArrayBitOperator(_cells);
    }

    public long Generation { get; private set; } = 0;

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


    public byte[] CreateSnapshot()
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
        await fs.WriteAsync(snapshot);
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
        lock (_lock)
        {
            EvolveInternal();
        }
    }

    #region Internal Methods

    private void ToggleCellInternal(int row, int col)
    {
        var bPos = _bpc.Transform(row, col);
        _bitOperator.Toggle(ref bPos);
        _rCells.Add((row, col));
    }
    private void ClearInternal()
    {
        Array.Clear(_cells, 0, _cells.Length);
        Generation = 0;
    }
    private byte[] CreateSnapshotInternal()
    {
        var bytes = new byte[_cells.Length];

        Buffer.BlockCopy(_cells, 0, bytes, 0, _cells.Length);

        return bytes;
    }
    private void EvolveInternal()
    {
        byte n;
        var snapshot = new ByteArrayBitOperator(CreateSnapshotInternal());

        for (int r = 0; r < _rowCount; r++)
        {
            for (int c = 0; c < _colWidth; c++)
            {
                n = CountAliveNeighbors(snapshot, _bpc, r, c);

                var bPos = _bpc.Transform(r, c);
                if (snapshot.Get(ref bPos))
                {
                    if (n < 2 || n > 3)
                    {
                        _bitOperator.Set(ref bPos, false);
                    }
                }
                else
                {
                    if (n == 3)
                    {
                        _bitOperator.Set(ref bPos, true);
                    }
                }
            }
        }

        Generation++;
    }

    private static byte CountAliveNeighbors(ByteArrayBitOperator src, BitPositionConvert cvt, int row, int col)
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

public class ByteArrayBitOperator(byte[] bytes)
{
    private readonly byte[] _bytes = bytes;

    public byte[] Bytes => _bytes;

    public bool Get(ref BitPosition bPos)
    {
        byte mask = (byte)(1 << bPos.BitIndex);

        return (_bytes[bPos.ByteArrayIndex] & mask) != 0;
    }

    public void Set(ref BitPosition bPos, bool value)
    {
        byte mask = (byte)(1 << bPos.BitIndex);

        if (value)
        {
            _bytes[bPos.ByteArrayIndex] |= mask;
        }
        else
        {
            _bytes[bPos.ByteArrayIndex] &= (byte)~mask;
        }
    }

    public void Toggle(ref BitPosition bPos)
    {
        byte mask = (byte)(1 << bPos.BitIndex);

        _bytes[bPos.ByteArrayIndex] ^= mask;
    }
}