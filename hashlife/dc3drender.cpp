#include "pch.h"
#include "dc3drender.h"
#include <vector>

void fatal(const char* msg, HRESULT hr, bool exit = true) {

#if defined(DEBUG) || defined(_DEBUG)
    auto message = std::string(msg) + " " + std::to_string(hr);
    MessageBoxA(NULL, message.c_str(), "Error", MB_OK);
    if (exit) {
        TerminateProcess(GetCurrentProcess(), 0);
    }
#endif
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

    if (g_pVertexShader) {
        g_pVertexShader->Release();
        g_pVertexShader = nullptr;
    }

    if (g_pPixelShader) {
        g_pPixelShader->Release();
        g_pPixelShader = nullptr;
    }

    if (g_pVertexLayout) {
        g_pVertexLayout->Release();
        g_pVertexLayout = nullptr;
    }

    if (g_pDepthStencil) {
        g_pDepthStencil->Release();
        g_pDepthStencil = nullptr;
    }

    if (g_pDepthStencilView) {
        g_pDepthStencilView->Release();
        g_pDepthStencilView = nullptr;
    }
}

void dc3drender::UpdateConstantBuffer() {

    if (!g_pImmediateContext) return;
    if (!g_pConstantBuffer) return;

    DirectX::XMMATRIX world = DirectX::XMMatrixIdentity();
    DirectX::XMMATRIX view = DirectX::XMMatrixLookAtLH(
        DirectX::XMVectorSet(0.0f, 0.0f, -5.0f, 0.0f),
        DirectX::XMVectorSet(0.0f, 0.0f, 0.0f, 0.0f),
        DirectX::XMVectorSet(0.0f, 1.0f, 0.0f, 0.0f)
    );
    DirectX::XMMATRIX projection = DirectX::XMMatrixPerspectiveFovLH(
        DirectX::XM_PIDIV4,
        static_cast<float>(currwd) / static_cast<float>(currht),
        0.01f,
        100.0f
    );

    auto wvp = world * view * projection;
    g_pImmediateContext->UpdateSubresource(g_pConstantBuffer, 0, nullptr, &wvp, 0, 0);
}

HRESULT dc3drender::LoadShaders() {
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
        fatal("CreatePixelShader failed", hr);
        return hr;
    }

    // 设置输入布局
    g_pImmediateContext->IASetInputLayout(g_pVertexLayout);
    // 设置顶点着色器
    g_pImmediateContext->VSSetShader(g_pVertexShader, nullptr, 0);
    // 设置像素着色器
    g_pImmediateContext->PSSetShader(g_pPixelShader, nullptr, 0);

    return S_OK;
}
HRESULT dc3drender::EnsureDirect3DResources(HWND hWnd) {
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

    if (renderinfo != nullptr) {
        swprintf((wchar_t*)renderinfo, 256, L"Direct3D 11.0, %s", featureLevel == D3D_FEATURE_LEVEL_11_0 ? L"Hardware" : L"Software");
    }

    // 检查结果
    if (FAILED(hr)) {
        fatal("D3D11CreateDeviceAndSwapChain failed", hr);
        return hr;
    }

    // 设置图元拓扑结构 -> 三角形列表
    g_pImmediateContext->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);

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

    // 创建深度模板缓冲区
    D3D11_TEXTURE2D_DESC depthStencilDesc;
    ZeroMemory(&depthStencilDesc, sizeof(depthStencilDesc));
    depthStencilDesc.Width = currwd;
    depthStencilDesc.Height = currht;
    depthStencilDesc.MipLevels = 1;
    depthStencilDesc.ArraySize = 1;
    depthStencilDesc.Format = DXGI_FORMAT_D24_UNORM_S8_UINT;
    depthStencilDesc.SampleDesc.Count = 1;
    depthStencilDesc.SampleDesc.Quality = 0;
    depthStencilDesc.Usage = D3D11_USAGE_DEFAULT;
    depthStencilDesc.BindFlags = D3D11_BIND_DEPTH_STENCIL;
    depthStencilDesc.CPUAccessFlags = 0;
    depthStencilDesc.MiscFlags = 0;
    // 创建深度模板缓冲区
    hr = g_pDevice->CreateTexture2D(&depthStencilDesc, nullptr, &g_pDepthStencil);
    if (FAILED(hr)) {
        return hr;
    }

    // 创建深度模板视图
    D3D11_DEPTH_STENCIL_VIEW_DESC depthStencilViewDesc;
    ZeroMemory(&depthStencilViewDesc, sizeof(depthStencilViewDesc));
    depthStencilViewDesc.Format = depthStencilDesc.Format;
    depthStencilViewDesc.ViewDimension = D3D11_DSV_DIMENSION_TEXTURE2D;
    depthStencilViewDesc.Texture2D.MipSlice = 0;

    hr = g_pDevice->CreateDepthStencilView(g_pDepthStencil, &depthStencilViewDesc, &g_pDepthStencilView);
    if (FAILED(hr)) {
        return hr;
    }

    g_pImmediateContext->OMSetRenderTargets(1, &g_pRenderTargetView, g_pDepthStencilView);

    // 设置视口
    D3D11_VIEWPORT vp{};
    vp.Width = (FLOAT)currwd;
    vp.Height = (FLOAT)currht;
    vp.MinDepth = 0.0f;
    vp.MaxDepth = 1.0f;
    vp.TopLeftX = 0;
    vp.TopLeftY = 0;
    g_pImmediateContext->RSSetViewports(1, &vp);

    InitializeVertexBuffer();
    InitializeDirectWrite();

    return S_OK;
}

