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

class dc3drender : public liferender {

private:

    // 全局声明 Direct3D 变量
    ID3D11Device* g_pDevice = nullptr;
    ID3D11DeviceContext* g_pImmediateContext = nullptr;
    IDXGISwapChain* g_pSwapChain = nullptr;
    ID3D11RenderTargetView* g_pRenderTargetView = nullptr;

    ID3D11Buffer* g_pVertexBuffer = nullptr;

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

    bool initialized = false;

    int LoadShaders();
    HRESULT EnsureDirect3DResources(HWND hWnd); // 初始化 Direct3D

    HRESULT InitializeDirectWrite(); // 初始化 DirectWrite
    void CleanupDirectWrite();

    void CleanupDirect3D(); // 清理 Direct3D 资源 

    // 绘制 RGBA 数据
    void DrawRGBAData(unsigned char* rgbadata, int x, int y, int w, int h);
    void DrawCells(unsigned char* pmdata, int x, int y, int w, int h, int pmscale);

public:

    HWND chWnd;
    int currwd, currht;              // current width and height of viewport  

    dc3drender(int w, int h, HWND hWnd) {
        currwd = w;
        currht = h;
        chWnd = hWnd;

        BackgroundColor = new FLOAT[4]{ 0.1456f, 0.1456f, 0.1456f, 1.0f };
        GridlineColor = new FLOAT[4]{ 0.0f, 0.0f, 0.0f, 1.0f };
    }

    ~dc3drender() {
        CleanupDirect3D();
        CleanupDirectWrite();
    }

    void initialize()
    {
        if (initialized) return;

        HRESULT hr = EnsureDirect3DResources(chWnd);
        if (FAILED(hr)) {
            MessageBoxA(
                NULL,
                (std::string("Direct3D initialization failed: ") + std::to_string(hr)).c_str(),
                "Error",
                MB_OK);
            return;
        }

        if (LoadShaders() != S_OK) {
            MessageBoxA(NULL, "Shader loading failed", "Error", MB_OK);
            return;
        }

        initialized = true;
    }
    void begindraw() {
        initialize();
    }

    void enddraw() {
        if (g_pSwapChain) {
            g_pSwapChain->Present(0, 0);
        }

        if (g_pImmediateContext) {
            g_pImmediateContext->ClearState();
        }
    }

    void clear() {
        if (g_pImmediateContext) {
            g_pImmediateContext->ClearRenderTargetView(g_pRenderTargetView, BackgroundColor);
        }
    }

    void drawtext(int x, int y, const wchar_t* text);
    void drawselection(VIEWINFO* pvi);
    void drawgridlines(int cellsize);

    virtual void pixblit(int x, int y, int w, int h, unsigned char* pmdata, int pmscale);

    virtual void getcolors(unsigned char** r, unsigned char** g, unsigned char** b)
    {
        *r = (unsigned char*)colors;
        *g = (unsigned char*)colors + 2;
        *b = (unsigned char*)colors + 4;
    }
};