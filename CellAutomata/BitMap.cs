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

    public bool Get(ref BitPosition bPos)
    {
        if (bPos.ByteArrayIndex < 0 | bPos.ByteArrayIndex >= _bytes.Length) return false;

        byte mask = (byte)(1 << bPos.BitIndex);

        return (_bytes[bPos.ByteArrayIndex] & mask) != 0;
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

    public void Toggle(ref BitPosition bPos)
    {
        byte mask = (byte)(1 << bPos.BitIndex);

        _bytes[bPos.ByteArrayIndex] ^= mask;
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

    public bool Get(ref BitPosition bPos)
    {
        return _bytes[bPos.Index] > 0;
    }

    public void Set(ref BitPosition bPos, bool value)
    {
        _bytes[bPos.Index] = (byte)(value ? 1 : 0);
    }

    public void Toggle(ref BitPosition bPos)
    {
        _bytes[bPos.Index] ^= 1;
    }
}