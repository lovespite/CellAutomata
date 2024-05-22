﻿using System.Diagnostics;
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using CellAutomata.Algos;
using CellAutomata.Render;
using CellAutomata.Util;

namespace CellAutomata;

public class CellEnvironment(ILifeMap bitmap)
{
    private readonly ILifeMap _lifemap = bitmap;

    /// <summary>
    /// Milliseconds
    /// </summary>
    public int MsGenInterval
    {
        get => _lifemap.GenInterval;
        set
        {
            if (value < 10)
            {
                _lifemap.GenInterval = 10;
            }
            else if (value > 10_000)
            {
                _lifemap.GenInterval = 10_000;
            }
            else
            {
                _lifemap.GenInterval = value;
            }
        }
    }

    public IDCRender GetDCRender()
    {
        return LifeMap.GetDCRender();
    }

    public ILifeMap LifeMap => _lifemap;

    private readonly object _lock = new();

    public void Lock(Action<ILifeMap> action)
    {
        lock (_lock)
        {
            action(_lifemap);
        }
    }

    public long Population => _lifemap.Population;
    public long Generation
    {
        get => _lifemap.Generation;
    }

    public int ThreadCount
    {
        get => _lifemap.ThreadCount;
        set => _lifemap.ThreadCount = value;
    }

    public long MsCPUTime => _lifemap.MsCPUTime; // milliseconds 


    public void NextGeneration()
    {
        lock (_lock)
        {
            _lifemap.NextGeneration();
        }
    }

    public IReadOnlyCollection<Point> GetRegionAliveCells(Rectangle rect)
    {
        lock (_lock)
        {
            return _lifemap.QueryRegion(true, rect);
        }
    }

    public void ActivateCell(int row, int col)
    {
        lock (_lock)
        {
            ActivateCellInternal(row, col);
        }
    }

    public void DeactivateCell(int row, int col)
    {
        lock (_lock)
        {
            DeactivateCellInternal(row, col);
        }
    }

    public void Reset()
    {
        lock (_lock)
        {
            ResetInternal();
        }
    }

    public ILifeMap CreateSnapshot()
    {
        lock (_lock)
        {
            return CreateSnapshotInternal();
        }
    }

    public async Task SaveTo(string file, IProgressReporter? progress = null)
    {
        progress?.ReportProgress(0, "Collecting...", TimeSpan.Zero);
        await Task.Delay(100);

        var cells = _lifemap.GetLocations(true);
        using var fs = File.Create(file);

        var buffer = new byte[Marshal.SizeOf<int>() * 2];
        float totalCount = cells.Length;
        progress?.ReportProgress(0, "Saving data...", TimeSpan.Zero);

        for (int i = 0; i < cells.Length; i++)
        {
            if (progress?.IsAborted ?? false) return;

            var cell = cells[i];
            unsafe
            {
                fixed (byte* ptr = buffer)
                {
                    var p = (int*)ptr;
                    p[0] = cell.X;
                    p[1] = cell.Y;
                }
            }

            await fs.WriteAsync(buffer);

            if (i % 10_000 == 0)
            {
                progress?.ReportProgress(i / totalCount, $"Saving data... {i}", TimeSpan.Zero);
                await Task.Delay(1);
            }
        }

        progress?.ReportProgress(1, "Done.", TimeSpan.Zero);
    }

    public async Task LoadFrom(string file, IProgressReporter? progress = null)
    {
        progress?.ReportProgress(0, "Reading file...", TimeSpan.Zero);
        var buffer = new byte[Marshal.SizeOf<int>() * 2];
        using var fs = File.OpenRead(file);
        using var br = new BinaryReader(fs);

        await Task.Delay(1000);
        float totalCount = fs.Length / buffer.Length;
        var count = 0;
        while (fs.Position < fs.Length)
        {
            if (progress?.IsAborted ?? false) return;

            await fs.ReadAsync(buffer);
            unsafe
            {
                fixed (byte* ptr = buffer)
                {
                    var p = (int*)ptr;

                    ActivateCell(p[1], p[0]);
                }
            }

            count++;
            if (count % 10000 == 0)
            {
                progress?.ReportProgress(count / totalCount, $"Loading... {count}", TimeSpan.Zero);
                await Task.Delay(1);
            }
        }

        progress?.ReportProgress(1, "Done.", TimeSpan.Zero);

        await Task.Delay(100);
    }

    #region Internal Methods 

    private void ActivateCellInternal(int row, int col)
    {
        _lifemap.Set(row, col, true);
    }

    private void DeactivateCellInternal(int row, int col)
    {
        _lifemap.Set(row, col, false);
    }

    private void ResetInternal()
    {
        _lifemap.Clear();
    }

    private ILifeMap CreateSnapshotInternal()
    {
        return _lifemap.CreateSnapshot();
    }

    #endregion
}
