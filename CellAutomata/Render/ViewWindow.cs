﻿using CellAutomata.Algos;
using SharpDX.Mathematics.Interop;
using System.Diagnostics;

namespace CellAutomata.Render;

public class ViewWindow : ViewWindowBase
{
    private readonly nint _canvas;
    public ViewWindow(CellEnvironment cells, Size vwSize, nint canvas)
        : base(cells, vwSize.Width, vwSize.Height, 1)
    {
        if (cells.LifeMap is not HashLifeMap)
            throw new ArgumentException("ViewWindowDx2dRaw is only support for HashLifeMap");

        _canvas = canvas;

        _vwSize = vwSize;
        _center = new Point(0, 0);
    }

    private Size _vwSize;
    private Point _center;

    public override void MoveTo(int left, int top)
    {
        _centerX = left;
        _centerY = top;

        _center = new Point(left, top);
    }

    public override void Resize(int pixelViewWidth, int pixelViewHeight)
    {
        _vwSize = new Size(pixelViewWidth, pixelViewHeight);

        base.Resize(pixelViewWidth, pixelViewHeight);
    }

    public override void Draw(Graphics? graphics)
    {
        DrawMainView4(_cellEnvironment.LifeMap.GetDCRender());
    }
    private ulong _frames = 0;
    private readonly Stopwatch _sw = Stopwatch.StartNew();
    private readonly Stopwatch _sw2 = Stopwatch.StartNew();

    private float _fps; // frames per second 
    private string text = string.Empty;
    private void DrawMainView4(IDCRender render)
    {
        if (_canvas == 0) return;

        var mag = _cellSize > 0
            ? (int)Math.Log2(_cellSize)
            : 0;

        var selection = GetSelection();

        VIEWINFO selview = new();

        if (selection.IsEmpty)
        {
            selview.EMPTY = 1;
        }
        else
        {
            selview.EMPTY = 0;

            selview.psl_x1 = selection.X;
            selview.psl_y1 = selection.Y;
            selview.psl_x2 = selection.Right;
            selview.psl_y2 = selection.Bottom;
        }

        render.DrawViewportDC(_canvas, mag, _vwSize, _center, ref selview, text);

        if (_sw.ElapsedMilliseconds > 500)
        {
            _fps = (float)(_frames / _sw.Elapsed.TotalSeconds);
            _frames = 0;
            _sw.Restart();

        }

        if (_sw2.ElapsedMilliseconds > 100)
        {
            text =
                $"{_cellEnvironment.LifeMap.GetType().Name}, {_cellEnvironment.LifeMap.Rule}\n" +
                $"Population: {_cellEnvironment.Population:#,0}, Generation: {_cellEnvironment.Generation:#,0}\n" +
                $"Position: {MouseCellPoint}  Mag: {mag}, View: {_center}\n" +
                $"CPU Time: {_cellEnvironment.MsCPUTime:#,0} ms  FPS: {_fps:0.0}";

            _sw2.Restart();
        }

        ++_frames;
    }
}