using CellAutomata.Algos;

namespace CellAutomata.Util;

public class RandomFillAlgorithm(double threshold = 0.5d) : IRandomFillAlgorithm
{
    private readonly double _threshold = threshold;

    public static readonly RandomFillAlgorithm Shared10 = new(0.1d);
    public static readonly RandomFillAlgorithm Shared25 = new(0.25);
    public static readonly RandomFillAlgorithm Shared50 = new();
    public static readonly RandomFillAlgorithm Shared75 = new(0.75d);
    public static readonly RandomFillAlgorithm Shared100 = new(1.0d);

    private readonly Random _random = new();

    public void Generate(RectangleL rect, I2DBitMutator bitmap)
    {
        for (int y = 0; y < rect.Height - 1; y++)
        {
            for (int x = 0; x < rect.Width - 1; x++)
            {
                bitmap.Set(y + rect.Top, x + rect.Left, _random.NextDouble() <= _threshold);
            }
        }
    }

    public bool GetNoise(long x, long y, long z)
    {
        return _random.NextDouble() <= _threshold;
    }
}