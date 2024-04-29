using System.Buffers;
using System.Runtime.InteropServices;

namespace CellAutomata;

public class BitMap : IBitMap
{
    private readonly byte[] _bytes;
    private readonly IPositionConvert _bpc;

    public BitMap(int rows, int columns)
    {
        var byteLength = (int)Math.Ceiling((double)(rows * columns) / 8);
        _bytes = ArrayPool<byte>.Shared.Rent(byteLength);
        _bpc = new BitPositionConvert(columns, rows);
    }

    public void Dispose()
    {
        ArrayPool<byte>.Shared.Return(_bytes);
        GC.SuppressFinalize(this);
    }

    public void Clear()
    {
        Array.Clear(_bytes, 0, _bytes.Length);
    }

    public IPositionConvert Bpc => _bpc;

    public IBitMap CreateSnapshot()
    {
        var clone = new BitMap(_bpc.Height, _bpc.Width);

        unsafe
        {
            fixed (byte* src = _bytes)
            fixed (byte* dst = clone.Bytes)
            {
                Buffer.MemoryCopy(src, dst, _bytes.Length, _bytes.Length);
            }
        }

        return clone;
    }

    public IBitMap CreateRegionSnapshot(Rectangle rect)
    {
        if (rect.IsEmpty) throw new ArgumentException("Rectangle is empty", nameof(rect));
        var clone = new BitMap(rect.Height, rect.Width);

        for (int row = 0; row < rect.Height; row++)
        {
            for (int col = 0; col < rect.Width; col++)
            {
                var srcBPos = _bpc.Transform(rect.Y + row, rect.X + col);
                var dstBPos = clone.Bpc.Transform(row, col);

                clone.Set(ref dstBPos, Get(ref srcBPos));
            }
        }

        return clone;
    }

    public void BlockCopy(IBitMap source, Rectangle sourceRect, Rectangle destRect, CopyMode mode = CopyMode.Overwrite)
    {
        if (sourceRect.IsEmpty) throw new ArgumentException("Source rectangle is empty", nameof(sourceRect));
        if (destRect.IsEmpty) throw new ArgumentException("Destination rectangle is empty", nameof(destRect));

        for (int row = 0; row < sourceRect.Height; row++)
        {
            for (int col = 0; col < sourceRect.Width; col++)
            {
                var srcBPos = source.Bpc.Transform(sourceRect.Y + row, sourceRect.X + col);
                var dstBPos = _bpc.Transform(destRect.Y + row, destRect.X + col);

                var dstVal = Get(ref dstBPos);
                var srcVal = source.Get(ref srcBPos);
                switch (mode)
                {
                    case CopyMode.Overwrite:
                        Set(ref dstBPos, srcVal);
                        break;
                    case CopyMode.Or:
                        Set(ref dstBPos, dstVal | srcVal);
                        break;
                    case CopyMode.And:
                        Set(ref dstBPos, dstVal & srcVal);
                        break;
                    case CopyMode.Xor:
                        Set(ref dstBPos, dstVal ^ srcVal);
                        break;
                }
            }
        }
    }

    public void BlockCopy(IBitMap source, Point destLocation, CopyMode mode = CopyMode.Overwrite)
    {
        var sourceRect = new Rectangle(0, 0, source.Bpc.Width, source.Bpc.Height);
        var destRect = new Rectangle(destLocation, sourceRect.Size);

        BlockCopy(source, sourceRect, destRect, mode);
    }

    public byte[] Bytes => _bytes;

    public bool Get(int row, int col)
    {
        var bPos = _bpc.Transform(row, col);
        return Get(ref bPos);
    }
    public bool Get(ref BitPosition bPos)
    {
        if (bPos.ByteArrayIndex < 0 | bPos.ByteArrayIndex >= _bytes.Length) return false;

        byte mask = (byte)(1 << bPos.BitIndex);

        return (_bytes[bPos.ByteArrayIndex] & mask) != 0;
    }
    public bool Get(ref Point point)
    {
        var bPos = _bpc.Transform(point.Y, point.X);
        return Get(ref bPos);
    }

