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

bitmaprender::~bitmaprender() {}

void bitmaprender::killrect(int x, int y, int w, int h) {
    // 背景颜色 BGRA (0, 0, 0, 255) 代表黑色
    const BYTE bgra[] = { 0, 0, 0, 255 };
    UINT64 pos;

    for (UINT64 j = 0; j < h; j++) {
        UINT64 row = currht - 1 - (j + y);
        if (row >= currht) continue; // 避免越界
        for (UINT64 i = 0; i < w; i++) {
            if (i + x >= currwd) continue; // 避免越界
            pos = row * stride + (i + x) * 4;
            BYTE* pixel = bitmapdataBGRA + pos;
            memcpy(pixel, bgra, 4);
        }
    }
}


void bitmaprender::pixblit(int x, int y, int w, int h, char* pmdata, int pmscale) {
    // note: pmdata is in RGB format,
    // we need to convert it to BGRA format

    if (pmscale != 1) return;

    int pmstride = w * 3;
    UINT64 pos;
    UINT64 bbound = (UINT64)stride * currht;

    for (UINT64 j = 0; j < h; j++) {
        UINT64 row = currht - 1 - (j + y);
        if (row >= currht) continue; // 避免越界
        for (UINT64 i = 0; i < w; i++) {
            if (i + x >= currwd) continue; // 避免越界
            pos = row * stride + (i + x) * 4;
            if (pos >= bbound) continue;

            BYTE* pixel = bitmapdataBGRA + pos;

            // RGB -> BGRA
            pixel[0] = pmdata[j * pmstride + i * 3 + 2]; // B
            pixel[1] = pmdata[j * pmstride + i * 3 + 1]; // G
            pixel[2] = pmdata[j * pmstride + i * 3];     // R
            pixel[3] = 255; // A
        }
    }
}

