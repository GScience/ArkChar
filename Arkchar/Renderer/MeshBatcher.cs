using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpGL;
using SharpGL.Enumerations;

namespace Arkchar.Renderer
{
    public class MeshBatcher
    {
        private readonly List<MeshItem> items;
        private readonly Queue<MeshItem> freeItems;
        private VertexPositionColorTexture[] vertexArray = { };
        private short[] triangles = { };

        private uint _vertexBuffer;
        private int _bufferSize;

        public MeshBatcher(OpenGL gl)
        {
            items = new List<MeshItem>(256);
            freeItems = new Queue<MeshItem>(256);
            EnsureCapacity(256, 512);

            var bufferList = new uint[1];
            gl.GenBuffers(1, bufferList);
            _vertexBuffer = bufferList[0];
        }

        /// <summary>Returns a pooled MeshItem.</summary>
        public MeshItem NextItem(int vertexCount, int triangleCount)
        {
            MeshItem item = freeItems.Count > 0 ? freeItems.Dequeue() : new MeshItem();
            if (item.vertices.Length < vertexCount) item.vertices = new VertexPositionColorTexture[vertexCount];
            if (item.triangles.Length < triangleCount) item.triangles = new int[triangleCount];
            item.vertexCount = vertexCount;
            item.triangleCount = triangleCount;
            items.Add(item);
            return item;
        }

        private void EnsureCapacity(int vertexCount, int triangleCount)
        {
            if (vertexArray.Length < vertexCount) vertexArray = new VertexPositionColorTexture[vertexCount];
            if (triangles.Length < triangleCount) triangles = new short[triangleCount];
        }

        public void Draw(OpenGL gl)
        {
            gl.MatrixMode(MatrixMode.Projection);
            gl.LoadIdentity();
            gl.Ortho(-400, 400, -400, 400, -1, 1);

            if (items.Count == 0) return;

            int itemCount = items.Count;
            int vertexCount = 0, triangleCount = 0;
            for (int i = 0; i < itemCount; i++)
            {
                MeshItem item = items[i];
                vertexCount += item.vertexCount;
                triangleCount += item.triangleCount;
            }
            EnsureCapacity(vertexCount, triangleCount);

            vertexCount = 0;
            triangleCount = 0;
            uint lastTexture = 0;
            for (var i = 0; i < itemCount; i++)
            {
                var item = items[i];
                var itemVertexCount = item.vertexCount;

                if (item.texture != lastTexture || vertexCount + itemVertexCount > short.MaxValue)
                {
                    FlushVertexArray(gl, vertexCount, triangleCount);
                    vertexCount = 0;
                    triangleCount = 0;
                    lastTexture = item.texture;
                    BindTexture(gl, lastTexture);
                }

                int[] itemTriangles = item.triangles;
                int itemTriangleCount = item.triangleCount;
                for (int ii = 0, t = triangleCount; ii < itemTriangleCount; ii++, t++)
                    triangles[t] = (short)(itemTriangles[ii] + vertexCount);
                triangleCount += itemTriangleCount;

                Array.Copy(item.vertices, 0, vertexArray, vertexCount, itemVertexCount);
                vertexCount += itemVertexCount;

                item.texture = 0;
                freeItems.Enqueue(item);
            }
            FlushVertexArray(gl, vertexCount, triangleCount);
            items.Clear();
        }

        private void BindTexture(OpenGL gl, uint texture)
        {
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, texture);
        }

        private void FlushVertexArray(OpenGL gl, int vertexCount, int triangleCount)
        {
            if (vertexCount == 0 || triangleCount == 0)
                return;

            gl.PushMatrix();

            gl.Translate(0, - 300, 0);

            var bufferArrayData = vertexArray;

            gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, _vertexBuffer);

            if (_bufferSize < sizeof(float) * vertexCount * 8)
                gl.BufferData(
                    OpenGL.GL_ARRAY_BUFFER,
                    sizeof(float) * vertexCount * 8,
                    Marshal.UnsafeAddrOfPinnedArrayElement(bufferArrayData, 0),
                    OpenGL.GL_DYNAMIC_DRAW);
            else
                gl.BufferSubData(
                    OpenGL.GL_ARRAY_BUFFER, 
                    0, 
                    sizeof(float) * vertexCount * 8,
                    Marshal.UnsafeAddrOfPinnedArrayElement(bufferArrayData, 0));

            _bufferSize = Math.Max(sizeof(float) * vertexCount * 8, _bufferSize);

            gl.EnableClientState(OpenGL.GL_VERTEX_ARRAY);
            gl.EnableClientState(OpenGL.GL_COLOR_ARRAY);
            gl.EnableClientState(OpenGL.GL_TEXTURE_COORD_ARRAY);

            gl.VertexPointer(2, OpenGL.GL_FLOAT, 8 * sizeof(float), (IntPtr)0);
            gl.ColorPointer(4, OpenGL.GL_FLOAT, 8 * sizeof(float), (IntPtr)(2 * sizeof(float)));
            gl.TexCoordPointer(2, OpenGL.GL_FLOAT, 8 * sizeof(float), (IntPtr)(6 * sizeof(float)));

            gl.DrawElements(
                OpenGL.GL_TRIANGLES, triangleCount, 
                OpenGL.GL_UNSIGNED_SHORT,
                Marshal.UnsafeAddrOfPinnedArrayElement(triangles, 0));

            gl.DisableClientState(OpenGL.GL_VERTEX_ARRAY);
            gl.DisableClientState(OpenGL.GL_COLOR_ARRAY);
            gl.DisableClientState(OpenGL.GL_TEXTURE_COORD_ARRAY);

            gl.PopMatrix();
        }
    }
}