    public void Set(int row, int col, bool value)
    {
        var bPos = _bpc.Transform(row, col);
        Set(ref bPos, value);
    }
    public void Set(ref BitPosition bPos, bool value)
    {
        if (bPos.ByteArrayIndex < 0 | bPos.ByteArrayIndex >= _bytes.Length) return;
        byte mask = (byte)(1 << bPos.BitIndex);

        if (value)
        {
            _bytes[bPos.ByteArrayIndex] |= mask;
        }
        else
        {
            _bytes[bPos.ByteArrayIndex] &= (byte)~mask;
        }
    }
    public void Set(ref Point point, bool value)
    {
        var bPos = _bpc.Transform(point.Y, point.X);
        Set(ref bPos, value);
    }

    public void Toggle(ref BitPosition bPos)
    {
        byte mask = (byte)(1 << bPos.BitIndex);

        _bytes[bPos.ByteArrayIndex] ^= mask;
    }

    public Point[] QueryRegion(bool val, Rectangle rect)
    {
        var list = new List<Point>(Math.Min(5000, rect.Width * rect.Height));

        for (int row = rect.Top; row < rect.Bottom; row++)
        {
            for (int col = rect.Left; col < rect.Right; col++)
            {
                var bPos = _bpc.Transform(row, col);
                if (Get(ref bPos) == val)
                {
                    list.Add(bPos.Location);
                }
            }
        }

        return list.ToArray();
    }

    public long QueryRegionCount(bool val, Rectangle rect)
    {
        long count = 0;

        for (int row = rect.Top; row < rect.Bottom; row++)
        {
            for (int col = rect.Left; col < rect.Right; col++)
            {
                var bPos = _bpc.Transform(row, col);
                if (Get(ref bPos) == val)
                {
                    count++;
                }
            }
        }

        return count;
    }
}

public class FastBitMap : IBitMap
{
    private readonly byte[] _bytes;
    private readonly IPositionConvert _bpc;
    public FastBitMap(int rows, int columns)
    {
        var byteLength = (rows * columns);
        _bytes = ArrayPool<byte>.Shared.Rent(byteLength);

        _bpc = new FastBitPositionConvert(columns, rows);
    }

    public void Clear()
    {
        Array.Clear(_bytes, 0, _bytes.Length);
    }

    public Point[] QueryRegion(bool val, Rectangle rect)
    {
        var list = new List<Point>(Math.Min(5000, rect.Width * rect.Height));

        for (int row = rect.Top; row < rect.Bottom; row++)
        {
            for (int col = rect.Left; col < rect.Right; col++)
            {
                var bPos = _bpc.Transform(row, col);
                if (Get(ref bPos) == val)
                {
                    list.Add(bPos.Location);
                }
            }
        }

        return list.ToArray();
    }

    public long QueryRegionCount(bool val, Rectangle rect)
    {
        long count = 0;

        for (int row = rect.Top; row < rect.Bottom; row++)
        {
            for (int col = rect.Left; col < rect.Right; col++)
            {
                var bPos = _bpc.Transform(row, col);
                if (Get(ref bPos) == val)
                {
                    count++;
                }
            }
        }

        return count;
    }

    public void Dispose()
    {
        ArrayPool<byte>.Shared.Return(_bytes);
        GC.SuppressFinalize(this);
    }

    public IPositionConvert Bpc => _bpc;

    public IBitMap CreateSnapshot()
    {
        var clone = new FastBitMap(_bpc.Height, _bpc.Width);

        unsafe
        {
            fixed (byte* src = _bytes)
            fixed (byte* dst = clone.Bytes)
            {
                Buffer.MemoryCopy(src, dst, _bytes.Length, _bytes.Length);
            }
        }

        return clone;
    }

