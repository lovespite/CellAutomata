#include "pch.h"

#include <iostream>
#include "combaseapi.h"
#include <cstdint>

#include <wincodec.h>
#pragma comment(lib, "windowscodecs.lib")

#include "d2d1.h" 
#include <dwrite.h>
#pragma comment(lib, "d2d1.lib")
#pragma comment(lib, "dwrite.lib")

#include <d3d11.h>
#include <DirectXMath.h>
#include <d3dcompiler.h>

#pragma comment (lib, "d3d11.lib")
#pragma comment (lib, "D3DCompiler.lib")


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

const UINT32 MAX_CELLS = 256 * 256; // ×î´óÏ¸°ûÊý
class liferender {
public:
    liferender() {}
    virtual ~liferender();

    float pmscale = 1.0f;
    UINT64 vertices = 0;
    const wchar_t* renderinfo = nullptr;
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
    virtual void begindraw() = 0;
    virtual void enddraw() = 0;
    virtual void clear() = 0;

    void setpmscale(float scale) { pmscale = scale; }
    virtual void resize(int w, int h) = 0;

    // draw text at the given location
    virtual void drawtext(int x, int y, const wchar_t* text) = 0;

    // draw the selection rectangle
    virtual void drawselection(VIEWINFO* pvi) = 0;

    // draw grid lines at the given cell size
    virtual void drawgridlines(int cellsize) = 0;

    virtual void drawlogo() = 0;

    virtual void destroy() = 0;

};
const unsigned char colors[] = {
    0, 255, // r: dead, alive 
    0, 255, // g: dead, alive
    0, 255  // b: dead, alive
};

#endif

