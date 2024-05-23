using System.Diagnostics;
using System.Drawing.Drawing2D;
using CellAutomata.Algos;
using CellAutomata.Render;
using CellAutomata.Util;
using Brush = System.Drawing.Brush;

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

    private (ILifeMap, SizeL)? _clipboard;

    private readonly Thread _painting;

    public const int ThreadCount = 16;

    public Form1()
    {
        InitializeComponent();
        canvas.AllowDrop = true;
        canvas.Resize += Form1_Resize;

        var lifeMap = new HashLifeMap(rule: "B3/S23");

        _env = new CellEnvironment(lifeMap)
        {
            ThreadCount = ThreadCount
        };

        _painting = new Thread(Render);
    }

    #region Public Methods

    public void Suspend(Action action, bool invokeRequired = false)
    {
        bool wasRunning = _cts != null;
        bool wasSuspended = _env.GetDcRender().IsSuspended;

        Stop();
        _env.GetDcRender().Suspend();

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

        if (!wasSuspended) _env.GetDcRender().Resume();
        if (wasRunning) Start();
    }

    public Task SuspendAsync(Action action, bool invokeRequired = false)
    {
        bool wasRunning = _cts != null;
        bool wasSuspended = _env.GetDcRender().IsSuspended;
        Stop(); // ֹͣ��ǰ����
        _env.GetDcRender().Suspend();

        Task t = invokeRequired
            ? Task.Run(() => Invoke(action))
            : Task.Run(action);

        return t.ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                HandleException(task.Exception!);
            }

            if (!wasSuspended) _env.GetDcRender().Resume();
            if (wasRunning) Start();
        });
    }

    public async Task SuspendAsync(Func<Task> action, bool invokeRequired = false)
    {
        bool wasRunning = _cts != null;
        bool wasSuspended = _env.GetDcRender().IsSuspended;
        Stop(); // ֹͣ��ǰ����
        _env.GetDcRender().Suspend();

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
            HandleException(ex); // �����쳣
        }

        if (!wasSuspended) _env.GetDcRender().Resume();
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
                tcs.SetException(ex); // �����쳣
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
            Invoke((MethodInvoker)delegate { ShowException(ex); });
            return;
        }

        MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    protected void ShowMessage(string message)
    {
        if (InvokeRequired)
        {
            Invoke((MethodInvoker)delegate { ShowMessage(message); });
            return;
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
                Invoke((MethodInvoker)delegate { _view.Draw(null); });
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

        btnStartStop.Text = @"Start";
        Debug.WriteLine("Stopped");
    }

    private void Start()
    {
        _cts = new CancellationTokenSource();
        _evolutionThread = new Thread(EvolutionThread);
        btnStartStop.Text = @"Stop";
        _evolutionThread.Start();
        Debug.WriteLine("Started");
    }

    private void SetClipboard(RectangleL rect)
    {
        if (rect.Area() > AsyncAreaThreshold)
        {
            Debug.WriteLine("Async copy");
            TaskProgressReporter.Watch(async reporter =>
            {
                var bitmap = await _env.LifeMap.CreateRegionSnapshotAsync(rect, reporter);
                _clipboard = (bitmap, rect.Size);
            }, "Copying");
        }
        else
        {
            var bitmap = _env.LifeMap.CreateRegionSnapshot(rect);
            _clipboard = (bitmap, rect.Size);
        }
    }

    private void ClearRegion(RectangleL rect)
    {
        if (rect.IsEmpty) return;

        if (rect.Area() > AsyncAreaThreshold)
        {
            TaskProgressReporter.Watch(
                async reporter => { await SuspendAsync(() => _env.LifeMap.ClearRegionAsync(rect, reporter)); },
                "Clearing");
        }
        else
        {
            Suspend(() => _env.LifeMap.ClearRegion(rect));
        }
    }

    private async Task RotateRegionAsync(RectangleL selection, IProgressReporter? reporter)
    {
        var snapshot = await _env.LifeMap.CreateRegionSnapshotAsync(selection, reporter);

        _env.LifeMap.ClearRegion(selection);

        // ��ȡѡ�����ĵ� 
        var centerX = selection.Left + (long)Math.Floor(selection.Width / 2.0d);
        var centerY = selection.Top + (long)Math.Floor(selection.Height / 2.0d);

        var prList = new List<PointL>();

        double total = selection.Area();
        double count = 0;

        // 90��˳ʱ����ת
        for (var row = selection.Top; row < selection.Bottom; row++)
        {
            for (var col = selection.Left; col < selection.Right; col++)
            {
                if (reporter?.IsAborted == true) return;

                // �����ѡ�����ĵ�����
                var relX = col - centerX;
                var relY = row - centerY;

                // ��ת����
                var rotatedX = -relY;
                var rotatedY = relX;

                // ������λ�õľ�������
                var p2 = new PointL(centerX + rotatedX, centerY + rotatedY);
                var p0 = new PointL(col - selection.Left, row - selection.Top);

                _env.LifeMap.Set(p2, snapshot.Get(p0));

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

        _view.SetSelection(new PointL(pr1, pc1), new PointL(pr2, pc2));
    }

    private async Task FlipRegionUpDown(RectangleL selection, IProgressReporter? reporter)
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

                var p0 = new PointL(col - selection.Left, row - selection.Top);
                var p1 = new PointL(col, selection.Bottom - 1 - (row - selection.Top));

                _env.LifeMap.Set(p1, snapshot.Get(p0));

                if (reporter is null) continue;

                if ((++count) % ReportInterval == 0)
                {
                    reporter.ReportProgress((float)(count / total), "Flipping ...", TimeSpan.Zero);
                    await Task.Delay(1);
                }
            }
        }
    }

    private async Task FlipRegionLeftRight(RectangleL selection, IProgressReporter? reporter)
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

                var p0 = new PointL(col - selection.Left, row - selection.Top);
                var p1 = new PointL(selection.Right - 1 - (col - selection.Left), row);

                _env.LifeMap.Set(p1, snapshot.Get(p0));

                if (reporter is null) continue;

                if ((++count) % ReportInterval == 0)
                {
                    reporter.ReportProgress((float)(count / total), "Flipping ...", TimeSpan.Zero);
                    await Task.Delay(1);
                }
            }
        }
    }

    private void RandomFill(RectangleL selection, IRandomFillAlgorithm algo)
    {
        if (selection.IsEmpty) return;
        if (selection.Area() > AsyncAreaThreshold)
        {
            TaskProgressReporter.Watch(async reporter =>
            {
                await Task.Delay(100);
                await SuspendAsync(() => Fill(reporter));
            }, "Filling");
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

                    if (++count % ReportInterval == 0)
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
        }, "Loading");
    }

    private void LoadGollyRle(string file)
    {
        TaskProgressReporter.WatchNoAbort(async reporter =>
        {
            await Task.Delay(100);
            await SuspendAsync(() => _env.LifeMap.ReadRle(file));
            await Task.Delay(100);

            reporter.ReportProgress(1, "Done.", TimeSpan.Zero);
        }, "Loading");
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
        }, "Saving");
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
        label1.Text = $@"GenInterval: {_env.MsGenInterval} ms";

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

        Stop(); // stop evolution 
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
        Stop(); // stop evolution

        using var dialog = new OpenFileDialog();
        dialog.Title = @"Load Binary BitMap File";
        dialog.Filter = @"Binary BitMap|*.bbm|All|*.*";
        dialog.InitialDirectory = Path.Combine(Application.StartupPath, "Conways");
        dialog.Multiselect = false;

        if (dialog.ShowDialog() != DialogResult.OK) return;
        LoadBbmFile(dialog.FileName);

        _view.ClearSelection();
        View_MoveToCenterOfAllCells_Click(sender, e); // move to center
    }

    private void File_LoadGollyFile_Click(object sender, EventArgs e)
    {
        Stop(); // stop evolution

        using var dialog = new OpenFileDialog();
        dialog.Filter = @"Golly RLE|*.rle;*.mc;*.mc.gz|MCells|*.mcl|All|*.*";
        dialog.Title = @"Load Golly RLE File";
        dialog.Multiselect = false;

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
            dialog.Filter = @"Binary BitMap|*.bbm|All|*.*";
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

        if (bounds.Size.Area() > AsyncAreaThreshold)
        {
            TaskProgressReporter.Watch(async reporter =>
            {
                await Task.Delay(100);
                await SuspendAsync(() => Clear(reporter));
            }, "Clearing");
        }
        else
        {
            _ = SuspendAsync(() => Clear());
        }

        async Task Clear(IProgressReporter? reporter = null)
        {
            double total = bounds.Size.Area();
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
                        reporter.ReportProgress((float)(count / total), "Clearing ...", TimeSpan.Zero);
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
            var p1 = new PointL(
                x: aliveCells.Min(p => p.X),
                y: aliveCells.Min(p => p.Y)
            );

            var p2 = new PointL(
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
        SizeL size;

        (data, size) = _clipboard.Value;

        var p = _view.GetSelection().Location;

        if (size.Area() > AsyncAreaThreshold)
        {
            TaskProgressReporter.Watch(async reporter =>
            {
                await Task.Delay(100);
                await SuspendAsync(() => _env.LifeMap.BlockCopyAsync(data, size, p, _mode, reporter));
            }, "Pasting");
        }
        else
        {
            Suspend(() => _env.LifeMap.BlockCopy(data, size, p, _mode));
        }

        _view.SetSelection(p, new PointL(p.X + size.Width - 1, p.Y + size.Height - 1));
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
            }, "Rotating");
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
            }, "Flipping");
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
            }, "Flipping");
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

        RandomFill(selection, BerlinNoise.Create());
    }

    private void Edit_FillGabor_Click(object sender, EventArgs e)
    {
        var selection = _view.GetSelection();
        if (selection.IsEmpty) return;

        RandomFill(selection, GaborFillAlgorithm.Create(selection));
    }

    private void Edit_FillWorley_Click(object sender, EventArgs e)
    {
        var selection = _view.GetSelection();
        if (selection.IsEmpty) return;

        RandomFill(selection, new WorleyFillAlgorithm(10, selection));
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
        label1.Text = @$"GenInterval: {_env.MsGenInterval} ms";
    }

    private void Action_SpeedDown_Click(object sender, EventArgs e)
    {
        _env.MsGenInterval += 10;
        label1.Text = @$"GenInterval: {_env.MsGenInterval} ms";
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

        var center = new PointL(
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
            var array = coordStr!
                .Split(',')
                .Select(x => x.Trim())
                .Select(int.Parse)
                .ToArray();

            if (array.Length != 2) throw new FormatException();

            _view.MoveTo(array[0], array[1]);
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    private void View_ZoomIn_Click(object sender, EventArgs e)
    {
        _view.ZoomIn(); // �Ŵ�
    }

    private void View_ZoomOut_Click(object sender, EventArgs e)
    {
        _view.ZoomOut(); // ��С
    }

    private void View_Suspend_Click(object? sender, EventArgs e)
    {
        if (_env.GetDcRender().IsSuspended)
        {
            var wasRunning = _floatLayer?.Tag as bool? ?? false;

            _floatLayer?.Dispose();
            _floatLayer = null;
            _env.GetDcRender().Resume();

            if (wasRunning) Start();
            BtnSuspendView.Text = @"Suspend";
        }
        else
        {
            var wasRunning = _cts != null;
            Stop();
            _env.GetDcRender().Suspend();
            BtnSuspendView.Text = @"Resume";

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

            _floatLayer.Paint += (_, args) =>
            {
                var g = args.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Black);
                int vw = _floatLayer.Width;
                int vh = _floatLayer.Height;

                int circleSize = 100;
                int symbolSize = 30;

                // draw a circle
                g.FillEllipse(_foreBrush, vw / 2 - circleSize / 2, vh / 2 - circleSize / 2, circleSize, circleSize);

                // draw a pause symbol
                g.FillRectangle(_backBrush, vw / 2 - symbolSize / 2, vh / 2 - symbolSize / 2, symbolSize, symbolSize);
                g.FillRectangle(_foreBrush, vw / 2 - symbolSize / 3 / 2, vh / 2 - symbolSize / 2 - 1, symbolSize / 3,
                    symbolSize + 2);

                // draw a text 
                var text = "Click To Resume";
                var size = g.MeasureString(text, Font);
                g.DrawString(text, Font, _foreBrush, vw / 2F - size.Width / 2, vh / 2F + circleSize / 2F + 10);
            };

            canvas.Parent!.Controls.Add(_floatLayer);
            _floatLayer.BringToFront();
        }
    }

    #endregion
}