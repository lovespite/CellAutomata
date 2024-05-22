using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using CellAutomata.Algos;
using CellAutomata.Render;
using CellAutomata.Util;
namespace CellAutomata;

public partial class Form1 : Form
{
    private readonly CellEnvironment _env;

    private ViewWindowBase _view = null!;
    private Thread? _evolutionThread = null;
    private CancellationTokenSource? _cts = null;

    private CopyMode _mode = CopyMode.Overwrite;

    public const int AsyncAreaThreshold = 1_000_000; // 5M cells
    public const int ReportInterval = 100_000; // 100K cells

    private Point _dragStartPos;
    private Point _dragStartViewPos;

    private PictureBox? _floatLayer;
    private readonly Brush _foreBrush = new SolidBrush(Color.White);
    private readonly Brush _backBrush = new SolidBrush(Color.Black);

    private bool _isSelecting = false;
    private bool _isDraggingView = false;

    private (ILifeMap, Size)? _clipboard;

    private readonly Thread _painting;

    public const int ThreadCount = 16;
    public Form1()
    {
        InitializeComponent();
        canvas.AllowDrop = true;

        canvas.Resize += Form1_Resize;

        var use3d = !Environment.GetCommandLineArgs().Contains("--disbale-3d-render=true");
        if (use3d)
        {
            Debug.WriteLine("3D render enabled");
        }

        var lifemap = new HashLifeMap(
            rule: "B3/S23",
            use3dRender: use3d
            );

        _env = new CellEnvironment(lifemap)
        {
            ThreadCount = ThreadCount
        };

        _painting = new Thread(Render);

        // g = canvas.CreateGraphics();
    }

    #region Public Methods

    public void Suspend(Action action, bool invokeRequired = false)
    {
        bool wasRunning = _cts != null;
        bool wasSuspended = _env.GetDCRender().IsSuspended;

        Stop();
        _env.GetDCRender().Suspend();

        try
        {
            if (invokeRequired)
            {
                Invoke(action);
            }
            else
            {
                action();
            }
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }

        if (!wasSuspended) _env.GetDCRender().Resume();
        if (wasRunning) Start();
    }

    public Task SuspendAsync(Action action, bool invokeRequired = false)
    {
        bool wasRunning = _cts != null;
        bool wasSuspended = _env.GetDCRender().IsSuspended;
        Stop();  // 停止当前操作
        _env.GetDCRender().Suspend();

        Task t = invokeRequired
            ? Task.Run(() => Invoke(action))
            : Task.Run(action);

        return t.ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                HandleException(task.Exception!);
            }

