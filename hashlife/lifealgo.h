#ifndef LIFEALGO_H
#define LIFEALGO_H
#include "util.h"
#include "bigint.h" 
#include "liferender.h"
#include "viewport.h"

typedef unsigned long long BIGINT;

class lifealgo {
public:
    lifealgo() { increment = 1; }
    virtual const char* setrule(const char* s) = 0;
    virtual const char* getrule() = 0;
    virtual void init(int w, int h) = 0;
    virtual void setcell(int x, int y) = 0;
    virtual int getcell(int x, int y) = 0;
    virtual void clearcell(int x, int y) = 0;
    virtual BIGINT getpopulation() = 0;
    virtual int isEmpty() = 0;
    virtual void endofpattern() = 0;
    virtual void swap() = 0;
    virtual void draw(viewport& vp, liferender& renderer) = 0;
    virtual void findedges(bigint* t, bigint* l, bigint* b, bigint* r) = 0;
    virtual BIGINT nextstep(int i, int n, int needpop) = 0;
    virtual void setinc(int inc) { increment = inc; }
    virtual void lowerRightPixel(bigint& x, bigint& y, int mag) = 0;
    virtual const char* readmacrocell(char* line) = 0;
    virtual BIGINT nextstep() {
        for (int i = 1; i < increment; i++) {
            nextstep(0, 1, 0);
            swap();
        }
        BIGINT r = nextstep(0, 1, 1);
        swap();
        return r;
    }
    int increment;
};
#endif