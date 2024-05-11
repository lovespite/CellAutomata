using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System.Buffers;
using System.Diagnostics;
using System.Drawing;

namespace CellAutomata;

public class ViewWindowDx2d : ViewWindowBase
{
    private readonly ID2dContext _ctx;

    private readonly RawColor4 _aliveColor = new(1, 1, 1, 1); // white
    private readonly RawColor4 _deadColor = new(0, 0, 0, 1); // black
    private readonly RawColor4 _idleColor = new(0.5f, 0.5f, 0.5f, 1); // gray
    private readonly RawColor4 _gridLineColor = new(0.2f, 0.2f, 0.2f, 1); // dark gray
    private readonly RawColor4 _thumbnailViewWindowColor = new(1, 0, 0, 1); // red
    private readonly RawColor4 _selectionColor = new(0xb2 / 255f, 0xd2 / 255f, 0x35 / 255f, 0.45f); // light blue
    private readonly RawColor4 _selectionBorderColor = new(0xb2 / 255f, 0xd2 / 255f, 0x35 / 255f, 1);
    private readonly RawColor4 _textColor = new(0, 1, 0, 1); // green

    private SolidColorBrush _aliveBrush = null!;
    private SolidColorBrush _deadBrush = null!;
    private SolidColorBrush _idleBrush = null!;
    private SolidColorBrush _gridLineBrush = null!;
    private SolidColorBrush _thumbnailViewWindowBrush = null!;
    private SolidColorBrush _selectionBrush = null!;
    private SolidColorBrush _selectionBorderBrush = null!;
    private SolidColorBrush _textBrush = null!;

    private readonly SharpDX.DirectWrite.Factory _textFactory = new();
    private readonly TextFormat _textForamt;
    private readonly TextRenderer _textRender;

    public nint CanvasHandle { get; set; }

    public ViewWindowDx2d(CellEnvironment cellEnvironment, ID2dContext ctx, int pixelViewWidth, int pixelViewHeight, float cellSize)
        : base(cellEnvironment, pixelViewWidth, pixelViewHeight, cellSize)
    {
        _textRender = new TextRenderer();
        _ctx = ctx;

        _textForamt = new TextFormat(_textFactory, "Trebuchet MS", 11f);

        CreateBrushes();
    }

    public override void MoveTo(int left, int top)
    {
        _centerX = left;
        _centerY = top;
    }

    private void CreateBrushes()
    {
        var renderer = _ctx.GetRenderer();
        _aliveBrush = new SolidColorBrush(renderer, _aliveColor); // black
        _deadBrush = new SolidColorBrush(renderer, _deadColor); // white
        _idleBrush = new SolidColorBrush(renderer, _idleColor); // gray
        _gridLineBrush = new SolidColorBrush(renderer, _gridLineColor); // dark gray
        _thumbnailViewWindowBrush = new SolidColorBrush(renderer, _thumbnailViewWindowColor); // red
        _selectionBrush = new SolidColorBrush(renderer, _selectionColor); // light blue
        _selectionBorderBrush = new SolidColorBrush(renderer, _selectionBorderColor); // blue
        _textBrush = new SolidColorBrush(renderer, _textColor);
    }

    private void DisposeBrushes()
    {
        _aliveBrush.Dispose();
        _deadBrush.Dispose();
        _idleBrush.Dispose();
        _gridLineBrush.Dispose();
        _thumbnailViewWindowBrush.Dispose();
        _selectionBrush.Dispose();
        _selectionBorderBrush.Dispose();
        _textBrush.Dispose();
    }

    public override void Resize(int pixelViewWidth, int pixelViewHeight)
    {
        _ctx.Resize(
           width: pixelViewWidth,
           height: pixelViewHeight
        );

        DisposeBrushes();
        CreateBrushes();

        base.Resize(pixelViewWidth, pixelViewHeight);
    }

    private ulong _frames = 0;
    private readonly Stopwatch _sw = Stopwatch.StartNew();

    private float _fps; // frames per second 

    public override void Draw(Graphics? g)
    {
        var renderer = _ctx.GetRenderer();

        var bitmap = _cellEnvironment.BitMap;
        var genText =
            $"Generation: {_cellEnvironment.Generation:#,0} Population: {_cellEnvironment.Population:#,0}\n" +
            $"Position: {MouseCellPoint}\n" +
            $"CPU Time: {_cellEnvironment.MsCPUTime:#,0}\n" +
            $"GPS: {_gps:0.0} FPS: {_fps:0.0}\n";

        renderer.BeginDraw();

        renderer.Clear(_deadColor); // black  


        if (_cellSize == 1)
            DrawMainView3(bitmap);
        else
            DrawMainView2(bitmap);

        DrawGridLines();
        DrawSelection();

        DrawGenerationText(genText);

        renderer.EndDraw();

        if (_sw.ElapsedMilliseconds > 1000)
        {
            _fps = (float)(_frames / _sw.Elapsed.TotalSeconds);
            _frames = 0;
            _sw.Restart();
        }
        ++_frames;
    }

