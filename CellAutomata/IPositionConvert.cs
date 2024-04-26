namespace CellAutomata
{
    public interface IPositionConvert
    {
        int Height { get; }
        int Width { get; }

        BitPosition Transform(int row, int column);
    }
}