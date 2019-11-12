using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Arkchar.Renderer
{
    [StructLayout(LayoutKind.Explicit, Pack = 4)]
    public struct Vector2
    {
        [FieldOffset(0)] 
        public float x;

        [FieldOffset(4)] 
        public float y;

        public Vector2(float x,float y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
