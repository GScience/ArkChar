using Spine;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Media;
using SDL2;
using SharpGL;
using SharpGL.Enumerations;

namespace Arkchar.Renderer
{
    public class SkeletonTextureLoader : TextureLoader
    {
        private OpenGL _gl;

        public SkeletonTextureLoader(OpenGL gl)
        {
            _gl = gl;
        }

        public void Load(AtlasPage page, string path)
        {
            IntPtr tmp = SDL_image.IMG_Load(path);
            tmp = SDL.SDL_ConvertSurfaceFormat(tmp, SDL.SDL_PIXELFORMAT_ABGR8888, 0);
            SDL.SDL_Surface surface = Marshal.PtrToStructure<SDL.SDL_Surface>(tmp);

            var textureIdList = new uint[1];
            _gl.GenTextures(1, textureIdList);
            var textureId = textureIdList[0];

            _gl.BindTexture(OpenGL.GL_TEXTURE_2D, textureId);
            _gl.TexImage2D(OpenGL.GL_TEXTURE_2D, 0, OpenGL.GL_RGBA, surface.w, surface.h, 0, OpenGL.GL_RGBA, OpenGL.GL_UNSIGNED_BYTE, surface.pixels);
            _gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);
            _gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR);

            SDL.SDL_FreeSurface(tmp);

            page.rendererObject = textureId;
        }

        public void Unload(object texture)
        {
            var deletedTextureList = new[] {(uint) texture};
            _gl.DeleteTextures(1, deletedTextureList);
        }
    }
}
