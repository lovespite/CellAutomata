#include "pch.h"

#include "liferender.h"
#include <iostream>
#include "combaseapi.h"
#include <cstdint>

#include <wincodec.h>
#pragma comment(lib, "windowscodecs.lib")

#include "d2d1.h" 
#include <dwrite.h>
#pragma comment(lib, "d2d1.lib")
#pragma comment(lib, "dwrite.lib")  

class dcrender : public liferender {

private:
    // 全局 Direct2D 变量
    ID2D1Factory* g_pD2DFactory = NULL;
    ID2D1HwndRenderTarget* g_pRenderTarget = NULL;
    IWICImagingFactory* pWICFactory = NULL;
    ID2D1Bitmap* g_pBitmap = NULL;

    // 全局 DirectWrite 变量    
    IDWriteFactory* g_pDWriteFactory = nullptr;
    IDWriteTextFormat* g_pTextFormat = nullptr;

    // 画刷
    ID2D1SolidColorBrush* pZerBrush = nullptr;
    ID2D1SolidColorBrush* pSelBrush = nullptr;
    ID2D1SolidColorBrush* pGridline = nullptr;

    ID2D1SolidColorBrush* pBackBrush = nullptr;
    ID2D1SolidColorBrush* pFontBrush = nullptr;

    ID2D1SolidColorBrush* pLiveCell = nullptr;
    ID2D1SolidColorBrush* pDeadCell = nullptr;


    void InitDirectWrite();// 初始化 DirectWrite，创建文本格式 

    void EnsureDirect2DResources(HWND hWnd); // 初始化 Direct2D  

    void CleanupDirect2D(); // 清理 Direct2D 资源 

    void UpdateBitmap(unsigned char* rgbadata, int w, int h);// 更新位图 

    // 绘制 RGBA 数据
    void DrawRGBAData(unsigned char* rgbadata, int x, int y, int w, int h);
    void DrawCells(unsigned char* pmdata, int x, int y, int w, int h, int pmscale);

public:

    HWND chWnd;
    int currwd, currht;              // current width and height of viewport  

    dcrender(int w, int h, HWND hWnd) {
        currwd = w;
        currht = h;
        chWnd = hWnd;
    }

    ~dcrender() {
        CleanupDirect2D();
    }

    void begindraw() {
        EnsureDirect2DResources(chWnd);

        if (g_pRenderTarget) {
            g_pRenderTarget->BeginDraw();
        }
    }

    void enddraw() {
        if (g_pRenderTarget) {
            g_pRenderTarget->EndDraw();
        }
    }

    void clear() {
        if (g_pRenderTarget) {
            g_pRenderTarget->Clear(D2D1::ColorF(0.1456f, 0.1456f, 0.1456f, 1.0f));
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
