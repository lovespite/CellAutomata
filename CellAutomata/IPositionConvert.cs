namespace CellAutomata
{
    public interface IPositionConvert
    {
        int Height { get; }
        int Width { get; }
        long Length { get; }

        BitPosition Transform(int row, int column);
    }
}