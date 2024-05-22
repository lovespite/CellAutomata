using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using Timer = System.Threading.Timer;

namespace CellAutomata.Util;

public partial class TaskProgressReporter : Form, IProgressReporter
{
    private readonly TaskCompletionSource<object?> _formInit;
    private readonly CancellationTokenSource? _cts;

    public bool IsAborted => _cts?.IsCancellationRequested ?? false;

    public CancellationToken CancelToken => _cts?.Token ?? CancellationToken.None;

    public TaskProgressReporter(string title = "Task", string description = "Please wait ...", bool allowAbort = false)
    {
        InitializeComponent();

        _formInit = new TaskCompletionSource<object?>();

        _cts = allowAbort ? new CancellationTokenSource() : null;
        btnAbort.Enabled = allowAbort;
        ControlBox = false; // hide close button

        Text = title;
        lbDescription.Text = description;

        progressBar1.Minimum = 0;
        progressBar1.Maximum = 1000;
    }

    private void TaskProgressReporter_Load(object sender, EventArgs e)
    {
        lbStatus.Text = string.Empty;
        lbTimeRemaining.Text = string.Empty;
        progressBar1.Value = 0;

        _formInit.SetResult(null); // form is ready
    }

    public void Wait(Task task)
    {
        if (IsDisposed) return; // already disposed

        _formInit.Task.ContinueWith(_ =>
        {
            // close form when task is done
            task.ContinueWith(_ => Invoke(Dispose));
        });

        Show();
    }

    public new void Show()
    {
        ShowDialog();
    }

    public new void ShowDialog()
    {
        if (IsDisposed) return;
        base.ShowDialog(); // blocking 
    }

    public void ReportProgress(float progress)
    {
        if (IsDisposed) return; // already disposed
        if (InvokeRequired)
        {
            Invoke(new Action(() => ReportProgress(progress)));
            return;
        }

        progressBar1.Value = (int)(Math.Clamp(progress, 0f, 1f) * 1000);
        lbPercentage.Text = string.Format("{0:0.0%}", progress);
    }

    public void ReportTimeRemaining(TimeSpan timeRemaining)
    {
        if (IsDisposed) return; // already disposed
        if (InvokeRequired)
        {
            Invoke(new Action(() => ReportTimeRemaining(timeRemaining)));
            return;
        }

        lbTimeRemaining.Text = $"{timeRemaining.Hours:D2}:{timeRemaining.Minutes:D2}:{timeRemaining.Seconds:D2} remaining.";
    }

    public void ReportStatus(string status)
    {
        if (IsDisposed) return; // already disposed
        if (InvokeRequired)
        {
            Invoke(new Action(() => ReportStatus(status)));
            return;
        }

        lbStatus.Text = status;
    }

    private Stopwatch _stopwatch = Stopwatch.StartNew();
    public void ReportProgress(float progress, string status, TimeSpan timeRemaining)
    {
        if (_stopwatch.ElapsedMilliseconds < 500) return; // avoid updating too frequently
        _stopwatch.Restart();

        Debug.WriteLine($"{progress:0.0%} - {status} - {timeRemaining.Hours:D2}:{timeRemaining.Minutes:D2}:{timeRemaining.Seconds:D2} remaining.");

        if (IsDisposed) return;
        if (InvokeRequired)
        {
            Invoke(new Action(() => ReportProgress(progress, status, timeRemaining)));
            return;
        }

        progressBar1.Value = (int)(Math.Clamp(progress, 0f, 1f) * 1000);
        // progress is between 0 and 1
        lbPercentage.Text = string.Format("{0:0.0%}", progress);
        lbStatus.Text = status;
        lbTimeRemaining.Text = $"{timeRemaining.Hours:D2}:{timeRemaining.Minutes:D2}:{timeRemaining.Seconds:D2} remaining.";
    }


    private void BtnAbort_Click(object sender, EventArgs e)
    {
        if (IsDisposed) return; // already disposed
        _cts?.Cancel();
        Dispose();
    }


    public static void Watch(Func<IProgressReporter, Task> func, string title = "Task", string description = "Please wait ...")
    {
        using var reporter = new TaskProgressReporter(title, description, true);
        reporter.Wait(func(reporter));
    }

    public static void Watch(Action<IProgressReporter> action, string title = "Task", string description = "Please wait ...")
    {
        using var reporter = new TaskProgressReporter(title, description, true);
        reporter.Wait(Task.Run(() => action(reporter)));
    }

    public static void WatchNoAbort(Func<IProgressReporter, Task> func, string title = "Task", string description = "Please wait ...")
    {
        using var reporter = new TaskProgressReporter(title, description, false);
        reporter.Wait(func(reporter));
    }

    public static void WatchNoAbort(Action<IProgressReporter> action, string title = "Task", string description = "Please wait ...")
    {
        using var reporter = new TaskProgressReporter(title, description, false);
        reporter.Wait(Task.Run(() => action(reporter)));
    }
}

