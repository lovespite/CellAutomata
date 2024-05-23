using CellAutomata.Algos;

namespace CellAutomata.Util;

public class BerlinNoise : IRandomFillAlgorithm
{
    public static readonly BerlinNoise Shared = new();
    public static BerlinNoise Create(float threshold = 0.1f) => new BerlinNoise(threshold);

    private readonly Random _random = new();
    private readonly float _threshold;

    private readonly int[] _permutation = Enumerable.Range(0, 512).ToArray();

    public BerlinNoise(float threshold = 0.1f)
    {
        for (int i = 0; i < 256; i++)
        {
            int j = _random.Next(256);
            (_permutation[j], _permutation[i]) = (_permutation[i], _permutation[j]);
        }

        Array.Copy(_permutation, 0, _permutation, 256, 256); // Duplicate the permutation to avoid buffer overflow
        _threshold = threshold;
    }

    private double Noise(double x, double y, double z)
    {
        // ReSharper disable InconsistentNaming
        int X = (int)Math.Floor(x) & 255;
        int Y = (int)Math.Floor(y) & 255;
        int Z = (int)Math.Floor(z) & 255;

        x -= Math.Floor(x);
        y -= Math.Floor(y);
        z -= Math.Floor(z);

        double u = Fade(x);
        double v = Fade(y);
        double w = Fade(z);

        int A = _permutation[X] + Y;
        int AA = _permutation[A] + Z;
        int AB = _permutation[A + 1] + Z;
        int B = _permutation[X + 1] + Y;
        int BA = _permutation[B] + Z;
        int BB = _permutation[B + 1] + Z;

        return Lerp(w, Lerp(v, Lerp(u, Grad(_permutation[AA], x, y, z),
                    Grad(_permutation[BA], x - 1, y, z)),
                Lerp(u, Grad(_permutation[AB], x, y - 1, z),
                    Grad(_permutation[BB], x - 1, y - 1, z))),
            Lerp(v, Lerp(u, Grad(_permutation[AA + 1], x, y, z - 1),
                    Grad(_permutation[BA + 1], x - 1, y, z - 1)),
                Lerp(u, Grad(_permutation[AB + 1], x, y - 1, z - 1),
                    Grad(_permutation[BB + 1], x - 1, y - 1, z - 1))));
        // ReSharper restore InconsistentNaming
    }

    private static double Fade(double t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    private static double Lerp(double t, double a, double b)
    {
        return a + t * (b - a);
    }

    private static double Grad(int hash, double x, double y, double z)
    {
        int h = hash & 15;
        double u = h < 8 ? x : y;
        double v = h < 4 ? y : h == 12 || h == 14 ? x : z;
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }

    private double OctaveNoise(double x, double y, double z, int octaves, double persistence)
    {
        double total = 0;
        double frequency = 1;
        double amplitude = 1;
        double maxValue = 0;

        for (int i = 0; i < octaves; i++)
        {
            total += Noise(x * frequency, y * frequency, z * frequency) * amplitude;

            maxValue += amplitude;

            amplitude *= persistence;
            frequency *= 2;
        }

        return total / maxValue;
    }

    public void Generate(RectangleL rect, I2DBitMutator bitmap)
    {
        for (var y = 0; y < rect.Height; y++)
        {
            for (var x = 0; x < rect.Width; x++)
            {
                double value = OctaveNoise(x * 0.1, y * 0.1, 0, 8, 0.5);
                bitmap.Set(y + rect.Top, x + rect.Left, value >= _threshold);
            }
        }
    }

    public bool GetNoise(long x, long y, long z)
    {
        return OctaveNoise(x * 0.1, y * 0.1, z * 0.1, 8, 0.5) >= _threshold;
    }
}