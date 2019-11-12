using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Arkchar.Renderer
{
    [StructLayout(LayoutKind.Explicit, Pack = 4)]
    public struct Color
    {
        [FieldOffset(0)]
        public float r;
        [FieldOffset(4)]
        public float g;
        [FieldOffset(8)]
        public float b;
        [FieldOffset(12)]
        public float a;

        public Color(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }
    }
}
