namespace CellAutomata;

public struct BitPosition
{
    public long Index;
    public long ByteArrayIndex;
    public byte BitIndex;

    public int Column => Location.X;
    public int Row => Location.Y;
    public Point Location;
}