    public IBitMap CreateRegionSnapshot(Rectangle rect)
    {
        if (rect.IsEmpty) throw new ArgumentException("Rectangle is empty", nameof(rect));

        var clone = new FastBitMap(rect.Height, rect.Width);

        unsafe
        {
            fixed (byte* src = _bytes)
            fixed (byte* dst = clone.Bytes)
            {
                for (int row = 0; row < rect.Height; row++)
                {
                    var srcBPos = _bpc.Transform(rect.Y + row, rect.X);
                    var dstBPos = clone.Bpc.Transform(row, 0);
                    Buffer.MemoryCopy(src + srcBPos.Index, dst + dstBPos.Index, rect.Width, rect.Width);
                }
            }
        }

        return clone;
    }

    public void BlockCopy(IBitMap source, Rectangle sourceRect, Rectangle destRect, CopyMode mode = CopyMode.Overwrite)
    {
        if (sourceRect.IsEmpty) throw new ArgumentException("Source rectangle is empty", nameof(sourceRect));
        if (destRect.IsEmpty) throw new ArgumentException("Destination rectangle is empty", nameof(destRect));

        unsafe
        {
            fixed (byte* src = source.Bytes)
            fixed (byte* dst = _bytes)
            {
                for (int row = 0; row < sourceRect.Height; row++)
                {
                    var srcBPos = source.Bpc.Transform(sourceRect.Y + row, sourceRect.X);
                    var dstBPos = _bpc.Transform(destRect.Y + row, destRect.X);

                    switch (mode)
                    {
                        case CopyMode.Overwrite:
                            Buffer.MemoryCopy(src + srcBPos.Index, dst + dstBPos.Index, sourceRect.Width, sourceRect.Width);
                            break;
                        case CopyMode.Or:
                        case CopyMode.And:
                        case CopyMode.Xor:
                        default:
                            throw new NotImplementedException();

                    }

                }
            }
        }
    }

    public void BlockCopy(IBitMap source, Point destLocation, CopyMode mode = CopyMode.Overwrite)
    {
        var sourceRect = new Rectangle(0, 0, source.Bpc.Width, source.Bpc.Height);
        var destRect = new Rectangle(destLocation, sourceRect.Size);

        BlockCopy(source, sourceRect, destRect, mode);
    }

    public byte[] Bytes => _bytes;

    public bool Get(int row, int col)
    {
        var bPos = _bpc.Transform(row, col);
        return Get(ref bPos);
    }
    public bool Get(ref BitPosition bPos)
    {
        return _bytes[bPos.Index] > 0;
    }
    public bool Get(ref Point point)
    {
        var bPos = _bpc.Transform(point.Y, point.X);
        return Get(ref bPos);
    }

    public void Set(int row, int col, bool value)
    {
        var bPos = _bpc.Transform(row, col);
        Set(ref bPos, value);
    }
    public void Set(ref BitPosition bPos, bool value)
    {
        _bytes[bPos.Index] = (byte)(value ? 1 : 0);
    }
    public void Set(ref Point point, bool value)
    {
        var bPos = _bpc.Transform(point.Y, point.X);
        Set(ref bPos, value);
    }

    public void Toggle(ref BitPosition bPos)
    {
        _bytes[bPos.Index] ^= 1;
    }
}

public class ArrayBitMap : IBitMap
{
    public byte[] Bytes { get; } = null!;

    public IPositionConvert Bpc { get; } = null!;

    private readonly ArrayCell.Factory _cellFactory;

    public ArrayBitMap(int width, int height)
    {
        Bytes = [];
        Bpc = new ArrayPositionConvert(width, height);
        _cellFactory = new ArrayCell.Factory(1000);
    }

