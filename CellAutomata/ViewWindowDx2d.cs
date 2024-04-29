using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using System.Diagnostics;
using System.Drawing;

namespace CellAutomata;

public class ViewWindowDx2d : ViewWindow
{
    private readonly ID2dContext _ctx;

    private readonly RawColor4 _aliveColor = new(1, 1, 1, 1); // white
    private readonly RawColor4 _deadColor = new(0, 0, 0, 1); // black
    private readonly RawColor4 _idleColor = new(0.5f, 0.5f, 0.5f, 1); // gray
    private readonly RawColor4 _gridLineColor = new(0.2f, 0.2f, 0.2f, 1); // dark gray
    private readonly RawColor4 _thumbnailViewWindowColor = new(1, 0, 0, 1); // red
    private readonly RawColor4 _selectionColor = new(0, 0, 1, 0.45f); // light blue
    private readonly RawColor4 _selectionBorderColor = new(0, 0, 1, 1); // blue
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

    public ViewWindowDx2d(CellEnvironment cellEnvironment, ID2dContext ctx, int width, int height, int cellSize)
        : base(cellEnvironment, width, height, cellSize)
    {
        _textRender = new TextRenderer();
        _ctx = ctx;

        _textForamt = new TextFormat(_textFactory, "Trebuchet MS", 11f);

        CreateBrushes();
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

    public override void Resize(int width, int height, int cellSize)
    {
        _ctx.Resize(
           width: width * cellSize,
           height: height * cellSize
        );

        DisposeBrushes();
        CreateBrushes();

        base.Resize(width, height, cellSize);
    }

    private ulong _frames = 0;
    private readonly Stopwatch _sw = Stopwatch.StartNew();

    private float _fps;

    public override void Draw(Graphics? g)
    {
        var renderer = _ctx.GetRenderer();
        var totalRows = _cellEnvironment.Height;
        var totalColumns = _cellEnvironment.Width;

        var bitmap = _cellEnvironment.BitMap;

        renderer.BeginDraw();

        renderer.Clear(_deadColor); // black  
        DrawMainView2(bitmap);
        DrawGridLines();
        DrawSelection();
        DrawThumbnail(bitmap, totalRows, totalColumns);

        var genText =
            $"Generation: {_cellEnvironment.Generation:#,0}; " +
            $"Population: {_cellEnvironment.Population:#,0}; " +
            $"CPU Time: {_cellEnvironment.MsCPUTime:#,0} ms; " +
            $"FPS: {_fps:0.0}";

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

        var layout = new TextLayout(_textFactory, text, _textForamt, _width * _cellSize, _height * _cellSize);
        layout.SetDrawingEffect(_textBrush, new TextRange(0, text.Length));
        var metrics = layout.HitTestTextRange(0, text.Length, 15, 0);
        var rect = new RawRectangleF(15, 0, metrics[0].Width + 15, metrics[0].Height);

        renderer.FillRectangle(rect, _deadBrush);
        layout.Draw(_textRender, 15, 0);
    }

    private void DrawMainView2(IBitMap bitmap)
    {
        var viewRect = new Rectangle(
             x: _left,
             y: _top,
              width: _width,
              height: _height);

            var points = bitmap.QueryRegion(true, viewRect);

        var renderer = _ctx.GetRenderer();

        foreach (var point in points)
        {
            var x = (point.X - _left) * _cellSize;
            var y = (point.Y - _top) * _cellSize;

            var rect = new RawRectangleF(
                left: x,
                top: y,
                right: x + _cellSize,
                bottom: y + _cellSize);

            renderer.FillRectangle(rect, _aliveBrush);
        }

    }

    private void DrawMainView(IBitMap bitmap)
    {
        var renderer = _ctx.GetRenderer();
        var cellSize = _cellSize;

        RawRectangleF lineRect;
        for (int row = _top; row < _top + _height; row++)
        {
            var top = (row - _top) * cellSize;
            int lineContunuesWidth = 0;

            lineRect.Left = 0;
            lineRect.Right = 0;
            lineRect.Top = top;
            lineRect.Bottom = top + cellSize;

            for (int col = _left; col < _left + _width; col++)
            {

                if (row >= _cellEnvironment.Height || col >= _cellEnvironment.Width)
                {
                    var rect = new RawRectangleF(
                        left: (col - _left) * cellSize,
                        top: (row - _top) * cellSize,
                        right: (col - _left + 1) * cellSize,
                        bottom: (row - _top + 1) * cellSize
                    );
                    renderer.FillRectangle(rect, _idleBrush);
                    continue;
                }

                var bPos = bitmap.Bpc.Transform(row, col);
                var cell = bitmap.Get(ref bPos);

                if (cell)
                {
                    // alive
                    lineContunuesWidth += cellSize;
                    continue;
                }

                // dead

                if (lineContunuesWidth > 0)
                {
                    lineRect.Right = lineRect.Left + lineContunuesWidth;
                    renderer.FillRectangle(lineRect, _aliveBrush);

                    lineContunuesWidth = 0;
                    lineRect.Left = lineRect.Right + cellSize;
                }
                else
                {
                    lineRect.Left += cellSize;
                }
            }

            if (lineContunuesWidth > 0)
            {
                lineRect.Right = lineRect.Left + lineContunuesWidth;
                renderer.FillRectangle(lineRect, _aliveBrush);
            }
        }
    }

    private void DrawGridLines()
    {
        if (_cellSize < 10) return;
        var renderer = _ctx.GetRenderer();
        var cellSize = _cellSize;

        for (int row = 0; row < _height; row++)
        {
            var y = row * cellSize;
            renderer.DrawLine(new RawVector2(0, y), new RawVector2(_width * cellSize, y), _gridLineBrush);
        }

        for (int col = 0; col < _width; col++)
        {
            var x = col * cellSize;
            renderer.DrawLine(new RawVector2(x, 0), new RawVector2(x, _height * cellSize), _gridLineBrush);
        }
    }

    private void DrawSelection()
    {
        var renderer = _ctx.GetRenderer();
        var selection = GetSelection();

        if (selection.IsEmpty)
        {
            return;
        }

        var left = (selection.X - _left) * _cellSize;
        var top = (selection.Y - _top) * _cellSize;

        var rect = new RawRectangleF(
            left: left,
            top: top,
            right: left + selection.Width * _cellSize,
            bottom: top + selection.Height * _cellSize);

        renderer.FillRectangle(rect, _selectionBrush);
        renderer.DrawRectangle(rect, _selectionBorderBrush);
    }

    private void DrawThumbnail(IBitMap bitmap, int totalRows, int totalColumns)
    {
        var renderer = _ctx.GetRenderer();
        var thumbWidth = ThumbnailWidth;
        var cellSize = 1;
        var thumbHeight = totalRows * thumbWidth / totalColumns;

        // top right corner
        var thumbLeft = 15;
        var thumbTop = 15;

        var rect = new RawRectangleF(
            left: thumbLeft,
            top: thumbTop,
            right: thumbLeft + thumbWidth,
            bottom: thumbTop + thumbHeight);

        // draw thumbnail background
        renderer.FillRectangle(rect, _deadBrush);

        // draw thumbnail border
        renderer.DrawRectangle(rect, _aliveBrush);

        // draw thumbnail cells
        //for (int row = 0; row < totalRows; row += 10)
        //{
        //    for (int col = 0; col < totalColumns; col += 10)
        //    {
        //        var bPos = bitmap.Bpc.Transform(row, col);
        //        var cell = bitmap.Get(ref bPos);

        //        if (!cell) continue;

        //        var calcX = thumbLeft + ((col / (float)totalColumns) * thumbWidth);
        //        var calcY = thumbTop + ((row / (float)totalRows) * thumbHeight);

        //        var rect2 = new RawRectangleF(
        //            left: calcX,
        //            top: calcY,
        //            right: cellSize + calcX,
        //            bottom: cellSize + calcY
        //            );

        //        renderer.FillRectangle(rect2, _aliveBrush);
        //    }
        //}

        // draw thumbnail view window
        var thumbViewWidth = Math.Min(thumbWidth, _width / (float)totalColumns * thumbWidth);
        var thumbViewHeight = Math.Min(thumbHeight, _height / (float)totalRows * thumbHeight);
        var thumbViewLeft = thumbLeft + (_left / (float)totalColumns * thumbWidth);
        var thumbViewTop = thumbTop + (_top / (float)totalRows * thumbHeight);

        // adjust thumbViewWidth 
        thumbViewWidth = Math.Min(thumbViewWidth, thumbLeft + thumbWidth - thumbViewLeft);

        // adjust thumbViewHeight
        thumbViewHeight = Math.Min(thumbViewHeight, thumbViewTop + thumbHeight - thumbViewTop);

        var thumbViewRect = new RawRectangleF(
            left: thumbViewLeft,
            top: thumbViewTop,
            right: thumbViewLeft + thumbViewWidth,
            bottom: thumbViewTop + thumbViewHeight);

        renderer.DrawRectangle(thumbViewRect, _thumbnailViewWindowBrush);
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
