#include "pch.h"

#include "d2d1.h" 
#include <dwrite.h>
#include <wincodec.h>
#include "combaseapi.h"
#include <cstdint>

#pragma comment(lib, "d2d1.lib")
#pragma comment(lib, "dwrite.lib")
#pragma comment(lib, "windowscodecs.lib")

// This file is part of Golly.
// See docs/License.html for the copyright notice.

/**
 *   Encapsulate a class capable of rendering a life universe.
 *   Note that we only use blitbit calls (no setpixel).
 *   Coordinates are in the same coordinate system as the
 *   viewport min/max values.
 *
 *   Also note that the render is responsible for deciding how
 *   to scale bits up as necessary, whether using the graphics
 *   hardware or the CPU.  Blits will only be called with
 *   reasonable bitmaps (32x32 or larger, probably) so the
 *   overhead should not be horrible.  Also, the bitmap must
 *   have zeros for all pixels to the left and right of those
 *   requested to be rendered (just for simplicity).
 *
 *   If clipping is needed, it's the responsibility of these
 *   routines, *not* the caller (although the caller should make
 *   every effort to not call these routines with out of bound
 *   values).
 */
#ifndef LIFERENDER_H
#define LIFERENDER_H
class liferender {
public:
    liferender() {}
    virtual ~liferender();

    // First two methods (pixblit/getcolors) only called for normal
    // display renderers.  For "getstate" renderers, these will never
    // be called.
    // pixblit is used to draw a pixel map by passing data in two formats:
    // If pmscale == 1 then pm data contains 4*w*h bytes where each
    // byte quadruplet contains the RGBA values for the corresponding pixel.
    // If pmscale > 1 then pm data contains (w/pmscale)*(h/pmscale) bytes
    // where each byte is a cell state (0..255).  This allows the rendering
    // code to display either icons or colors.
    virtual void pixblit(int x, int y, int w, int h, unsigned char* pm, int pmscale) = 0;

    // the drawing code needs access to the current layer's colors,
    // and to the transparency values for dead pixels and live pixels
    virtual void getcolors(unsigned char** r, unsigned char** g, unsigned char** b) = 0;

};
const unsigned char colors[] = {
    0, 255, // r: dead, alive 
    0, 255, // g: dead, alive
    0, 255  // b: dead, alive
};

static uint32_t RGBToInt(uint8_t r, uint8_t g, uint8_t b) {
    return (static_cast<uint32_t>(r) << 16) | (static_cast<uint32_t>(g) << 8) | static_cast<uint32_t>(b);
}

struct VIEWINFO
{
    INT32 EMPTY;
    INT32 psl_x1; // selection rect point 1
    INT32 psl_y1; // selection rect point 1
    INT32 psl_x2; // selection rect point 2
    INT32 psl_y2; // selection rect point 2
};

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
    ID2D1SolidColorBrush* pSelBrush = nullptr;
    ID2D1SolidColorBrush* pGridline = nullptr;

    ID2D1SolidColorBrush* pBackBrush = nullptr;
    ID2D1SolidColorBrush* pFontBrush = nullptr;

    ID2D1SolidColorBrush* pLiveCell = nullptr;
    ID2D1SolidColorBrush* pDeadCell = nullptr;
     
    void InitDirectWrite();// 初始化 DirectWrite，创建文本格式 
    void EnsureDirect2DResources(HWND hWnd); // 初始化 Direct2D 
    void UpdateBitmap(unsigned char* rgbadata, int w, int h);// 更新位图 
    void CleanupDirect2D(); // 清理 Direct2D 资源

    // 绘制 RGBA 数据
    void DrawRGBAData(unsigned char* rgbadata, int x, int y, int w, int h);
    void DrawCells(unsigned char* rgbadata, int x, int y, int w, int h, int pmscale);

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
            g_pRenderTarget->Clear(D2D1::ColorF(D2D1::ColorF::Black));
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
#endif

