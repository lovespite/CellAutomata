namespace CellAutomata
{
    public interface IByteArrayBitOperator
    {
        byte[] Bytes { get; }
        IPositionConvert Bpc { get; }

        bool Get(ref BitPosition bPos);
        void Set(ref BitPosition bPos, bool value);
        void Toggle(ref BitPosition bPos);

        IByteArrayBitOperator Clone();
    }
}