HRESULT dc3drender::InitializeVertexBuffer()
{
    HRESULT hr = S_OK;

    D3D11_BUFFER_DESC bd = {};
    bd.Usage = D3D11_USAGE_DYNAMIC;
    bd.ByteWidth = sizeof(Vertex) * 6 * MAX_CELLS;
    bd.BindFlags = D3D11_BIND_VERTEX_BUFFER;
    bd.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE;

    hr = g_pDevice->CreateBuffer(&bd, nullptr, &g_pVertexBuffer);
    if (FAILED(hr)) {
        fatal("CreateBuffer failed", hr);
        return hr;
    }

    bd.Usage = D3D11_USAGE_DEFAULT;
    bd.ByteWidth = sizeof(DirectX::XMMATRIX);
    bd.BindFlags = D3D11_BIND_CONSTANT_BUFFER;
    bd.CPUAccessFlags = 0;
    bd.MiscFlags = 0;
    bd.StructureByteStride = 0;

    hr = g_pDevice->CreateBuffer(&bd, nullptr, &g_pConstantBuffer);
    if (FAILED(hr)) {
        fatal("CreateConstantBuffer failed", hr);
        return hr;
    }

    UINT stride = sizeof(Vertex);
    UINT offset = 0;
    g_pImmediateContext->IASetVertexBuffers(0, 1, &g_pVertexBuffer, &stride, &offset);
    g_pImmediateContext->DSSetConstantBuffers(0, 1, &g_pConstantBuffer); // 设置常量缓冲区

    return S_OK;
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
        D2D1::PixelFormat(
            DXGI_FORMAT_R8G8B8A8_UNORM,
            D2D1_ALPHA_MODE_PREMULTIPLIED),
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
    if (g_pLiveBrush) g_pLiveBrush->Release();
    if (g_pSelBrush) g_pSelBrush->Release();
    if (g_pGridline) g_pGridline->Release();
}

