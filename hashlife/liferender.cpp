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


// ���� BitmapInfo �ṹ��
BITMAPINFO g_BitmapInfo = {};

void InitializeBitmapInfo(int w, int h)
{
    memset(&g_BitmapInfo, 0, sizeof(BITMAPINFO));
    g_BitmapInfo.bmiHeader.biSize = sizeof(BITMAPINFOHEADER);
    g_BitmapInfo.bmiHeader.biWidth = w;
    g_BitmapInfo.bmiHeader.biHeight = -h; // ͼ��������top-down��
    g_BitmapInfo.bmiHeader.biPlanes = 1;
    g_BitmapInfo.bmiHeader.biBitCount = 32; // ÿ����32λ��RGBA
    g_BitmapInfo.bmiHeader.biCompression = BI_RGB; // ��ѹ��
}

void DrawRGBAData(HDC dc, unsigned char* rgbadata, int x, int y, int w, int h)
{
    if (!dc || !rgbadata || w <= 0 || h <= 0)
        return;

    // ��ʼ�� BitmapInfo��ֻ���״ε���ʱ��
    if (g_BitmapInfo.bmiHeader.biWidth != w || g_BitmapInfo.bmiHeader.biHeight != -h)
        InitializeBitmapInfo(w, h);

    // ʹ�� StretchDIBits ��λͼ���Ƶ�Ŀ�� HDC
    StretchDIBits(
        dc,             // Ŀ���豸������
        x,              // �����������Ͻ� X ����
        y,              // �����������Ͻ� Y ����
        w,              // ����������
        h,              // ��������߶�
        0,              // ͼ���������Ͻ� X ����
        0,              // ͼ���������Ͻ� Y ����
        w,              // ͼ�����ݿ��
        h,              // ͼ�����ݸ߶�
        rgbadata,       // ԭʼ RGBA ����
        &g_BitmapInfo,  // λͼ��Ϣ
        DIB_RGB_COLORS, // RGB ��ɫ
        SRCCOPY         // ���Ʋ�����ֱ�Ӹ���
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
