using System.Diagnostics;

namespace CellAutomata.Util;

public sealed partial class TaskProgressReporter : Form, IProgressReporter
{
    private readonly TaskCompletionSource<object?> _formInit;
    private readonly CancellationTokenSource? _cts;

    public bool IsAborted => _cts?.IsCancellationRequested ?? false;

    public CancellationToken CancelToken => _cts?.Token ?? CancellationToken.None;

    private TaskProgressReporter(string title = "Task", string description = "Please wait ...", bool allowAbort = false)
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

    private void Wait(Task task)
    {
        if (IsDisposed) return; // already disposed

        // ReSharper disable once MethodSupportsCancellation
        _formInit.Task.ContinueWith(_ =>
        {
            // close form when task is done
            // ReSharper disable once MethodSupportsCancellation
            task.ContinueWith(_ => Invoke(Dispose));
        });

        Show();
    }

    private new void Show()
    {
        ShowDialog();
    }

    private new void ShowDialog()
    {
        if (IsDisposed) return;
        base.ShowDialog(); // blocking 
    }

    public void ReportProgress(float progress)
    {
        if (IsDisposed) return; // already disposed
        if (InvokeRequired)
        {
            Invoke(() => ReportProgress(progress));
            return;
        }

        progressBar1.Value = (int)(Math.Clamp(progress, 0f, 1f) * 1000);
        lbPercentage.Text = $@"{progress:0.0%}";
    }

    public void ReportTimeRemaining(TimeSpan timeRemaining)
    {
        if (IsDisposed) return; // already disposed
        if (InvokeRequired)
        {
            Invoke(() => ReportTimeRemaining(timeRemaining));
            return;
        }

        lbTimeRemaining.Text =
            $@"{timeRemaining.Hours:D2}:{timeRemaining.Minutes:D2}:{timeRemaining.Seconds:D2} remaining.";
    }

    public void ReportStatus(string status)
    {
        if (IsDisposed) return; // already disposed
        if (InvokeRequired)
        {
            Invoke(() => ReportStatus(status));
            return;
        }

        lbStatus.Text = status;
    }

    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

    public void ReportProgress(float progress, string status, TimeSpan timeRemaining)
    {
        if (_stopwatch.ElapsedMilliseconds < 500) return; // avoid updating too frequently
        _stopwatch.Restart();

        Debug.WriteLine(
            $"{progress:0.0%} - {status} - {timeRemaining.Hours:D2}:{timeRemaining.Minutes:D2}:{timeRemaining.Seconds:D2} remaining.");

        if (IsDisposed) return;
        if (InvokeRequired)
        {
            Invoke(() => ReportProgress(progress, status, timeRemaining));
            return;
        }

        progressBar1.Value = (int)(Math.Clamp(progress, 0f, 1f) * 1000);
        // progress is between 0 and 1
        lbPercentage.Text = $@"{progress:0.0%}";
        lbStatus.Text = status;
        lbTimeRemaining.Text =
            @$"{timeRemaining.Hours:D2}:{timeRemaining.Minutes:D2}:{timeRemaining.Seconds:D2} remaining.";
    }


    private void BtnAbort_Click(object sender, EventArgs e)
    {
        if (IsDisposed) return; // already disposed
        _cts?.Cancel();
        Dispose();
    }

    public static void Watch(Func<IProgressReporter, Task> func, string title = "Task",
        string description = "Please wait ...")
    {
        using var reporter = new TaskProgressReporter(title, description, true);
        reporter.Wait(func(reporter));
    }

    public static void WatchNoAbort(Func<IProgressReporter, Task> func, string title = "Task",
        string description = "Please wait ...")
    {
        using var reporter = new TaskProgressReporter(title, description);
        reporter.Wait(func(reporter));
    }
}