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
#include "pch.h"
#include "liferender.h" 

liferender::~liferender()
{
}

// ���� BitmapInfo �ṹ��
BITMAPINFO g_BitmapInfo = {};

void dcrender::InitDirectWrite() {
    HRESULT hr = DWriteCreateFactory(DWRITE_FACTORY_TYPE_SHARED, __uuidof(IDWriteFactory), reinterpret_cast<IUnknown**>(&g_pDWriteFactory));
    if (SUCCEEDED(hr)) {
        // �����ı���ʽ����
        g_pDWriteFactory->CreateTextFormat(
            L"Trebuchet MS",                 // �����������
            nullptr,                  // ���弯
            DWRITE_FONT_WEIGHT_NORMAL,
            DWRITE_FONT_STYLE_NORMAL,
            DWRITE_FONT_STRETCH_NORMAL,
            11.0f,                    // �ֺ�
            L"",                      // ���Ա�ǩ
            &g_pTextFormat
        );

        // �����ı����뷽ʽ
        if (g_pTextFormat) {
            g_pTextFormat->SetTextAlignment(DWRITE_TEXT_ALIGNMENT_LEADING);
            g_pTextFormat->SetParagraphAlignment(DWRITE_PARAGRAPH_ALIGNMENT_NEAR);
        }

        g_pRenderTarget->CreateSolidColorBrush(D2D1::ColorF(D2D1::ColorF::Purple, 0.45f), &pBackBrush);
        g_pRenderTarget->CreateSolidColorBrush(D2D1::ColorF(D2D1::ColorF::YellowGreen), &pFontBrush);
    }
}

void dcrender::EnsureDirect2DResources(HWND hWnd) {
    if (!g_pD2DFactory) {
        D2D1CreateFactory(D2D1_FACTORY_TYPE_SINGLE_THREADED, &g_pD2DFactory);
    }

    if (!g_pRenderTarget) {
        RECT rc;
        GetClientRect(hWnd, &rc);

        D2D1_SIZE_U size = D2D1::SizeU(rc.right - rc.left, rc.bottom - rc.top);
        g_pD2DFactory->CreateHwndRenderTarget(
            D2D1::RenderTargetProperties(),
            D2D1::HwndRenderTargetProperties(hWnd, size),
            &g_pRenderTarget
        );
    }

    if (!pWICFactory) {
        HRESULT ret = CoCreateInstance(CLSID_WICImagingFactory, NULL, 0x1, IID_PPV_ARGS(&pWICFactory));

        if (ret != 0) {
            MessageBoxA(NULL, "Create WICImagingFactory failed", "Error", MB_OK);
        }
    }

    if (!g_pDWriteFactory) {
        InitDirectWrite();
    }

    if (!pSelBrush) {
        g_pRenderTarget->CreateSolidColorBrush(D2D1::ColorF(D2D1::ColorF::Green, 0.45f), &pSelBrush);
    }

    if (!pGridline) {
        g_pRenderTarget->CreateSolidColorBrush(D2D1::ColorF(D2D1::ColorF::Gray, 0.45f), &pGridline);
    }

    if (!pLiveCell || !pDeadCell) {
        unsigned char* r;
        unsigned char* g;
        unsigned char* b;

        getcolors(&r, &g, &b);

        int deadRGBA = RGBToInt(r[0], g[0], b[0]);
        int liveRGBA = RGBToInt(r[1], g[1], b[1]);

        g_pRenderTarget->CreateSolidColorBrush(D2D1::ColorF(liveRGBA), &pLiveCell);
        g_pRenderTarget->CreateSolidColorBrush(D2D1::ColorF(deadRGBA), &pDeadCell);
    }
}

void dcrender::UpdateBitmap(unsigned char* rgbadata, int w, int h) {

    IWICBitmap* pWICBitmap = NULL;
    pWICFactory->CreateBitmapFromMemory(
        w, h, GUID_WICPixelFormat32bppPBGRA, w * 4, w * h * 4, rgbadata, &pWICBitmap);

    g_pRenderTarget->CreateBitmapFromWicBitmap(pWICBitmap, NULL, &g_pBitmap);
    pWICBitmap->Release();
}

void dcrender::CleanupDirect2D() {
    if (g_pBitmap) {
        g_pBitmap->Release();
        g_pBitmap = NULL;
    }
    if (g_pRenderTarget) {
        g_pRenderTarget->Release();
        g_pRenderTarget = NULL;
    }
    if (pWICFactory) {
        pWICFactory->Release();
        pWICFactory = NULL;
    }
    if (g_pD2DFactory) {
        g_pD2DFactory->Release();
        g_pD2DFactory = NULL;
    }
}

