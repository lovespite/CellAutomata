#include "pch.h"

#include "liferender.h"
#include <cstdint> 
#include <iostream>

#include <d3d11.h>
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

struct ConstantBuffer {
    DirectX::XMMATRIX WorldViewProjection;
};

class dc3drender : public liferender {

private:

    // 全局声明 Direct3D 变量
    ID3D11Device* g_pDevice = nullptr;
    ID3D11DeviceContext* g_pImmediateContext = nullptr;
    IDXGISwapChain* g_pSwapChain = nullptr;
    ID3D11RenderTargetView* g_pRenderTargetView = nullptr;

    ID3D11Buffer* g_pConstantBuffer = nullptr;

    ID3D11Buffer* g_pVertexBuffer = nullptr;

    ID3D11Texture2D* g_pDepthStencil = nullptr;
    ID3D11DepthStencilView* g_pDepthStencilView = nullptr;

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
    ID2D1SolidColorBrush* g_pLiveBrush = nullptr;

    FLOAT* BackgroundColor;
    FLOAT* GridlineColor;
    DirectX::XMFLOAT4* pLiveColor;

    bool initialized = false;

    void UpdateConstantBuffer();

    HRESULT LoadShaders();
    HRESULT EnsureDirect3DResources(HWND hWnd); // 初始化 Direct3D
    HRESULT InitializeVertexBuffer(); // 初始化顶点缓冲区

    HRESULT InitializeDirectWrite(); // 初始化 DirectWrite
    void CleanupDirectWrite();

    void CleanupDirect3D(); // 清理 Direct3D 资源 

    // 绘制 RGBA 数据
    void DrawRGBAData(unsigned char* rgbadata, int x, int y, int w, int h);
    void DrawCells(unsigned char* pmdata, int x, int y, int w, int h, int pmscale);
    void DrawCells2D(unsigned char* pmdata, int x, int y, int w, int h, int pmscale);

public:

    HWND chWnd;
    int currwd, currht;              // current width and height of viewport  

    dc3drender(int w, int h, HWND hWnd) {
        currwd = w;
        currht = h;
        chWnd = hWnd;

        BackgroundColor = new FLOAT[4]{ 0.1456f, 0.1456f, 0.1456f, 1.0f };
        GridlineColor = new FLOAT[4]{ 0.0f, 0.0f, 0.0f, 1.0f };
        pLiveColor = new DirectX::XMFLOAT4{ 1.0f, 1.0f, 1.0f, 1.0f };

        renderinfo = (const wchar_t*)malloc(sizeof(wchar_t) * 256);
    }

    ~dc3drender() {
        CleanupDirect3D();
        CleanupDirectWrite();
    }

    void initialize()
    {
        if (initialized) return;
        EnsureDirect3DResources(chWnd);
        LoadShaders();
        initialized = true;
    }

    void begindraw() {
        initialize();
        vertices = 0;
    }

    void enddraw();

    void clear() {
        if (g_pImmediateContext) {
            g_pImmediateContext->ClearRenderTargetView(g_pRenderTargetView, BackgroundColor);
            g_pImmediateContext->ClearDepthStencilView(g_pDepthStencilView, D3D11_CLEAR_DEPTH, 1.0f, 0);
        }
    }

    void drawtext(int x, int y, const wchar_t* text);
    void drawselection(VIEWINFO* pvi);
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
