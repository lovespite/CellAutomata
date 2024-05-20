using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Mathematics.Interop;

namespace CellAutomata;

public class TextRenderer : TextRendererBase
{
    private RenderTarget? _renderTarget;
    private SolidColorBrush? _defaultBrush;

    public void AssignResources(RenderTarget renderTarget, SolidColorBrush defaultBrush)
    {
        this._renderTarget = renderTarget;
        this._defaultBrush = defaultBrush;
    }

    public override Result DrawGlyphRun(object clientDrawingContext, float baselineOriginX, float baselineOriginY, MeasuringMode measuringMode, GlyphRun glyphRun, GlyphRunDescription glyphRunDescription, ComObject clientDrawingEffect)
    {
        if (_renderTarget is null) return Result.Ok;

        var renderTarget = _renderTarget;

        SolidColorBrush sb;
        if (clientDrawingEffect is not null and SolidColorBrush)
        {
            sb = (SolidColorBrush)clientDrawingEffect;
        }
        else
        {
            sb = _defaultBrush ?? new SolidColorBrush(_renderTarget, new RawColor4(1, 1, 1, 1));
        }

        try
        {
            renderTarget.DrawGlyphRun(new RawVector2(baselineOriginX, baselineOriginY), glyphRun, sb, measuringMode);
            return Result.Ok;
        }
        catch
        {
            return Result.Fail;
        }
    }
}