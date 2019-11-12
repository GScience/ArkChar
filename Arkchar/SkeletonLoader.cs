using Spine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpGL;

namespace Arkchar
{
    public static class SkeletonLoader
    {
        public static OpenGL gl;

        public static string ResourceFloder
        {
            get
            {
                return Environment.CurrentDirectory + "\\Assets\\";
            }
        }

        public static Skeleton Load(string name)
        {
            var path = name.Replace('.', '\\');

            Atlas atlas = new Atlas(ResourceFloder + path + ".atlas", new Renderer.SkeletonTextureLoader(gl));
            var binary = new SkeletonBinary(atlas);
            var data = binary.ReadSkeletonData(ResourceFloder + path + ".skel");
            return new Skeleton(data);
        }
    }
}
