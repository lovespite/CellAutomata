#include "pch.h"
/*** /

This file is part of Golly, a Game of Life Simulator.
Copyright (C) 2012 Andrew Trevorrow and Tomas Rokicki.

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

 Web site:  http://sourceforge.net/projects/golly
 Authors:   rokicki@gmail.com  andrew@trevorrow.com

                        / ***/



#include "liferender.h"


liferender::~liferender()
{
}

dcrender::~dcrender() {}


// 缓存 BitmapInfo 结构体
BITMAPINFO g_BitmapInfo = {};

void InitializeBitmapInfo(int w, int h)
{
    memset(&g_BitmapInfo, 0, sizeof(BITMAPINFO));
    g_BitmapInfo.bmiHeader.biSize = sizeof(BITMAPINFOHEADER);
    g_BitmapInfo.bmiHeader.biWidth = w;
    g_BitmapInfo.bmiHeader.biHeight = -h; // 图像正立（top-down）
    g_BitmapInfo.bmiHeader.biPlanes = 1;
    g_BitmapInfo.bmiHeader.biBitCount = 32; // 每像素32位：RGBA
    g_BitmapInfo.bmiHeader.biCompression = BI_RGB; // 无压缩
}

void DrawRGBAData(HDC dc, unsigned char* rgbadata, int x, int y, int w, int h)
{
    if (!dc || !rgbadata || w <= 0 || h <= 0)
        return;

    // 初始化 BitmapInfo（只在首次调用时）
    if (g_BitmapInfo.bmiHeader.biWidth != w || g_BitmapInfo.bmiHeader.biHeight != -h)
        InitializeBitmapInfo(w, h);

    // 使用 StretchDIBits 将位图绘制到目标 HDC
    StretchDIBits(
        dc,             // 目标设备上下文
        x,              // 绘制区域左上角 X 坐标
        y,              // 绘制区域左上角 Y 坐标
        w,              // 绘制区域宽度
        h,              // 绘制区域高度
        0,              // 图像数据左上角 X 坐标
        0,              // 图像数据左上角 Y 坐标
        w,              // 图像数据宽度
        h,              // 图像数据高度
        rgbadata,       // 原始 RGBA 数据
        &g_BitmapInfo,  // 位图信息
        DIB_RGB_COLORS, // RGB 颜色
        SRCCOPY         // 绘制操作：直接复制
    );
}


void dcrender::pixblit(int x, int y, int w, int h, unsigned char* pmdata, int pmscale) {

    if (x >= currwd || y >= currht) return;
    if (x + w <= 0 || y + h <= 0) return;

    // stride is the horizontal pixel width of the image data
    int stride = w / pmscale;

    // clip data outside viewport
    if (pmscale > 1) {
        // pmdata contains 1 byte per `pmscale' pixels, so we must be careful
        // and adjust x, y, w and h by multiples of `pmscale' only.
        if (x < 0) {
            int dx = -x / pmscale * pmscale;
            pmdata += dx / pmscale;
            w -= dx;
            x += dx;
        }
        if (y < 0) {
            int dy = -y / pmscale * pmscale;
            pmdata += dy / pmscale * stride;
            h -= dy;
            y += dy;
        }
        if (x + w >= currwd + pmscale) w = (currwd - x + pmscale - 1) / pmscale * pmscale;
        if (y + h >= currht + pmscale) h = (currht - y + pmscale - 1) / pmscale * pmscale;
    }

    if (pmscale == 1) {
        // draw RGBA pixel data at scale 1:1
        DrawRGBAData(chdc, pmdata, x, y, w, h);

    }
    else {
        // draw magnified cells, assuming pmdata contains (w/pmscale)*(h/pmscale) bytes
        // where each byte contains a cell state
        // DrawCells(chdc, pmdata, x, y, w / pmscale, h / pmscale, pmscale, stride, currlayer->numicons, celltexture);
    }
}