            if (!wasSuspended) _env.GetDCRender().Resume();
            if (wasRunning) Start();
        });
    }

    public async Task SuspendAsync(Func<Task> action, bool invokeRequired = false)
    {
        bool wasRunning = _cts != null;
        bool wasSuspended = _env.GetDCRender().IsSuspended;
        Stop();  // 停止当前操作
        _env.GetDCRender().Suspend();

        try
        {
            if (invokeRequired)
            {
                await RunOnUIThread(action);
            }
            else
            {
                await action();
            }
        }
        catch (Exception ex)
        {
            HandleException(ex); // 处理异常
        }

        if (!wasSuspended) _env.GetDCRender().Resume();
        if (wasRunning) Start();
    }

    #endregion

    #region Private Methods
    private async Task RunOnUIThread(Func<Task> action)
    {
        var tcs = new TaskCompletionSource<object?>();
        Invoke((MethodInvoker)async delegate
        {
            try
            {
                await action();
                tcs.SetResult(null);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex); // 传递异常
            }
        });
        await tcs.Task;
    }
    private void HandleException(Exception ex, bool showMessageBox = true)
    {
        if (showMessageBox) ShowException(ex);
        if (ex is AggregateException ae)
        {
            foreach (var e in ae.InnerExceptions)
            {
                Debug.WriteLine(e.ToString());
            }
        }
        else
        {
            Debug.WriteLine(ex.ToString());
        }
    }
    protected void ShowException(Exception ex)
    {
        if (InvokeRequired)
        {
            Invoke((MethodInvoker)delegate { ShowException(ex); }); return;
        }

        MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
    protected void ShowMessage(string message)
    {
        if (InvokeRequired)
        {
            Invoke((MethodInvoker)delegate { ShowMessage(message); }); return;
        }

        MessageBox.Show(message, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    private void Render()
    {
        while (!IsDisposed)
        {
            Thread.Sleep(10); // 100 fps max
            if (_view is null) continue;

            try
            {
                Invoke((MethodInvoker)delegate
                {
                    _view.Draw(null);
                });
            }
            catch
            {
                // ignore
            }
        }
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

            Thread.Sleep(_env.MsGenInterval);
        }

        Debug.WriteLine("EvolutionThread stopped");
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
    private void SetClipboard(Rectangle rect)
    {
        if (rect.Area() > AsyncAreaThreshold)
        {
            Debug.WriteLine("Async copy");
            TaskProgressReporter.Watch(async reporter =>
            {
                var bitmap = await _env.LifeMap.CreateRegionSnapshotAsync(rect, reporter);
                _clipboard = (bitmap, rect.Size);
            }, "Copying", "Please wait ...");
        }
        else
        {
            var bitmap = _env.LifeMap.CreateRegionSnapshot(rect);
            _clipboard = (bitmap, rect.Size);
        }

    }
    private void ClearRegion(Rectangle rect)
    {
        if (rect.IsEmpty) return;

        if (rect.Area() > AsyncAreaThreshold)
        {
            TaskProgressReporter.Watch(async reporter =>
            {
                await SuspendAsync(() => _env.LifeMap.ClearRegionAsync(rect, reporter));
            }, "Clearing", "Please wait ...");
        }
        else
        {
            Suspend(() => _env.LifeMap.ClearRegion(rect));
        }
    }
    private async Task RotateRegionAsync(Rectangle selection, IProgressReporter? reporter)
    {
        var snapshot = await _env.LifeMap.CreateRegionSnapshotAsync(selection, reporter);

        _env.LifeMap.ClearRegion(selection);

        // 获取选区中心点 
        int centerX = selection.Left + (int)Math.Floor(selection.Width / 2.0);
        int centerY = selection.Top + (int)Math.Floor(selection.Height / 2.0);

        var prList = new List<Point>();

        double total = selection.Area();
        double count = 0;

        // 90度顺时针旋转
        for (var row = selection.Top; row < selection.Bottom; row++)
        {
            for (var col = selection.Left; col < selection.Right; col++)
            {
                if (reporter?.IsAborted == true) return;

                // 相对于选区中心的坐标
                int relX = col - centerX;
                int relY = row - centerY;

                // 旋转坐标
                int rotatedX = -relY;
                int rotatedY = relX;

                // 计算新位置的绝对坐标
                var p2 = new Point(centerX + rotatedX, centerY + rotatedY);
                var p0 = new Point(col - selection.Left, row - selection.Top);

                _env.LifeMap.Set(ref p2, snapshot.Get(ref p0));

                if (
                    row == selection.Top
                    || row == selection.Bottom - 1
                    || col == selection.Left
                    || col == selection.Right - 1
                    )
                {
                    prList.Add(p2);
                }
                if (reporter is null) continue;

                if ((++count) % ReportInterval == 0)
                {
                    reporter.ReportProgress((float)(count / total), "Rotating ...", TimeSpan.Zero);
                    await Task.Delay(1);
                }
            }
        }

        var pr1 = prList.Min(p => p.X);
        var pr2 = prList.Max(p => p.X);
        var pc1 = prList.Min(p => p.Y);
        var pc2 = prList.Max(p => p.Y);

        _view.SetSelection(new Point(pr1, pc1), new Point(pr2, pc2));
    }

    private async Task FlipRegionUpDown(Rectangle selection, IProgressReporter? reporter)
    {
        var snapshot = await _env.LifeMap.CreateRegionSnapshotAsync(selection, reporter);

        _env.LifeMap.ClearRegion(selection);

        double total = selection.Area();
        double count = 0;

        for (var row = selection.Top; row < selection.Bottom; row++)
        {
            for (var col = selection.Left; col < selection.Right; col++)
            {
                if (reporter?.IsAborted == true) return;

                var p0 = new Point(col - selection.Left, row - selection.Top);
                var p1 = new Point(col, selection.Bottom - 1 - (row - selection.Top));

                _env.LifeMap.Set(ref p1, snapshot.Get(ref p0));

                if (reporter is null) continue;

                if ((++count) % ReportInterval == 0)
                {
                    reporter.ReportProgress((float)(count / total), "Flipping ...", TimeSpan.Zero);
                    await Task.Delay(1);
                }
            }
        }
    }

    private async Task FlipRegionLeftRight(Rectangle selection, IProgressReporter? reporter)
    {
        var snapshot = await _env.LifeMap.CreateRegionSnapshotAsync(selection, reporter);

        _env.LifeMap.ClearRegion(selection);

        double total = selection.Area();
        double count = 0;

        for (var row = selection.Top; row < selection.Bottom; row++)
        {
            for (var col = selection.Left; col < selection.Right; col++)
            {
                if (reporter?.IsAborted == true) return;

                var p0 = new Point(col - selection.Left, row - selection.Top);
                var p1 = new Point(selection.Right - 1 - (col - selection.Left), row);

                _env.LifeMap.Set(ref p1, snapshot.Get(ref p0));

                if (reporter is null) continue;

                if ((++count) % ReportInterval == 0)
                {
                    reporter.ReportProgress((float)(count / total), "Flipping ...", TimeSpan.Zero);
                    await Task.Delay(1);
                }
            }
        }
    }

    private void RandomFill(Rectangle selection, IRandomFillAlgorithm algo)
    {
        if (selection.IsEmpty) return;
        var rnd = new Random();

        if (selection.Area() > AsyncAreaThreshold)
        {
            TaskProgressReporter.Watch(async reporter =>
            {
                await Task.Delay(100);
                await SuspendAsync(() => Fill(reporter));
            }, "Filling", "Please wait ...");
        }
        else
        {
            Suspend(() => algo.Generate(selection, _env.LifeMap));
        }

        async Task Fill(IProgressReporter? reporter = null)
        {
            double total = selection.Area();
            double count = 0;
            for (var row = selection.Top; row < selection.Bottom; row++)
            {
                for (var col = selection.Left; col < selection.Right; col++)
                {
                    if (reporter?.IsAborted == true) return;

                    if (algo.GetNoise(col, row, 0))
                    {
                        _env.ActivateCell(row, col);
                    }

                    if (reporter is null) continue;

                    if ((++count) % ReportInterval == 0)
                    {
                        reporter.ReportProgress((float)(count / total), "Filling ...", TimeSpan.Zero);
                        await Task.Delay(1);
                    }
                }
            }
        }
    }

    private void LoadBbmFile(string file)
    {
        TaskProgressReporter.Watch(async reporter =>
        {
            await Task.Delay(100);
            await SuspendAsync(() => _env.LoadFrom(file, reporter));
        }, "Loading", "Please wait ...");
    }

    private void LoadGollyRle(string file)
    {
        TaskProgressReporter.WatchNoAbort(async reporter =>
        {
            await Task.Delay(100);
            await SuspendAsync(() => _env.LifeMap.ReadRle(file), false);
            await Task.Delay(100);

            reporter.ReportProgress(1, "Done.", TimeSpan.Zero);
        }, "Loading", "Please wait ...");
    }

    private void SaveBbmFile(string file)
    {
        //using var reporter = new TaskProgressReporter("Saving", "Please wait ...");
        //var t = SuspendAsync(() => _env.SaveTo(file, reporter));
        //reporter.Wait(t);

        TaskProgressReporter.Watch(async reporter =>
        {
            await Task.Delay(100);
            await SuspendAsync(() => _env.SaveTo(file, reporter));
        }, "Saving", "Please wait ...");
    }

    #endregion

    #region Form Event Handlers
    private void Form1_Resize(object? sender, EventArgs e)
    {
        _view.Resize(canvas.Width, canvas.Height);
        _floatLayer?.Invalidate();
    }
    private void Form1_Load(object sender, EventArgs e)
    {
        label1.Text = $"GenInterval: {_env.MsGenInterval} ms";

        var vw = canvas.Width;
        var vh = canvas.Height;
        //var renderContext = new D2dWindowContext(vw, vh, canvas.Handle);
        //_view = new ViewWindowDx2d(_env, renderContext, vw, vh, 8f)
        //{
        //    CanvasHandle = canvas.Handle,
        //};

        _view = new ViewWindow(_env, new Size(vw, vh), canvas.Handle);
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
    private void Form1_FormClosed(object sender, FormClosedEventArgs e)
    {
        _cts?.Cancel();
        _evolutionThread?.Join();
    }

    #endregion

    #region Canvas Event Handlers   
    private void Canvas_MouseDown(object sender, MouseEventArgs e)
    {
        _view.MousePoint = e.Location;

        if (e.Button == MouseButtons.Right)
        {
            // show context menu 
            return;
        }


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

            return;
        }

    }
    private void Canvas_MouseMove(object sender, MouseEventArgs e)
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
                    var cellSize = _view.CellSize;

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
    private void Canvas_MouseUp(object sender, MouseEventArgs e)
    {
        canvas.Cursor = Cursors.Default;
        _isSelecting = false;
        _isDraggingView = false;
    }
    private void Canvas_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
            e.Effect = DragDropEffects.Copy;
    }
    private void Canvas_DragDrop(object sender, DragEventArgs e)
    {
        var files = (string[]?)e.Data?.GetData(DataFormats.FileDrop);
        if (files is null || files.Length == 0) return;

        var file = files[0];
        if (!File.Exists(file)) return;

        Stop();// stop evolution 
        LoadBbmFile(file);
    }
    private void Canvas_MouseLeave(object sender, EventArgs e)
    {
        _view.MousePoint = new Point(-1, -1);
    }

    #endregion

    #region File Menu Event Handlers
    private void File_Load_Click(object sender, EventArgs e)
    {
        Stop();// stop evolution

        using var dialog = new OpenFileDialog
        {
            Title = "Load Binary BitMap File",
            Filter = "Binary BitMap|*.bbm|All|*.*",
            InitialDirectory = Path.Combine(Application.StartupPath, "Conways"),
            Multiselect = false,
        };

        if (dialog.ShowDialog() != DialogResult.OK) return;
        LoadBbmFile(dialog.FileName);

        _view.ClearSelection();
        View_MoveToCenterOfAllCells_Click(sender, e); // move to center
    }

    private void File_LoadGollyFile_Click(object sender, EventArgs e)
    {
        Stop();// stop evolution

        using var dialog = new OpenFileDialog
        {
            Filter = "Golly RLE|*.rle;*.mc;*.mc.gz|MCells|*.mcl|All|*.*",
            Title = "Load Golly RLE File",
            Multiselect = false,
        };

        var ret = dialog.ShowDialog();
        if (ret != DialogResult.OK) return;

        var file = dialog.FileName;
        if (!File.Exists(file)) return;

        LoadGollyRle(file);

        _view.ClearSelection();
        View_MoveToCenterOfAllCells_Click(sender, e); // move to center
    }

    private void File_Save_Click(object sender, EventArgs e)
    {
        Suspend(() =>
        {
            using var dialog = new SaveFileDialog();
            dialog.Filter = "Binary BitMap|*.bbm|All|*.*";
            dialog.DefaultExt = "bbm";
            dialog.OverwritePrompt = true;
            dialog.InitialDirectory = Path.Combine(Application.StartupPath, "Conways");
            dialog.FileName = $"{DateTime.Now:yyMMddHHmmss}.bbm";

            if (dialog.ShowDialog() != DialogResult.OK) return;
            SaveBbmFile(dialog.FileName);
        });
    }
    private void File_Exit_Click(object sender, EventArgs e)
    {
        Close();
    }


    #endregion

    #region Edit Menu Event Handlers
    private void Edit_Reset_Click(object sender, EventArgs e)
    {
        Stop();
        Suspend(_env.Reset); // clear all cells
    }
    private void Edit_SelectAll_Click(object sender, EventArgs e)
    {
        var bounds = _env.LifeMap.GetBounds();
        _view.SetSelection(bounds.Location, bounds.Location2);
    }
    private void Edit_ClearSelected_Click(object sender, EventArgs e)
    {
        if (!_view.IsSelected) return;

        ClearRegion(_view.GetSelection());
    }

    private void Edit_ClearUnselected_Click(object sender, EventArgs e)
    {
        var selection = _view.GetSelection();
        if (selection.IsEmpty) return;

        var bounds = _env.LifeMap.GetBounds();

        if (bounds.Size.Area > AsyncAreaThreshold)
        {
            TaskProgressReporter.Watch(async reporter =>
            {
                await Task.Delay(100);
                await SuspendAsync(() => Clear(reporter));
            }, "Clearing", "Please wait ...");
        }
        else
        {
            _ = SuspendAsync(() => Clear());
        }

        async Task Clear(IProgressReporter? reporter = null)
        {
            double total = bounds.Size.Area;
            double count = 0;
            for (var row = bounds.Top; row <= bounds.Bottom; row++)
            {
                for (var col = bounds.Left; col <= bounds.Right; col++)
                {
                    if (reporter?.IsAborted == true) return;

                    if (!selection.Contains((int)col, (int)row))
                    {
                        _env.DeactivateCell((int)row, (int)col);
                    }

                    if (reporter is null) continue;
                    if ((++count) % ReportInterval == 0)
                    {
                        reporter?.ReportProgress((float)(count / total), "Clearing ...", TimeSpan.Zero);
                        await Task.Delay(1);
                    }
                }
            }
        }
    }
    private void Edit_ClearSelection_Click(object sender, EventArgs e)
    {
        // clear selection region, not cells
        _view.ClearSelection();
    }
    private void Edit_ShrinkSelection_Click(object sender, EventArgs e)
    {
        if (!_view.IsSelected) return;

        var selection = _view.GetSelection();

        var aliveCells = _env.GetRegionAliveCells(selection);

        if (aliveCells.Count != 0)
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
    }
    private void Edit_Copy_Click(object sender, EventArgs e)
    {
        if (!_view.IsSelected) return;

        var selection = _view.GetSelection();

        Suspend(() => SetClipboard(selection));
    }
    private void Edit_Cut_Click(object sender, EventArgs e)
    {
        if (!_view.IsSelected) return;

        var selection = _view.GetSelection();

        Suspend(() =>
        {
            SetClipboard(selection);
            ClearRegion(selection);
        });
    }
    private void Edit_Paste_Click(object sender, EventArgs e)
    {
        if (!_view.IsSelected) return;
        if (_clipboard is null) return;

        ILifeMap data;
        Size size;

        (data, size) = _clipboard.Value;
        if (data is null) return;

        var bitmap = data;

        var p = _view.GetSelection().Location;

        if (size.Area() > AsyncAreaThreshold)
        {
            TaskProgressReporter.Watch(async reporter =>
            {
                await Task.Delay(100);
                await SuspendAsync(() => _env.LifeMap.BlockCopyAsync(data, size, p, _mode, reporter));
            }, "Pasting", "Please wait ...");
        }
        else
        {
            Suspend(() => _env.LifeMap.BlockCopy(data, size, p, _mode));
        }

        _view.SetSelection(p, new Point(p.X + size.Width - 1, p.Y + size.Height - 1));
    }
    private void Edit_ChangePasteMethods_Click(object sender, EventArgs e)
    {
        if (sender is not ToolStripMenuItem item) return;
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
    private void Edit_FillSelectedRegion_Click(object sender, EventArgs e)
    {
        if (!_view.IsSelected) return;

        var selection = _view.GetSelection();
        if (selection.IsEmpty) return;

        RandomFill(selection, RandomFillAlgorithm.Shared100);
    }
    private void Edit_RotateSelected_Click(object sender, EventArgs e)
    {
        var selection = _view.GetSelection();
        if (selection.IsEmpty) return;
        if (selection.Area() > AsyncAreaThreshold)
        {
            TaskProgressReporter.Watch(async reporter =>
            {
                await Task.Delay(100);
                await SuspendAsync(() => RotateRegionAsync(selection, reporter));
            }, "Rotating", "Please wait ...");
        }
        else
        {
            Suspend(() => RotateRegionAsync(selection, null).Wait());
        }
    }
    private void Edit_FlipUpDownSelected_Click(object sender, EventArgs e)
    {
        var selection = _view.GetSelection();
        if (selection.IsEmpty) return;

        if (selection.Area() > AsyncAreaThreshold)
        {
            TaskProgressReporter.Watch(async reporter =>
            {
                await Task.Delay(100);
                await SuspendAsync(() => FlipRegionUpDown(selection, reporter));
            }, "Flipping", "Please wait ...");
        }
        else
        {
            Suspend(() => FlipRegionUpDown(selection, null).Wait());
        }
    }
    private void Edit_FlipLeftRight_Click(object sender, EventArgs e)
    {
        var selection = _view.GetSelection();
        if (selection.IsEmpty) return;

        if (selection.Area() > AsyncAreaThreshold)
        {
            TaskProgressReporter.Watch(async reporter =>
            {
                await Task.Delay(100);
                await SuspendAsync(() => FlipRegionLeftRight(selection, reporter));
            }, "Flipping", "Please wait ...");
        }
        else
        {
            Suspend(() => FlipRegionLeftRight(selection, null).Wait());
        }
    }
    private void Edit_RandomFill25_Click(object sender, EventArgs e)
    {
        var selection = _view.GetSelection();
        if (selection.IsEmpty) return;

        RandomFill(selection, RandomFillAlgorithm.Shared25);
    }
    private void Edit_RandomFill50_Click(object sender, EventArgs e)
    {
        var selection = _view.GetSelection();
        if (selection.IsEmpty) return;

        RandomFill(selection, RandomFillAlgorithm.Shared50);
    }
    private void Edit_RandomFill75_Click(object sender, EventArgs e)
    {
        var selection = _view.GetSelection();
        if (selection.IsEmpty) return;

        RandomFill(selection, RandomFillAlgorithm.Shared75);

    }
    private void Edit_FillBerlinNoise_Click(object sender, EventArgs e)
    {
        var selection = _view.GetSelection();
        if (selection.IsEmpty) return;

        RandomFill(selection, BerlinNoise.Create(0.1f));
    }
    private void View_FillGaborNoise_Click(object sender, EventArgs e)
    {
        var selection = _view.GetSelection();
        if (selection.IsEmpty) return;

        RandomFill(selection, GaborFillAlgorithm.Create(selection));
    }

    #endregion

    #region Action Menu Event Handlers
    private void Action_StartStop_Click(object sender, EventArgs e)
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
    private void Action_NextGeneration_Click(object sender, EventArgs e)
    {
        if (_cts is not null)
        {
            return;
        }

        _env.NextGeneration();
    }
    private void Action_SpeedUp_Click(object sender, EventArgs e)
    {
        _env.MsGenInterval -= 10;
        label1.Text = $"GenInterval: {_env.MsGenInterval} ms";
    }
    private void Action_SpeedDown_Click(object sender, EventArgs e)
    {
        _env.MsGenInterval += 10;
        label1.Text = $"GenInterval: {_env.MsGenInterval} ms";
    }
    private void Action_SetRule_Click(object sender, EventArgs e)
    {
        Suspend(() =>
        {
            var ret = Prompt.Show("Set Rule", "Rule string, like: b3/s23", out var newRule, _env.LifeMap.Rule);
            if (ret != DialogResult.OK)
            {
                return;
            }

            _env.LifeMap.Rule = newRule!;
        });
    }

    #endregion

    #region View Menu Event Handlers

    private void View_MoveToHome_Click(object sender, EventArgs e)
    {
        _view.MoveTo(0, 0);
    }
    private void View_MoveToCenterOfAllCells_Click(object sender, EventArgs e)
    {
        Suspend(() =>
        {
            var bounds = _env.LifeMap.GetBounds();
            _view.MoveTo(
                (int)bounds.Left + (int)bounds.Width / 2,
                (int)bounds.Top + (int)bounds.Height / 2
            );
        });
    }
    private void View_MovePointedCellToCenter_Click(object sender, EventArgs e)
    {
        var mcp = _view.MouseCellPoint;
        _view.MoveTo(mcp.X, mcp.Y);
    }
    private void View_MoveToCenterOfSelection_Click(object sender, EventArgs e)
    {
        var selection = _view.GetSelection();
        if (selection.IsEmpty) return;

        var center = new Point(
            selection.Left + (int)Math.Floor(selection.Width / 2.0),
            selection.Top + (int)Math.Floor(selection.Height / 2.0)
            );

        _view.MoveTo(center.X, center.Y);

    }
    private void View_MoveTo_Click(object sender, EventArgs e)
    {
        var ret = Prompt.Show("Move To", "Enter x, y (column, row)", out var coordStr, "0, 0");
        if (ret != DialogResult.OK) return;

        try
        {
            var coord = coordStr!
                .Split(',')
                .Select(x => x.Trim())
                .Select(int.Parse)
                .ToArray();

            if (coord.Length != 2) throw new FormatException();

            _view.MoveTo(coord[0], coord[1]);
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }
    private void View_ZoomIn_Click(object sender, EventArgs e)
    {
        _view.ZoomIn(); // 放大
    }
    private void View_ZoomOut_Click(object sender, EventArgs e)
    {
        _view.ZoomOut(); // 缩小
    }
    private void View_Suspend_Click(object? sender, EventArgs e)
    {
        if (_env.GetDCRender().IsSuspended)
        {
            var wasRunning = _floatLayer?.Tag as bool? ?? false;

            _floatLayer?.Dispose();
            _floatLayer = null;
            _env.GetDCRender().Resume();

            if (wasRunning) Start();
            BtnSuspendView.Text = "Suspend";
        }
        else
        {
            var wasRunning = _cts != null;
            Stop();
            _env.GetDCRender().Suspend();
            BtnSuspendView.Text = "Resume";

            _floatLayer = new PictureBox
            {
                BackColor = Color.Black,
                Width = canvas.Width,
                Height = canvas.Height,
                Location = canvas.Location,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                Tag = wasRunning,
            };

            _floatLayer.Click += View_Suspend_Click;

            _floatLayer.Paint += (sender, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Black);
                int vw = _floatLayer.Width;
                int vh = _floatLayer.Height;

                int circleSize = 100;
                int symbolSize = 30;

                // draw a circle
                g.FillEllipse(_foreBrush, vw / 2 - circleSize / 2, vh / 2 - circleSize / 2, circleSize, circleSize);

                // draw a pause symbol
                g.FillRectangle(_backBrush, vw / 2 - symbolSize / 2, vh / 2 - symbolSize / 2, symbolSize, symbolSize);
                g.FillRectangle(_foreBrush, vw / 2 - symbolSize / 3 / 2, vh / 2 - symbolSize / 2 - 1, symbolSize / 3, symbolSize + 2);

                // draw a text 
                var text = "Click To Resume";
                var size = g.MeasureString(text, Font);
                g.DrawString(text, Font, _foreBrush, vw / 2 - size.Width / 2, vh / 2 + circleSize / 2 + 10);
            };

            canvas.Parent!.Controls.Add(_floatLayer);
            _floatLayer.BringToFront();
        }
    }

    #endregion

}
