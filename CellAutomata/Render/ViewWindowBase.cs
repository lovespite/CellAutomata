using System.Diagnostics;
using CellAutomata.Algos;

namespace CellAutomata.Render;

public abstract class ViewWindowBase
{
    protected readonly CellEnvironment CellEnvironment;

    protected int PxViewWidth;
    protected int PxViewHeight;

    protected int Magnify = 0;
    public float CellSize => Magnify >= 0 ? 1 << Magnify : 1f / (1 << -Magnify);

    protected PointL SelStart;
    protected PointL SelEnd;
    protected long Selected = 0;
    protected readonly Font DefaultFont = new("Arial", 9);

    protected long CenterX = 0;
    protected long CenterY = 0;

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
        if (Magnify > -16)
        {
            Magnify--;
        }
    }

    public Point MousePoint { get; set; }

    public PointL MouseCellPoint
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

                return new PointL(CenterX + (long)relX, CenterY + (long)relY);
            }
        }
    }

    public bool IsSelected => Selected > 0;

    public PointL Location => new(CenterX, CenterY);
    public PointL SelectionEnd => SelEnd;

    public PointL SelectionStart => SelStart;

    public event EventHandler? SelectionChanged;

    public void ClearSelection()
    {
        Selected = 0;
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public RectangleL GetSelection()
    {
        if (Selected == 0)
        {
            return RectangleL.Empty;
        }

        var ps = SelStart;
        var pe = SelEnd;

        var x1 = Math.Min(ps.X, pe.X);
        var y1 = Math.Min(ps.Y, pe.Y);
        var x2 = Math.Max(ps.X, pe.X);
        var y2 = Math.Max(ps.Y, pe.Y);

        var w = x2 - x1 + 1;
        var h = y2 - y1 + 1;

        return new RectangleL(new PointL(x1, y1), new SizeL(w, h));
    }

    public virtual void MoveTo(long cx, long cy)
    {
        CenterX = cx;
        CenterY = cy;
    }

    public virtual void Resize(int pxViewWidth, int pxViewHeight)
    {
        PxViewWidth = pxViewWidth;
        PxViewHeight = pxViewHeight;
    }

    public void SetSelection(PointL p1, PointL p2)
    {
        SelStart = p1;
        SelEnd = p2;

        Selected = 1;

        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public abstract void Draw(Graphics? graphics);
}