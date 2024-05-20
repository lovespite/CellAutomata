namespace CellAutomata.Algos;

public struct SizeL
{
    public long Width;
    public long Height;

    public SizeL(long width, long height)
    {
        Width = width;
        Height = height;
    }

    public static implicit operator Size(SizeL size)
    {
        return new Size((int)size.Width, (int)size.Height);
    }
}

