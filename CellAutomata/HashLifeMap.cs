using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace CellAutomata;

public partial class HashLifeMap : ILifeMap
{
    const string HashLifeLib = "Hashlifelib/hashlife.dll";

    [LibraryImport(HashLifeLib, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial void CreateNewUniverse(string rule);

    [LibraryImport(HashLifeLib)]
    internal static partial void DestroyUniverse();

    [LibraryImport(HashLifeLib)]
    internal static partial void SetCell(int x, int y, [MarshalAs(UnmanagedType.Bool)] bool value);

    [LibraryImport(HashLifeLib)]
    internal static partial int GetCell(int x, int y);

    [LibraryImport(HashLifeLib)]
    internal static partial void NextStep(ref ulong pop);

    [LibraryImport(HashLifeLib)]
    internal static partial void SetThreadCount(int count);

    [LibraryImport(HashLifeLib)]
    internal static partial void GetPopulation(ref ulong pop);

    [LibraryImport(HashLifeLib)]
    internal static partial void GetRegion(int x, int y, int w, int h, [In, Out] byte[] buffer, int bufferSize);

    [LibraryImport(HashLifeLib)]
    internal static partial void SetRegion(int x, int y, int w, int h, [In] byte[] buffer, int bufferSize);

    [LibraryImport(HashLifeLib)]
    internal static partial void Version([In, Out] byte[] buffer, int bufferSize);


    private static readonly string _version;
    static HashLifeMap()
    {
        var buffer = new byte[256];
        Version(buffer, buffer.Length);
        _version = Encoding.UTF8.GetString(buffer).TrimEnd('\0');
        Debug.WriteLine($"Hashlifelib Version: {_version}");
    }

    private readonly string Rule;
    public HashLifeMap(string rule = "B3/S23")
    {
        Rule = rule;
        CreateNewUniverse(rule);
    }

    ~HashLifeMap()
    {
        DestroyUniverse();
    }


    public int ThreadCount { get; set; }

    public byte[] Bytes => [];

    private ulong _population = 0;

    public long MsGenerationTime { get; private set; }
    public long MsMemoryCopyTime { get; private set; }
    public long MsCPUTime => MsGenerationTime;
    public long Generation { get; private set; }
    public long Population
    {
        get
        {
            if (_isPopulationDirty)
            {
                GetPopulation(ref _population);
                _isPopulationDirty = false;
            }

            return (long)_population;
        }
    }

    private bool _isPopulationDirty = true;

    public bool Get(int row, int col)
    {
        return GetCell(col, row) != 0;
    }

    public void Set(int row, int col, bool value)
    {
        SetCell(col, row, value);
        _isPopulationDirty = true;
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
        DestroyUniverse();
        CreateNewUniverse(Rule);
        _population = 0;
        Generation = 0;
    }

    public ILifeMap CreateSnapshot()
    {
        throw new NotSupportedException();
    }


    public ILifeMap CreateRegionSnapshot(Rectangle rect)
    {
        var snapshot = new Array2LifeMap();

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
        if (!val) throw new NotSupportedException();

        var bufferLen = rect.Width * rect.Height / 8;

        if (bufferLen == 0)
        {
            return Array.Empty<Point>();
        }

        List<Point> points = [];
        int byteIndex = 0;
        int bitIndex = 0;

        var buffer = ArrayPool<byte>.Shared.Rent(bufferLen);
        try
        {
            GetRegion(rect.Left, rect.Top, rect.Width, rect.Height, buffer, buffer.Length);

            for (int row = 0; row < rect.Height; row++)
            {
                for (int col = 0; col < rect.Width; col++)
                {
                    if (val == ((buffer[byteIndex] & (1 << bitIndex)) != 0))
                    {
                        points.Add(new Point(col + rect.Left, row + rect.Top));
                    }

                    if (++bitIndex == 8)
                    {
                        bitIndex = 0;
                        ++byteIndex;
                    }
                }
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
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
        throw new NotSupportedException();
    }

    private readonly Stopwatch _sw = new();

    public void NextGeneration()
    {
        _sw.Restart();
        NextStep(ref _population);
        _sw.Stop();
        MsGenerationTime = _sw.ElapsedMilliseconds;
        _isPopulationDirty = false;
        Generation++;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

