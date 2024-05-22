using CellAutomata.Algos;

namespace CellAutomata.Util;

public class GaborFillAlgorithm : IRandomFillAlgorithm
{
    private readonly double frequency = 2.0d; // 控制频率
    private readonly double orientation = Math.PI / 4; // 方向
    private readonly double amplitude = 0.5d; // 振幅

    private readonly double sigma = 100; // 标准差，控制核的大小
    public GaborFillAlgorithm(double frequency, double orientation, double sigma, double amplitude)
    {
        this.frequency = frequency;
        this.orientation = orientation;
        this.amplitude = amplitude;
        this.sigma = sigma;
    }

    public static GaborFillAlgorithm Create(Rectangle rect)
    {
        var sigma = Math.Min(rect.Width, rect.Height) / 4;

        return new GaborFillAlgorithm(sigma);
    }

    public GaborFillAlgorithm(double sigma)
    {
        this.sigma = sigma;
    }

    public void Generate(Rectangle rect, I2DBitMutator bitmap)
    {
        for (int y = rect.Top; y < rect.Top + rect.Height; y++)
        {
            for (int x = rect.Left; x < rect.Left + rect.Width; x++)
            {
                double value = Gabor(x - rect.Left - rect.Width / 2, y - rect.Top - rect.Height / 2);
                bitmap.Set(y, x, Math.Abs(value) >= 0.1d);
            }
        }
    }

    public bool GetNoise(int x, int y, int z)
    {
        return Math.Abs(Gabor(x, y)) >= 0.1d;
    }

    private double Gabor(double x, double y)
    {
        double xPrime = x * Math.Cos(orientation) + y * Math.Sin(orientation);
        double yPrime = -x * Math.Sin(orientation) + y * Math.Cos(orientation);
        double gaussian = Math.Exp(-(xPrime * xPrime + yPrime * yPrime) / (2 * sigma * sigma));
        double sinusoidal = Math.Cos(2 * Math.PI * frequency * xPrime);
        return amplitude * gaussian * sinusoidal;
    }

}