    public void BlockCopy(IBitMap source, Rectangle sourceRect, Rectangle destRect, CopyMode mode = CopyMode.Overwrite)
    {
        if (sourceRect.IsEmpty) throw new ArgumentException("Source rectangle is empty", nameof(sourceRect));
        if (destRect.IsEmpty) throw new ArgumentException("Destination rectangle is empty", nameof(destRect));

        for (int row = 0; row < sourceRect.Height; row++)
        {
            for (int col = 0; col < sourceRect.Width; col++)
            {
                var srcBPos = source.Bpc.Transform(sourceRect.Y + row, sourceRect.X + col);
                var dstBPos = Bpc.Transform(destRect.Y + row, destRect.X + col);

                var dstVal = Get(ref dstBPos);
                var srcVal = source.Get(ref srcBPos);
                switch (mode)
                {
                    case CopyMode.Overwrite:
                        Set(ref dstBPos, srcVal);
                        break;
                    case CopyMode.Or:
                        Set(ref dstBPos, dstVal | srcVal);
                        break;
                    case CopyMode.And:
                        Set(ref dstBPos, dstVal & srcVal);
                        break;
                    case CopyMode.Xor:
                        Set(ref dstBPos, dstVal ^ srcVal);
                        break;
                }
            }
        }

    }

    public void BlockCopy(IBitMap source, Point destLocation, CopyMode mode = CopyMode.Overwrite)
    {
        BlockCopy(source, new Rectangle(0, 0, source.Bpc.Width, source.Bpc.Height), new Rectangle(destLocation, new Size(source.Bpc.Width, source.Bpc.Height)), mode);
    }

    public void Clear()
    {
        _cellFactory.ReturnAll();
    }

    public IBitMap CreateRegionSnapshot(Rectangle rect)
    {
        var snapshot = new ArrayBitMap(rect.Width, rect.Height);

        snapshot._cellFactory.Copy(_cellFactory, rect);

        return snapshot;
    }

    public IBitMap CreateSnapshot()
    {
        return CreateRegionSnapshot(new Rectangle(0, 0, Bpc.Width, Bpc.Height));
    }

    public void Dispose()
    {
        _cellFactory.Dispose();
        GC.SuppressFinalize(this);
    }

    public bool Get(int row, int col)
    {
        var p = new Point(col, row);
        return _cellFactory.IsAlive(ref p);
    }
    public bool Get(ref BitPosition bPos)
    {
        return _cellFactory.IsAlive(ref bPos.Location);
    }
    public bool Get(ref Point point)
    {
        return _cellFactory.IsAlive(ref point);
    }

    public void Set(int row, int col, bool value)
    {
        var p = new Point(col, row);
        Set(ref p, value);
    }
    public void Set(ref BitPosition bPos, bool value)
    {
        Set(ref bPos.Location, value);
    }
    public void Set(ref Point point, bool value)
    {
        var cell = _cellFactory.Get(ref point);
        if (value)
        {
            cell.ExtraInfo++;
        }
        else
        {
            --cell.ExtraInfo;
            if (cell.ExtraInfo <= 0)
            {
                _cellFactory.Return(ref point);
            }
        }
    }

    public Point[] QueryRegion(bool val, Rectangle rect)
    {
        var cells = _cellFactory.GetLocations();

        var list = new List<Point>(Math.Min(5000, rect.Width * rect.Height));

        for (long i = 0; i < cells.Length; ++i)
            if (rect.Contains(cells[i]))
            {
                list.Add(cells[i]);
            }

        return [.. list];
    }

    public long QueryRegionCount(bool val, Rectangle rect)
    {
        var cells = _cellFactory.GetLocations();
        if (rect.Left == 0 && rect.Top == 0 && rect.Right == Bpc.Width && rect.Bottom == Bpc.Height)
        {
            // if the rectangle is the whole bitmap, return the count of all cells
            return cells.Length;
        }

        long count = 0;

        for (long i = 0; i < cells.Length; ++i)
            if (rect.Contains(cells[i]))
            {
                count++;
            }

        return count;
    }
}