void dc3drender::DrawRGBAData(unsigned char* rgbadata, int x, int y, int w, int h) {
    if (!g_2dpRenderTarget) {
        return;
    }

    // 创建 Direct2D 位图属性
    D2D1_BITMAP_PROPERTIES bitmapProperties = D2D1::BitmapProperties(
        D2D1::PixelFormat(DXGI_FORMAT_R8G8B8A8_UNORM, D2D1_ALPHA_MODE_PREMULTIPLIED),
        96.0f, // DPI
        96.0f  // DPI
    );

    // 创建 Direct2D 位图
    ID2D1Bitmap* pBitmap = nullptr;
    HRESULT hr = g_2dpRenderTarget->CreateBitmap(
        D2D1::SizeU(w, h),
        rgbadata,
        w * 4, // 每行字节数
        &bitmapProperties,
        &pBitmap
    );

    if (FAILED(hr)) {
        return;
    }

    g_2dpRenderTarget->BeginDraw();
    g_2dpRenderTarget->SetTransform(D2D1::Matrix3x2F::Identity());

    // 绘制位图
    D2D1_RECT_F destinationRect = D2D1::RectF(
        static_cast<float>(x),
        static_cast<float>(y),
        static_cast<float>(x + w),
        static_cast<float>(y + h)
    );
    g_2dpRenderTarget->DrawBitmap(pBitmap, destinationRect);

    hr = g_2dpRenderTarget->EndDraw();
    if (FAILED(hr)) {
        // 处理绘制错误
    }

    // 释放位图资源
    pBitmap->Release();
}

void dc3drender::DrawCells(unsigned char* pmdata, int x, int y, int w, int h, int pmscale) {
    if (!g_pImmediateContext || !g_pDevice || !g_pVertexBuffer || !g_pVertexShader || !g_pPixelShader || !g_pVertexLayout) {
        fatal("DrawCells failed", 0);
        return;
    }

    static std::vector<Vertex> vertices(6 * MAX_CELLS); // 将顶点缓冲区作为静态变量
    UINT64 vertexCount = 0;
    Vertex* pVertices = vertices.data();

    // 生成顶点数据
    for (int i = 0; i < h; ++i) {
        for (int j = 0; j < w; ++j) {
            int index = i * w + j;
            if (pmdata[index] == 0) continue;

            float startX = static_cast<float>(x + j * pmscale);
            float startY = static_cast<float>(y + i * pmscale);
            float endX = startX + static_cast<float>(pmscale);
            float endY = startY + static_cast<float>(pmscale);

            // 左上角
            pVertices[vertexCount++] = { DirectX::XMFLOAT3(startX, startY, 0.0f), *pLiveColor };
            // 右上角
            pVertices[vertexCount++] = { DirectX::XMFLOAT3(endX, startY, 0.0f), *pLiveColor };
            // 右下角
            pVertices[vertexCount++] = { DirectX::XMFLOAT3(endX, endY, 0.0f), *pLiveColor };

            // 左上角
            pVertices[vertexCount++] = { DirectX::XMFLOAT3(startX, startY, 0.0f), *pLiveColor };
            // 右下角
            pVertices[vertexCount++] = { DirectX::XMFLOAT3(endX, endY, 0.0f), *pLiveColor };
            // 左下角
            pVertices[vertexCount++] = { DirectX::XMFLOAT3(startX, endY, 0.0f), *pLiveColor };
        }
    }

    if (vertexCount == 0) return; // 没有顶点数据

    this->vertices += vertexCount;

    // 更新顶点缓冲区
    D3D11_MAPPED_SUBRESOURCE mappedResource;
    HRESULT hr = g_pImmediateContext->Map(g_pVertexBuffer, 0, D3D11_MAP_WRITE_DISCARD, 0, &mappedResource);
    if (FAILED(hr)) {
        fatal("Map failed", hr);
        return;
    }

    if (mappedResource.pData) {
        memcpy(mappedResource.pData, pVertices, vertexCount * sizeof(Vertex));
        g_pImmediateContext->Unmap(g_pVertexBuffer, 0);
    }
    else {
        fatal("Map failed", 0);
    }

    // 设置顶点缓冲区
    UINT stride = sizeof(Vertex);
    UINT offset = 0;
    g_pImmediateContext->IASetVertexBuffers(0, 1, &g_pVertexBuffer, &stride, &offset);

    // 设置图元拓扑结构
    g_pImmediateContext->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);

    // 更新常量缓冲区
    // UpdateConstantBuffer();

    // 绘制顶点
    g_pImmediateContext->Draw(static_cast<UINT>(vertexCount), 0);
}

