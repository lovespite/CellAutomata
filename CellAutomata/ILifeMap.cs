namespace CellAutomata;

public enum CopyMode
{
    Overwrite,
    Or,
    And,
    Xor
}

public struct SizeL
{
    public long Width;
    public long Height;

    public SizeL(long width, long height)
    {
        Width = width;
        Height = height;
    }
}

public struct PointL
{
    public long X;
    public long Y;

    public PointL(long x, long y)
    {
        X = x;
        Y = y;
    }
}
public struct RectangleL
{
    public SizeL Size;
    public PointL Location;

    public long Top
    {
        get { return Location.Y; }
        set { Location.Y = value; }
    }

    public long X
    {
        get { return Left; }
        set { Left = value; }
    }

    public long Y
    {
        get { return Top; }
        set { Top = value; }
    }

    public long Left
    {
        get { return Location.X; }
        set { Location.X = value; }
    }

    public long Bottom
    {
        get { return Location.Y + Size.Height - 1; }
        set
        {

            if (value < Location.Y)
                throw new ArgumentOutOfRangeException("Bottom", "Bottom must be greater than or equal to Top");

            Size.Height = value - Location.Y + 1;
        }
    }

    public long Right
    {
        get { return Location.X + Size.Width - 1; }
        set
        {

            if (value < Location.X)
                throw new ArgumentOutOfRangeException("Right", "Right must be greater than or equal to Left");

            Size.Width = value - Location.X + 1;
        }
    }

    public long Width
    {
        get { return Size.Width; }
        set
        {

            if (value < 0)
                throw new ArgumentOutOfRangeException("Width", "Width must be greater than or equal to 0");

            Size.Width = value;
        }
    }

    public long Height
    {
        get { return Size.Height; }
        set
        {

            if (value < 0)
                throw new ArgumentOutOfRangeException("Height", "Height must be greater than or equal to 0");

            Size.Height = value;
        }
    }

    public RectangleL(PointL location, SizeL size)
    {
        Location = location;
        Size = size;
    }

    public RectangleL(long top, long left, long bottom, long right)
    {
        Location = new PointL(left, top);
        Size = new SizeL(right - left + 1, bottom - top + 1);
    }

    public static implicit operator Rectangle(RectangleL rect)
    {
        return new Rectangle((int)rect.Left, (int)rect.Top, (int)rect.Width, (int)rect.Height);
    }

    public static implicit operator RectangleL(Rectangle rect)
    {
        return new RectangleL(rect.Top, rect.Left, rect.Bottom, rect.Right);
    }
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
    RectangleL GetBounds();

    void NextGeneration();

    void SaveRle(Stream stream);
    void ReadRle(Stream stream);
}

