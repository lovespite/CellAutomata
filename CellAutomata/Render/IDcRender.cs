﻿using System.Runtime.InteropServices;
using CellAutomata.Algos;

namespace CellAutomata.Render;

[StructLayout(LayoutKind.Sequential)]
public struct ViewInfo
{
    public int EMPTY;
    public Int64 psl_x1; // selection rect point 1
    public Int64 psl_y1; // selection rect point 1
    public Int64 psl_x2; // selection rect point 2
    public Int64 psl_y2; // selection rect point 2 
}

public interface IDcRender
{
    bool IsSuspended { get; }

    void DrawViewportDc(nint hWndCanvas, int mag, Size vwSize, PointL center, ViewInfo selection, string text);
    void Suspend();
    void Resume();
}