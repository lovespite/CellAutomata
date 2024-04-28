using System.Diagnostics;
using System.Drawing;

namespace CellAutomata
{
    public abstract class ViewWindowBase
    {
        protected readonly CellEnvironment _cellEnvironment;

        protected int _width;
        protected int _height;
        protected int _cellSize;

        protected Point _selStart;
        protected Point _selEnd;
        protected int _selected = 0;
        protected readonly Font _font = new("Arial", 9);

        protected int _left = 0;
        protected int _top = 0;

        public ViewWindowBase(CellEnvironment cellEnvironment, int width, int height, int cellSize)
        {
            _cellEnvironment = cellEnvironment;
            _width = width;
            _height = height;
            _cellSize = cellSize;
        }

        public int Height => _height;
        public bool IsSelected => _selected > 0;

        public int Left => _left;
        public Point Location => new(_left, _top);
        public Point SelectionEnd => _selEnd;

        public Point SelectionStart => _selStart;

        public int ThumbnailWidth { get; set; } = 120;
        public int Top => _top;

        public int Width => _width;

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

            var x = Math.Min(ps.X, pe.X);
            var y = Math.Min(ps.Y, pe.Y);
            var width = Math.Abs(ps.X - pe.X) + 1;
            var height = Math.Abs(ps.Y - pe.Y) + 1;

            return new Rectangle(x, y, width, height);
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

        public virtual void Resize(int width, int height, int cellSize)
        {
            _width = width;
            _height = height;
            _cellSize = cellSize;

            if (_left + _width > _cellEnvironment.Width)
            {
                _left = _cellEnvironment.Width - _width;
            }

            if (_top + _height > _cellEnvironment.Height)
            {
                _top = _cellEnvironment.Height - _height;
            }
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

        private ulong _frames = 0;
        private readonly Stopwatch _sw = Stopwatch.StartNew();

        public abstract void Draw(Graphics? graphics); 
    }
}