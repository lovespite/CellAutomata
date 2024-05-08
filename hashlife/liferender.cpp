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
    for (int j = y; j < y + h; j++) {
        for (int i = x; i < x + w; i++) {
            BYTE* pixel = bitmapdataBGRA + j * stride + i * 4;
            memcpy(pixel, bgra, 4);
        }
    }
}


void bitmaprender::pixblit(int x, int y, int w, int h, char* pmdata, int pmscale) {
    unsigned char* r, * g, * b;
    getcolors(&r, &g, &b);

    if (pmscale == 1) {
        // pmdata 包含 3*w*h 字节的 RGB 数据
        for (int j = 0; j < h; j++) {
            for (int i = 0; i < w; i++) {
                BYTE* pixel = bitmapdataBGRA + (y + j) * stride + (x + i) * 4;
                pixel[0] = pmdata[(j * w + i) * 3 + 2]; // B
                pixel[1] = pmdata[(j * w + i) * 3 + 1]; // G
                pixel[2] = pmdata[(j * w + i) * 3 + 0]; // R
                pixel[3] = 255; // A
            }
        }
    }
    else {
        // pmdata 包含 (w/pmscale)*(h/pmscale) 字节的细胞状态数据
        for (int j = 0; j < h / pmscale; j++) {
            for (int i = 0; i < w / pmscale; i++) {
                unsigned char state = (unsigned char)pmdata[j * (w / pmscale) + i];
                for (int sj = 0; sj < pmscale; sj++) {
                    for (int si = 0; si < pmscale; si++) {
                        BYTE* pixel = bitmapdataBGRA + (y + j * pmscale + sj) * stride + (x + i * pmscale + si) * 4;
                        pixel[0] = b[state]; // B
                        pixel[1] = g[state]; // G
                        pixel[2] = r[state]; // R
                        pixel[3] = 255; // A
                    }
                }
            }
        }
    }
}

