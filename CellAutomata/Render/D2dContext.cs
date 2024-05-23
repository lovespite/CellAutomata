using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Device = SharpDX.Direct3D11.Device;
using Factory = SharpDX.DXGI.Factory;

namespace CellAutomata.Render;

public class D2dWindowContext : ID2dContext, IDisposable
{
    private readonly WindowRenderTarget _renderTarget;
    private int _width;
    private int _height;

    public int Width => _width;
    public int Height => _height;

    public void Dispose()
    {
        _renderTarget.Dispose();
        GC.SuppressFinalize(this);
    }

    public void Resize(int w, int h)
    {
        if (w == _width && h == _height) return;

        _renderTarget.Resize(new Size2(w, h));
        _width = w;
        _height = h;
    }

    public D2dWindowContext(int width, int height, nint hWnd)
    {
        _width = width;
        _height = height;
        var renderTargetProperties = new RenderTargetProperties
        {
            //PixelFormat = new PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied),
            //DpiX = 120,
            //DpiY = 120, 
            //Type = RenderTargetType.Hardware,
            //Usage = RenderTargetUsage.None,
            //MinLevel = SharpDX.Direct2D1.FeatureLevel.Level_DEFAULT,
        };
        var hWndRenderTargetProperties = new HwndRenderTargetProperties
        {
            Hwnd = hWnd,
            PixelSize = new Size2(width, height),
            PresentOptions = PresentOptions.Immediately,
        };

        _renderTarget = new WindowRenderTarget(new SharpDX.Direct2D1.Factory(), renderTargetProperties,
            hWndRenderTargetProperties)
        {
            AntialiasMode = AntialiasMode.PerPrimitive,
            TextAntialiasMode = TextAntialiasMode.Cleartype
        };
    }

    public RenderTarget GetRenderer()
    {
        return _renderTarget;
    }
}

public class D2dContext : IDisposable, ID2dContext
{
    public int Width { get; private set; }
    public int Height { get; private set; }

    private readonly Texture2D _backBuffer = null!;
    private readonly SwapChain _swapChain = null!;
    private readonly Factory _factory = null!;
    private readonly Device _device = null!;
    private RenderTargetView _renderView = null!;

    private RenderTarget _d2dRenderTarget = null!;

    private readonly nint _hWnd;
    private readonly SharpDX.Direct2D1.Factory _d2dFactory = new();

    public D2dContext(int width, int height, nint hWnd)
    {
        Width = width;
        Height = height;
        this._hWnd = hWnd;

        CreateRender(width, height, hWnd);
    }

    public RenderTarget GetRenderer()
    {
        return _d2dRenderTarget;
    }

    public void Resize(int width, int height)
    {
        if (Width == width && Height == height)
        {
            return;
        }

        DisposeInternal();
        CreateRender(width, height, _hWnd);
    }

    private void CreateRender(int width, int height, nint hWnd)
    {
        var swapChainDescription = new SwapChainDescription
        {
            BufferCount = 2,
            ModeDescription = new ModeDescription(width, height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
            Usage = Usage.RenderTargetOutput,
            OutputHandle = hWnd,
            SampleDescription = new SampleDescription(1, 0),
            IsWindowed = true
        };

        Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.BgraSupport, swapChainDescription,
            out Device device, out SwapChain swapChain);

        // ReSharper disable once AccessToStaticMemberViaDerivedType
        Texture2D backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);
        _renderView = new RenderTargetView(device, backBuffer);

        Surface surface = backBuffer.QueryInterface<Surface>();

        _d2dRenderTarget = new RenderTarget(_d2dFactory, surface,
            new RenderTargetProperties(new PixelFormat(Format.Unknown, AlphaMode.Premultiplied)));
    }

    public void Dispose()
    {
        DisposeInternal();

        GC.SuppressFinalize(this);
    }

    private void DisposeInternal()
    {
        _renderView.Dispose();
        _backBuffer.Dispose();
        _device.ImmediateContext.ClearState();
        _device.ImmediateContext.Flush();
        _device.Dispose();
        _device.Dispose();
        _swapChain.Dispose();
        _factory.Dispose();
    }
}