#include "pch.h"
#include "dc3drender.h"

void fatal(const char* msg, HRESULT hr, bool exit = true) {
    auto message = std::string(msg) + " " + std::to_string(hr);
    MessageBoxA(NULL, message.c_str(), "Error", MB_OK);

    if (exit) {
        TerminateProcess(GetCurrentProcess(), 0);
    }
}

HRESULT CompileShaderFromFile(
    const wchar_t* file,
    const char* entry,
    const char* model,
    ID3DBlob** ppBlobOut
) {
    // 从文件编译着色器

    DWORD dwShaderFlags = D3DCOMPILE_ENABLE_STRICTNESS;
#if defined(DEBUG) || defined(_DEBUG)
    dwShaderFlags |= D3DCOMPILE_DEBUG;
#endif

    ID3DBlob* pErrorBlob = nullptr;
    HRESULT hr = D3DCompileFromFile(
        file,
        nullptr,
        D3D_COMPILE_STANDARD_FILE_INCLUDE,
        entry,
        model,
        dwShaderFlags,
        0,
        ppBlobOut,
        &pErrorBlob
    );

    if (FAILED(hr)) {
        if (pErrorBlob) {
            OutputDebugStringA((char*)pErrorBlob->GetBufferPointer());
            pErrorBlob->Release();
        }
        return hr;
    }

    if (pErrorBlob) pErrorBlob->Release();

    return S_OK;
}

void dc3drender::CleanupDirect3D()
{
    if (g_pVertexBuffer) {
        g_pVertexBuffer->Release();
        g_pVertexBuffer = nullptr;
    }

    if (g_pRenderTargetView) {
        g_pRenderTargetView->Release();
        g_pRenderTargetView = nullptr;
    }

    if (g_pSwapChain) {
        g_pSwapChain->Release();
        g_pSwapChain = nullptr;
    }

    if (g_pImmediateContext) {
        g_pImmediateContext->Release();
        g_pImmediateContext = nullptr;
    }

    if (g_pDevice) {
        g_pDevice->Release();
        g_pDevice = nullptr;
    }
}

int dc3drender::LoadShaders() {
    HRESULT hr = S_OK;

    // 编译顶点着色器
    ID3DBlob* pVSBlob = nullptr;
    hr = D3DCompileFromFile(L"shader.fx", nullptr, nullptr, "VS", "vs_4_0", 0, 0, &pVSBlob, nullptr);
    if (FAILED(hr)) {
        fatal("VS: The FX file cannot be compiled. Please run this executable from the directory that contains the FX file.", hr);
        return hr;
    }

    // 创建顶点着色器
    hr = g_pDevice->CreateVertexShader(pVSBlob->GetBufferPointer(), pVSBlob->GetBufferSize(), nullptr, &g_pVertexShader);
    if (FAILED(hr)) {
        pVSBlob->Release();
        fatal("CreateVertexShader failed", hr);
        return hr;
    }

    // 定义输入布局
    D3D11_INPUT_ELEMENT_DESC layout[] = {
        { "POSITION", 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 0, D3D11_INPUT_PER_VERTEX_DATA, 0 },
        { "COLOR", 0, DXGI_FORMAT_R32G32B32A32_FLOAT, 0, 12, D3D11_INPUT_PER_VERTEX_DATA, 0 },
    };
    UINT numElements = ARRAYSIZE(layout);

    // 创建输入布局
    hr = g_pDevice->CreateInputLayout(layout, numElements, pVSBlob->GetBufferPointer(), pVSBlob->GetBufferSize(), &g_pVertexLayout);
    pVSBlob->Release();
    if (FAILED(hr)) {
        fatal("CreateInputLayout failed", hr);
        return hr;
    }

    // 设置输入布局
    g_pImmediateContext->IASetInputLayout(g_pVertexLayout);

    // 编译像素着色器
    ID3DBlob* pPSBlob = nullptr;
    hr = D3DCompileFromFile(L"shader.fx", nullptr, nullptr, "PS", "ps_4_0", 0, 0, &pPSBlob, nullptr);
    if (FAILED(hr)) {
        fatal("PS: The FX file cannot be compiled. Please run this executable from the directory that contains the FX file.", hr);
        return hr;
    }

    // 创建像素着色器
    hr = g_pDevice->CreatePixelShader(pPSBlob->GetBufferPointer(), pPSBlob->GetBufferSize(), nullptr, &g_pPixelShader);
    pPSBlob->Release();
    if (FAILED(hr)) {
        return hr;
    }

    return S_OK;
}

