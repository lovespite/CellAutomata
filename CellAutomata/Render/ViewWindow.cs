using System.Diagnostics;
using CellAutomata.Algos;

namespace CellAutomata.Render;

public class ViewWindow : ViewWindowBase
{
    private readonly nint _canvas;

    public ViewWindow(CellEnvironment cells, Size vwSize, nint canvas)
        : base(cells, vwSize.Width, vwSize.Height, 0)
    {
        if (cells.LifeMap is not HashLifeMap)
            throw new ArgumentException("ViewWindowDx2dRaw is only support for HashLifeMap");

        _canvas = canvas;

        _vwSize = vwSize;
        _center = new PointL(0, 0);
    }

    private Size _vwSize;
    private PointL _center; 

    public override void MoveTo(long left, long top)
    {
        CenterX = left;
        CenterY = top;

        _center = new PointL(left, top);
    }

    public override void Resize(int pixelViewWidth, int pixelViewHeight)
    {
        _vwSize = new Size(pixelViewWidth, pixelViewHeight);

        base.Resize(pixelViewWidth, pixelViewHeight);
    }

    protected override void Draw(Graphics? graphics)
    {
        DrawMainView4(CellEnvironment.LifeMap.GetDcRender());
    }

    private ulong _frames = 0;
    private readonly Stopwatch _sw = Stopwatch.StartNew();
    private readonly Stopwatch _sw2 = Stopwatch.StartNew();

    private float _fps; // frames per second 
    private string _text = string.Empty;

    private void DrawMainView4(IDcRender render)
    {
        if (_canvas == 0) return;

        var mag = Magnify;

        var selection = GetSelection();

        ViewInfo sel = new();

        if (selection.IsEmpty)
        {
            sel.EMPTY = 1;
        }
        else
        {
            sel.EMPTY = 0;

            sel.psl_x1 = selection.X;
            sel.psl_y1 = selection.Y;
            sel.psl_x2 = selection.Right;
            sel.psl_y2 = selection.Bottom;
        }

        render.DrawViewportDc(_canvas, mag, _vwSize, _center, sel, _text);

        if (_sw.ElapsedMilliseconds > 500)
        {
            _fps = (float)(_frames / _sw.Elapsed.TotalSeconds);
            _frames = 0;
            _sw.Restart();
        }

        if (_sw2.ElapsedMilliseconds > 100)
        {
            _text =
                $"{CellEnvironment.LifeMap.GetType().Name}, {CellEnvironment.LifeMap.Rule}\n" +
                $"Population: {CellEnvironment.Population:#,0}, Generation: {CellEnvironment.Generation:#,0}\n" +
                $"Position: {MouseCellPoint}  Mag: {mag}, View: {_center}\n" +
                $"CPU Time: {CellEnvironment.MsCpuTime:#,0} ms  FPS: {_fps:0.0}";

            _sw2.Restart();
        }

        ++_frames;
    }
}