#include "pch.h"

#include "liferender.h"
#include <cstdint> 
#include <iostream>

#include <d3d11.h>
#include <d3d11_1.h> 

#include <DirectXMath.h>
#include <d3dcompiler.h>

#pragma comment (lib, "d3d11.lib")
#pragma comment (lib, "D3DCompiler.lib")


#include <d2d1.h>
#include <dwrite.h>
#include <string>
#pragma comment(lib, "d2d1.lib")
#pragma comment(lib, "dwrite.lib") 

struct Vertex
{
    DirectX::XMFLOAT3 Pos;
    DirectX::XMFLOAT4 Color;
};

struct InstanceData {
    DirectX::XMINT4 position; // x, y, column, row
};

struct ConstantBuffer {
    DirectX::XMFLOAT4 rwhscale; // 16 bytes
    DirectX::XMFLOAT4 canvasSize; // 16 bytes
};

class dc3drender : public liferender {

private:

    // 全局声明 Direct3D 变量
    D3D11_VIEWPORT* g_vp;

    ID3D11Device* g_pDevice = nullptr;
    ID3D11DeviceContext* g_pImmediateContext = nullptr;
    IDXGISwapChain* g_pSwapChain = nullptr;
    ID3D11RenderTargetView* g_pRenderTargetView = nullptr;

    ID3D11Buffer* g_pConstantBuffer = nullptr;

    ID3D11Buffer* g_pCellVertexBuffer = nullptr;
    ID3D11Buffer* g_pInstanceBuffer = nullptr;

    //ID3D11Texture2D* g_pDepthStencil = nullptr;
    //ID3D11DepthStencilView* g_pDepthStencilView = nullptr;

    ID3D11VertexShader* g_pVertexShader = nullptr;
    ID3D11PixelShader* g_pPixelShader = nullptr;
    ID3D11InputLayout* g_pVertexLayout = nullptr;


    // Direct2D 和 DirectWrite 变量 
    IDWriteFactory* g_pDWriteFactory = nullptr;
    IDWriteTextFormat* g_pTextFormat = nullptr;
    ID2D1Factory* g_pD2DFactory = nullptr;
    ID2D1RenderTarget* g_2dpRenderTarget = nullptr;

    // 画刷  
    ID2D1SolidColorBrush* g_pSelBrush = nullptr;
    ID2D1SolidColorBrush* g_pGridline = nullptr;
    ID2D1SolidColorBrush* g_pBackBrush = nullptr;
    ID2D1SolidColorBrush* g_pFontBrush = nullptr;

    FLOAT* BackgroundColor;
    FLOAT* GridlineColor;
    DirectX::XMFLOAT4* pLiveColor;

    bool initialized = false;

    void UpdateConstantBuffer(float pmscale, DirectX::XMUINT2 canvasSize);
    void UpdateInstanceBuffer(InstanceData* pInstanceData, int numInstances);

    HRESULT LoadShaders();
    HRESULT EnsureDirect3DResources(HWND hWnd); // 初始化 Direct3D
    HRESULT InitializeVertexBuffer(); // 初始化顶点缓冲区
    HRESULT InitializeInstanceBuffer(); // 初始化实例缓冲区

    HRESULT InitializeDirectWrite(); // 初始化 DirectWrite
    void CleanupDirectWrite();

    void CleanupDirect3D(); // 清理 Direct3D 资源 

    // 绘制 RGBA 数据
    void DrawRGBAData(unsigned char* rgbadata, int x, int y, int w, int h);
    void DrawCells(unsigned char* pmdata, int x, int y, int w, int h, int pmscale);

    void WaitForGPU() {

        // 创建事件查询对象
        ID3D11Query* g_pEventQuery;
        D3D11_QUERY_DESC queryDesc;
        queryDesc.Query = D3D11_QUERY_EVENT;
        queryDesc.MiscFlags = 0;
        g_pDevice->CreateQuery(&queryDesc, &g_pEventQuery); // 创建事件查询对象

        if (g_pEventQuery == nullptr) {
            return;
        }

        // 等待 GPU 完成 
        g_pImmediateContext->End(g_pEventQuery); // 发出信号 
        while (S_OK != g_pImmediateContext->GetData(g_pEventQuery, NULL, 0, 0)) {
            // 等待 GPU 完成
            Sleep(1);
        }
    }

    bool _destroyed = false;
    bool _canRender = false;
    bool _isDrawing = false;

    bool _suspend = false;

public:

    HWND chWnd;
    int currwd, currht;              // current width and height of viewport  

    dc3drender(int w, int h, HWND hWnd) {
        currwd = w;
        currht = h;
        chWnd = hWnd;

        g_vp = new D3D11_VIEWPORT();

        g_vp->Width = float(currwd);
        g_vp->Height = float(currht);
        g_vp->MinDepth = 0.0f;
        g_vp->MaxDepth = 1.0f;
        g_vp->TopLeftX = 0;
        g_vp->TopLeftY = 0;

        BackgroundColor = new FLOAT[4]{ 0.1456f, 0.1456f, 0.1456f, 1.0f };
        GridlineColor = new FLOAT[4]{ 0.0f, 0.0f, 0.0f, 1.0f };
        pLiveColor = new DirectX::XMFLOAT4{ 1.0f, 1.0f, 1.0f, 1.0f };

        renderinfo = (const wchar_t*)malloc(sizeof(wchar_t) * 256);

        initialize();
    }

    void suspend() {
        _suspend = true;
    }

    void resume() {
        _suspend = false;

    }

    bool isdrawing() {
        return _isDrawing;
    }

    ~dc3drender() {
        destroy();
    }

    void waitfordrawing() {
        OutputDebugString(L"Waiting for drawing\n");
        while (_isDrawing)
        {
            Sleep(1);
        }
        WaitForGPU();
        OutputDebugString(L"Drawing finished\n");
    }

    void initialize()
    {
        if (initialized) return;
        EnsureDirect3DResources(chWnd);
        LoadShaders();
        initialized = true;
    }

    void resize(int w, int h);
    void begindraw();
    void enddraw();

    void clear() {
        if (g_pImmediateContext) {
            g_pImmediateContext->ClearRenderTargetView(g_pRenderTargetView, BackgroundColor);
            // g_pImmediateContext->ClearDepthStencilView(g_pDepthStencilView, D3D11_CLEAR_DEPTH, 1.0f, 0);
        }
    }

    void destroy() {
        if (_destroyed) return;

        CleanupDirect3D();
        CleanupDirectWrite();
        delete[] BackgroundColor;
        delete[] GridlineColor;
        delete pLiveColor;
        delete g_vp;
        _destroyed = true;
    }

    void drawtext(int x, int y, const wchar_t* text);
    void drawselection(float x1, float y1, float x2, float y2);
    void drawgridlines(int cellsize);

    void drawlogo();

    virtual void pixblit(int x, int y, int w, int h, unsigned char* pmdata, int pmscale);

    virtual void getcolors(unsigned char** r, unsigned char** g, unsigned char** b)
    {
        *r = (unsigned char*)colors;
        *g = (unsigned char*)colors + 2;
        *b = (unsigned char*)colors + 4;
    }
};
