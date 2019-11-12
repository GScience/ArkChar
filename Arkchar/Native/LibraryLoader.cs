using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Arkchar.Native
{
    public static class LibraryLoader
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string path);

        public static void Init()
        {
            string dllPath;

            if (Environment.Is64BitProcess)
                dllPath = Environment.CurrentDirectory + "\\Native\\x64\\";
            else
                dllPath = Environment.CurrentDirectory + "\\Native\\x86\\";

            LoadLibrary(dllPath + "SDL2.dll");
            LoadLibrary(dllPath + "SDL2_image.dll");
            LoadLibrary(dllPath + "zlib1.dll");
            LoadLibrary(dllPath + "libpng16-16.dll");
            LoadLibrary(dllPath + "libjpeg-9.dll");
            LoadLibrary(dllPath + "libtiff-5.dll");
            LoadLibrary(dllPath + "libwebp-7.dll");
        }
    }
}
