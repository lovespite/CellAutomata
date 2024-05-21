using System.Runtime.InteropServices;

namespace CellAutomata.Render;
[StructLayout(LayoutKind.Sequential)]
public struct VIEWINFO
{
    public int EMPTY;
    public int psl_x1; // selection rect point 1
    public int psl_y1; // selection rect point 1
    public int psl_x2; // selection rect point 2
    public int psl_y2; // selection rect point 2 
}
public interface IDCRender
{
    bool IsSuspended { get; }

    void DrawViewportDC(nint hWndCanvas, int mag, Size vwSize, Point center, ref VIEWINFO selection, string text);
    void Suspend();
    void Resume();
}

