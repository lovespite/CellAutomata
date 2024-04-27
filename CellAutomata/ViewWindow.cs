namespace CellAutomata;

public class ViewWindow
{
    private readonly CellEnvironment _cellEnvironment;

    private int _width;
    private int _height;
    private int _cellSize;

    private readonly Font _font = new("Arial", 12);

    private int _left = 0;
    private int _top = 0;

    public int Left => _left;
    public int Top => _top;
    public Point Location => new(_left, _top);

    public int ThumbnailWidth { get; set; } = 120;

    public void Resize(int width, int height, int cellSize)
    {
        _width = width;
        _height = height;
        _cellSize = cellSize;
    }

    public void MoveTo(int left, int top)
    {
        _left = left;
        _top = top;

        if (_left > _cellEnvironment.Width - _width)
        {
            _left = _cellEnvironment.Width - _width;
        }

        if (_top > _cellEnvironment.Height - _height)
        {
            _top = _cellEnvironment.Height - _height;
        }

        if (_left < 0)
        {
            _left = 0;
        }

        if (_top < 0)
        {
            _top = 0;
        }

    }

    public void Move(int deltaX, int deltaY)
    {
        MoveTo(_left + deltaX, _top + deltaY);
    }

    public ViewWindow(CellEnvironment cellEnvironment, int width, int height, int cellSize)
    {
        _cellEnvironment = cellEnvironment;
        _width = width;
        _height = height;
        _cellSize = cellSize;
    }

    public void Draw(Graphics graphics)
    {
        var bpc = _cellEnvironment.Bpc;
        var totalRows = _cellEnvironment.Height;
        var totalColumns = _cellEnvironment.Width;

        using var bitmap = _cellEnvironment.CreateSnapshot();
        var genText = $"Generation: {_cellEnvironment.Generation}, Calc: {_cellEnvironment.MsTimeUsed} ms/gen";

        graphics.Clear(Color.Black);
        DrawMainView(graphics, bitmap, bpc);
        DrawGenerationText(graphics, genText);
        DrawThumbnail(graphics, bitmap, bpc, totalRows, totalColumns);
    }

    private void DrawThumbnail(Graphics graphics, IByteArrayBitOperator bitmap, IPositionConvert bpc, int totalRows, int totalColumns)
    {
        var thumbWidth = ThumbnailWidth;
        var cellSize = 1;
        var thumbHeight = totalRows * thumbWidth / totalColumns;

        // top right corner
        var thumbLeft = _width * _cellSize - thumbWidth - 10;
        var thumbTop = 10;

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
        for (int row = 0; row < totalRows; row++)
        {
            for (int col = 0; col < totalColumns; col++)
            {
                var bPos = bpc.Transform(row, col);
                var cell = bitmap.Get(ref bPos);

                if (!cell) continue;

                var calcX = thumbLeft + ((col / (float)totalColumns) * thumbWidth);
                var calcY = thumbTop + ((row / (float)totalRows) * thumbHeight);

                graphics.FillRectangle(Brushes.White, calcX, calcY, cellSize, cellSize);
            }
        }


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

    private void DrawMainView(Graphics graphics, IByteArrayBitOperator bitmap, IPositionConvert bpc)
    {
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

                var bPos = bpc.Transform(row, col);
                var cell = bitmap.Get(ref bPos);

                if (!cell) continue;

                var calcX = (col - _left) * cellSize;
                var calcY = (row - _top) * cellSize;

                graphics.FillRectangle(Brushes.White, calcX, calcY, cellSize, cellSize);
            }
        }
    }

    private void DrawGenerationText(Graphics graphics, string genText)
    {
        var size = graphics.MeasureString(genText, _font);
        var rect = new RectangleF(0, 0, size.Width, size.Height);
        graphics.FillRectangle(Brushes.Black, rect);
        graphics.DrawString(genText, _font, Brushes.White, 0, 0);
    }
}