﻿using SharpDX.Direct2D1;

namespace CellAutomata
{
    public interface ID2dContext
    {
        int Height { get; }
        int Width { get; }

        RenderTarget GetRenderer();
        void Resize(int width, int height);
    }
}