﻿using System.Runtime.InteropServices;
using CellAutomata.Algos;
using CellAutomata.Render;
using CellAutomata.Util;

namespace CellAutomata;

public class CellEnvironment(ILifeMap bitmap)
{
    /// <summary>
    /// Milliseconds
    /// </summary>
    public int MsGenInterval
    {
        get => bitmap.GenInterval;
        set
        {
            if (value < 10)
            {
                bitmap.GenInterval = 10;
            }
            else if (value > 10_000)
            {
                bitmap.GenInterval = 10_000;
            }
            else
            {
                bitmap.GenInterval = value;
            }
        }
    }

    public IDcRender GetDcRender()
    {
        return LifeMap.GetDcRender();
    }

    public ILifeMap LifeMap => bitmap;

    private readonly object _lock = new();

    public void Lock(Action<ILifeMap> action)
    {
        lock (_lock)
        {
            action(bitmap);
        }
    }

    public long Population => bitmap.Population;

    public long Generation => bitmap.Generation;

    public int ThreadCount
    {
        get => bitmap.ThreadCount;
        set => bitmap.ThreadCount = value;
    }

    public long MsCpuTime => bitmap.MsCpuTime; // milliseconds 


    public void NextGeneration()
    {
        lock (_lock)
        {
            bitmap.NextGeneration();
        }
    }

    public IReadOnlyCollection<PointL> GetRegionAliveCells(RectangleL rect)
    {
        lock (_lock)
        {
            return bitmap.QueryRegion(true, rect);
        }
    }

    public void ActivateCell(long row, long col)
    {
        lock (_lock)
        {
            ActivateCellInternal(row, col);
        }
    }

    public void DeactivateCell(long row, long col)
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

        var cells = await bitmap.GetLocationsAsync(true, progress);
        await using var fs = File.Create(file);

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
                    p[0] = (int)cell.X;
                    p[1] = (int)cell.Y;
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
        await using var fs = File.OpenRead(file);
        using var br = new BinaryReader(fs);

        await Task.Delay(1000);
        float totalCount = fs.Length / (float)buffer.Length;
        var count = 0;
        while (fs.Position < fs.Length)
        {
            if (progress?.IsAborted ?? false) return;

            var readAsync = await fs.ReadAsync(buffer);
            if (readAsync < buffer.Length) break; // EOF
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

    private void ActivateCellInternal(long row, long col)
    {
        bitmap.Set(row, col, true);
    }

    private void DeactivateCellInternal(long row, long col)
    {
        bitmap.Set(row, col, false);
    }

    private void ResetInternal()
    {
        bitmap.Clear();
    }

    private ILifeMap CreateSnapshotInternal()
    {
        return bitmap.CreateSnapshot();
    }

    #endregion
}