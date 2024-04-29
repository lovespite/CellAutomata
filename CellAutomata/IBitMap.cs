namespace CellAutomata;

public enum CopyMode
{
    Overwrite,
    Or,
    And,
    Xor
}

public interface IBitMap : IDisposable
{
    byte[] Bytes { get; }
    IPositionConvert Bpc { get; }

    bool Get(ref BitPosition bPos);
    void Set(ref BitPosition bPos, bool value);

    bool Get(int row, int col);
    void Set(int row, int col, bool value);

    bool Get(ref Point point);
    void Set(ref Point point, bool value);

    void Clear();

    IBitMap CreateSnapshot();

    IBitMap CreateRegionSnapshot(Rectangle rect);

    void BlockCopy(IBitMap source, Rectangle sourceRect, Rectangle destRect, CopyMode mode = CopyMode.Overwrite);
    void BlockCopy(IBitMap source, Point destLocation, CopyMode mode = CopyMode.Overwrite);

    Point[] QueryRegion(bool val, Rectangle rect);
    long QueryRegionCount(bool val, Rectangle rect);
}