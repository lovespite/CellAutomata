namespace CellAutomata.Util;

public interface IProgressReporter
{
    /// <summary>
    /// 0-100
    /// </summary>
    /// <param name="progress"></param>
    void ReportProgress(float progress); // 0-100
    void ReportTimeRemaining(TimeSpan timeRemaining); // hh:mm:ss
    void ReportStatus(string status);

    void ReportProgress(float progress, string status, TimeSpan timeRemaining);

    bool IsAborted { get; }
    CancellationToken CancelToken { get; }
}

