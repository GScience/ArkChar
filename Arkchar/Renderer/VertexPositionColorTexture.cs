using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Arkchar.Renderer
{
    [StructLayout(LayoutKind.Explicit, Pack = 4)]
    public struct VertexPositionColorTexture
    {
        [FieldOffset(0)] 
        public Vector2 position;

        [FieldOffset(8)]
        public Color color;

        [FieldOffset(24)]
        public Vector2 textureCoordinate;
    }
}