    private void DrawGenerationText(string text)
    {
        var renderer = _ctx.GetRenderer();
        _textRender.AssignResources(renderer, _aliveBrush);

        var layout = new TextLayout(_textFactory, text, _textForamt, _pxViewWidth, _pxViewHeight);
        layout.SetDrawingEffect(_textBrush, new TextRange(0, text.Length));

        var metrics = layout.HitTestTextRange(0, text.Length, 15, 0);

        for (int i = 0; i < metrics.Length; i++)
        {
            var m = metrics[i];

            var rect = new RawRectangleF(
                left: m.Left,
                top: m.Top,
                right: m.Left + m.Width,
                bottom: m.Top + m.Height);

            renderer.FillRectangle(rect, _deadBrush);
        }

        layout.Draw(_textRender, 15, 0);
    }
    private Rectangle GetViewRect()
    {
        var columns = Math.Max(2, (int)Math.Ceiling(_pxViewWidth / _cellSize)); // at least 2 columns
        var rows = Math.Max(2, (int)Math.Ceiling(_pxViewHeight / _cellSize)); // at least 2 rows

        var viewRect = new Rectangle(
            x: _centerX - columns / 2,
            y: _centerY - rows / 2,
            width: columns,
            height: rows);
        return viewRect;
    }


    private void DrawMainView2(ILifeMap bitmap)
    {
        var viewCX = _pxViewWidth * 0.5F;
        var viewCY = _pxViewHeight * 0.5F;

        var viewRect = GetViewRect();
        var points = bitmap.QueryRegion(true, viewRect);

        var renderer = _ctx.GetRenderer();

        RawRectangleF rect;
        for (int i = 0; i < points.Length; i++)
        {
            var p = points[i];

            rect.Left = viewCX + (p.X - _centerX) * _cellSize;
            rect.Top = viewCY + (p.Y - _centerY) * _cellSize;
            rect.Right = rect.Left + Math.Max(1, _cellSize);
            rect.Bottom = rect.Top + Math.Max(1, _cellSize);

            renderer.FillRectangle(rect, _aliveBrush);
        }
    }

    private readonly BitmapProperties bmpProps = new(new SharpDX.Direct2D1.PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Ignore));
    private void DrawMainView3(ILifeMap bitmap)
    {
        var renderer = _ctx.GetRenderer();
        // var viewCX = _pxViewWidth * 0.5F;
        // var viewCY = _pxViewHeight * 0.5F;

        Rectangle viewRect = GetViewRect();

        var bmpData = bitmap.DrawRegionBitmapBGRA(viewRect);
        using SharpDX.Direct2D1.Bitmap direct2DBitmap = new(renderer, new Size2(viewRect.Width, viewRect.Height), bmpProps);
        var stride = viewRect.Width * 4; // 4 bytes per pixel (BGRA)
        direct2DBitmap.CopyFromMemory(bmpData, stride);

        renderer.DrawBitmap(direct2DBitmap, new RawRectangleF(0, 0, _pxViewWidth, _pxViewHeight), 1, BitmapInterpolationMode.Linear);

        ArrayPool<byte>.Shared.Return(bmpData);
    }



    private void DrawSelection()
    {
        var viewCX = _pxViewWidth * 0.5F;
        var viewCY = _pxViewHeight * 0.5F;
        var renderer = _ctx.GetRenderer();
        var selection = GetSelection();

        if (selection.IsEmpty)
        {
            return;
        }

        var left = (selection.X - _centerX) * _cellSize + viewCX;
        var top = (selection.Y - _centerY) * _cellSize + viewCY;

        var rect = new RawRectangleF(
            left: left,
            top: top,
            right: left + selection.Width * _cellSize,
            bottom: top + selection.Height * _cellSize);

        renderer.FillRectangle(rect, _selectionBrush);
        // renderer.DrawRectangle(rect, _selectionBorderBrush);
    }

    private void DrawGridLines()
    {
        if (_cellSize < 8) return;

        var renderer = _ctx.GetRenderer();

        var cellSize = _cellSize;
        var viewCX = _pxViewWidth * 0.5F;
        var viewCY = _pxViewHeight * 0.5F;


        RawVector2 p1;
        RawVector2 p2;

        for (int x = (int)viewCX, i = 0; x < _pxViewWidth; x += (int)cellSize, i++)
        {
            p1 = new RawVector2(x, 0);
            p2 = new RawVector2(x, _pxViewHeight);
            renderer.DrawLine(p1, p2, _gridLineBrush, 1);

            if (i == 0) continue;
            p1 = new RawVector2(_pxViewWidth - x, 0);
            p2 = new RawVector2(_pxViewWidth - x, _pxViewHeight);
            renderer.DrawLine(p1, p2, _gridLineBrush, 1);
        }

        for (int y = (int)viewCY, i = 0; y < _pxViewHeight; y += (int)cellSize, i++)
        {
            p1 = new RawVector2(0, y);
            p2 = new RawVector2(_pxViewWidth, y);
            renderer.DrawLine(p1, p2, _gridLineBrush, 1);

            if (i == 0) continue;
            p1 = new RawVector2(0, _pxViewHeight - y);
            p2 = new RawVector2(_pxViewWidth, _pxViewHeight - y);
            renderer.DrawLine(p1, p2, _gridLineBrush, 1);
        }
    }

    public void Dispose()
    {
        _aliveBrush.Dispose();
        _deadBrush.Dispose();
        _idleBrush.Dispose();
        _gridLineBrush.Dispose();
        _thumbnailViewWindowBrush.Dispose();
        _selectionBrush.Dispose();
        _selectionBorderBrush.Dispose();
    }
}
