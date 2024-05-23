using CellAutomata.Util;

namespace CellAutomata.Algos;

public interface I2DBitMutator
{
    byte this[long x, long y] { get; set; }
    void Clear();
    void ClearRegion(RectangleL rect);
    Task ClearRegionAsync(RectangleL rect, IProgressReporter? reporter = null);

    bool Get(long row, long col);
    void Set(long row, long col, bool value);

    bool Get(PointL point);
    void Set(PointL point, bool value);
}