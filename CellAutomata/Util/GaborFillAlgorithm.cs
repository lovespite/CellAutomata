using CellAutomata.Algos;

namespace CellAutomata.Util;

public class GaborFillAlgorithm : IRandomFillAlgorithm
{
    private readonly double _frequency = 2.0d; // 控制频率
    private readonly double _orientation = Math.PI / 4; // 方向
    private readonly double _amplitude = 0.5d; // 振幅

    private readonly double _sigma; // 标准差，控制核的大小

    public GaborFillAlgorithm(double frequency, double orientation, double sigma, double amplitude)
    {
        this._frequency = frequency;
        this._orientation = orientation;
        this._amplitude = amplitude;
        this._sigma = sigma;
    }

    private readonly Rectangle _rectangle;

    public static GaborFillAlgorithm Create(Rectangle rect)
    {
        var sigma = Math.Min(rect.Width, rect.Height) / 4;

        return new GaborFillAlgorithm(sigma, rect);
    }

    private GaborFillAlgorithm(double sigma, Rectangle rect)
    {
        _sigma = sigma;
        _rectangle = rect;
    }

    public void Generate(Rectangle rect, I2DBitMutator bitmap)
    {
        for (int y = rect.Top; y < rect.Top + rect.Height; y++)
        {
            for (int x = rect.Left; x < rect.Left + rect.Width; x++)
            {
                double value = Gabor(x - rect.Left - rect.Width / 2f, y - rect.Top - rect.Height / 2f);
                bitmap.Set(y, x, Math.Abs(value) >= 0.1d);
            }
        }
    }

    public bool GetNoise(int x, int y, int z)
    {
        return Math.Abs(Gabor(
            x - _rectangle.Left - _rectangle.Width / 2f,
            y - _rectangle.Top - _rectangle.Height / 2f
        )) >= 0.1d;
    }

    private double Gabor(double x, double y)
    {
        double xPrime = x * Math.Cos(_orientation) + y * Math.Sin(_orientation);
        double yPrime = -x * Math.Sin(_orientation) + y * Math.Cos(_orientation);
        double gaussian = Math.Exp(-(xPrime * xPrime + yPrime * yPrime) / (2 * _sigma * _sigma));
        double sinusoidal = Math.Cos(2 * Math.PI * _frequency * xPrime);
        return _amplitude * gaussian * sinusoidal;
    }
}