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

int currwd, currht;              // current width and height of viewport  

liferender::~liferender()
{
}

bitmaprender::~bitmaprender() {}

void bitmaprender::killrect(int x, int y, int w, int h)
{
}

void bitmaprender::pixblit(int x, int y, int w, int h, char* pmdata, int pmscale)
{
    // is Tom's hashdraw code doing unnecessary work???
    if (x >= currwd || y >= currht) return;
    if (x + w <= 0 || y + h <= 0) return;

    // stride is the horizontal pixel width of the image data
    int stride = (pmscale == 1) ? w : w / pmscale;

    // clip pixmap to be drawn against viewport:
    if (pmscale == 1)
    {
        // pmdata contains 3 bytes per pixel
        if (x < 0) {
            pmdata -= 3 * x;
            w += x;
            x = 0;
        }
        if (y < 0) {
            pmdata -= 3 * y * stride;
            h += y;
            y = 0;
        }
        if (w > currwd) w = currwd;
        if (h > currht) h = currht;
    }
    else
    {
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


}

// deadcolors
const unsigned char colors[] = {
    0, 255, // r: dead, alive 
    0, 255, // g: dead, alive
    0, 255  // b: dead, alive
};

void bitmaprender::getcolors(unsigned char** r, unsigned char** g, unsigned char** b)
{
    *r = (unsigned char*)colors;
    *g = (unsigned char*)colors + 2;
    *b = (unsigned char*)colors + 4;
}
