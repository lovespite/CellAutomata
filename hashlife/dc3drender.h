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
#include <dxgi.h>

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

    // ȫ������ Direct3D ����
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


    // Direct2D �� DirectWrite ���� 
    IDWriteFactory* g_pDWriteFactory = nullptr;
    IDWriteTextFormat* g_pTextFormat = nullptr;
    ID2D1Factory* g_pD2DFactory = nullptr;
    ID2D1RenderTarget* g_2dpRenderTarget = nullptr;

    // ��ˢ  
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
    HRESULT EnsureDirect3DResources(HWND hWnd); // ��ʼ�� Direct3D
    HRESULT InitializeVertexBuffer(); // ��ʼ�����㻺����

    HRESULT InitializeDirectWrite(); // ��ʼ�� DirectWrite
    void CleanupDirectWrite();

    void CleanupDirect3D(); // ���� Direct3D ��Դ 

    // ���� RGBA ����
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

        //// ��������-��ͼ-ͶӰ����    
        //// �������
        //DirectX::XMMATRIX worldMatrix = DirectX::XMMatrixIdentity();

        //// ��ͼ���� 
        //DirectX::XMVECTOR eyePosition = DirectX::XMVectorSet(0.0f, 0.0f, -5.0f, 0.0f);
        //DirectX::XMVECTOR focusPoint = DirectX::XMVectorSet(0.0f, 0.0f, 0.0f, 0.0f);
        //DirectX::XMVECTOR upDirection = DirectX::XMVectorSet(0.0f, 1.0f, 0.0f, 0.0f);
        //DirectX::XMMATRIX viewMatrix = DirectX::XMMatrixLookAtLH(eyePosition, focusPoint, upDirection);

        //// ͶӰ����
        //float fovAngleY = 70.0f * DirectX::XM_PI / 180.0f;
        //float aspectRatio = static_cast<float>(currwd) / static_cast<float>(currht);
        //float nearZ = 0.01f;
        //float farZ = 100.0f;
        //DirectX::XMMATRIX projectionMatrix = DirectX::XMMatrixPerspectiveFovLH(fovAngleY, aspectRatio, nearZ, farZ);

        //// ���� WVP ����
        //DirectX::XMMATRIX wvpMatrix = worldMatrix * viewMatrix * projectionMatrix;
        //UpdateConstantBuffer(wvpMatrix); // ���³���������
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