// dllmain.cpp : 定义 DLL 应用程序的入口点。
#include "pch.h"
#include "hlifealgo.h" 

#include <Windows.h>
#include <string>
#include <algorithm>
#include <vector>
#include <cmath> 
#include <cstringt.h>

std::string _version = "alpha1.1";

static std::vector<lifealgo*> algos;

extern "C" __declspec(dllexport) void Version(char* versionBuffer, int bufferSize)
{
    memcpy_s(versionBuffer, bufferSize, _version.c_str(), _version.size());
}

extern "C" __declspec(dllexport) int CreateNewUniverse(const char* rule)
{

    auto algo = new hlifealgo();
    algo->init(0, 0);
    algo->setrule(rule);
    algo->setinc(1);

    auto index = algos.size();
    algos.push_back(algo);

    return index;
}

extern "C" __declspec(dllexport) void SetCell(int index, int x, int y, bool alive)
{
    if (index < 0 || index >= algos.size())
    {
        return;
    }

    auto algo = algos[index];

    if (algo == nullptr)
    {
        return;
    }

    if (alive)
    {
        algo->setcell(x, y);
    }
    else
    {
        algo->clearcell(x, y);
    }
}

extern "C" __declspec(dllexport) int GetCell(int index, int x, int y)
{
    if (index < 0 || index >= algos.size())
    {
        return -2;
    }

    auto algo = algos[index];

    if (algo == nullptr)
    {
        return -1;
    }

    return algo->getcell(x, y);
}

extern "C" __declspec(dllexport) void NextStep(int index, BIGINT * pop)
{
    if (index < 0 || index >= algos.size())
    {
        return;
    }

    auto algo = algos[index];

    if (algo == nullptr)
    {
        return;
    }

    *pop = algo->nextstep();
}

extern "C" __declspec(dllexport) void GetPopulation(int index, BIGINT * pop)
{
    if (index < 0 || index >= algos.size())
    {
        return;
    }

    auto algo = algos[index];

    if (algo == nullptr)
    {
        return;
    }

    *pop = algo->getpopulation();
}

extern "C" __declspec(dllexport) void DestroyUniverse(int index)
{
    if (index < 0 || index >= algos.size())
    {
        return;
    }

    auto algo = algos[index];

    if (algo != nullptr)
    {
        delete algo;

        algos[index] = nullptr;
        algo = nullptr;
    }
}

extern "C" __declspec(dllexport) BIGINT GetRegion(int index, int x, int y, int w, int h, BYTE * buffer, long bufferLen)
{
    if (index < 0 || index >= algos.size())
    {
        return -3;
    }

    auto algo = algos[index];

    if (algo == nullptr)
    {
        return -1;
    }

    if (bufferLen < w * h / 8)
    {
        // buffer is too small
        return -2;
    }
    BIGINT pop = 0;

    int byteIndex = 0;
    int bitIndex = 0;

    // memset(buffer, 0, bufferLen); // clear buffer

    for (int i = 0; i < h; i++)
    {
        for (int j = 0; j < w; j++)
        {
            if (algo->getcell(x + j, y + i))
            {
                buffer[byteIndex] |= (1 << bitIndex);
                ++pop;
            }
            else
            {
                buffer[byteIndex] &= ~(1 << bitIndex);
            }

            ++bitIndex;
            if (bitIndex == 8)
            {
                bitIndex = 0;
                ++byteIndex;
            }
        }
    }

    return pop;
}

extern "C" __declspec(dllexport) void SetRegion(int index, int x, int y, int w, int h, BYTE * buffer, long bufferLen)
{
    if (index < 0 || index >= algos.size())
    {
        return;
    }

    auto algo = algos[index];

    if (algo == nullptr)
    {
        return;
    }

    if (bufferLen < w * h / 8)
    {
        // buffer is too small
        return;
    }

    int byteIndex = 0;
    int bitIndex = 0;

    for (int i = 0; i < h; i++)
    {
        for (int j = 0; j < w; j++)
        {
            if (buffer[byteIndex] & (1 << bitIndex))
            {
                algo->setcell(x + j, y + i);
            }
            else
            {
                algo->clearcell(x + j, y + i);
            }

            ++bitIndex;
            if (bitIndex == 8)
            {
                bitIndex = 0;
                ++byteIndex;
            }
        }
    }
}

typedef long long BigInt;

extern "C" __declspec(dllexport) void FindEdges(int index, BigInt * t, BigInt * l, BigInt * b, BigInt * r)
{
    if (index < 0 || index >= algos.size())
    {
        return;
    }

    auto algo = algos[index];

    if (algo == nullptr)
    {
        return;
    }

    bigint top, left, bottom, right;
    algo->findedges(&top, &left, &bottom, &right);

    *t = top.toint64();
    *l = left.toint64();
    *b = bottom.toint64();
    *r = right.toint64();
}

