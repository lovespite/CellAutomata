namespace CellAutomata;

public class ViewWindow
{
    private readonly CellEnvironment _cellEnvironment;

    private int _width;
    private int _height;
    private int _cellSize;

    private Point _selStart;
    private Point _selEnd;

    public Point SelectionStart => _selStart;
    public Point SelectionEnd => _selEnd;

    private int _selected = 0;
    public bool IsSelected => _selected > 0;

    public Rectangle GetSelection()
    {
        if (_selected == 0)
        {
            return Rectangle.Empty;
        }

        var ps = _selStart;
        var pe = _selEnd;

        var x = Math.Min(ps.X, pe.X);
        var y = Math.Min(ps.Y, pe.Y);
        var width = Math.Abs(ps.X - pe.X) + 1;
        var height = Math.Abs(ps.Y - pe.Y) + 1;

        return new Rectangle(x, y, width, height);
    }

    public void SetSelection(Point p1, Point p2)
    {
        p1.X = Math.Max(0, p1.X);
        p1.Y = Math.Max(0, p1.Y);

        p2.X = Math.Max(0, p2.X);
        p2.Y = Math.Max(0, p2.Y);

        p1.X = Math.Min(_cellEnvironment.Width - 1, p1.X);
        p1.Y = Math.Min(_cellEnvironment.Height - 1, p1.Y);

        p2.X = Math.Min(_cellEnvironment.Width - 1, p2.X);
        p2.Y = Math.Min(_cellEnvironment.Height - 1, p2.Y);

        _selStart = p1;
        _selEnd = p2;

        _selected = 1;

        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ClearSelection()
    {
        _selected = 0;
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? SelectionChanged;

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
        DrawGridLines(graphics);
        DrawSelection(graphics);
        DrawGenerationText(graphics, genText);
        DrawThumbnail(graphics, bitmap, bpc, totalRows, totalColumns);
    }

    private void DrawThumbnail(Graphics graphics, IBitMap bitmap, IPositionConvert bpc, int totalRows, int totalColumns)
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

    private void DrawMainView(Graphics graphics, IBitMap bitmap, IPositionConvert bpc)
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

    private readonly Pen _gridPen = new(Color.FromArgb(0x12, Color.White), 1);
    private void DrawGridLines(Graphics graphics)
    {
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
    private void DrawSelection(Graphics graphics)
    {
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

    private void DrawGenerationText(Graphics graphics, string genText)
    {
        var size = graphics.MeasureString(genText, _font);
        var rect = new RectangleF(0, 0, size.Width, size.Height);
        graphics.FillRectangle(Brushes.Black, rect);
        graphics.DrawString(genText, _font, Brushes.White, 0, 0);
    }
}