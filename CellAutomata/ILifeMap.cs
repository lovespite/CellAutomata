using System.Diagnostics;

namespace CellAutomata;

public enum CopyMode
{
    Overwrite,
    Or,
    And,
    Xor
}

public interface ILifeMap : IDisposable
{
    int ThreadCount { get; set; }
    byte[] Bytes { get; }

    long MsGenerationTime { get; }
    long MsMemoryCopyTime { get; }
    long MsCPUTime { get; }
    long Generation { get; }
    long Population { get; }

    bool Get(int row, int col);
    void Set(int row, int col, bool value);

    bool Get(ref Point point);
    void Set(ref Point point, bool value);

    void Clear();

    ILifeMap CreateSnapshot();

    ILifeMap CreateRegionSnapshot(Rectangle rect);

    void BlockCopy(ILifeMap source, Size srcSize, Point dstLocation, CopyMode mode = CopyMode.Overwrite);

    Point[] QueryRegion(bool val, Rectangle rect);
    long QueryRegionCount(bool val, Rectangle rect);

    Point[] GetLocations(bool val);

    void NextGeneration();
}

public class HashLifeMap : ILifeMap
{
    private QuadTreeNode _root;
    private readonly Dictionary<QuadTreeNode, QuadTreeNode> _memoizationTable;

    public HashLifeMap()
    {
        _root = new QuadTreeNode(0, 1);
        _memoizationTable = [];
    }
    public QuadTreeNode Root => _root;

    public int ThreadCount { get; set; }

    public byte[] Bytes => [];

    public long MsGenerationTime { get; private set; }
    public long MsMemoryCopyTime { get; private set; }
    public long MsCPUTime { get; private set; }
    public long Generation { get; private set; }
    public long Population => _root.AliveCount;

    public bool Get(int row, int col)
    {
        return _root.Get(row, col);
    }

    public void Set(int row, int col, bool value)
    {
        _root.Set(row, col, value);
    }

    public bool Get(ref Point point)
    {
        return Get(point.Y, point.X);
    }

    public void Set(ref Point point, bool value)
    {
        Set(point.Y, point.X, value);
    }

    public void Clear()
    {
        _root.Clear();
    }

    public ILifeMap CreateSnapshot()
    {
        var snapshot = new HashLifeMap();
        snapshot._root = _root.Clone();
        return snapshot;
    }

    public ILifeMap CreateRegionSnapshot(Rectangle rect)
    {
        var snapshot = new HashLifeMap();

        for (int row = rect.Top; row < rect.Bottom; row++)
        {
            for (int col = rect.Left; col < rect.Right; col++)
            {
                snapshot.Set(row, col, Get(row, col));
            }
        }

        return snapshot;
    }

    public void BlockCopy(ILifeMap source, Size srcSize, Point dstLocation, CopyMode mode = CopyMode.Overwrite)
    {
        for (int row = 0; row < srcSize.Height; row++)
        {
            for (int col = 0; col < srcSize.Width; col++)
            {
                var srcPoint = new Point(col, row);
                var dstPoint = new Point(col + dstLocation.X, row + dstLocation.Y);

                bool srcValue = source.Get(ref srcPoint);
                bool dstValue = Get(ref dstPoint);

                switch (mode)
                {
                    case CopyMode.Overwrite:
                        Set(ref dstPoint, srcValue);
                        break;
                    case CopyMode.Or:
                        Set(ref dstPoint, srcValue || dstValue);
                        break;
                    case CopyMode.And:
                        Set(ref dstPoint, srcValue && dstValue);
                        break;
                    case CopyMode.Xor:
                        Set(ref dstPoint, srcValue ^ dstValue);
                        break;
                }
            }
        }
    }

    public Point[] QueryRegion(bool val, Rectangle rect)
    {
        List<Point> points = new List<Point>();
        for (int row = rect.Top; row < rect.Bottom; row++)
        {
            for (int col = rect.Left; col < rect.Right; col++)
            {
                if (Get(row, col) == val)
                {
                    points.Add(new Point(col, row));
                }
            }
        }
        return [.. points];
    }

    public long QueryRegionCount(bool val, Rectangle rect)
    {
        long count = 0;
        for (int row = rect.Top; row < rect.Bottom; row++)
        {
            for (int col = rect.Left; col < rect.Right; col++)
            {
                if (Get(row, col) == val)
                {
                    count++;
                }
            }
        }
        return count;
    }

    public Point[] GetLocations(bool val)
    {
        return QueryRegion(val, new Rectangle(0, 0, 1 << 16, 1 << 16));
    }

    public void NextGeneration()
    {
        var sw = Stopwatch.StartNew();
        _memoizationTable.Clear();
        _root = _root.NextGeneration(_memoizationTable);
        sw.Stop();

        MsMemoryCopyTime = 0;
        MsGenerationTime = sw.ElapsedMilliseconds;
        MsCPUTime = MsMemoryCopyTime + MsGenerationTime;

        Generation++;
    }

    public void Dispose()
    {
        _memoizationTable.Clear();
    }
}


