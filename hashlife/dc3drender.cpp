#include "pch.h"
#include "dc3drender.h"
#include <vector>
#include "DXTrace.h"

#include <wrl/client.h>
template <class T>
using ComPtr = Microsoft::WRL::ComPtr<T>;

void fatal(const char* msg, HRESULT hr, bool exit = true) {

#if defined(DEBUG) || defined(_DEBUG)
    auto message = std::string(msg) + " " + std::to_string(hr);
    MessageBoxA(NULL, message.c_str(), "Error", MB_OK);
    if (exit) {
        TerminateProcess(GetCurrentProcess(), 0);
    }
#endif
}

static HRESULT CompileShaderFromFile(
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

static bool InitDirect3D(
    HWND m_hMainWnd,
    int m_ClientWidth,
    int m_ClientHeight,
    ID3D11Device** g_pd3dDevice,
    ID3D11DeviceContext** g_pd3dImmediateContext,
    IDXGISwapChain** g_pSwapChain
)
{
    ComPtr<ID3D11Device> m_pd3dDevice;
    ComPtr<ID3D11DeviceContext> m_pd3dImmediateContext;
    ComPtr<IDXGISwapChain> m_pSwapChain;

    HRESULT hr = S_OK;

    // 创建D3D设备 和 D3D设备上下文
    UINT createDeviceFlags = D3D10_CREATE_DEVICE_BGRA_SUPPORT; // 添加 BGRA 支持标志
#if defined(DEBUG) || defined(_DEBUG)  
    createDeviceFlags |= D3D11_CREATE_DEVICE_DEBUG;
#endif
    // 驱动类型数组
    D3D_DRIVER_TYPE driverTypes[] =
    {
        D3D_DRIVER_TYPE_HARDWARE,
        D3D_DRIVER_TYPE_WARP,
        D3D_DRIVER_TYPE_REFERENCE,
    };
    UINT numDriverTypes = ARRAYSIZE(driverTypes);

    // 特性等级数组
    D3D_FEATURE_LEVEL featureLevels[] =
    {
        D3D_FEATURE_LEVEL_11_1,
        D3D_FEATURE_LEVEL_11_0,
    };
    UINT numFeatureLevels = ARRAYSIZE(featureLevels);

    D3D_FEATURE_LEVEL featureLevel;
    D3D_DRIVER_TYPE d3dDriverType;
    for (UINT driverTypeIndex = 0; driverTypeIndex < numDriverTypes; driverTypeIndex++)
    {
        d3dDriverType = driverTypes[driverTypeIndex];
        hr = D3D11CreateDevice(nullptr, d3dDriverType, nullptr, createDeviceFlags, featureLevels, numFeatureLevels,
            D3D11_SDK_VERSION, m_pd3dDevice.GetAddressOf(), &featureLevel, m_pd3dImmediateContext.GetAddressOf());

        if (hr == E_INVALIDARG)
        {
            // Direct3D 11.0 的API不承认D3D_FEATURE_LEVEL_11_1，所以我们需要尝试特性等级11.0以及以下的版本
            hr = D3D11CreateDevice(nullptr, d3dDriverType, nullptr, createDeviceFlags, &featureLevels[1], numFeatureLevels - 1,
                D3D11_SDK_VERSION, m_pd3dDevice.GetAddressOf(), &featureLevel, m_pd3dImmediateContext.GetAddressOf());
        }

        if (SUCCEEDED(hr))
            break;
    }

    if (FAILED(hr))
    {
        MessageBox(0, L"D3D11CreateDevice Failed.", 0, 0);
        return false;
    }

    // 检测是否支持特性等级11.0或11.1
    if (featureLevel != D3D_FEATURE_LEVEL_11_0 && featureLevel != D3D_FEATURE_LEVEL_11_1)
    {
        MessageBox(0, L"Direct3D Feature Level 11 unsupported.", 0, 0);
        return false;
    }

    ComPtr<IDXGIDevice> dxgiDevice = nullptr;
    ComPtr<IDXGIAdapter> dxgiAdapter = nullptr;
    ComPtr<IDXGIFactory1> dxgiFactory1 = nullptr;	// D3D11.0(包含DXGI1.1)的接口类 

    // 为了正确创建 DXGI交换链，首先我们需要获取创建 D3D设备 的 DXGI工厂，否则会引发报错：
    // "IDXGIFactory::CreateSwapChain: This function is being called with a device from a different IDXGIFactory."
    HR(m_pd3dDevice.As(&dxgiDevice));
    HR(dxgiDevice->GetAdapter(dxgiAdapter.GetAddressOf()));
    HR(dxgiAdapter->GetParent(__uuidof(IDXGIFactory1), reinterpret_cast<void**>(dxgiFactory1.GetAddressOf())));

    // 填充DXGI_SWAP_CHAIN_DESC用以描述交换链
    DXGI_SWAP_CHAIN_DESC sd;
    ZeroMemory(&sd, sizeof(sd));
    sd.BufferDesc.Width = m_ClientWidth;
    sd.BufferDesc.Height = m_ClientHeight;
    sd.BufferDesc.RefreshRate.Numerator = 60;
    sd.BufferDesc.RefreshRate.Denominator = 1;
    sd.BufferDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
    sd.BufferDesc.ScanlineOrdering = DXGI_MODE_SCANLINE_ORDER_UNSPECIFIED;
    sd.BufferDesc.Scaling = DXGI_MODE_SCALING_UNSPECIFIED;

    sd.SampleDesc.Count = 1;
    sd.SampleDesc.Quality = 0;
    sd.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
    sd.BufferCount = 2;
    sd.OutputWindow = m_hMainWnd;
    sd.Windowed = TRUE;
    sd.SwapEffect = DXGI_SWAP_EFFECT_FLIP_SEQUENTIAL;
    sd.Flags = 0;
    HR(dxgiFactory1->CreateSwapChain(m_pd3dDevice.Get(), &sd, m_pSwapChain.GetAddressOf()));

    *g_pd3dDevice = m_pd3dDevice.Detach();// 释放ComPtr对象的所有权
    *g_pd3dImmediateContext = m_pd3dImmediateContext.Detach();
    *g_pSwapChain = m_pSwapChain.Detach();

    return true;
}

void dc3drender::CleanupDirect3D()
{
    if (g_pCellVertexBuffer) {
        g_pCellVertexBuffer->Release();
        g_pCellVertexBuffer = nullptr;
    }

    if (g_pInstanceBuffer) {
        g_pInstanceBuffer->Release();
        g_pInstanceBuffer = nullptr;
    }

    if (g_pRenderTargetView) {
        g_pRenderTargetView->Release();
        g_pRenderTargetView = nullptr;
    }

    if (g_pSwapChain) {
        // 释放交换链
        g_pSwapChain->Release();
        g_pSwapChain = nullptr;
        OutputDebugString(L"Release SwapChain\n");
    }

    if (g_pImmediateContext) {
        g_pImmediateContext->ClearState();// 清除设备上下文的状态
        g_pImmediateContext->Flush();// 强制提交所有未提交的命令 

        OutputDebugStringW(L"ClearState and Flush\n");
        WaitForGPU(); // 等待GPU完成所有任务
        g_pImmediateContext->Release();
        OutputDebugString(L"Release ImmediateContext\n");
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

    //if (g_pDepthStencil) {
    //    g_pDepthStencil->Release();
    //    g_pDepthStencil = nullptr;
    //}

    //if (g_pDepthStencilView) {
    //    g_pDepthStencilView->Release();
    //    g_pDepthStencilView = nullptr;
    //}
}

void dc3drender::UpdateConstantBuffer(float pmscale, DirectX::XMUINT2 canvasSize)
{
    if (!g_pImmediateContext) return;
    DirectX::XMFLOAT4 rwhscale = DirectX::XMFLOAT4(pmscale, pmscale / canvasSize.x * 2, pmscale / canvasSize.y * 2, 0.0f);

    // 数据
    ConstantBuffer cb;
    cb.rwhscale = rwhscale;
    cb.canvasSize = DirectX::XMFLOAT4((float)canvasSize.x, (float)canvasSize.y, 0.0f, 0.0f);

    if (!g_pConstantBuffer) {

        auto cbSize = sizeof(ConstantBuffer);

        // 创建常量缓冲区
        D3D11_BUFFER_DESC bufferDesc = {};
        bufferDesc.Usage = D3D11_USAGE_DYNAMIC;
        bufferDesc.ByteWidth = cbSize;
        bufferDesc.BindFlags = D3D11_BIND_CONSTANT_BUFFER;
        bufferDesc.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE;

        D3D11_SUBRESOURCE_DATA initData = {};
        initData.pSysMem = &cb;

        HRESULT hr = g_pDevice->CreateBuffer(&bufferDesc, &initData, &g_pConstantBuffer);

        if (FAILED(hr)) {
            fatal("Create Constant Buffer failed", hr);
            return;
        }

        g_pImmediateContext->VSSetConstantBuffers(0, 1, &g_pConstantBuffer);

        return;
    }

    // 更新常量缓冲区
    D3D11_MAPPED_SUBRESOURCE mappedResource;
    HRESULT hr = g_pImmediateContext->Map(g_pConstantBuffer, 0, D3D11_MAP_WRITE_DISCARD, 0, &mappedResource);

    if (FAILED(hr)) {
        fatal("Map constant buffer failed", hr);
        return;
    }

    if (mappedResource.pData) {
        memcpy(mappedResource.pData, &cb, sizeof(ConstantBuffer));
        g_pImmediateContext->Unmap(g_pConstantBuffer, 0);
    }
    else {
        fatal("Map failed", 0);
    }

    g_pImmediateContext->VSSetConstantBuffers(0, 1, &g_pConstantBuffer);
}

void dc3drender::UpdateInstanceBuffer(InstanceData* pInstanceData, int numInstances)
{
    if (!g_pImmediateContext || !g_pInstanceBuffer) return;

    D3D11_MAPPED_SUBRESOURCE mappedResource;
    HRESULT hr = g_pImmediateContext->Map(g_pInstanceBuffer, 0, D3D11_MAP_WRITE_DISCARD, 0, &mappedResource);
    if (FAILED(hr)) {
        fatal("Map instance buffer failed", hr);
        return;
    }

    if (mappedResource.pData) {
        memcpy(mappedResource.pData, pInstanceData, sizeof(InstanceData) * numInstances);
        g_pImmediateContext->Unmap(g_pInstanceBuffer, 0);
    }
    else {
        fatal("Map failed", 0);
    }
}

HRESULT dc3drender::LoadShaders() {
    HRESULT hr = S_OK;

    // 编译顶点着色器
    ID3DBlob* pVSBlob = nullptr;
    ID3DBlob* errorBlob = nullptr;

    DWORD dwShaderFlags = D3DCOMPILE_ENABLE_STRICTNESS;

#if defined(DEBUG) || defined(_DEBUG)
    dwShaderFlags |= D3DCOMPILE_DEBUG;
    // 在Debug环境下禁用优化以避免出现一些不合理的情况
    dwShaderFlags |= D3DCOMPILE_SKIP_OPTIMIZATION;
#endif

    hr = D3DCompileFromFile(L"shader.fx", nullptr, nullptr, "VS", "vs_5_0", dwShaderFlags, 0, &pVSBlob, &errorBlob); // 编译顶点着色器
    if (FAILED(hr)) {
        if (errorBlob != nullptr)
        {
            auto message = (char*)errorBlob->GetBufferPointer();
            OutputDebugStringA(message);
            fatal(message, hr);
            errorBlob->Release();
            return hr;
        }
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
        // 顶点数据
        { "POSITION", 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 0, D3D11_INPUT_PER_VERTEX_DATA, 0 },
        { "COLOR", 0, DXGI_FORMAT_R32G32B32A32_FLOAT, 0, 12, D3D11_INPUT_PER_VERTEX_DATA, 0 },
        // 实例数据
        { "INSTANCE_POSITION", 0, DXGI_FORMAT_R32G32B32A32_SINT, 1, 0, D3D11_INPUT_PER_INSTANCE_DATA, 1 },
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
    hr = D3DCompileFromFile(L"shader.fx", nullptr, nullptr, "PS", "ps_5_0", dwShaderFlags, 0, &pPSBlob, &errorBlob); // 编译像素着色器
    if (FAILED(hr)) {

        if (errorBlob != nullptr)
        {
            auto message = (char*)errorBlob->GetBufferPointer();
            OutputDebugStringA(message);
            fatal(message, hr);
            errorBlob->Release();
            return hr;
        }

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

    return S_OK;
}

HRESULT dc3drender::EnsureDirect3DResources(HWND hWnd) {
    HRESULT hr = S_OK;


    // 创建交换链描述
    DXGI_SWAP_CHAIN_DESC sd;
    ZeroMemory(&sd, sizeof(sd));
    sd.BufferCount = 2;
    sd.BufferDesc.Width = currwd;
    sd.BufferDesc.Height = currht;
    sd.BufferDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
    sd.BufferDesc.RefreshRate.Numerator = 60;
    sd.BufferDesc.RefreshRate.Denominator = 1;
    sd.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
    sd.BufferDesc.ScanlineOrdering = DXGI_MODE_SCANLINE_ORDER_UNSPECIFIED;
    sd.BufferDesc.Scaling = DXGI_MODE_SCALING_UNSPECIFIED;
    sd.OutputWindow = hWnd;
    sd.SampleDesc.Count = 1;
    sd.SampleDesc.Quality = 0;
    sd.Windowed = TRUE;
    sd.SwapEffect = DXGI_SWAP_EFFECT_FLIP_SEQUENTIAL;
    // 创建设备、交换链和设备上下文    
    UINT createDeviceFlags = D3D11_CREATE_DEVICE_BGRA_SUPPORT;  // 添加 BGRA 支持标志

#if defined(DEBUG) || defined(_DEBUG)

    createDeviceFlags |= D3D11_CREATE_DEVICE_DEBUG;

#endif // DEBUG 

    D3D_FEATURE_LEVEL featureLevel;
    OutputDebugString(L"Creating Direct3D device and swap chain\n");
    HR(
        D3D11CreateDeviceAndSwapChain(
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
        )
    );

    if (renderinfo != nullptr) {
        swprintf((wchar_t*)renderinfo, 256, L"Direct3D 11.0");
    }

    //bool ok = InitDirect3D(hWnd, currwd, currht, &g_pDevice, &g_pImmediateContext, &g_pSwapChain);

    //if (!ok) {
    //    fatal("InitDirect3D failed", 0);
    //    return E_FAIL;
    //}
    // swprintf((wchar_t*)renderinfo, 256, L"Direct3D 11.0");


    // 设置图元拓扑结构 -> 三角形列表
    g_pImmediateContext->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);

    // 获取交换链中的后台缓冲区并创建渲染目标视图
    ID3D11Texture2D* pBackBuffer = nullptr;
    HR(g_pSwapChain->GetBuffer(0, __uuidof(ID3D11Texture2D), (LPVOID*)&pBackBuffer));
    HR(g_pDevice->CreateRenderTargetView(pBackBuffer, nullptr, &g_pRenderTargetView));
    pBackBuffer->Release();

    //// 创建深度模板缓冲区
    //D3D11_TEXTURE2D_DESC depthStencilDesc;
    //ZeroMemory(&depthStencilDesc, sizeof(depthStencilDesc));
    //depthStencilDesc.Width = currwd;
    //depthStencilDesc.Height = currht;
    //depthStencilDesc.MipLevels = 1;
    //depthStencilDesc.ArraySize = 1;
    //depthStencilDesc.Format = DXGI_FORMAT_D24_UNORM_S8_UINT;
    //depthStencilDesc.SampleDesc.Count = 1;
    //depthStencilDesc.SampleDesc.Quality = 0;
    //depthStencilDesc.Usage = D3D11_USAGE_DEFAULT;
    //depthStencilDesc.BindFlags = D3D11_BIND_DEPTH_STENCIL;
    //depthStencilDesc.CPUAccessFlags = 0;
    //depthStencilDesc.MiscFlags = 0;
    //// 创建深度模板缓冲区
    //HR(g_pDevice->CreateTexture2D(&depthStencilDesc, nullptr, &g_pDepthStencil));

    //// 创建深度模板视图
    //D3D11_DEPTH_STENCIL_VIEW_DESC depthStencilViewDesc;
    //ZeroMemory(&depthStencilViewDesc, sizeof(depthStencilViewDesc));
    //depthStencilViewDesc.Format = depthStencilDesc.Format;
    //depthStencilViewDesc.ViewDimension = D3D11_DSV_DIMENSION_TEXTURE2D;
    //depthStencilViewDesc.Texture2D.MipSlice = 0;

    //HR(g_pDevice->CreateDepthStencilView(g_pDepthStencil, &depthStencilViewDesc, &g_pDepthStencilView));

    //g_pImmediateContext->OMSetRenderTargets(1, &g_pRenderTargetView, g_pDepthStencilView);

    InitializeVertexBuffer(); // 初始化顶点缓冲区
    InitializeInstanceBuffer(); // 初始化实例缓冲区
    InitializeDirectWrite();

    return S_OK;
}

HRESULT dc3drender::InitializeVertexBuffer()
{
    using namespace DirectX;
    HRESULT hr = S_OK;

    Vertex cellVertices[6] = {
        //{ XMFLOAT3(-0.5f, 0.5f, 0.0f), *pLiveColor },
        //{ XMFLOAT3(0.5f, 0.5f, 0.0f), *pLiveColor },
        //{ XMFLOAT3(0.5f, -0.5f, 0.0f), *pLiveColor },
        //{ XMFLOAT3(-0.5f, 0.5f, 0.0f), *pLiveColor },
        //{ XMFLOAT3(0.5f, -0.5f, 0.0f), *pLiveColor },
        //{ XMFLOAT3(-0.5f, -0.5f, 0.0f), *pLiveColor },

        { XMFLOAT3(-0.0f, 1.0f, 0.0f), *pLiveColor },
        { XMFLOAT3(1.0f, 1.0f, 0.0f), *pLiveColor },
        { XMFLOAT3(1.0f, -0.0f, 0.0f), *pLiveColor },
        { XMFLOAT3(-0.0f, 1.0f, 0.0f), *pLiveColor },
        { XMFLOAT3(1.0f, -0.0f, 0.0f), *pLiveColor },
        { XMFLOAT3(-0.0f, -0.0f, 0.0f), *pLiveColor },
    };

    // 创建顶点缓冲区
    D3D11_BUFFER_DESC bufferDesc = {};
    bufferDesc.Usage = D3D11_USAGE_DEFAULT;
    bufferDesc.ByteWidth = sizeof(cellVertices);
    bufferDesc.BindFlags = D3D11_BIND_VERTEX_BUFFER;
    bufferDesc.CPUAccessFlags = 0;

    D3D11_SUBRESOURCE_DATA initData = {};
    initData.pSysMem = cellVertices;

    hr = g_pDevice->CreateBuffer(&bufferDesc, &initData, &g_pCellVertexBuffer);
    if (FAILED(hr)) {
        fatal("Create Cell Vertex Buffer failed", hr);
    }
    return S_OK;
}

HRESULT dc3drender::InitializeInstanceBuffer()
{
    // 在初始化时创建实例缓冲区 
    D3D11_BUFFER_DESC bufferDesc = {};
    bufferDesc.Usage = D3D11_USAGE_DYNAMIC;
    bufferDesc.ByteWidth = sizeof(InstanceData) * MAX_CELLS;
    bufferDesc.BindFlags = D3D11_BIND_VERTEX_BUFFER;
    bufferDesc.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE;

    HRESULT hr = g_pDevice->CreateBuffer(&bufferDesc, nullptr, &g_pInstanceBuffer);
    if (FAILED(hr)) {
        fatal("Create Instance Buffer failed", hr);
    }

    return hr;
}

HRESULT dc3drender::InitializeDirectWrite() {
    HRESULT hr = S_OK;

    // 创建 Direct2D 工厂
    HR(D2D1CreateFactory(D2D1_FACTORY_TYPE_SINGLE_THREADED, &g_pD2DFactory));

    // 创建 DirectWrite 工厂
    HR(DWriteCreateFactory(DWRITE_FACTORY_TYPE_SHARED, __uuidof(IDWriteFactory), reinterpret_cast<IUnknown**>(&g_pDWriteFactory)));

    // 创建文本格式
    HR(
        g_pDWriteFactory->CreateTextFormat(
            L"Trebuchet MS",
            nullptr,
            DWRITE_FONT_WEIGHT_NORMAL,
            DWRITE_FONT_STYLE_NORMAL,
            DWRITE_FONT_STRETCH_NORMAL,
            11.0f,
            L"en-us",
            &g_pTextFormat
        )
    );

    // 获取交换链中的后台缓冲区
    ID3D11Texture2D* pBackBuffer = nullptr;
    HR(g_pSwapChain->GetBuffer(0, __uuidof(ID3D11Texture2D), (LPVOID*)&pBackBuffer));

    // 获取 DXGI 表面
    IDXGISurface* dxgiSurface = nullptr;
    HR(pBackBuffer->QueryInterface(__uuidof(IDXGISurface), (void**)&dxgiSurface));
    pBackBuffer->Release(); // 释放资源 

    // 创建 Direct2D 渲染目标
    D2D1_RENDER_TARGET_PROPERTIES props = D2D1::RenderTargetProperties(
        D2D1_RENDER_TARGET_TYPE_DEFAULT,
        D2D1::PixelFormat(
            DXGI_FORMAT_R8G8B8A8_UNORM,
            D2D1_ALPHA_MODE_PREMULTIPLIED
        ),
        0,
        0
    );

    HR(
        g_pD2DFactory->CreateDxgiSurfaceRenderTarget(
            dxgiSurface,
            &props,
            &g_2dpRenderTarget
        )
    );
    dxgiSurface->Release();

    HR(g_2dpRenderTarget->CreateSolidColorBrush(D2D1::ColorF(D2D1::ColorF::YellowGreen), &g_pFontBrush));
    HR(g_2dpRenderTarget->CreateSolidColorBrush(D2D1::ColorF(D2D1::ColorF::Purple, 0.45f), &g_pBackBrush));
    HR(g_2dpRenderTarget->CreateSolidColorBrush(D2D1::ColorF(D2D1::ColorF::Green, 0.45f), &g_pSelBrush));
    HR(g_2dpRenderTarget->CreateSolidColorBrush(D2D1::ColorF(D2D1::ColorF::Gray, 1.0f), &g_pGridline));

    return S_OK;
}

void dc3drender::CleanupDirectWrite()
{
    if (g_pTextFormat) g_pTextFormat->Release();
    if (g_2dpRenderTarget) g_2dpRenderTarget->Release();
    if (g_pDWriteFactory) g_pDWriteFactory->Release();
    if (g_pD2DFactory) g_pD2DFactory->Release();
    if (g_pFontBrush) g_pFontBrush->Release();
    if (g_pBackBrush) g_pBackBrush->Release();
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
    // g_2dpRenderTarget->SetTransform(D2D1::Matrix3x2F::Identity());

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
    if (!g_pImmediateContext || !g_pDevice || !g_pVertexShader || !g_pPixelShader || !g_pVertexLayout) {
        fatal("DrawCells failed", 0);
        return;
    }
    using namespace DirectX;
    static std::vector<InstanceData> instanceData(MAX_CELLS);
    UINT instanceCount = 0;

    // 生成顶点数据
    for (int i = 0; i < h; ++i) { // 行 row
        for (int j = 0; j < w; ++j) { // 列 column
            int index = i * w + j;
            if (pmdata[index] == 0) continue;

            instanceData[instanceCount++] = { XMINT4(x, y, j, i) };
        }
    }

    if (instanceCount == 0) return; // 没有顶点数据

    this->vertices += instanceCount;

    // 更新实例缓冲区
    UpdateInstanceBuffer(instanceData.data(), instanceCount);

    // 设置顶点缓冲区
    static UINT strides[2] = { sizeof(Vertex), sizeof(InstanceData) };
    static UINT offsets[2] = { 0, 0 };
    static ID3D11Buffer* buffers[2] = { g_pCellVertexBuffer, g_pInstanceBuffer };

    g_pImmediateContext->IASetVertexBuffers(0, 2, buffers, strides, offsets);
    // 设置图元拓扑结构
    g_pImmediateContext->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);

    // 绘制实例
    g_pImmediateContext->DrawInstanced(6, instanceCount, 0, 0);
}

void dc3drender::resize(int w, int h) {

    waitfordrawing(); // 等待绘制完成
    bool wasSuspended = _suspend;
    suspend(); // 暂停绘制

    currwd = w;
    currht = h;
    CleanupDirectWrite();

    g_pRenderTargetView->Release();
    g_pRenderTargetView = nullptr;

    // 清理资源 
    g_pImmediateContext->OMSetRenderTargets(0, nullptr, nullptr);
    g_pImmediateContext->ClearState();
    g_pImmediateContext->Flush();// 强制提交所有未提交的命令 

    HR(g_pSwapChain->ResizeBuffers(2, currwd, currht, DXGI_FORMAT_R8G8B8A8_UNORM, 0));

    ID3D11Texture2D* pBackBuffer = nullptr;
    HR(g_pSwapChain->GetBuffer(0, __uuidof(ID3D11Texture2D), (LPVOID*)&pBackBuffer));
    HR(g_pDevice->CreateRenderTargetView(pBackBuffer, nullptr, &g_pRenderTargetView));
    pBackBuffer->Release(); // 释放资源

    g_vp->Width = float(currwd);
    g_vp->Height = float(currht);
    if (g_pImmediateContext) {
        g_pImmediateContext->RSSetViewports(1, g_vp);
    }

    InitializeDirectWrite();

    if (!wasSuspended) resume(); // 恢复绘制
}

void dc3drender::begindraw() {
    if (_suspend) return;
    initialize();
    vertices = 0;

    // 更新常量缓冲区
    UpdateConstantBuffer(
        pmscale,
        DirectX::XMUINT2(currwd, currht)
    );

    // 重新绑定渲染目标
    g_pImmediateContext->OMSetRenderTargets(1, &g_pRenderTargetView, nullptr);

    // 设置输入布局
    g_pImmediateContext->IASetInputLayout(g_pVertexLayout);
    // 设置顶点着色器
    g_pImmediateContext->VSSetShader(g_pVertexShader, nullptr, 0);
    // 设置像素着色器
    g_pImmediateContext->PSSetShader(g_pPixelShader, nullptr, 0);
    // 更新视口
    g_pImmediateContext->RSSetViewports(1, g_vp);

    _canRender = true;
    _isDrawing = true;
}

void dc3drender::enddraw() {
    if (g_pSwapChain) {
        auto hr = g_pSwapChain->Present(0, 0);
        if (FAILED(hr)) {
            fatal("Present failed", hr);
        }

        _canRender = false;
        _isDrawing = false;

        return;
    }

    _canRender = false;
    _isDrawing = false;
    fatal("enddraw failed", 0);
}

void dc3drender::drawtext(int x, int y, const wchar_t* text)
{
    if (!_canRender) return;
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
    // g_2dpRenderTarget->SetTransform(D2D1::Matrix3x2F::Identity());

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

void dc3drender::drawselection(VIEWINFO* pvi)
{
    if (!_canRender) return;
    if (!g_2dpRenderTarget) return;
    if (!g_pSelBrush) return;

    g_2dpRenderTarget->BeginDraw();
    // g_2dpRenderTarget->SetTransform(D2D1::Matrix3x2F::Identity());

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
    if (!_canRender) return;
    if (!g_2dpRenderTarget) return;
    if (!g_pGridline) return;

    g_2dpRenderTarget->BeginDraw();
    // g_2dpRenderTarget->SetTransform(D2D1::Matrix3x2F::Identity());

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
    if (!_canRender) return;
}

void dc3drender::pixblit(int x, int y, int w, int h, unsigned char* pmdata, int pmscale)
{
    if (!_canRender) return;
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
