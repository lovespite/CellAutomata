﻿namespace CellAutomata.Algos;

public struct SizeL
{
    public long Width;
    public long Height;

    public static readonly SizeL Empty = new(0, 0);

    public bool IsEmpty => Width == 0 && Height == 0;
    
    public override string ToString()
    {
        return $"width={Width}, height={Height}";
    }

    public long Area() => Width * Height;

    public SizeL(long width, long height)
    {
        Width = width;
        Height = height;
    }

    public static implicit operator Size(SizeL size)
    {
        return new Size((int)size.Width, (int)size.Height);
    }
}