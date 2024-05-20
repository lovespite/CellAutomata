namespace CellAutomata.Algos;

public interface I2DBitMutator
{
    byte this[int x, int y] { get; set; }
    void Clear();
    void ClearRect(ref Rectangle rect);

    bool Get(int row, int col);
    void Set(int row, int col, bool value);

    bool Get(ref Point point);
    void Set(ref Point point, bool value);
}

