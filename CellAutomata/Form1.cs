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
        pictureBox1.AllowDrop = true;

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
        this.MouseWheel += Form1_MouseWheel;
    }

    private void Form1_MouseWheel(object? sender, MouseEventArgs e)
    {
        var delta = e.Delta;
        var cellSize = CellSize;

        if (delta > 0)
        {
            cellSize += 1;
        }
        else
        {
            cellSize -= 1;
        }

        if (cellSize < inputSize.Minimum)
        {
            cellSize = (int)inputSize.Minimum;
        }
        else if (cellSize > inputSize.Maximum)
        {
            cellSize = (int)inputSize.Maximum;
        }

        CellSize = cellSize;
        _view.Resize(ViewWidth, ViewHeight, CellSize);
        pictureBox1.Invalidate();
    }

    private Point _dragStartPos;
    private Point _dragStartViewPos;

    private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
    {
        // select cell [Right Mouse]
        if (e.Button == MouseButtons.Right)
        {
            var col = e.X / CellSize + _view.Left;
            var row = e.Y / CellSize + _view.Top;

            if (col < 0 || row < 0
                || col >= _env.Width || row >= _env.Height) // out of bounds
            {
                _view.ClearSelection();
                return;
            }

            var p = new Point(col, row);

            _view.SetSelection(p, p);
            pictureBox1.Invalidate();
            return;
        }

        if (e.Button == MouseButtons.Left && ModifierKeys.HasFlag(Keys.Control))
        {
            _dragStartPos = e.Location;
            _dragStartViewPos = _view.Location;
            pictureBox1.Cursor = Cursors.SizeAll;

            return;
        }
    }

    private void pictureBox1_Paint(object sender, PaintEventArgs e)
    {
        _view.Draw(e.Graphics);
    }

    private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            // drag view [Ctrl + Left Mouse]
            if (ModifierKeys.HasFlag(Keys.Control))
            {
                HandleMoveView(e);
            }
            else
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

                if (ModifierKeys.HasFlag(Keys.Shift))
                {
                    // deactivate cells , [Shift + Left Mouse]
                    _env.DeactivateCell(row, col);
                }
                else
                {
                    // activate cells, [Left Mouse]
                    _env.ActivateCell(row, col);
                }
            }

            pictureBox1.Invalidate();
            return;
        }

        if (e.Button == MouseButtons.Right)
        {
            if (ModifierKeys.HasFlag(Keys.Control))
            {
                // ???, [Ctrl + Right Mouse]
            }
            else if (ModifierKeys.HasFlag(Keys.Shift))
            {
                // ???, [Shift + Right Mouse]
            }
            else
            {
                // drag selection, [Right Mouse]
                var col = e.X / CellSize + _view.Left;
                var row = e.Y / CellSize + _view.Top;

                if (col < 0 || row < 0
                                   || col >= _env.Width || row >= _env.Height) // out of bounds
                {
                    return;
                }

                var ps = _view.SelectionStart;
                var pe = new Point(col, row);

                _view.SetSelection(ps, pe);
            }

            pictureBox1.Invalidate();
            return;
        }
    }

    private void HandleMoveView(MouseEventArgs e)
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
    }

    private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
    {
        pictureBox1.Cursor = Cursors.Default;
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

    private async void btnLoad_Click(object sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog();
        dialog.Filter = "Binary BitMap|*.bbm|All|*.*";
        dialog.InitialDirectory = Path.Combine(Application.StartupPath, "Conways");

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            var path = dialog.FileName;

            await _env.LoadFrom(path);
            pictureBox1.Invalidate();
        }
    }

    private async void btnSave_Click(object sender, EventArgs e)
    {
        using var dialog = new SaveFileDialog();
        dialog.Filter = "Binary BitMap|*.bbm|All|*.*";
        dialog.DefaultExt = "bbm";
        dialog.OverwritePrompt = true;
        dialog.InitialDirectory = Path.Combine(Application.StartupPath, "Conways");
        dialog.FileName = $"{DateTime.Now:yyMMddHHmmss}.bbm";

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            var path = dialog.FileName;

            await _env.SaveTo(path);
        }
    }

    private void pictureBox1_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
            e.Effect = DragDropEffects.Copy;
    }

    private async void pictureBox1_DragDrop(object sender, DragEventArgs e)
    {
        var files = (string[]?)e.Data?.GetData(DataFormats.FileDrop);
        if (files is null || files.Length == 0) return;

        var file = files[0];
        if (!File.Exists(file)) return;

        await _env.LoadFrom(file);
        pictureBox1.Invalidate();
    }

    private void exitToolStripMenuItem_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void clearSelectedCellsToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (!_view.IsSelected) return;

        var selection = _view.GetSelection();

        for (var row = selection.Top; row < selection.Bottom; row++)
        {
            for (var col = selection.Left; col < selection.Right; col++)
            {
                _env.DeactivateCell(row, col);
            }
        }

        pictureBox1.Invalidate();
    }

    private IBitMap? _clipboard;
    private void copyToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (!_view.IsSelected) return;

        try
        {
            var selection = _view.GetSelection();
            var snapshot = _env.CreateSnapshot();

            var bitmap = snapshot.CreateRegionSnapshot(selection);
            _clipboard = bitmap;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void cutToolStripMenuItem_Click(object sender, EventArgs e)
    {
        copyToolStripMenuItem_Click(sender, e);
        clearSelectedCellsToolStripMenuItem_Click(sender, e);// clear selected cells
    }

    private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (!_view.IsSelected) return;

        var data = _clipboard;
        if (data is null) return;

        var bitmap = data;

        var p = _view.GetSelection().Location;

        _env.Lock(b =>
        {
            b.BlockCopy(bitmap!, p);
        });

        _view.SetSelection(p, new Point(p.X + bitmap.Bpc.Width - 1, p.Y + bitmap.Bpc.Height - 1));

        pictureBox1.Invalidate();

    }

    private void fillToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (!_view.IsSelected) return;

        var selection = _view.GetSelection();

        for (var row = selection.Top; row < selection.Bottom; row++)
        {
            for (var col = selection.Left; col < selection.Right; col++)
            {
                _env.ActivateCell(row, col);
            }
        }

        pictureBox1.Invalidate();
    }

    private void clearSelectionToolStripMenuItem_Click(object sender, EventArgs e)
    {
        _view.ClearSelection();
        pictureBox1.Invalidate();
    }

    private void shrinkSelectionToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (!_view.IsSelected) return;

        var selection = _view.GetSelection();

        var aliveCells = _env.GetRegionAliveCells(selection);

        if (aliveCells.Any())
        {
            var p1 = new Point(
                x: aliveCells.Min(p => p.X),
                y: aliveCells.Min(p => p.Y)
                );

            var p2 = new Point(
                x: aliveCells.Max(p => p.X),
                y: aliveCells.Max(p => p.Y)
                );


            _view.SetSelection(p1, p2);
        }
        else
        {
            _view.ClearSelection();
        }


        pictureBox1.Invalidate();
    }
}
