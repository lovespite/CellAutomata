using CellAutomata.Algos;

namespace CellAutomata.Util;

public interface IRandomFillAlgorithm
{
    void Generate(RectangleL rect, I2DBitMutator bitmap);
    bool GetNoise(long x, long y, long z);
}
