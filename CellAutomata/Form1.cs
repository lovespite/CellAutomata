using System.Diagnostics;
using System.Windows.Forms;

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
        var envWidth = 1000;
        var vw = ViewWidth;
        var vh = ViewHeight;

        var initLeft = envWidth / 2 - vw / 2;
        var initTop = envWidth / 2 - vh / 2;

        var bitmap = new BitMap(envWidth, envWidth);

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

    private Point _dragStartPos;
    private Point _dragStartViewPos;

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
        else if (e.Button == MouseButtons.Right && ModifierKeys.HasFlag(Keys.Control))
        {
            _dragStartPos = e.Location;
            _dragStartViewPos = _view.Location;
            pictureBox1.Cursor = Cursors.SizeAll;
        }
    }

    private void pictureBox1_Paint(object sender, PaintEventArgs e)
    {
        _view.Draw(e.Graphics);
    }

    private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Right && ModifierKeys.HasFlag(Keys.Control))
        {
            var cellSize = CellSize;

            var deltaX = _dragStartPos.X - e.X;
            var deltaY = _dragStartPos.Y - e.Y;

            var directionX = Math.Sign(deltaX);
            var directionY = Math.Sign(deltaY);

            var absDeltaX = Math.Max(Math.Abs(deltaX) / cellSize, 1);
            var absDeltaY = Math.Max(Math.Abs(deltaY) / cellSize, 1);

            var curX = _dragStartViewPos.X + directionX * absDeltaX;
            var curY = _dragStartViewPos.Y + directionY * absDeltaY;
            _view.MoveTo(curX, curY);

            pictureBox1.Invalidate();
            return;
        }

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

            _env.ActivateCell(row, col);
            pictureBox1.Invalidate();
            return;
        }

        if (e.Button == MouseButtons.Right)
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

            _env.DeactivateCell(row, col);
            pictureBox1.Invalidate();
            return;
        }
    }

    private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Right && ModifierKeys.HasFlag(Keys.Control))
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
            // _env.Evolve();
            _env.EvolveMultiThread(16);
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

    private void Form1_FormClosed(object sender, FormClosedEventArgs e)
    {
        _cts?.Cancel();
        _evolutionThread?.Join();
    }

    private void btnLoad_Click(object sender, EventArgs e)
    {

    }

    private void btnSave_Click(object sender, EventArgs e)
    {
    }
}