HRESULT dc3drender::EnsureDirect3DResources(HWND hWnd) {
    if (initialized) return S_OK;

    HRESULT hr = S_OK;

    // 创建交换链描述
    DXGI_SWAP_CHAIN_DESC sd;
    ZeroMemory(&sd, sizeof(sd));
    sd.BufferCount = 1;
    sd.BufferDesc.Width = currwd;
    sd.BufferDesc.Height = currht;
    sd.BufferDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
    sd.BufferDesc.RefreshRate.Numerator = 60;
    sd.BufferDesc.RefreshRate.Denominator = 1;
    sd.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
    sd.OutputWindow = hWnd;
    sd.SampleDesc.Count = 1;
    sd.SampleDesc.Quality = 0;
    sd.Windowed = TRUE;

    // 创建设备、交换链和设备上下文    
    UINT createDeviceFlags = D3D11_CREATE_DEVICE_BGRA_SUPPORT;  // 添加 BGRA 支持标志
    D3D_FEATURE_LEVEL featureLevel;
    hr = D3D11CreateDeviceAndSwapChain(
        nullptr,
        D3D_DRIVER_TYPE_HARDWARE,
        nullptr,
        createDeviceFlags,
        nullptr,
        0,
        D3D11_SDK_VERSION,
        &sd,
        &g_pSwapChain,
        &g_pDevice,
        &featureLevel,
        &g_pImmediateContext
    );
    if (FAILED(hr)) {
        fatal("D3D11CreateDeviceAndSwapChain failed", hr);
        return hr;
    }

    // 获取交换链中的后台缓冲区并创建渲染目标视图
    ID3D11Texture2D* pBackBuffer = nullptr;
    hr = g_pSwapChain->GetBuffer(0, __uuidof(ID3D11Texture2D), (LPVOID*)&pBackBuffer);
    if (FAILED(hr)) {
        fatal("GetBuffer failed", hr);
        return hr;
    }

    hr = g_pDevice->CreateRenderTargetView(pBackBuffer, nullptr, &g_pRenderTargetView);
    pBackBuffer->Release();
    if (FAILED(hr)) {
        fatal("CreateRenderTargetView failed", hr);
        return hr;
    }

    g_pImmediateContext->OMSetRenderTargets(1, &g_pRenderTargetView, nullptr);

    // 设置视口
    D3D11_VIEWPORT vp = {};
    vp.Width = (FLOAT)currwd;
    vp.Height = (FLOAT)currht;
    vp.MinDepth = 0.0f;
    vp.MaxDepth = 1.0f;
    vp.TopLeftX = 0;
    vp.TopLeftY = 0;
    g_pImmediateContext->RSSetViewports(1, &vp);

    return InitializeDirectWrite();
}

