namespace CellAutomata.Algos;

public struct RectangleL
{
    public SizeL Size;
    public PointL Location;
    internal static readonly RectangleL Empty = new(new PointL(0, 0), SizeL.Empty);

    public override string ToString()
    {
        return $"x={Location.X}, y={Location.Y}, width={Size.Width}, height={Size.Height}";
    }

    public long Area() => Size.Width * Size.Height;

    public readonly bool IsEmpty => Size.IsEmpty;

    public readonly PointL Location2 => new(Location.X + Size.Width - 1, Location.Y + Size.Height - 1);

    public long Top
    {
        get => Location.Y;
        set => Location.Y = value;
    }

    public long X
    {
        get => Left;
        set => Left = value;
    }

    public long Y
    {
        get => Top;
        set => Top = value;
    }

    public long Left
    {
        get => Location.X;
        set => Location.X = value;
    }

    public long Bottom
    {
        get => Location.Y + Size.Height;
        set
        {
            if (value < Location.Y)
                throw new ArgumentOutOfRangeException(nameof(Bottom), @"Bottom must be greater than or equal to Top");

            Size.Height = value - Location.Y;
        }
    }

    public long Right
    {
        get => Location.X + Size.Width;
        set
        {
            if (value < Location.X)
                throw new ArgumentOutOfRangeException(nameof(Right), @"Right must be greater than or equal to Left");

            Size.Width = value - Location.X;
        }
    }

    public long Width
    {
        get => Size.Width;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(Width), @"Width must be greater than or equal to 0");

            Size.Width = value;
        }
    }

    public long Height
    {
        get => Size.Height;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(Height), @"Height must be greater than or equal to 0");

            Size.Height = value;
        }
    }

    public bool Contains(long x, long y)
    {
        return x >= Location.X && x < Location.X + Size.Width && y >= Location.Y && y < Location.Y + Size.Height;
    }

    public bool Contains(PointL pt)
    {
        return Contains(pt.X, pt.Y);
    }

    public bool Contains(RectangleL rect)
    {
        return (Location.X <= rect.Location.X) && (rect.Location.X + rect.Size.Width <= Location.X + Size.Width) &&
               (Location.Y <= rect.Location.Y) && (rect.Location.Y + rect.Size.Height <= Location.Y + Size.Height);
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