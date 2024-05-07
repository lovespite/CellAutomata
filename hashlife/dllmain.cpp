// dllmain.cpp : 定义 DLL 应用程序的入口点。
#include "pch.h"
#include "hlifealgo.h"
#include <iostream>
#include <fstream>

#include <Windows.h>
#include <string>


std::string _version = "alpha1.0";

static int threadcount = 1;
static lifealgo* algo = nullptr;
BIGINT population = 0;

extern "C" __declspec(dllexport) void Version(char* versionBuffer, int bufferSize)
{
    memcpy_s(versionBuffer, bufferSize, _version.c_str(), _version.size());
}

extern "C" __declspec(dllexport) void SetThreadCount(int count)
{
    threadcount = count;
}

extern "C" __declspec(dllexport) void CreateNewUniverse(const char* rule)
{
    if (algo != nullptr)
    {
        population = 0;
        delete algo;
    }

    algo = new hlifealgo();
    algo->init(0, 0);
    algo->setrule(rule);
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

extern "C" __declspec(dllexport) int GetCell(int x, int y)
{
    if (algo == nullptr)
    {
        return -1;
    }

    return algo->getcell(x, y);
}

extern "C" __declspec(dllexport) void NextStep(BIGINT * pop)
{
    if (algo == nullptr)
    {
        return;
    }

    population = algo->nextstep();
    *pop = population;
}

extern "C" __declspec(dllexport) void GetPopulation(BIGINT * pop)
{
    if (algo == nullptr)
    {
        return;
    }

    *pop = algo->getpopulation();
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

extern "C" __declspec(dllexport) void SetRegion(int x, int y, int w, int h, BYTE * buffer, long bufferLen)
{
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