HRESULT dc3drender::InitializeDirectWrite() {
    HRESULT hr = S_OK;

    // 创建 Direct2D 工厂
    hr = D2D1CreateFactory(D2D1_FACTORY_TYPE_SINGLE_THREADED, &g_pD2DFactory);
    if (FAILED(hr)) {
        fatal("D2D1CreateFactory failed", hr);
        return hr;
    }

    // 创建 DirectWrite 工厂
    hr = DWriteCreateFactory(DWRITE_FACTORY_TYPE_SHARED, __uuidof(IDWriteFactory), reinterpret_cast<IUnknown**>(&g_pDWriteFactory));
    if (FAILED(hr)) {
        fatal("DWriteCreateFactory failed", hr);
        return hr;
    }

    // 创建文本格式
    hr = g_pDWriteFactory->CreateTextFormat(
        L"Trebuchet MS",
        nullptr,
        DWRITE_FONT_WEIGHT_NORMAL,
        DWRITE_FONT_STYLE_NORMAL,
        DWRITE_FONT_STRETCH_NORMAL,
        11.0f,
        L"en-us",
        &g_pTextFormat
    );
    if (FAILED(hr)) {
        fatal("CreateTextFormat failed", hr);
        return hr;
    }

    // 获取交换链中的后台缓冲区
    ID3D11Texture2D* pBackBuffer = nullptr;
    hr = g_pSwapChain->GetBuffer(0, __uuidof(ID3D11Texture2D), (LPVOID*)&pBackBuffer);
    if (FAILED(hr)) {
        fatal("GetBuffer failed", hr);
        return hr;
    }

    // 获取 DXGI 表面
    IDXGISurface* dxgiSurface = nullptr;
    hr = pBackBuffer->QueryInterface(__uuidof(IDXGISurface), (void**)&dxgiSurface);
    pBackBuffer->Release();
    if (FAILED(hr)) {
        fatal("QueryInterface failed", hr);
        return hr;
    }

    if (dxgiSurface == nullptr) {
        fatal("dxgiSurface is null", 0xF001);
        return 0xF001;
    }

    // 创建 Direct2D 渲染目标
    D2D1_RENDER_TARGET_PROPERTIES props = D2D1::RenderTargetProperties(
        D2D1_RENDER_TARGET_TYPE_DEFAULT,
        D2D1::PixelFormat(DXGI_FORMAT_R8G8B8A8_UNORM, D2D1_ALPHA_MODE_PREMULTIPLIED),
        0,
        0
    );

    hr = g_pD2DFactory->CreateDxgiSurfaceRenderTarget(
        dxgiSurface,
        &props,
        &g_2dpRenderTarget
    );
    dxgiSurface->Release();
    if (FAILED(hr)) {
        fatal("CreateDxgiSurfaceRenderTarget failed", hr);
        return hr;
    }

    hr = g_2dpRenderTarget->CreateSolidColorBrush(D2D1::ColorF(D2D1::ColorF::YellowGreen), &g_pFontBrush);
    if (FAILED(hr)) {
        fatal("CreateSolidColorBrush-font failed", hr);
        return hr;
    }

    hr = g_2dpRenderTarget->CreateSolidColorBrush(D2D1::ColorF(D2D1::ColorF::Purple, 0.45f), &g_pBackBrush);
    if (FAILED(hr)) {
        fatal("CreateSolidColorBrush-back failed", hr);
        return hr;
    }

    return S_OK;
}

void dc3drender::CleanupDirectWrite() {
    if (g_2dpRenderTarget) g_2dpRenderTarget->Release();
    if (g_pTextFormat) g_pTextFormat->Release();
    if (g_pDWriteFactory) g_pDWriteFactory->Release();
    if (g_pD2DFactory) g_pD2DFactory->Release();
    if (g_pFontBrush) g_pFontBrush->Release();
    if (g_pBackBrush) g_pBackBrush->Release();
}

void dc3drender::DrawRGBAData(unsigned char* rgbadata, int x, int y, int w, int h)
{

}

void dc3drender::DrawCells(unsigned char* pmdata, int x, int y, int w, int h, int pmscale)
{
    
}

void dc3drender::drawtext(int x, int y, const wchar_t* text) {
    if (!g_2dpRenderTarget || !g_pTextFormat) {
        return;
    }

    // 创建文本布局以便测量文本
    IDWriteTextLayout* pTextLayout = nullptr;
    HRESULT hr = g_pDWriteFactory->CreateTextLayout(
        text,        // 要渲染的文本
        wcslen(text),// 文本的长度
        g_pTextFormat,// 文本格式
        currwd,      // 最大宽度 
        currht,      // 最大高度 
        &pTextLayout // 输出的文本布局
    );

    if (FAILED(hr)) return;

    DWRITE_TEXT_METRICS textMetrics;
    hr = pTextLayout->GetMetrics(&textMetrics);
    if (FAILED(hr)) {
        pTextLayout->Release();
        return;
    }

    g_2dpRenderTarget->BeginDraw();
    g_2dpRenderTarget->SetTransform(D2D1::Matrix3x2F::Identity());

    // 根据文本尺寸设定矩形
    D2D1_RECT_F rectangle = D2D1::RectF(
        static_cast<float>(x),
        static_cast<float>(y),
        static_cast<float>(x) + textMetrics.widthIncludingTrailingWhitespace,
        static_cast<float>(y) + textMetrics.height
    );

    // 绘制背景
    g_2dpRenderTarget->FillRectangle(&rectangle, g_pBackBrush);

    // 绘制文本
    g_2dpRenderTarget->DrawTextLayout(
        D2D1::Point2F(static_cast<float>(x), static_cast<float>(y)),
        pTextLayout,
        g_pFontBrush,
        D2D1_DRAW_TEXT_OPTIONS_CLIP
    );
    hr = g_2dpRenderTarget->EndDraw();

    pTextLayout->Release(); // 释放文本布局资源  
}

