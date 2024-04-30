using System.Collections.Concurrent;
using System.Diagnostics;

namespace CellAutomata;

public class ArrayCell
{
    // public long ExtraInfo { get; set; }

    private ArrayCell()
    {
    }

    public class Factory : IDisposable
    {
        public const int MaxPoolCapcity = 10000;
        private readonly ConcurrentQueue<ArrayCell> _pool;
        private readonly ConcurrentDictionary<Point, ArrayCell> _cells = [];
        private readonly long _initialCapacity;

        public Factory(long initialCapacity)
        {
            _initialCapacity = initialCapacity;
            _pool = new ConcurrentQueue<ArrayCell>();
            Task.Run(() =>
            {
                for (int i = 0; i < _initialCapacity; i++)
                {
                    _pool.Enqueue(new ArrayCell());
                }
            });
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

            if (rect.IsEmpty)
            {
                // Copy all cells

                foreach (var cell in src._cells)
                {
                    if (!_pool.TryDequeue(out var newCell))
                    {
                        newCell = new ArrayCell();
                    }

                    // newCell.ExtraInfo = cell.Value.ExtraInfo;

                    _cells[cell.Key] = newCell;
                }

                return;
            }

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

                // newCell.ExtraInfo = cell.Value.ExtraInfo;

                _cells[newLoc] = newCell;
            }
        }

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
            if (_cells.TryGetValue(p, out var cell)) return cell;

            _pool.TryDequeue(out cell);

            cell ??= new ArrayCell();

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
                // cell.ExtraInfo = 0;
                _cells.TryRemove(p, out _);

                if (_pool.Count >= MaxPoolCapcity) return; // Pool is full
                _pool.Enqueue(cell);
            }
        }
    }
}

public class ArrayCell2
{
    // public long ExtraInfo { get; set; }

    private ArrayCell2()
    {
    }

    public class Factory : IDisposable
    {
        public const int MaxPoolCapcity = 10000;
        private readonly ConcurrentDictionary<Point, bool> _cells = [];
        private readonly long _initialCapacity;

        public Factory(long initialCapacity)
        {
            _initialCapacity = initialCapacity;
        }

        public void Dispose()
        {
            _cells.Clear();
            GC.SuppressFinalize(this);
        }

        public void Copy(Factory src, Rectangle rect)
        {
            _cells.Clear();

            if (rect.IsEmpty)
            {
                // Copy all cells

                foreach (var cell in src._cells)
                {
                    _cells[cell.Key] = true;
                }

                return;
            }

            foreach (var cell in src._cells)
            {
                if (!rect.Contains(cell.Key)) continue;


                var newLoc = new Point(
                    cell.Key.X - rect.Left,
                    cell.Key.Y - rect.Top);

                // newCell.ExtraInfo = cell.Value.ExtraInfo;

                _cells[newLoc] = true;
            }
        }

        public KeyValuePair<Point, bool>[] GetCellsWithLocation()
        {
            return [.. _cells];
        }

        public bool[] GetCells()
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

        public bool Get(ref Point p)
        {
            if (_cells.TryGetValue(p, out var cell)) return cell;


            return _cells[p] = true;
        }

        public void ReturnAll()
        {
            _cells.Clear();
        }

        public void Return(ref Point p)
        {
            // cell.ExtraInfo = 0;
            _cells.TryRemove(p, out _);
        }
    }
}