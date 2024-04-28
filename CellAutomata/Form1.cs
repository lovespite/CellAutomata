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

    public const int EnvWidth = 10_000;

    #region Properties
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
            return canvas.Width;
        }
    }

    public int ViewWidth
    {
        get
        {
            return (int)Math.Ceiling((decimal)canvas.Width / (decimal)CellSize);
        }
    }

    public int ViewRealHeight
    {
        get
        {
            return canvas.Height;
        }
    }

    public int ViewHeight
    {
        get
        {
            return (int)Math.Ceiling((decimal)canvas.Height / (decimal)CellSize);
        }
    }

    #endregion

    private readonly Graphics g;

    public Form1()
    {
        InitializeComponent();
        Resize += Form1_Resize;
        canvas.AllowDrop = true;

        _env = new CellEnvironment(new FastBitMap(EnvWidth, EnvWidth));
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
        // _view.Draw(null);
    }

    private void Form1_Resize(object? sender, EventArgs e)
    {
        _view.Resize(ViewWidth, ViewHeight, CellSize);
        RePaint();
    }

    private void Form1_Load(object sender, EventArgs e)
    {
        var vw = ViewWidth;
        var vh = ViewHeight;
        var initLeft = EnvWidth / 2 - vw / 2;
        var initTop = EnvWidth / 2 - vh / 2;

        _view = new ViewWindowDx2d(_env, new D2dWindowContext(canvas.Width, canvas.Height, canvas.Handle), vw, vh, CellSize);
        _view.MoveTo(initLeft, initTop);

        MouseWheel += Form1_MouseWheel;

        _painting.Start();
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

        RePaint();
    }

    private Point _dragStartPos;
    private Point _dragStartViewPos;

    private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
    {
        // drag view [Ctrl + Left Mouse]
        if (e.Button == MouseButtons.Left && ModifierKeys.HasFlag(Keys.Control))
        {
            _dragStartPos = e.Location;
            _dragStartViewPos = _view.Location;
            canvas.Cursor = Cursors.SizeAll;

            return;
        }

        // select cell [Left Mouse]
        if (e.Button == MouseButtons.Left)
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

            RePaint();
            return;
        }

    }

    private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
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

            if (ModifierKeys.HasFlag(Keys.Shift))
            {
                // ???, [Shift + Left Mouse]
            }
            else if (ModifierKeys.HasFlag(Keys.Control))
            {
                // drag view [Ctrl + Left Mouse] 
                HandleMoveView(e);
            }
            else
            {
                // drag selection,  [Left Mouse] 
                if (col < 0 || row < 0
                                   || col >= _env.Width || row >= _env.Height) // out of bounds
                {
                    return;
                }

                var ps = _view.SelectionStart;
                var pe = new Point(col, row);

                _view.SetSelection(ps, pe);
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
        canvas.Cursor = Cursors.Default;
    }

    private void inputSize_ValueChanged(object sender, EventArgs e)
    {
        var curLeft = _view.Left;
        var curTop = _view.Top;
        var curWidth = _view.Width;
        var curHeight = _view.Height;

        var vw = ViewWidth;
        var vh = ViewHeight;

        curLeft += (curWidth - vw) / 2;
        curTop += (curHeight - vh) / 2;

        _view.Resize(vw, vh, CellSize);
        _view.MoveTo(curLeft, curTop);

        RePaint();
    }

    private void EvolutionThread()
    {
        while (_cts is not null && !_cts.IsCancellationRequested)
        {
            // _env.Evolve();
            _env.EvolveMultiThread(16);

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
            b.BlockCopy(bitmap!, p, _mode);
        });

        _view.SetSelection(p, new Point(p.X + bitmap.Bpc.Width - 1, p.Y + bitmap.Bpc.Height - 1));

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

        if (!Enum.TryParse<CopyMode>(text, out _mode)) return;

        foreach (var menu in pasteMethodToolStripMenuItem.DropDownItems)
        {
            if (menu is ToolStripMenuItem mItem)
            {
                mItem.Checked = string.Equals(text, mItem.Text, StringComparison.OrdinalIgnoreCase);
            }
        }

    }

    private void locateFirstCellToolStripMenuItem_Click(object sender, EventArgs e)
    {
        for (var row = 0; row < _env.Height; row++)
        {
            for (var col = 0; col < _env.Width; col++)
            {
                if (_env.IsAlive(row, col))
                {
                    _view.MoveTo(
                        col - _view.Width / 2,
                        row - _view.Height / 2
                        );
                    ;
                    return;
                }
            }
        }
    }
}
