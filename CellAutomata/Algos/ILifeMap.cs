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
    long MsCpuTime { get; }
    long Generation { get; }
    long Population { get; }

    int GenInterval { get; set; }
    string Rule { get; set; }

    IDcRender GetDcRender();

    ILifeMap CreateSnapshot();
    Task<ILifeMap> CreateSnapshotAsync(IProgressReporter reporter);

    ILifeMap CreateRegionSnapshot(RectangleL rect);
    Task<ILifeMap> CreateRegionSnapshotAsync(RectangleL rect, IProgressReporter? reporter = null);

    void BlockCopy(ILifeMap source, SizeL srcSize, PointL dstLocation, CopyMode mode = CopyMode.Overwrite);
    Task BlockCopyAsync(ILifeMap source, SizeL srcSize, PointL dstLocation, CopyMode mode = CopyMode.Overwrite, IProgressReporter? reporter = null);

    PointL[] QueryRegion(bool val, RectangleL rect);
    Task<PointL[]> QueryRegionAsync(bool val, RectangleL rect, IProgressReporter? reporter = null);

    long QueryRegionCount(bool val, RectangleL rect);
    Task<long> QueryRegionCountAsync(bool val, RectangleL rect, IProgressReporter? reporter = null);

    PointL[] GetLocations(bool val);
    Task<PointL[]> GetLocationsAsync(bool val, IProgressReporter? reporter = null);

    RectangleL GetBounds();

    void NextGeneration();

    void ReadRle(string filename);

    Bitmap DrawRegionBitmap(RectangleL rectangle);
    byte[] DrawRegionBitmapBgra(RectangleL rectangle);

    PointL At(int x, int y);
}

