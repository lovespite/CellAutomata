using System.Diagnostics;
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using CellAutomata.Algos;

namespace CellAutomata;

public class CellEnvironment
{
    private readonly ILifeMap _lifemap;

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

    public ILifeMap LifeMap => _lifemap;

    private readonly object _lock = new();

    public CellEnvironment(ILifeMap bitmap)
    {
        _lifemap = bitmap;
    }

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

    public bool IsAlive(int row, int col)
    {
        lock (_lock)
        {
            return _lifemap.Get(row, col);
        }
    }

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

    public void Clear()
    {
        lock (_lock)
        {
            ClearInternal();
        }
    } 

    public ILifeMap CreateSnapshot()
    {
        lock (_lock)
        {
            return CreateSnapshotInternal();
        }
    }

    public async Task SaveTo(string file)
    {
        var cells = _lifemap.GetLocations(true);
        using var fs = File.Create(file);

        var buffer = new byte[cells.Length * Marshal.SizeOf<int>() * 2];
        unsafe
        {
            fixed (byte* ptr = buffer)
            {
                var p = (int*)ptr;
                for (int i = 0; i < cells.Length; i++)
                {
                    var cell = cells[i];
                    p[i * 2] = cell.X;
                    p[i * 2 + 1] = cell.Y;
                }
            }
        }

        await fs.WriteAsync(buffer);
    }

    public async Task LoadFrom(string file)
    {
        var buffer = new byte[Marshal.SizeOf<int>() * 2];
        using var fs = File.OpenRead(file);
        using var br = new BinaryReader(fs);

        while (fs.Position < fs.Length)
        {
            await fs.ReadAsync(buffer);
            unsafe
            {
                fixed (byte* ptr = buffer)
                {
                    var p = (int*)ptr;

                    ActivateCell(p[1], p[0]);
                }
            }
        }
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

    private void ClearInternal()
    {
        _lifemap.Clear();
    }

    private ILifeMap CreateSnapshotInternal()
    {
        return _lifemap.CreateSnapshot();
    }

    #endregion
}