void dcrender::drawtext(int x, int y, const wchar_t* text) {
    if (!g_pRenderTarget || !g_pTextFormat || !text) return;  // �򻯴�����

    HRESULT hr = S_OK;

    // �����ı������Ա�����ı�
    IDWriteTextLayout* pTextLayout = nullptr;
    hr = g_pDWriteFactory->CreateTextLayout(
        text,        // Ҫ��Ⱦ���ı�
        wcslen(text),// �ı��ĳ���
        g_pTextFormat,// �ı���ʽ
        currwd,      // ����� 
        currht,      // ���߶� 
        &pTextLayout // ������ı�����
    );

    if (SUCCEEDED(hr)) {
        // ��ȡ�ı��ĳߴ�
        DWRITE_TEXT_METRICS textMetrics;
        hr = pTextLayout->GetMetrics(&textMetrics);
        if (SUCCEEDED(hr)) {
            // �����ı��ߴ��趨����
            D2D1_RECT_F rectangle = D2D1::RectF(
                static_cast<float>(x),
                static_cast<float>(y),
                static_cast<float>(x) + textMetrics.widthIncludingTrailingWhitespace,
                static_cast<float>(y) + textMetrics.height
            );

            // ���Ʊ���
            g_pRenderTarget->FillRectangle(&rectangle, pBackBrush);

            // �����ı�
            g_pRenderTarget->DrawTextLayout(
                D2D1::Point2F(static_cast<float>(x), static_cast<float>(y)),
                pTextLayout,
                pFontBrush,
                D2D1_DRAW_TEXT_OPTIONS_CLIP
            );
        }
        pTextLayout->Release(); // �ͷ��ı�������Դ
    }
}

void dcrender::drawselection(VIEWINFO* pvi) {
    if (!g_pRenderTarget || !pvi) return;  // �򻯴�����

    D2D1_RECT_F rectangle = D2D1::RectF(
        static_cast<float>(pvi->psl_x1),
        static_cast<float>(pvi->psl_y1),
        static_cast<float>(pvi->psl_x2),
        static_cast<float>(pvi->psl_y2)
    );

    g_pRenderTarget->FillRectangle(&rectangle, pSelBrush);
    g_pRenderTarget->DrawRectangle(&rectangle, pSelBrush);
}

void dcrender::drawgridlines(int cellsize) {
    if (!g_pRenderTarget) return;  // �򻯴�����

    int centerx = currwd / 2;
    int centery = currht / 2;

    // ���ƴ�ֱ�ߣ�����������
    for (float dx = centerx; dx >= 0; dx -= cellsize) {
        g_pRenderTarget->DrawLine(
            D2D1::Point2F(dx, 0.0f),
            D2D1::Point2F(dx, static_cast<float>(currht)),
            pGridline,
            1.0f // �߿�
        );
    }

    // ���ƴ�ֱ�ߣ�����������
    for (float dx = centerx + cellsize; dx <= currwd; dx += cellsize) {
        g_pRenderTarget->DrawLine(
            D2D1::Point2F(dx, 0.0f),
            D2D1::Point2F(dx, static_cast<float>(currht)),
            pGridline,
            1.0f // �߿�
        );
    }

    // ����ˮƽ�ߣ�����������
    for (float dy = centery; dy >= 0; dy -= cellsize) {
        g_pRenderTarget->DrawLine(
            D2D1::Point2F(0.0f, dy),
            D2D1::Point2F(static_cast<float>(currwd), dy),
            pGridline,
            1.0f // �߿�
        );
    }

    // ����ˮƽ�ߣ�����������
    for (float dy = centery + cellsize; dy <= currht; dy += cellsize) {
        g_pRenderTarget->DrawLine(
            D2D1::Point2F(0.0f, dy),
            D2D1::Point2F(static_cast<float>(currwd), dy),
            pGridline,
            1.0f // �߿�
        );
    }

}

void dcrender::DrawRGBAData(unsigned char* rgbadata, int x, int y, int w, int h) {

    UpdateBitmap(rgbadata, w, h);

    // ����λͼ 
    g_pRenderTarget->DrawBitmap(g_pBitmap, D2D1::RectF(x, y, x + w, y + h));
    g_pBitmap->Release();
    g_pBitmap = NULL;
}

void dcrender::DrawCells(unsigned char* pmdata, int x, int y, int w, int h, int pmscale) {

    // draw magnified cells, assuming pmdata contains (w/pmscale)*(h/pmscale) bytes
    // where each byte contains a cell state
    D2D1_RECT_F rect = D2D1::RectF(0, 0, 0, 0);
    for (int row = 0; row < h; row++) {
        for (int col = 0; col < w; col++) {
            unsigned char cellState = pmdata[col + row * w]; // ��ȡϸ��״̬ 
            // ���ƾ���
            if (cellState == 0) continue;

            rect.left = x + pmscale * col;
            rect.top = y + pmscale * row;
            rect.right = rect.left + pmscale;
            rect.bottom = rect.top + pmscale;

            g_pRenderTarget->FillRectangle(&rect, pLiveCell);
        }
    }

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
        DrawRGBAData(pmdata, x, y, w, h);
    }
    else {
        // draw magnified cells, assuming pmdata contains (w/pmscale)*(h/pmscale) bytes
        // where each byte contains a cell state
        DrawCells(pmdata, x, y, w / pmscale, h / pmscale, pmscale);
    }
}
