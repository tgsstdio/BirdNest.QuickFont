﻿using System;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;


namespace QuickFont
{
    public class TexturePage : IDisposable
    {
        int gLTexID;
        int width;
        int height;
		long mHandle;

        public int GLTexID { get { return gLTexID; } }
		public long Resident { get { return mHandle; } }
        public int Width { get { return width; } }
        public int Height { get { return height; } }


        public TexturePage(string filePath)
        {
            var bitmap = new QBitmap(filePath);
            CreateTexture(bitmap.bitmapData);
            bitmap.Free();
        }

        public TexturePage(BitmapData dataSource)
        {
            CreateTexture(dataSource);
        }


        private void CreateTexture(BitmapData dataSource)
        {
            width = dataSource.Width;
            height = dataSource.Height;

            Helper.SafeGLEnable(EnableCap.Texture2D, () =>
            {
                GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

                GL.GenTextures(1, out gLTexID);
                GL.BindTexture(TextureTarget.Texture2D, gLTexID);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, dataSource.Scan0);

				mHandle = GL.Arb.GetTextureHandle(gLTexID);
				GL.Arb.MakeTextureHandleResident(mHandle);
            });
        }


        public void Dispose()
        {
			GL.Arb.MakeTextureHandleNonResident(mHandle);
            GL.DeleteTexture(gLTexID);
        }




    }
}
