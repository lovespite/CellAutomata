﻿using SharpDX.Mathematics.Interop;
using System.Diagnostics;

namespace CellAutomata;

public class ViewWindowDx2dRaw : ViewWindowBase
{
    private readonly nint _canvas;
    public ViewWindowDx2dRaw(CellEnvironment cells, Size vwSize, nint canvas)
        : base(cells, vwSize.Width, vwSize.Height, 1)
    {
        if (cells.BitMap is not HashLifeMap)
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
        DrawMainView4(_cellEnvironment.BitMap);
    }
    private ulong _frames = 0;
    private readonly Stopwatch _sw = Stopwatch.StartNew();

    private float _fps; // frames per second 
    private string text = string.Empty;
    private void DrawMainView4(ILifeMap bitmap)
    {
        if (bitmap is not HashLifeMap hlm) return;
        if (_canvas == 0) return;

        var mag = _cellSize > 0
            ? (int)Math.Log2(_cellSize)
            : 0;

        var viewCX = _pxViewWidth * 0.5F;
        var viewCY = _pxViewHeight * 0.5F;
        var selection = GetSelection();

        VIEWINFO selview = new();

        if (selection.IsEmpty)
        {
            selview.EMPTY = 1;
        }
        else
        {
            selview.EMPTY = 0;
            var left = (selection.X - _centerX) * _cellSize + viewCX;
            var top = (selection.Y - _centerY) * _cellSize + viewCY;

            //var rect = new RawRectangleF(
            //    left: left,
            //    top: top,
            //    right: left + selection.Width * _cellSize,
            //    bottom: top + selection.Height * _cellSize);

            selview.psl_x1 = (int)left - 1;
            selview.psl_y1 = (int)top - 1;
            selview.psl_x2 = (int)(left + selection.Width * _cellSize) + 1;
            selview.psl_y2 = (int)(top + selection.Height * _cellSize) + 1;
        }


        hlm.DrawRegionDC(_canvas, mag, _vwSize, _center, ref selview, text);

        if (_sw.ElapsedMilliseconds > 500)
        {
            _fps = (float)(_frames / _sw.Elapsed.TotalSeconds);
            _frames = 0;
            _sw.Restart();

            text =
                $"Generation: {_cellEnvironment.Generation:#,0} Population: {_cellEnvironment.Population:#,0}\n" +
                $"Position: {MouseCellPoint}\n" +
                $"CPU Time: {_cellEnvironment.MsCPUTime:#,0}\n" +
                $"GPS: {_gps:0.0} FPS: {_fps:0.0}";
        }
        ++_frames;
    }
}