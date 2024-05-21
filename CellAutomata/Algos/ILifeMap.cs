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

    ILifeMap CreateSnapshot();

    ILifeMap CreateRegionSnapshot(Rectangle rect);

    void BlockCopy(ILifeMap source, Size srcSize, Point dstLocation, CopyMode mode = CopyMode.Overwrite);

    Point[] QueryRegion(bool val, Rectangle rect);
    long QueryRegionCount(bool val, Rectangle rect);

    Point[] GetLocations(bool val);
    RectangleL GetBounds();

    void NextGeneration();
     
    void ReadRle(string filename);

    Bitmap DrawRegionBitmap(Rectangle rectangle);
    byte[] DrawRegionBitmapBGRA(Rectangle rectangle);

    PointL At(int x, int y);
}

