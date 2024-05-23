using System.Runtime.InteropServices;
using CellAutomata.Render;

namespace CellAutomata.Algos;

internal static partial class HashLifeMapStatic
{
    const string HashLifeLib = "Hashlifelib/hashlife.dll";

    public static readonly string LibVersion;

    static HashLifeMapStatic()
    {
        var buffer = new byte[256];
        
        HashLifeMapStatic.Version(buffer, buffer.Length);
        var version = System.Text.Encoding.UTF8.GetString(buffer).TrimEnd('\0'); 
        LibVersion = version;

        buffer = null;
    }

    [LibraryImport(HashLifeLib)]
    internal static partial void AtViewport(int renderCtx, int px, int py, ref long row, ref long col);

    [LibraryImport(HashLifeLib, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int CreateNewUniverse(string rule);

    [LibraryImport(HashLifeLib, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int SetUniverseRule(int index, string rule);

    [LibraryImport(HashLifeLib)]
    internal static partial void GetUniverseRule(int index, IntPtr buffer, int bufferSize);

    [LibraryImport(HashLifeLib)]
    internal static partial int CreateRender(int w, int h, nint canvas, int use3d);

    [LibraryImport(HashLifeLib)]
    internal static partial void SuspendRender(int index);

    [LibraryImport(HashLifeLib)]
    internal static partial void ResumeRender(int index);

    [LibraryImport(HashLifeLib)]
    internal static partial void DestroyRender(int renderCtx);

    [LibraryImport(HashLifeLib)]
    internal static partial void ResizeViewport(int renderCtx, int w, int h);

    [LibraryImport(HashLifeLib)]
    internal static partial void DestroyUniverse(int index);

    [LibraryImport(HashLifeLib)]
    internal static partial void DrawRegionBitmap(int index, IntPtr bitmapBuffer, long stride, int x, int y, int w,
        int h);

    [LibraryImport(HashLifeLib)]
    internal static partial void DrawRegionBitmapBGRA(int index, IntPtr bitmapBuffer, long stride, int x, int y, int w,
        int h);

    [LibraryImport(HashLifeLib, StringMarshalling = StringMarshalling.Utf16)]
    internal static partial void DrawViewport(int renderCtx, int index, int mag, int x, int y, int w, int h,
        ref ViewInfo viewInfo, string text);

    [LibraryImport(HashLifeLib)]
    internal static partial void FindEdges(int index, ref Int64 top, ref Int64 left, ref Int64 bottom, ref Int64 right);

    [LibraryImport(HashLifeLib)]
    internal static partial int GetCell(int index, int x, int y);

    [LibraryImport(HashLifeLib)]
    internal static partial void GetPopulation(int index, ref ulong pop);

    [LibraryImport(HashLifeLib)]
    internal static partial ulong GetRegion(int index, int x, int y, int w, int h, [In, Out] byte[] buffer,
        int bufferSize);

    [LibraryImport(HashLifeLib)]
    internal static partial void NextStep(int index, ref ulong pop);

    [LibraryImport(HashLifeLib)]
    internal static partial void SetCell(int index, int x, int y, [MarshalAs(UnmanagedType.Bool)] bool value);

    [LibraryImport(HashLifeLib)]
    internal static partial void SetRegion(int index, int x, int y, int w, int h, [In] byte[] buffer, int bufferSize);

    [LibraryImport(HashLifeLib)]
    internal static partial void SetThreadCount(int index, int count);

    [LibraryImport(HashLifeLib)]
    internal static partial void Version([In, Out] byte[] buffer, int bufferSize);

    [LibraryImport(HashLifeLib, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int ReadRleFile(int index, string filename, IntPtr errBuffer, ulong errBufferSize);
}