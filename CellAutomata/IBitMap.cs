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
    void Toggle(ref BitPosition bPos);

    IBitMap CreateSnapshot();

    IBitMap CreateRegionSnapshot(Rectangle rect);

    void BlockCopy(IBitMap source, Rectangle sourceRect, Rectangle destRect, CopyMode mode = CopyMode.Overwrite);
    void BlockCopy(IBitMap source, Point destLocation, CopyMode mode = CopyMode.Overwrite);
}