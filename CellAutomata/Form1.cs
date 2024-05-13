using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
namespace CellAutomata;

public partial class Form1 : Form
{
    private readonly CellEnvironment _env;

    private ViewWindowBase _view = null!;
    private Thread? _evolutionThread = null;
    private CancellationTokenSource? _cts = null;

    private readonly Thread _painting;

    public const int ThreadCount = 16;

    #region Properties
    public float CellSize
    {
        get
        {
            return _view.CellSize;
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

    #endregion

    private readonly Graphics g;

    public Form1()
    {
        InitializeComponent();
        canvas.AllowDrop = true;

        canvas.Resize += Form1_Resize;

        var lifemap = new HashLifeMap();
        _env = new CellEnvironment(lifemap)
        {
            ThreadCount = ThreadCount
        };

        _painting = new Thread(Render);

        g = canvas.CreateGraphics();
    }

    public void Render()
    {
        while (!IsDisposed)
        {
            Thread.Sleep(1);
            if (_view is null) continue;

            try
            {
                Invoke((MethodInvoker)delegate
                {
                    _view.Draw(g);
                });
            }
            catch
            {
                // ignore
            }
        }
    }

    public void RePaint()
    {
    }

    private void Form1_Resize(object? sender, EventArgs e)
    {
        _view.Resize(canvas.Width, canvas.Height);
    }

    private void Form1_Load(object sender, EventArgs e)
    {
        var vw = canvas.Width;
        var vh = canvas.Height;
        //var renderContext = new D2dWindowContext(vw, vh, canvas.Handle);
        //_view = new ViewWindowDx2d(_env, renderContext, vw, vh, 8f)
        //{
        //    CanvasHandle = canvas.Handle,
        //};

        _view = new ViewWindowDx2dRaw(_env, new Size(vw, vh), canvas.Handle);
        MouseWheel += Form1_MouseWheel;

        _painting.Start();
    }

    private void Form1_MouseWheel(object? sender, MouseEventArgs e)
    {
        var delta = e.Delta;

        if (delta > 0)
        {
            _view.ZoomIn();
        }
        else
        {
            _view.ZoomOut();
        }
    }

    private Point _dragStartPos;
    private Point _dragStartViewPos;

    private bool _isSelecting = false;
    private bool _isDraggingView = false;

    private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
    {
        _view.MousePoint = e.Location;
        // drag view [Ctrl + Left Mouse]
        if (e.Button == MouseButtons.Left && ModifierKeys.HasFlag(Keys.Control))
        {
            if (_isSelecting) return;
            _isDraggingView = true;

            _dragStartPos = e.Location;
            _dragStartViewPos = _view.Location;
            canvas.Cursor = Cursors.SizeAll;

            return;
        }

        // select cell [Left Mouse]
        if (e.Button == MouseButtons.Left)
        {
            if (_isDraggingView) return;
            _isSelecting = true;

            //var col = e.X / CellSize + _view.Left;
            //var row = e.Y / CellSize + _view.Top;

            var p = _view.MouseCellPoint;

            _view.SetSelection(p, p);

            RePaint();
            return;
        }

    }

    private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
    {
        _view.MousePoint = e.Location;
        if (e.Button == MouseButtons.Left)
        {
            if (ModifierKeys.HasFlag(Keys.Shift))
            {
                // ???, [Shift + Left Mouse]
            }
            else if (ModifierKeys.HasFlag(Keys.Control))
            {
                // drag view [Ctrl + Left Mouse] 
                if (_isDraggingView)
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

                    _view.MoveTo((int)curX, (int)curY);
                }
            }
            else
            {
                if (_isSelecting)
                {
                    // drag selection,  [Left Mouse]  

                    var ps = _view.SelectionStart;
                    var pe = _view.MouseCellPoint;

                    _view.SetSelection(ps, pe);
                }
            }
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
            }
            return;
        }
    }


    private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
    {
        canvas.Cursor = Cursors.Default;
        _isSelecting = false;
        _isDraggingView = false;
    }

    private void inputSize_ValueChanged(object sender, EventArgs e)
    {
    }

    private void EvolutionThread()
    {
        var gens = 0ul;
        var sw = Stopwatch.StartNew();

        while (_cts is not null && !_cts.IsCancellationRequested)
        {
            _env.NextGeneration();
            ++gens;

            if (sw.ElapsedMilliseconds > 1000)
            {
                var genPerSec = gens / (float)sw.Elapsed.TotalSeconds;

                gens = 0;
                sw.Restart();
                _view.Gps = genPerSec;
            }

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
        RePaint();
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
            RePaint();
        }

        await Task.Delay(10);
        _view.ClearSelection();
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
        RePaint();
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

        RePaint();
    }

    private (ILifeMap, Size)? _clipboard;
    private void copyToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (!_view.IsSelected) return;

        try
        {
            var selection = _view.GetSelection();

            var bitmap = _env.BitMap.CreateRegionSnapshot(selection);
            _clipboard = (bitmap, selection.Size);
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
        if (_clipboard is null) return;

        ILifeMap data;
        Size size;

        (data, size) = _clipboard.Value;
        if (data is null) return;

        var bitmap = data;

        var p = _view.GetSelection().Location;

        _env.Lock(b =>
        {
            b.BlockCopy(data, size, p, _mode);
        });

        _view.SetSelection(p, new Point(p.X + size.Width - 1, p.Y + size.Height - 1));

        RePaint();

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

        RePaint();
    }

    private void clearSelectionToolStripMenuItem_Click(object sender, EventArgs e)
    {
        _view.ClearSelection();
        RePaint();
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


        RePaint();
    }

    private CopyMode _mode = CopyMode.Overwrite;
    private void pasteMethods_Click(object sender, EventArgs e)
    {
        var item = sender as ToolStripMenuItem;
        if (item is null) return;
        var text = item.Text;
        if (text == null) return;

        if (!Enum.TryParse(text, out _mode)) return;

        foreach (var menu in pasteMethodToolStripMenuItem.DropDownItems)
        {
            if (menu is ToolStripMenuItem mItem)
            {
                mItem.Checked = string.Equals(text, mItem.Text, StringComparison.OrdinalIgnoreCase);
            }
        }

    }

    private void nextGenerationToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (_cts is not null)
        {
            // thread running...
            return;
        }

        _env.NextGeneration();
    }

    private void homeToolStripMenuItem_Click(object sender, EventArgs e)
    {
        _view.MoveTo(0, 0);
    }

    private void canvas_MouseLeave(object sender, EventArgs e)
    {
        _view.MousePoint = new Point(-1, -1);
    }

    private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var bounds = _env.BitMap.GetBounds();
        _view.SetSelection(bounds.Location, bounds.Location2);
    }

    private void fitToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var bounds = _env.BitMap.GetBounds();
        _view.MoveTo(
            (int)bounds.Left + (int)bounds.Width / 2,
            (int)bounds.Top + (int)bounds.Height / 2
            );

    }

    private void clearUnselectedCellsToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var selection = _view.GetSelection();
        if (selection.IsEmpty) return;

        var bounds = _env.BitMap.GetBounds();

        for (var row = bounds.Top; row <= bounds.Bottom; row++)
        {
            for (var col = bounds.Left; col <= bounds.Right; col++)
            {
                if (!selection.Contains((int)col, (int)row))
                {
                    _env.DeactivateCell((int)row, (int)col);
                }
            }
        }
    }

    private void rotateToolStripMenuItem_Click(object sender, EventArgs e)
    {
    }
}
