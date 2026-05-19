using System;
using System.Runtime.InteropServices;
using System.Text;

/// <summary>
/// Clase para sacar una ventana de windows para seleccionar una carpeta
/// </summary>
public static class WinDirSelect
{
    [DllImport("shell32.dll", EntryPoint = "SHBrowseForFolderW", CharSet = CharSet.Unicode)]
    private static extern IntPtr SHBrowseForFolder(ref BROWSEINFO lpbi);

    [DllImport("shell32.dll", EntryPoint = "SHGetPathFromIDListW", CharSet = CharSet.Unicode)]
    private static extern bool SHGetPathFromIDList(IntPtr pidl, StringBuilder pszPath);

    [DllImport("ole32.dll")]
    private static extern void CoTaskMemFree(IntPtr pv);

    [DllImport("user32.dll", EntryPoint = "SendMessageW", CharSet = CharSet.Unicode)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", EntryPoint = "SendMessageW", CharSet = CharSet.Unicode)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, string lParam);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct BROWSEINFO
    {
        public IntPtr hwndOwner;
        public IntPtr pidlRoot;
        public string pszDisplayName;
        public string lpszTitle;
        public uint ulFlags;
        public BrowseCallbackProc lpfn;
        public IntPtr lParam;
        public int iImage;
    }

    private delegate int BrowseCallbackProc(IntPtr hwnd, uint uMsg, IntPtr lParam, IntPtr lpData);

    private const uint BIF_FLAGS = 0x0001 | 0x0040;
    private const uint BFFM_INITIALIZED = 1;
    private const uint BFFM_SETSELECTION = 0x0467;

    private static string _initialPath;

    private static int BrowseCallback(IntPtr hwnd, uint uMsg, IntPtr lParam, IntPtr lpData)
    {
        if (uMsg == BFFM_INITIALIZED && !string.IsNullOrEmpty(_initialPath))
        {
            SendMessage(hwnd, BFFM_SETSELECTION, (IntPtr)1, _initialPath);
        }
        return 0;
    }

    public static string Open(string title = "Select folder", string initialPath = null)
    {
        _initialPath = initialPath;

        var callback = new BrowseCallbackProc(BrowseCallback);

        var bi = new BROWSEINFO
        {
            hwndOwner = IntPtr.Zero,
            pidlRoot = IntPtr.Zero,
            pszDisplayName = new string('\0', 260),
            lpszTitle = title,
            ulFlags = BIF_FLAGS,
            lpfn = callback,
            lParam = IntPtr.Zero,
        };

        IntPtr pidl = SHBrowseForFolder(ref bi);
        if (pidl == IntPtr.Zero) return null;

        var sb = new StringBuilder(260);
        bool ok = SHGetPathFromIDList(pidl, sb);
        CoTaskMemFree(pidl);

        GC.KeepAlive(callback);

        return ok ? sb.ToString() : null;
    }
}