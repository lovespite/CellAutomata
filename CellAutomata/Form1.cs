using System.Diagnostics;

namespace CellAutomata;

public partial class Form1 : Form
{
    private readonly CellEnvironment _env;

    private readonly ViewWindow _view;
    private Thread? _evolutionThread = null;
    private CancellationTokenSource? _cts = null;

    public int CellSize
    {
        get
        {
            return (int)inputSize.Value;
        }
        set
        {
            inputSize.Value = value;
        }
    }

    public int Speed
    {
        get
        {
            return (int)inputSpeed.Value;
        }
        set
        {
            inputSpeed.Value = value;
        }
    }

    public int ViewRealWidth
    {
        get
        {
            return pictureBox1.Width;
        }
    }

    public int ViewWidth
    {
        get
        {
            return (int)Math.Ceiling((decimal)pictureBox1.Width / (decimal)CellSize);
        }
    }

    public int ViewRealHeight
    {
        get
        {
            return pictureBox1.Height;
        }
    }

    public int ViewHeight
    {
        get
        {
            return (int)Math.Ceiling((decimal)pictureBox1.Height / (decimal)CellSize);
        }
    }

    public Form1()
    {
        InitializeComponent();
        var envWidth = 10000;
        var vw = ViewWidth;
        var vh = ViewHeight;

        var initLeft = envWidth / 2 - vw / 2;
        var initTop = envWidth / 2 - vh / 2;

        var bitmap = new FastBitMap(new byte[envWidth * envWidth], envWidth, envWidth);

        _env = new CellEnvironment(bitmap);
        _view = new ViewWindow(_env, vw, vh, CellSize);
        _view.MoveTo(initLeft, initTop);

        Resize += Form1_Resize;
    }

    private void Form1_Resize(object? sender, EventArgs e)
    {
        _view.Resize(ViewWidth, ViewHeight, CellSize);
        pictureBox1.Invalidate();
    }

    private void Form1_Load(object sender, EventArgs e)
    {

    }

    private int _dragStartX;
    private int _dragStartY;
    private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            var col = e.X / CellSize + _view.Left;
            var row = e.Y / CellSize + _view.Top;

            if (col < 0 || row < 0)
            {
                return;
            }
            if (col >= _env.Width || row >= _env.Height)
            {
                return;
            }


            _env.ToggleCell(row, col);
            pictureBox1.Invalidate();
        }
        else if (e.Button == MouseButtons.Right)
        {
            _dragStartX = e.X;
            _dragStartY = e.Y;
            pictureBox1.Cursor = Cursors.SizeAll;
        }
    }

    private void pictureBox1_Paint(object sender, PaintEventArgs e)
    {
        _view.Draw(e.Graphics);
    }

    private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Right)
        {
            var dx = -(e.X - _dragStartX) / CellSize / 4;
            var dy = -(e.Y - _dragStartY) / CellSize / 4;
            _view.Move(dx, dy);
            pictureBox1.Invalidate();
        }
    }

    private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Right)
        {
            pictureBox1.Cursor = Cursors.Default;
        }
    }

    private void inputSize_ValueChanged(object sender, EventArgs e)
    {
        _view.Resize(ViewWidth, ViewHeight, CellSize);
        pictureBox1.Invalidate();
    }

    private async Task ReDraw()
    {
        await Task.Run(() =>
        {
            Invoke((MethodInvoker)delegate
            {
                pictureBox1.Invalidate();
            });
        });
    }

    private void EvolutionThread()
    {
        while (_cts is not null && !_cts.IsCancellationRequested)
        {
            _env.Evolve();
            _ = ReDraw();

            Debug.WriteLine($"Generation: {_env.Generation}");
            Thread.Sleep(Speed);
        }

        Debug.WriteLine("EvolutionThread stopped");
    }


    private void btnStartStop_Click(object sender, EventArgs e)
    {
        if (_evolutionThread is null)
        {
            Start();
        }
        else
        {
            Stop();
        }
    }

    private void Stop()
    {
        if (_cts is null) return;

        _cts.Cancel();
        _evolutionThread!.Join();
        _evolutionThread = null;
        _cts = null;

        btnStartStop.Text = "Start";
        Debug.WriteLine("Stopped");
    }

    private void Start()
    {
        _cts = new CancellationTokenSource();
        _evolutionThread = new Thread(EvolutionThread);
        btnStartStop.Text = "Stop";
        _evolutionThread.Start();
        Debug.WriteLine("Started");
    }

    private void btnClear_Click(object sender, EventArgs e)
    {
        Stop();
        _env.Clear();
        pictureBox1.Invalidate();
    }
}


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

        var bitmap = _cellEnvironment.CreateSnapshot();
        var genText = $"Generation: {_cellEnvironment.Generation}, {_cellEnvironment.MsTimeUsed} ms";

        graphics.Clear(Color.Black);
        DrawMainView(graphics, bitmap, bpc);
        DrawGenerationText(graphics, genText);
        DrawThumbnail(graphics, bitmap, bpc, totalRows, totalColumns);
    }

    private void DrawThumbnail(Graphics graphics, IByteArrayBitOperator bitmap, IPositionConvert bpc, int totalRows, int totalColumns)
    {
        var thumbWidth = 240;
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