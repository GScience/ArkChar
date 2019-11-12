using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpGL;
using Spine;

namespace Arkchar.Renderer
{
    public class SkeletonMeshRenderer
    {
        private const int TL = 0;
        private const int TR = 1;
        private const int BL = 2;
        private const int BR = 3;

        MeshBatcher batcher;
        float[] vertices = new float[8];
        int[] quadTriangles = { 0, 1, 2, 1, 3, 2 };

        private bool premultipliedAlpha;

        public bool PremultipliedAlpha { get { return premultipliedAlpha; } set { premultipliedAlpha = value; } }

        public SkeletonMeshRenderer(OpenGL gl)
        {
            batcher = new MeshBatcher(gl);
            Bone.yDown = false;
        }

        public void Draw(OpenGL gl, Skeleton skeleton)
        {
            if (skeleton == null)
                return;

            float[] vertices = this.vertices;
            var drawOrder = skeleton.DrawOrder;
            var drawOrderItems = skeleton.DrawOrder.Items;
            float skeletonR = skeleton.R, skeletonG = skeleton.G, skeletonB = skeleton.B, skeletonA = skeleton.A;
            for (int i = 0, n = drawOrder.Count; i < n; i++)
            {
                Slot slot = drawOrderItems[i];
                Attachment attachment = slot.Attachment;
                if (attachment is RegionAttachment)
                {
                    RegionAttachment regionAttachment = (RegionAttachment)attachment;

                    MeshItem item = batcher.NextItem(4, 6);
                    item.triangles = quadTriangles;
                    VertexPositionColorTexture[] itemVertices = item.vertices;

                    AtlasRegion region = (AtlasRegion)regionAttachment.RendererObject;
                    item.texture = (uint) region.page.rendererObject;

                    Color color;
                    float a = skeletonA * slot.A * regionAttachment.A;
                    if (premultipliedAlpha)
                    {
                        color = new Color(
                                skeletonR * slot.R * regionAttachment.R * a,
                                skeletonG * slot.G * regionAttachment.G * a,
                                skeletonB * slot.B * regionAttachment.B * a, a);
                    }
                    else
                    {
                        color = new Color(
                                skeletonR * slot.R * regionAttachment.R,
                                skeletonG * slot.G * regionAttachment.G,
                                skeletonB * slot.B * regionAttachment.B, a);
                    }
                    itemVertices[TL].color = color;
                    itemVertices[BL].color = color;
                    itemVertices[BR].color = color;
                    itemVertices[TR].color = color;

                    regionAttachment.ComputeWorldVertices(slot.Bone, vertices);
                    itemVertices[TL].position = new Vector2(vertices[RegionAttachment.X1], vertices[RegionAttachment.Y1]);
                    itemVertices[BL].position = new Vector2(vertices[RegionAttachment.X2], vertices[RegionAttachment.Y2]);
                    itemVertices[BR].position = new Vector2(vertices[RegionAttachment.X3], vertices[RegionAttachment.Y3]);
                    itemVertices[TR].position = new Vector2(vertices[RegionAttachment.X4], vertices[RegionAttachment.Y4]);

                    float[] uvs = regionAttachment.UVs;
                    itemVertices[TL].textureCoordinate = new Vector2(uvs[RegionAttachment.X1], uvs[RegionAttachment.Y1]);
                    itemVertices[BL].textureCoordinate = new Vector2(uvs[RegionAttachment.X2], uvs[RegionAttachment.Y2]);
                    itemVertices[BR].textureCoordinate = new Vector2(uvs[RegionAttachment.X3], uvs[RegionAttachment.Y3]);
                    itemVertices[TR].textureCoordinate = new Vector2(uvs[RegionAttachment.X4], uvs[RegionAttachment.Y4]);
                }
                else if (attachment is MeshAttachment)
                {
                    MeshAttachment mesh = (MeshAttachment)attachment;
                    int vertexCount = mesh.WorldVerticesLength;
                    if (vertices.Length < vertexCount) vertices = new float[vertexCount];
                    mesh.ComputeWorldVertices(slot, vertices);

                    int[] triangles = mesh.Triangles;
                    MeshItem item = batcher.NextItem(vertexCount, triangles.Length);
                    item.triangles = triangles;

                    AtlasRegion region = (AtlasRegion)mesh.RendererObject;
                    item.texture = (uint)region.page.rendererObject;

                    Color color;
                    float a = skeletonA * slot.A * mesh.A;
                    if (premultipliedAlpha)
                    {
                        color = new Color(
                                skeletonR * slot.R * mesh.R * a,
                                skeletonG * slot.G * mesh.G * a,
                                skeletonB * slot.B * mesh.B * a, a);
                    }
                    else
                    {
                        color = new Color(
                                skeletonR * slot.R * mesh.R,
                                skeletonG * slot.G * mesh.G,
                                skeletonB * slot.B * mesh.B, a);
                    }

                    float[] uvs = mesh.UVs;
                    VertexPositionColorTexture[] itemVertices = item.vertices;
                    for (int ii = 0, v = 0; v < vertexCount; ii++, v += 2)
                    {
                        itemVertices[ii].color = color;
                        itemVertices[ii].position = new Vector2(vertices[v], vertices[v + 1]);
                        itemVertices[ii].textureCoordinate = new Vector2(uvs[v], uvs[v + 1]);
                    }
                }
            }

            batcher.Draw(gl);
        }
    }
}
