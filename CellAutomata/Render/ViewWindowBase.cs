using System.Diagnostics;

namespace CellAutomata.Render;

public abstract class ViewWindowBase
{
    protected readonly CellEnvironment CellEnvironment;

    protected int PxViewWidth;
    protected int PxViewHeight;

    protected int Magnify = 0;
    public float CellSize => Magnify >= 0 ? 1 << Magnify : 1f / (1 << -Magnify);

    protected Point SelStart;
    protected Point SelEnd;
    protected int Selected = 0;
    protected readonly Font DefaultFont = new("Arial", 9);

    protected int CenterX = 0;
    protected int CenterY = 0;

    protected float GenPerSecond; // generations per second

    public float Gps
    {
        set => GenPerSecond = value;
    }

    public ViewWindowBase(CellEnvironment cellEnvironment, int pixelViewWidth, int pixelViewHeight, int magnify)
    {
        CellEnvironment = cellEnvironment;
        PxViewWidth = pixelViewWidth;
        PxViewHeight = pixelViewHeight;
        Magnify = magnify;
    }

    public void ZoomIn()
    {
        if (Magnify < 6)
        {
            Magnify++;
        }
    }

    public void ZoomOut()
    {
        if (Magnify > -31)
        {
            Magnify--;
        }
    }

    public Point MousePoint { get; set; }

    public Point MouseCellPoint
    {
        get
        {
            try
            {
                return CellEnvironment.LifeMap.At(MousePoint.X, MousePoint.Y);
            }
            catch (NotImplementedException)
            {
                Debug.WriteLine("Fallback to MouseCellPoint");
                double cs = CellSize;

                var relX = (MousePoint.X - PxViewWidth / 2d) / cs;
                var relY = (MousePoint.Y - PxViewHeight / 2d) / cs;

                if (relX < 0) relX -= 1;
                if (relY < 0) relY -= 1;

                return new Point(CenterX + (int)relX, CenterY + (int)relY);
            }
        }
    }

    public bool IsSelected => Selected > 0;

    public int Left => CenterX;
    public Point Location => new(CenterX, CenterY);
    public Point SelectionEnd => SelEnd;

    public Point SelectionStart => SelStart;

    public int ThumbnailWidth { get; set; } = 120;
    public int Top => CenterY;

    public event EventHandler? SelectionChanged;

    public void ClearSelection()
    {
        Selected = 0;
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public Rectangle GetSelection()
    {
        if (Selected == 0)
        {
            return Rectangle.Empty;
        }

        var ps = SelStart;
        var pe = SelEnd;

        var x1 = Math.Min(ps.X, pe.X);
        var y1 = Math.Min(ps.Y, pe.Y);
        var x2 = Math.Max(ps.X, pe.X);
        var y2 = Math.Max(ps.Y, pe.Y);

        var w = x2 - x1 + 1;
        var h = y2 - y1 + 1;

        return new Rectangle(x1, y1, w, h);
    }

    public virtual void MoveTo(int cx, int cy)
    {
        CenterX = cx;
        CenterY = cy;
    }

    public virtual void Resize(int pxViewWidth, int pxViewHeight)
    {
        PxViewWidth = pxViewWidth;
        PxViewHeight = pxViewHeight;
    }

    public void SetSelection(Point p1, Point p2)
    {
        SelStart = p1;
        SelEnd = p2;

        Selected = 1;

        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public abstract void Draw(Graphics? graphics);
}