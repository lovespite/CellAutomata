// dllmain.cpp : 定义 DLL 应用程序的入口点。
#include "pch.h"
#include "hlifealgo.h"
#include <iostream>
#include <fstream>

#include <Windows.h>
#include <string>

static lifealgo* algo = nullptr;
BIGINT population = 0;

extern "C" __declspec(dllexport) void CreateNewUniverse(int width, int height)
{
    if (algo != nullptr)
    {
        population = 0;
        delete algo;
    }

    algo = new hlifealgo();
    algo->init(0, 0);
    algo->setinc(1);
}

extern "C" __declspec(dllexport) void SetCell(int x, int y, bool alive)
{
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

extern "C" __declspec(dllexport) void NextStep()
{
    if (algo == nullptr)
    {
        return;
    }

    population = algo->nextstep();
}

extern "C" __declspec(dllexport) void GetPopulation(BIGINT * pop)
{
    if (algo == nullptr)
    {
        return;
    }

    *pop = population;
}

extern "C" __declspec(dllexport) void DestroyUniverse()
{
    if (algo != nullptr)
    {
        delete algo;
        algo = nullptr;
        population = 0;
    }
}

extern "C" __declspec(dllexport) int GetRegion(int x, int y, int w, int h, BYTE * buffer, long bufferLen)
{
    if (algo == nullptr)
    {
        return -1;
    }

    if (bufferLen < w * h / 8)
    {
        // buffer is too small
        return -2;
    }

    int byteIndex = 0;
    int bitIndex = 0;

    memset(buffer, 0, bufferLen); // clear buffer
    BIGINT pop = 0;

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

            bitIndex++;
            if (bitIndex == 8)
            {
                bitIndex = 0;
                byteIndex++;
            }
        }
    }

    return pop;
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