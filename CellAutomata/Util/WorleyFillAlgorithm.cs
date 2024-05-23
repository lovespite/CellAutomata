using CellAutomata.Algos;

namespace CellAutomata.Util;
internal class WorleyFillAlgorithm : IRandomFillAlgorithm
{
    private readonly Random _random = new Random();
    private readonly List<Point> _featurePoints;

    public WorleyFillAlgorithm(int numPoints, Rectangle bounds)
    {
        // 在指定区域内生成特征点
        _featurePoints = [];
        for (int i = 0; i < numPoints; i++)
        {
            int x = _random.Next(bounds.Left, bounds.Right);
            int y = _random.Next(bounds.Top, bounds.Bottom);
            _featurePoints.Add(new Point(x, y));
        }
    }

    public void Generate(Rectangle rect, I2DBitMutator bitmap)
    {
        for (int y = rect.Top; y < rect.Top + rect.Height; y++)
        {
            for (int x = rect.Left; x < rect.Left + rect.Width; x++)
            {
                double minDist = double.MaxValue;
                foreach (var point in _featurePoints)
                {
                    double dist = Math.Sqrt(Math.Pow(x - point.X, 2) + Math.Pow(y - point.Y, 2));
                    if (dist < minDist)
                    {
                        minDist = dist;
                    }
                }
                // 使用某个阈值决定是否设置位，这里我们简单地使用特征点距离的平均值

                if (minDist < 6) // 假设阈值为10
                {
                    if (_random.NextDouble() < 0.5)
                        bitmap.Set(y, x, true);
                }
            }
        }
    }

    public bool GetNoise(int x, int y, int z)
    {
        double minDist = double.MaxValue;
        foreach (var point in _featurePoints)
        {
            double dist = Math.Sqrt(Math.Pow(x - point.X, 2) + Math.Pow(y - point.Y, 2));
            if (dist < minDist)
            {
                minDist = dist;
            }
        }
        return minDist < 6 && _random.NextDouble() < 0.5;
    }

}