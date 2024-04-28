
using System.Diagnostics;

namespace CellAutomata;

public class ViewWindow : ViewWindowBase
{
    public ViewWindow(CellEnvironment cellEnvironment, int width, int height, int cellSize) : base(cellEnvironment, width, height, cellSize)
    { 
    }

    private ulong _frames = 0;
    private readonly Stopwatch _sw = Stopwatch.StartNew();

    private float _fps;

    public override void Draw(Graphics? graphics)
    {
        if (graphics is null) return;

        var totalRows = _cellEnvironment.Height;
        var totalColumns = _cellEnvironment.Width;

        using var bitmap = _cellEnvironment.CreateSnapshot(); 
        var genText = 
            $"Generation: {_cellEnvironment.Generation}, " +
            $"CPU Time: {_cellEnvironment.MsCPUTime} ms, " +
            $"FPS: {_fps:0.0}";

        graphics.Clear(Color.Black);
        DrawMainView(graphics, bitmap);
        DrawGridLines(graphics);
        DrawSelection(graphics);
        DrawGenerationText(graphics, genText);
        DrawThumbnail(graphics, bitmap, totalRows, totalColumns);

        if (_sw.ElapsedMilliseconds > 1000)
        {
            _fps = (float)(_frames / _sw.Elapsed.TotalSeconds);
            _frames = 0;
            _sw.Restart();
        }
        ++_frames;
    }

    protected void DrawThumbnail(Graphics? graphics, IBitMap bitmap, int totalRows, int totalColumns)
    {
        if (graphics is null) return;

        var thumbWidth = ThumbnailWidth;
        var cellSize = 1;
        var thumbHeight = totalRows * thumbWidth / totalColumns;

        // top right corner
        var thumbLeft = 15;
        var thumbTop = 15;

        var rect = new Rectangle(
            x: thumbLeft,
            y: thumbTop,
            width: thumbWidth,
            height: thumbHeight);

        // draw thumbnail background
        graphics.FillRectangle(Brushes.Black, rect);

        // draw thumbnail border
        graphics.DrawRectangle(Pens.White, rect);

        // draw thumbnail cells
        //for (int row = 0; row < totalRows; row++)
        //{
        //    for (int col = 0; col < totalColumns; col++)
        //    {
        //        var bPos = bitmap.Bpc.Transform(row, col);
        //        var cell = bitmap.Get(ref bPos);

        //        if (!cell) continue;

        //        var calcX = thumbLeft + ((col / (float)totalColumns) * thumbWidth);
        //        var calcY = thumbTop + ((row / (float)totalRows) * thumbHeight);

        //        graphics.FillRectangle(Brushes.White, calcX, calcY, cellSize, cellSize);
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

        var thumbViewRect = new RectangleF(
            x: thumbViewLeft,
            y: thumbViewTop,
            width: thumbViewWidth,
            height: thumbViewHeight);

        graphics.DrawRectangle(Pens.Red, thumbViewRect);

    }

    protected void DrawMainView(Graphics? graphics, IBitMap bitmap)
    {
        if (graphics is null) return;

        var cellSize = _cellSize;

        for (int row = _top; row < _top + _height; row++)
        {
            for (int col = _left; col < _left + _width; col++)
            {
                if (row >= _cellEnvironment.Height || col >= _cellEnvironment.Width)
                {
                    graphics.FillRectangle(Brushes.Gray, (col - _left) * cellSize, (row - _top) * cellSize, cellSize, cellSize);
                    continue;
                }

                var bPos = bitmap.Bpc.Transform(row, col);
                var cell = bitmap.Get(ref bPos);

                if (!cell) continue;

                var calcX = (col - _left) * cellSize;
                var calcY = (row - _top) * cellSize;

                graphics.FillRectangle(Brushes.White, calcX, calcY, cellSize, cellSize);
            }
        }
    }

    private readonly Pen _gridPen = new(Color.FromArgb(0x12, Color.White), 1);
    protected void DrawGridLines(Graphics? graphics)
    {
        if (graphics is null) return;

        var cellSize = _cellSize;

        for (int row = 0; row < _height; row++)
        {
            graphics.DrawLine(_gridPen, 0, row * cellSize, _width * cellSize, row * cellSize);
        }

        for (int col = 0; col < _width; col++)
        {
            graphics.DrawLine(_gridPen, col * cellSize, 0, col * cellSize, _height * cellSize);
        }
    }

    private readonly Brush _selectionBrush = new SolidBrush(Color.FromArgb(0x45, Color.Blue));
    protected void DrawSelection(Graphics? graphics)
    {
        if (graphics is null) return;

        var selection = GetSelection();

        if (selection.IsEmpty)
        {
            return;
        }

        var rect = new Rectangle(
            x: (selection.X - _left) * _cellSize,
            y: (selection.Y - _top) * _cellSize,
            width: selection.Width * _cellSize,
            height: selection.Height * _cellSize);

        graphics.FillRectangle(_selectionBrush, rect);
        graphics.DrawRectangle(Pens.Blue, rect);
    }

    protected void DrawGenerationText(Graphics? graphics, string genText)
    {
        if (graphics is null) return;

        var size = graphics.MeasureString(genText, _font);
        var rect = new RectangleF(15, 0, size.Width, size.Height);
        graphics.FillRectangle(Brushes.Black, rect);
        graphics.DrawString(genText, _font, Brushes.White, rect);
    }
}