void dc3drender::DrawCells2D(unsigned char* pmdata, int x, int y, int w, int h, int pmscale)
{
    if (!g_2dpRenderTarget) {
        return;
    }

    if (!g_pLiveBrush) {
        HRESULT hr = g_2dpRenderTarget->CreateSolidColorBrush(D2D1::ColorF(D2D1::ColorF::White), &g_pLiveBrush);
        if (FAILED(hr) || !g_pLiveBrush) return;
    }

    g_2dpRenderTarget->BeginDraw();
    g_2dpRenderTarget->SetTransform(D2D1::Matrix3x2F::Identity());

    D2D1_RECT_F rect = D2D1::RectF(0, 0, 0, 0);
    for (int row = 0; row < h; row++) {
        for (int col = 0; col < w; col++) {
            unsigned char state = pmdata[col + row * w]; // 获取细胞状态 
            // 绘制矩形
            if (state == 0) continue;

            rect.left = x + pmscale * col;
            rect.top = y + pmscale * row;
            rect.right = rect.left + pmscale;
            rect.bottom = rect.top + pmscale;

            g_2dpRenderTarget->FillRectangle(&rect, g_pLiveBrush);
        }
    }

    g_2dpRenderTarget->EndDraw();
}

void dc3drender::enddraw() {
    if (g_pSwapChain) {
        auto hr = g_pSwapChain->Present(0, 0);
        if (FAILED(hr)) {
            fatal("Present failed", hr);
        }
    }

    if (g_pImmediateContext) {
        // g_pImmediateContext->ClearState();
    }
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

void dc3drender::drawlogo()
{
    static DirectX::XMFLOAT4  logoColor(1.0f, 1.0f, 0.0f, 1.0f); // RGB 黄色：1.0f, 1.0f, 0.0f
    static std::vector<Vertex> vertices = {
        { DirectX::XMFLOAT3(0.0f, 0.0f, 0.0f),  logoColor },
        { DirectX::XMFLOAT3(0.0f, 100.0f, 0.0f), logoColor },
        { DirectX::XMFLOAT3(100.0f, 100.0f, 0.0f), logoColor },
        { DirectX::XMFLOAT3(0.0f, 0.0f, 0.0f), logoColor },
        { DirectX::XMFLOAT3(100.0f, 100.0f, 0.0f), logoColor },
        { DirectX::XMFLOAT3(100.0f, 0.0f, 0.0f), logoColor }
    }; // 6 个顶点, 矩形

    // 更新顶点缓冲区
    D3D11_MAPPED_SUBRESOURCE mappedResource;
    HRESULT hr = g_pImmediateContext->Map(g_pVertexBuffer, 0, D3D11_MAP_WRITE_DISCARD, 0, &mappedResource);
    if (FAILED(hr)) {
        fatal("Map failed", hr);
        return;
    }

    if (mappedResource.pData) {
        memcpy(mappedResource.pData, vertices.data(), vertices.size() * sizeof(Vertex));
        g_pImmediateContext->Unmap(g_pVertexBuffer, 0);
    }
    else {
        fatal("Map failed", 0);
    }

    // 设置顶点缓冲区
    UINT stride = sizeof(Vertex);
    UINT offset = 0;
    g_pImmediateContext->IASetVertexBuffers(0, 1, &g_pVertexBuffer, &stride, &offset);

    // 设置图元拓扑结构
    g_pImmediateContext->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);

    // 更新常量缓冲区
    // UpdateConstantBuffer();

    // 绘制顶点
    g_pImmediateContext->Draw(static_cast<UINT>(vertices.size()), 0); // 6 个顶点
}

void dc3drender::pixblit(int x, int y, int w, int h, unsigned char* pmdata, int pmscale)
{
    if (x >= currwd || y >= currht) return;
    if (x + w <= 0 || y + h <= 0) return;

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
