using System.Runtime.InteropServices;

namespace CellAutomata.Render;
[StructLayout(LayoutKind.Sequential)]
public struct ViewInfo
{
    public int EMPTY;
    public int psl_x1; // selection rect point 1
    public int psl_y1; // selection rect point 1
    public int psl_x2; // selection rect point 2
    public int psl_y2; // selection rect point 2 
}
public interface IDcRender
{
    bool IsSuspended { get; }

    void DrawViewportDc(nint hWndCanvas, int mag, Size vwSize, Point center, ref ViewInfo selection, string text);
    void Suspend();
    void Resume();
}

