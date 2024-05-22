using CellAutomata.Algos;

namespace CellAutomata.Util;

public class RandomFillAlgorithm(double threshold = 0.5d) : IRandomFillAlgorithm
{
    private readonly double _threshold = threshold;

    public static readonly RandomFillAlgorithm Shared10 = new(0.1d);
    public static readonly RandomFillAlgorithm Shared25 = new(0.25);
    public static readonly RandomFillAlgorithm Shared50 = new(0.5d);
    public static readonly RandomFillAlgorithm Shared75 = new(0.75d);
    public static readonly RandomFillAlgorithm Shared100 = new(1.0d);

    private readonly Random random = new();

    public void Generate(Rectangle rect, I2DBitMutator bitmap)
    {
        for (int y = 0; y < rect.Height; y++)
        {
            for (int x = 0; x < rect.Width; x++)
            {
                bitmap.Set(y + rect.Top, x + rect.Left, random.NextDouble() <= _threshold);
            }
        }
    }

    public bool GetNoise(int x, int y, int z)
    {
        return random.NextDouble() <= _threshold;
    }
}