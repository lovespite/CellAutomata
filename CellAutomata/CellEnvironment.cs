using System.Diagnostics;
using System.Drawing.Printing;
using System.Runtime.InteropServices;

namespace CellAutomata;

public class CellEnvironment
{
    private readonly ILifeMap _bitmap;

    /// <summary>
    /// Milliseconds
    /// </summary>
    public int MsGenInterval
    {
        get => _bitmap.GenInterval;
        set
        {
            if (value < 10)
            {
                _bitmap.GenInterval = 10;
            }
            else if (value > 10_000)
            {
                _bitmap.GenInterval = 10_000;
            }
            else
            {
                _bitmap.GenInterval = value;
            }
        }
    }

    public ILifeMap BitMap => _bitmap;

    private readonly object _lock = new();

    public CellEnvironment(ILifeMap bitmap)
    {
        _bitmap = bitmap;
    }

    public void Lock(Action<ILifeMap> action)
    {
        lock (_lock)
        {
            action(_bitmap);
        }
    }

    public long Population => _bitmap.Population;
    public long Generation
    {
        get => _bitmap.Generation;
    }

    public int ThreadCount
    {
        get => _bitmap.ThreadCount;
        set => _bitmap.ThreadCount = value;
    }

    public long MsCPUTime => _bitmap.MsCPUTime; // milliseconds
    public long MsMemoryCopyTime => _bitmap.MsMemoryCopyTime; // milliseconds
    public long MsGenerationTime => _bitmap.MsGenerationTime; // milliseconds

    public bool IsAlive(int row, int col)
    {
        lock (_lock)
        {
            return _bitmap.Get(row, col);
        }
    }

    public void NextGeneration()
    {
        lock (_lock)
        {
            _bitmap.NextGeneration();
        }
    }

    public IReadOnlyCollection<Point> GetRegionAliveCells(Rectangle rect)
    {
        lock (_lock)
        {
            return _bitmap.QueryRegion(true, rect);
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
        var cells = _bitmap.GetLocations(true);
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
        _bitmap.Set(row, col, true);
    }

    private void DeactivateCellInternal(int row, int col)
    {
        _bitmap.Set(row, col, false);
    }

    private void ClearInternal()
    {
        _bitmap.Clear();
    }

    private ILifeMap CreateSnapshotInternal()
    {
        return _bitmap.CreateSnapshot();
    }

    #endregion
}
