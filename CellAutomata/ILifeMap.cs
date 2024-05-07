namespace CellAutomata;

public enum CopyMode
{
    Overwrite,
    Or,
    And,
    Xor
}

public interface ILifeMap : IDisposable
{
    int ThreadCount { get; set; }
    byte[] Bytes { get; }

    long MsGenerationTime { get; }
    long MsMemoryCopyTime { get; }
    long MsCPUTime { get; }
    long Generation { get; }
    long Population { get; }

    bool Get(int row, int col);
    void Set(int row, int col, bool value);

    bool Get(ref Point point);
    void Set(ref Point point, bool value);

    void Clear();

    ILifeMap CreateSnapshot();

    ILifeMap CreateRegionSnapshot(Rectangle rect);

    void BlockCopy(ILifeMap source, Size srcSize, Point dstLocation, CopyMode mode = CopyMode.Overwrite);

    Point[] QueryRegion(bool val, Rectangle rect);
    long QueryRegionCount(bool val, Rectangle rect);

    Point[] GetLocations(bool val);

    void NextGeneration();
}

 