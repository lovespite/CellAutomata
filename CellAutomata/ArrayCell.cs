using System.Collections.Concurrent;
using System.Diagnostics;

namespace CellAutomata;

public class ArrayCell
{
    public long ExtraInfo { get; set; }

    private ArrayCell()
    {
    }

    public class Factory : IDisposable
    {
        public const int MaxPoolCapcity = 10000;
        private readonly ConcurrentQueue<ArrayCell> _pool = new();
        private readonly long _initialCapacity;

        public Factory(long initialCapacity)
        {
            _initialCapacity = initialCapacity;
            for (int i = 0; i < initialCapacity; i++)
            {
                _pool.Enqueue(new ArrayCell());
            }
        }

        public void Dispose()
        {
            _pool.Clear();
            _cells.Clear();
            GC.SuppressFinalize(this);
        }

        public void Copy(Factory src, Rectangle rect)
        {
            _cells.Clear();

            foreach (var cell in src._cells)
            {
                if (!rect.Contains(cell.Key)) continue;

                if (!_pool.TryDequeue(out var newCell))
                {
                    newCell = new ArrayCell();
                }

                var newLoc = new Point(
                    cell.Key.X - rect.Left,
                    cell.Key.Y - rect.Top);

                newCell.ExtraInfo = cell.Value.ExtraInfo;

                _cells[newLoc] = newCell;
            }
        }

        private ConcurrentDictionary<Point, ArrayCell> _cells = [];

        public KeyValuePair<Point, ArrayCell>[] GetCellsWithLocation()
        {
            return [.. _cells];
        }

        public ArrayCell[] GetCells()
        {
            return [.. _cells.Values];
        }

        public Point[] GetLocations()
        {
            return [.. _cells.Keys];
        }

        public bool HasCell(ref Point p)
        {
            return _cells.ContainsKey(p);
        }

        public bool IsAlive(ref Point p)
        {
            return _cells.ContainsKey(p);
        }

        public bool TryGet(ref Point p, out ArrayCell? cell)
        {
            return _cells.TryGetValue(p, out cell);
        }

        public ArrayCell Get(ref Point p)
        {
            if (p.X < 0 || p.Y < 0) throw new ArgumentException("Row or column is less than zero");

            if (_cells.TryGetValue(p, out var cell)) return cell;

            _pool.TryDequeue(out cell);

            if (cell is null)
            {
                cell = new ArrayCell();
            }

            _cells[p] = cell;
            return cell;
        }

        public void ReturnAll()
        {
            var cells = GetCells();
            _cells.Clear();

            Task.Run(() =>
            {
                // Return all cells to the pool
                for (int i = 0; i < cells.Length; i++)
                {
                    if (_pool.Count >= MaxPoolCapcity) return; // Pool is full
                    _pool.Enqueue(cells[i]);
                }
            });
        }

        public void Return(ref Point p)
        {
            if (_cells.TryGetValue(p, out var cell))
            {
                cell.ExtraInfo = 0;
                _cells.TryRemove(p, out _);

                if (_pool.Count >= MaxPoolCapcity) return; // Pool is full
                _pool.Enqueue(cell);
            }
        }
    }
}
