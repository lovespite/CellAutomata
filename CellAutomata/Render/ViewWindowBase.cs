using CellAutomata.Algos;
using System.Diagnostics;
using System.Drawing;

namespace CellAutomata.Render;
public abstract class ViewWindowBase
{
    protected readonly CellEnvironment _cellEnvironment;

    protected int _pxViewWidth;
    protected int _pxViewHeight;

    protected float _cellSize;
    public float CellSize => _cellSize;

    protected Point _selStart;
    protected Point _selEnd;
    protected int _selected = 0;
    protected readonly Font _font = new("Arial", 9);

    protected int _centerX = 0;
    protected int _centerY = 0;

    protected float _gps; // generations per second

    public float Gps
    {
        set => _gps = value;
    }

    public ViewWindowBase(CellEnvironment cellEnvironment, int pixelViewWidth, int pixelViewHeight, float cellSize)
    {
        _cellEnvironment = cellEnvironment;
        _pxViewWidth = pixelViewWidth;
        _pxViewHeight = pixelViewHeight;
        _cellSize = cellSize;
    }

    public void ZoomIn()
    {
        var isMoveView = MousePoint.X >= 0 && MousePoint.Y >= 0;
        var mcp = MouseCellPoint; // current mouse cell point

        if (_cellSize < 21)
            _cellSize *= 2;

        //if (!isMoveView) return;
        //var newMcp = MouseCellPoint; // new mouse cell point

        //var dx = mcp.X - newMcp.X;
        //var dy = mcp.Y - newMcp.Y;

        //_centerX += dx;
        //_centerY += dy;
    }

    public void ZoomOut()
    {
        var isMoveView = MousePoint.X >= 0 && MousePoint.Y >= 0;
        var mcp = MouseCellPoint;

        if (_cellSize > 1)
        {
            _cellSize /= 2;
        }

        //if (!isMoveView) return;
        //var newMcp = MouseCellPoint;

        //var dx = mcp.X - newMcp.X;
        //var dy = mcp.Y - newMcp.Y;

        //_centerX += dx;
        //_centerY += dy;
    }

    public Point MousePoint { get; set; }
    public Point MouseCellPoint
    {
        get
        {
            try
            {
                return _cellEnvironment.LifeMap.At(MousePoint.X, MousePoint.Y);
            }
            catch (NotImplementedException)
            {
                Debug.WriteLine("Fallback to MouseCellPoint");

                var relX = (MousePoint.X - _pxViewWidth / 2) / _cellSize;
                var relY = (MousePoint.Y - _pxViewHeight / 2) / _cellSize;

                if (relX < 0) relX -= 1;
                if (relY < 0) relY -= 1;

                return new Point(_centerX + (int)relX, _centerY + (int)relY);
            }
        }
    }

    public bool IsSelected => _selected > 0;

    public int Left => _centerX;
    public Point Location => new(_centerX, _centerY);
    public Point SelectionEnd => _selEnd;

    public Point SelectionStart => _selStart;

    public int ThumbnailWidth { get; set; } = 120;
    public int Top => _centerY;

    public event EventHandler? SelectionChanged;

    public void ClearSelection()
    {
        _selected = 0;
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public Rectangle GetSelection()
    {
        if (_selected == 0)
        {
            return Rectangle.Empty;
        }

        var ps = _selStart;
        var pe = _selEnd;

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
        _centerX = cx;
        _centerY = cy;
    }

    public virtual void Resize(int pxViewWidth, int pxViewHeight)
    {
        _pxViewWidth = pxViewWidth;
        _pxViewHeight = pxViewHeight;
    }

    public void SetSelection(Point p1, Point p2)
    {
        _selStart = p1;
        _selEnd = p2;

        _selected = 1;

        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public abstract void Draw(Graphics? graphics);
}