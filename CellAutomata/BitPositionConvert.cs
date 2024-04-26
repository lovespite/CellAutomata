namespace CellAutomata;

public class BitPositionConvert : IPositionConvert
{
    private readonly int _colWidth;
    private readonly int _rowCount;

    public BitPositionConvert(int colWidth, int rowCount)
    {
        _colWidth = colWidth;
        _rowCount = rowCount;
    }

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

public class FastBitPositionConvert : IPositionConvert
{
    private readonly int _colWidth;
    private readonly int _rowCount;

    public FastBitPositionConvert(int colWidth, int rowCount)
    {
        _colWidth = colWidth;
        _rowCount = rowCount;
    }

    public int Width => _colWidth;
    public int Height => _rowCount;

    public BitPosition Transform(int row, int column)
    {
        var index = row * _colWidth + column;

        BitPosition bitPosition = new()
        {
            Index = index,
            ByteArrayIndex = index,
            BitIndex = 0
        };

        return bitPosition;
    }
}