void dc3drender::drawselection(VIEWINFO* pvi) {
    if (!g_2dpRenderTarget) return;

    // 确保有一个画刷可用
    if (!g_pSelBrush) {
        HRESULT hr = g_2dpRenderTarget->CreateSolidColorBrush(D2D1::ColorF(D2D1::ColorF::Green, 0.45f), &g_pSelBrush);
        if (FAILED(hr) || !g_pSelBrush) return;
    }

    g_2dpRenderTarget->BeginDraw();
    g_2dpRenderTarget->SetTransform(D2D1::Matrix3x2F::Identity());

    // 创建选区矩形
    D2D1_RECT_F selectionRect = D2D1::RectF(
        static_cast<float>(pvi->psl_x1),
        static_cast<float>(pvi->psl_y1),
        static_cast<float>(pvi->psl_x2),
        static_cast<float>(pvi->psl_y2)
    );

    // 绘制选区矩形
    g_2dpRenderTarget->FillRectangle(&selectionRect, g_pSelBrush);
    g_2dpRenderTarget->DrawRectangle(&selectionRect, g_pSelBrush, 1.0f);

    HRESULT hr = g_2dpRenderTarget->EndDraw();
}

void dc3drender::drawgridlines(int cellsize)
{
    if (!g_2dpRenderTarget) return;

    if (!g_pGridline) {
        HRESULT hr = g_2dpRenderTarget->CreateSolidColorBrush(D2D1::ColorF(D2D1::ColorF::Gray, 1.0f), &g_pGridline);
        if (FAILED(hr) || !g_pGridline) return;
    }


    g_2dpRenderTarget->BeginDraw();
    g_2dpRenderTarget->SetTransform(D2D1::Matrix3x2F::Identity());

    // 0 -> ht
    for (float dy = 0.0f; dy <= currht; dy += cellsize) {
        g_2dpRenderTarget->DrawLine(
            D2D1::Point2F(0.0f, dy),
            D2D1::Point2F(static_cast<float>(currwd), dy),
            g_pGridline,
            1.0f // 线宽
        );
    }

    for (float dx = 0.0f; dx <= currwd; dx += cellsize) {
        g_2dpRenderTarget->DrawLine(
            D2D1::Point2F(dx, 0.0f),
            D2D1::Point2F(dx, static_cast<float>(currht)),
            g_pGridline,
            1.0f // 线宽
        );
    }

    g_2dpRenderTarget->EndDraw();
}

void dc3drender::pixblit(int x, int y, int w, int h, unsigned char* pmdata, int pmscale)
{
    if (x >= currwd || y >= currht) return;
    if (x + w <= 0 || y + h <= 0) return;

    // stride is the horizontal pixel width of the image data
    int stride = w / pmscale;

    // clip data outside viewport
    if (pmscale > 1) {
        //int vcentx = currwd / 2.0f;
        //int vcenty = currht / 2.0f;

        //float offsetx = vcentx % pmscale;
        //float offsety = vcenty % pmscale;

        //x += offsetx;
        //y += offsety;
    }

    if (pmscale == 1) {
        // draw RGBA pixel data at scale 1:1
        DrawRGBAData(pmdata, x, y, w, h);
    }
    else {
        // draw magnified cells, assuming pmdata contains (w/pmscale)*(h/pmscale) bytes
        // where each byte contains a cell state
        DrawCells(pmdata, x, y, w / pmscale, h / pmscale, pmscale);
    }
}