public class QuadTreeNode
{
    public QuadTreeNode?[] Children { get; private set; }
    public int Level { get; private set; }
    public long AliveCount { get; private set; }
    public int Size { get; private set; }

    public QuadTreeNode(int level, int size)
    {
        Level = level;
        Size = size;
        Children = new QuadTreeNode[4]; // Assuming null for empty children initially
        AliveCount = 0;
    }

    public bool Get(int row, int col)
    {
        if (Level == 0)
        {
            return false;
        }

        int halfSize = Size / 2;
        int index = (row >= halfSize ? 2 : 0) + (col >= halfSize ? 1 : 0);

        return Children[index]?.Get(row % halfSize, col % halfSize) ?? false;
    }

    private bool isDirty; // 新增标记，用于延迟更新

    public QuadTreeNode Set(int row, int col, bool value)
    {
        if (Level == 0)
        {
            if (this.AliveCount == (value ? 1 : 0)) return this; // 如果状态未改变，则返回当前节点
            return new QuadTreeNode(0, 1) { AliveCount = value ? 1 : 0 };
        }

        int halfSize = Size / 2;
        int index = (row >= halfSize ? 2 : 0) + (col >= halfSize ? 1 : 0);

        QuadTreeNode updatedNode = (Children[index] ?? new QuadTreeNode(Level - 1, halfSize))
            .Set(row % halfSize, col % halfSize, value);

        if (Children[index] == updatedNode) return this; // 如果子节点没有变化，返回当前节点

        // 克隆当前节点并更新相应的子节点
        QuadTreeNode result = this.Clone();
        result.Children[index] = updatedNode;

        // 重新计算活细胞数
        result.AliveCount = 0;
        for (int i = 0; i < 4; i++)
        {
            result.AliveCount += result.Children[i]?.AliveCount ?? 0;
        }

        return result;
    }

    public QuadTreeNode Clone()
    {
        var result = new QuadTreeNode(Level, Size)
        {
            AliveCount = AliveCount
        };
        for (int i = 0; i < 4; i++)
        {
            if (Children[i] != null)
            {
                result.Children[i] = Children[i]?.Clone();
            }
        }
        return result;
    }

    public void Clear()
    {
        for (int i = 0; i < 4; i++)
        {
            if (Children[i] != null)
            {
                Children[i]?.Clear();
                Children[i] = null;
            }
        }
        AliveCount = 0;
    }

    public QuadTreeNode NextGeneration(Dictionary<QuadTreeNode, QuadTreeNode> memoizationTable)
    {
        // 检查是否已经计算过此节点的下一代
        if (memoizationTable.TryGetValue(this, out QuadTreeNode? cachedResult))
        {
            return cachedResult;
        }

        QuadTreeNode result;
        if (Level == 0)
        {
            // 对于叶节点，应用游戏规则计算下一状态
            result = ApplyRules();
        }
        else
        {
            // 递归计算子节点的下一代
            QuadTreeNode?[] newChildren = new QuadTreeNode[4];
            bool childrenChanged = false;
            for (int i = 0; i < 4; i++)
            {
                QuadTreeNode? originalChild = Children[i];
                QuadTreeNode? nextChild = originalChild?.NextGeneration(memoizationTable);
                newChildren[i] = nextChild;
                if (nextChild != originalChild)
                {
                    childrenChanged = true;
                }
            }

            if (!childrenChanged)
            {
                result = this;  // 如果所有子节点都未更改，可以直接返回当前节点
            }
            else
            {
                result = new QuadTreeNode(Level, Size) { Children = newChildren };
                result.UpdateAliveCount();  // 更新存活细胞计数
            }
        }

        memoizationTable[this] = result;  // 存储计算结果以供未来使用
        return result;
    }

    private QuadTreeNode ApplyRules()
    {
        // 根据生命游戏的规则计算当前节点的下一状态
        return new QuadTreeNode(0, 1)
        {
            AliveCount = GetNextGenerationAliveCount()
        };
    }

    private long GetNextGenerationAliveCount()
    {
        long count = 0;
        for (int i = 0; i < 4; i++)
        {
            count += Children[i]?.AliveCount ?? 0;
        }

        if (count == 3 || count == 2 && Children[0]?.AliveCount == 1)
        {
            return 1;
        }
        return 0;
    }

    private void UpdateAliveCount()
    {
        AliveCount = 0;
        foreach (var child in Children)
        {
            if (child != null)
            {
                AliveCount += child.AliveCount;
            }
        }
    }


    public QuadTreeNode? Merge(QuadTreeNode? other)
    {
        if (other == null)
        {
            return null;
        }

        if (Level == 0)
        {
            return other;
        }

        QuadTreeNode result = new QuadTreeNode(Level, Size);
        result.AliveCount = 0;
        for (int i = 0; i < 4; i++)
        {
            if (Children[i] == null)
            {
                result.Children[i] = other.Children[i];
            }
            else if (other.Children[i] == null)
            {
                result.Children[i] = Children[i];
            }
            else
            {
                result.Children[i] = Children[i]?.Merge(other.Children[i]);
            }
            result.AliveCount += result.Children[i]?.AliveCount ?? 0;
        }
        return result;
    }
}
