using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace TestNVR
{
    public static class Win32
    {
        [DllImport("libNvr2mp4.dll", EntryPoint = "GenerateMP4File", CallingConvention = CallingConvention.StdCall)]
        public extern static bool GenerateMP4File(string sfilePath, byte[] pBuffer, ulong dwBufSize, long dwDataType);

        [DllImport("libNvr2mp4.dll", EntryPoint = "CloseMP4File", CallingConvention = CallingConvention.StdCall)]
        public extern static void CloseMP4File();
    }
}
