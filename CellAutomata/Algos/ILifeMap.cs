using CellAutomata.Render;
using CellAutomata.Util;

namespace CellAutomata.Algos;

public enum CopyMode
{
    Overwrite,
    Or,
    And,
    Xor
}

public interface ILifeMap : I2DBitMutator, IDisposable
{
    int ThreadCount { get; set; }
    byte[] Bytes { get; }

    long MsGenerationTime { get; }
    long MsMemoryCopyTime { get; }
    long MsCPUTime { get; }
    long Generation { get; }
    long Population { get; }

    int GenInterval { get; set; }
    string Rule { get; set; }

    IDCRender GetDCRender();

    ILifeMap CreateSnapshot();
    Task<ILifeMap> CreateSnapshotAsync(IProgressReporter reporter);

    ILifeMap CreateRegionSnapshot(Rectangle rect);
    Task<ILifeMap> CreateRegionSnapshotAsync(Rectangle rect, IProgressReporter? reporter = null);

    void BlockCopy(ILifeMap source, Size srcSize, Point dstLocation, CopyMode mode = CopyMode.Overwrite);
    Task BlockCopyAsync(ILifeMap source, Size srcSize, Point dstLocation, CopyMode mode = CopyMode.Overwrite, IProgressReporter? reporter = null);

    Point[] QueryRegion(bool val, Rectangle rect);
    Task<Point[]> QueryRegionAsync(bool val, Rectangle rect, IProgressReporter? reporter = null);

    long QueryRegionCount(bool val, Rectangle rect);
    Task<long> QueryRegionCountAsync(bool val, Rectangle rect, IProgressReporter? reporter = null);

    Point[] GetLocations(bool val);
    Task<Point[]> GetLocationsAsync(bool val, IProgressReporter? reporter = null);

    RectangleL GetBounds();

    void NextGeneration();

    void ReadRle(string filename);

    Bitmap DrawRegionBitmap(Rectangle rectangle);
    byte[] DrawRegionBitmapBGRA(Rectangle rectangle);

    PointL At(int x, int y);
}