// 定义导出函数接口
extern "C" __declspec(dllexport) void DrawRegionBitmap(int index, BYTE * bitmapBuffer, long stride, int x, int y, int w, int h)
{
    // 检查生命游戏实例索引是否有效

    if (index < 0 || index >= algos.size())
    {
        return;
    }

    // 获取生命游戏实例
    auto algo = algos[index];

    // 检查生命游戏算法实例是否存在
    if (algo == nullptr)
    {
        return;
    }

    // 从底部开始填充（Bitmap像素数据通常是倒置的，即从底部开始到顶部）
    for (int row = 0; row < h; ++row)
    {
        // 获取当前位图行的偏移量
        int rowOffset = (h - 1 - row) * stride;

        // 遍历每一列
        for (int col = 0; col < w; ++col)
        {
            // 确定当前位图缓冲区的位置
            int pixelByte = rowOffset + col / 8;
            int pixelBit = 7 - col % 8;

            // 检查当前生命游戏的细胞是否活跃
            if (algo->getcell(x + col, y + row))
            {
                bitmapBuffer[pixelByte] |= (1 << pixelBit); // 设置位
            }
            else
            {
                bitmapBuffer[pixelByte] &= ~(1 << pixelBit); // 清除位
            }
        }
    }
}

extern "C" __declspec(dllexport) void DrawRegionBitmapBGRA(
    int index,
    uint8_t * bitmapBuffer, int stride,
    int x, int y, int w, int h)
{
    // 检查生命游戏实例索引是否有效
    if (index < 0 || index >= algos.size())
    {
        return;
    }

    // 获取生命游戏实例
    auto algo = algos[index];

    if (algo == nullptr)
    {
        return;
    }

    // 从顶部开始填充（正常的位图绘制顺序）
    for (int row = 0; row < h; ++row)
    {
        int rowOffset = row * stride;

        for (int col = 0; col < w; ++col)
        {
            int pixelOffset = rowOffset + col * 4; // BGRA，每个像素占4个字节

            if (algo->getcell(x + col, y + row))
            {
                bitmapBuffer[pixelOffset] = 255;     // B
                bitmapBuffer[pixelOffset + 1] = 255; // G
                bitmapBuffer[pixelOffset + 2] = 255; // R
                bitmapBuffer[pixelOffset + 3] = 255; // A
            }
            else
            {
                bitmapBuffer[pixelOffset] = 0;     // B
                bitmapBuffer[pixelOffset + 1] = 0; // G
                bitmapBuffer[pixelOffset + 2] = 0; // R
                bitmapBuffer[pixelOffset + 3] = 255; // A
            }
        }
    }
}

static std::vector<dcrender*> renderctxs;

extern "C" __declspec(dllexport) int CreateRender(int w, int h, HWND hWnd)
{
    auto render = new dcrender(w, h, hWnd);
    renderctxs.push_back(render);

    return renderctxs.size() - 1;
}

extern "C" __declspec(dllexport) void DestroyRender(int index)
{
    if (index < 0 || index >= renderctxs.size())
    {
        return;
    }

    auto render = renderctxs[index];

    if (render != nullptr)
    {
        delete render;

        renderctxs[index] = nullptr;
        render = nullptr;
    }
}

extern "C" __declspec(dllexport) void DrawRegion(
    int renderIndex,
    int index, int mag,
    int x, int y, int w, int h, VIEWINFO * selection, const wchar_t* text)
{
    // 检查渲染上下文索引是否有效
    if (renderIndex < 0 || renderIndex >= renderctxs.size())
    {
        return;
    }

    // 获取渲染上下文
    auto render = renderctxs[renderIndex];

    if (render == nullptr)
    {
        return;
    }

    // 检查生命游戏实例索引是否有效
    if (index < 0 || index >= algos.size())
    {
        return;
    }

    // 获取生命游戏实例
    auto algo = algos[index];

    if (algo == nullptr) return;

    viewport vp(w, h);

    vp.moveto(x, y);
    vp.setmag(mag);

    render->begindraw();
    render->clear();

    algo->draw(vp, *render);

    if (mag > 3)
        render->drawgridlines(pow(2, mag));

    if (selection != nullptr && selection->EMPTY == 0)
    {
        render->drawselection(selection);
    }

    render->drawtext(10, 10, text);
    render->enddraw();
}


BOOL APIENTRY DllMain(HMODULE hModule,
    DWORD  ul_reason_for_call,
    LPVOID lpReserved
)
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}