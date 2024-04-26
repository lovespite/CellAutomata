namespace CellAutomata;

public class BitMap : IByteArrayBitOperator
{
    private readonly byte[] _bytes;
    private readonly IPositionConvert _bpc;

    public BitMap(byte[] bytes, int rows, int columns)
    {
        _bytes = bytes;
        _bpc = new BitPositionConvert(columns, rows);
    }

    public IPositionConvert Bpc => _bpc;

    public IByteArrayBitOperator Clone()
    {
        var bytes = new byte[_bytes.Length];
        Buffer.BlockCopy(_bytes, 0, bytes, 0, _bytes.Length);
        return new BitMap(bytes, _bpc.Height, _bpc.Width);
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
    public FastBitMap(byte[] bytes, int rows, int columns)
    {
        _bytes = bytes;
        _bpc = new FastBitPositionConvert(columns, rows);
    }

    public IPositionConvert Bpc => _bpc;

    public IByteArrayBitOperator Clone()
    {
        var bytes = new byte[_bytes.Length];
        Buffer.BlockCopy(_bytes, 0, bytes, 0, _bytes.Length);
        return new FastBitMap(bytes, _bpc.Height, _bpc.Width);
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