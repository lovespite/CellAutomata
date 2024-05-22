using CellAutomata.Algos;

namespace CellAutomata.Util;

public interface IRandomFillAlgorithm
{
    void Generate(Rectangle rect, I2DBitMutator bitmap);
    bool GetNoise(int x, int y, int z);
}
