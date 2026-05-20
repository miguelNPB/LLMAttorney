using System;
using System.Runtime.InteropServices;

/// <summary>
/// Clase para mostrar la ventana nativa de Windows para seleccionar archivos
/// </summary>
public static class WinFileSelect
{
    [DllImport("comdlg32.dll", EntryPoint = "GetOpenFileNameW", CharSet = CharSet.Unicode)]
    private static extern bool GetOpenFileName(ref OPENFILENAME lpofn);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct OPENFILENAME
    {
        public int lStructSize;
        public IntPtr hwndOwner;
        public IntPtr hInstance;
        public string lpstrFilter;
        public string lpstrCustomFilter;
        public int nMaxCustFilter;
        public int nFilterIndex;
        public IntPtr lpstrFile;
        public int nMaxFile;
        public string lpstrFileTitle;
        public int nMaxFileTitle;
        public string lpstrInitialDir;
        public string lpszTitle;
        public int Flags;
        public short nFileOffset;
        public short nFileExtension;
        public string lpstrDefExt;
        public IntPtr lCustData;
        public IntPtr lpfnHook;
        public string lpTemplateName;
        public IntPtr pvReserved;
        public int dwReserved;
        public int FlagsEx;
    }

    // flags
    private const int OFN_FILEMUSTEXIST = 0x00001000;
    private const int OFN_PATHMUSTEXIST = 0x00000800;
    private const int OFN_HIDEREADONLY = 0x00000004;
    private const int OFN_NOCHANGEDIR = 0x00000008;

    /// <summary>
    /// Abre un dialogo para seleccionar un fichero
    /// </summary>
    /// <param name="title">Titulo de la ventana</param>
    /// <param name="initialDir">Ruta inicial donde se abrira el navegador</param>
    /// <returns>La ruta completa del archivo seleccionado o null si se cancela</returns>
    public static string Open(string title = "Select a file", string initialDir = null, string extension = null)
    {
        string originalDirectory = Environment.CurrentDirectory;

        var ofn = new OPENFILENAME();
        ofn.lStructSize = Marshal.SizeOf(ofn);

        if (!string.IsNullOrEmpty(extension))
        {
            string extLimpia = extension.Replace(".", "").ToLower();
            string extMayus = extLimpia.ToUpper();
            ofn.lpstrFilter = $"{extMayus} Files (*.{extLimpia})\0*.{extLimpia}\0All Files (*.*)\0*.*\0";
        }
        else
        {
            ofn.lpstrFilter = "Supported Files (*.pdf;*.json)\0*.pdf;*.json\0PDF Files (*.pdf)\0*.pdf\0JSON Files (*.json)\0*.json\0All Files (*.*)\0*.*\0";
        }

        ofn.nFilterIndex = 1;

        int maxBufferSize = 32768;
        IntPtr bufferPtr = Marshal.AllocHGlobal(maxBufferSize * sizeof(char));

        byte[] zeroBuffer = new byte[maxBufferSize * sizeof(char)];
        Marshal.Copy(zeroBuffer, 0, bufferPtr, zeroBuffer.Length);

        ofn.lpstrFile = bufferPtr;
        ofn.nMaxFile = maxBufferSize;

        ofn.lpszTitle = title;
        ofn.lpstrInitialDir = initialDir;
        ofn.Flags = OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_HIDEREADONLY | OFN_NOCHANGEDIR;

        try
        {
            if (GetOpenFileName(ref ofn))
            {
                return Marshal.PtrToStringUni(ofn.lpstrFile);
            }
            return null;
        }
        finally
        {
            if (bufferPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(bufferPtr);
            }

            if (Environment.CurrentDirectory != originalDirectory)
            {
                Environment.CurrentDirectory = originalDirectory;
            }
        }
    }
}