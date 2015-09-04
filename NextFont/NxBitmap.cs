using QuickFont;
using System.IO;

namespace NextFont
{
	public class NxBitmap : IQBitmap, IQBitmapOperations<NxBitmap>
	{
		public NxBitmap ()
		{
			
		}

		public NxBitmap (FileStream fs)
		{
			throw new System.NotImplementedException ();
		}

		#region IQBitmapOperations implementation

		public void Blit (NxBitmap source, System.Drawing.Rectangle sourceRect, int px, int py)
		{
			throw new System.NotImplementedException ();
		}

		public void Blit (NxBitmap source, int srcPx, int srcPy, int srcW, int srcH, int destX, int destY)
		{
			throw new System.NotImplementedException ();
		}

		public void BlitMask (NxBitmap source, int srcPx, int srcPy, int srcW, int srcH, int px, int py)
		{
			throw new System.NotImplementedException ();
		}

		#endregion

		#region IQBitmap implementation
		public void DownScale32 (int newWidth, int newHeight)
		{
			throw new System.NotImplementedException ();
		}

		public void InitialiseBlankImage (int width, int height, System.Drawing.Imaging.PixelFormat format)
		{
			throw new System.NotImplementedException ();
		}

		public void BlurAlpha (int radius, int passes)
		{
			throw new System.NotImplementedException ();
		}

		public void Clear32 (byte r, byte g, byte b, byte a)
		{
			throw new System.NotImplementedException ();
		}

		public void Colour32 (byte r, byte g, byte b)
		{
			throw new System.NotImplementedException ();
		}

		public void Free ()
		{
			throw new System.NotImplementedException ();
		}

		public void RetargetGlyphRectangleOutwards (QFontGlyph glyph, bool setYOffset, byte alphaTolerance)
		{
			throw new System.NotImplementedException ();
		}

		public void Save (string fileName, System.Drawing.Imaging.ImageFormat format)
		{
			throw new System.NotImplementedException ();
		}

		public bool IsEmptyAlphaPixel (int px, int py, byte alphaEmptyPixelTolerance)
		{
			throw new System.NotImplementedException ();
		}

		public System.Drawing.Imaging.BitmapData GetBitmapData ()
		{
			throw new System.NotImplementedException ();
		}

		public int Width {
			get {
				throw new System.NotImplementedException ();
			}
		}

		public int Height {
			get {
				throw new System.NotImplementedException ();
			}
		}

		public System.Drawing.Imaging.PixelFormat Format {
			get {
				throw new System.NotImplementedException ();
			}
		}

		#endregion
	}
}

