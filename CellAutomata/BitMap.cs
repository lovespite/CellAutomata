using System.Buffers;

namespace CellAutomata;

public class BitMap : IByteArrayBitOperator
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

    public IByteArrayBitOperator Clone()
    {
        var clone = new BitMap(_bpc.Height, _bpc.Width);
        Buffer.BlockCopy(_bytes, 0, clone.Bytes, 0, _bytes.Length);
        return clone;
    }

    public byte[] Bytes => _bytes;

    public bool Get(ref BitPosition bPos)
    {
        byte mask = (byte)(1 << bPos.BitIndex);

        return (_bytes[bPos.ByteArrayIndex] & mask) != 0;
    }

    public void Set(ref BitPosition bPos, bool value)
    {
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

public class FastBitMap : IByteArrayBitOperator
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

    public IByteArrayBitOperator Clone()
    {
        var clone = new FastBitMap(_bpc.Height, _bpc.Width);
        Buffer.BlockCopy(_bytes, 0, clone.Bytes, 0, _bytes.Length);
        return clone;
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