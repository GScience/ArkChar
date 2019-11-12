using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arkchar.Renderer
{

    public class MeshItem
    {
        public uint texture;
        public int vertexCount, triangleCount;
        public VertexPositionColorTexture[] vertices = { };
        public int[] triangles = { };
    }
}
