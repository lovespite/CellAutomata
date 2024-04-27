namespace CellAutomata
{
    public interface IBitMap : IDisposable
    {
        byte[] Bytes { get; }
        IPositionConvert Bpc { get; }

        bool Get(ref BitPosition bPos);
        void Set(ref BitPosition bPos, bool value);
        void Toggle(ref BitPosition bPos);

        IBitMap CreateSnapshot();

        IBitMap CreateRegionSnapshot(Rectangle rect);

        void BlockCopy(IBitMap source, Rectangle sourceRect, Rectangle destRect);
        void BlockCopy(IBitMap source, Point destLocation);
    }
}