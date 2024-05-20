namespace CellAutomata.Algos;

public struct PointL
{
    public long X;
    public long Y;

    public long Row
    {
        get => Y;
        set => Y = value;
    }

    public long Column
    {
        get => X;
        set => X = value;
    }

    public PointL(long x, long y)
    {
        X = x;
        Y = y;
    }

    public static implicit operator Point(PointL point)
    {
        return new Point((int)point.X, (int)point.Y);
    }
}

