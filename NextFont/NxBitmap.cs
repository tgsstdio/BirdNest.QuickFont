using QuickFont;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace NextFont
{
	public class NxBitmap : IQBitmap, IQBitmapOperations<NxBitmap>
	{
		public NxBitmap ()
		{
			
		}

		private QBitmapData mTarget;
		private Bitmap mParent;
		public NxBitmap (Bitmap parent, QBitmapData target)
		{
			mParent = parent;
			mTarget = target;
		}

		#region IQBitmapOperations implementation

		public void Blit (NxBitmap source, System.Drawing.Rectangle sourceRect, int px, int py)
		{
			mTarget.Blit (source.GetBitmapData (), sourceRect, px, py);
		}

		public void Blit (NxBitmap source, int srcPx, int srcPy, int srcW, int srcH, int destX, int destY)
		{
			mTarget.Blit (source.GetBitmapData (), srcPx, srcPy, srcW, srcH, destX, destY);
		}

		public void BlitMask (NxBitmap source, int srcPx, int srcPy, int srcW, int srcH, int px, int py)
		{
			mTarget.BlitMask (source.GetBitmapData (), srcPx, srcPy, srcW, srcH, px, py);
		}

		#endregion

		#region IQBitmap implementation
		public void DownScale32 (int newWidth, int newHeight)
		{		
			var otherImage = new Bitmap (newWidth, newHeight, this.Format);
			var otherBitmap = otherImage.LockBits (new Rectangle (0, 0, newWidth, newHeight), ImageLockMode.ReadWrite, this.Format);

			var otherBitmapData = new QBitmapData (otherBitmap);
			mTarget.DownScaleThis (newWidth, newHeight, otherBitmapData, this.Format);

			ReplaceInternalData (otherImage, otherBitmapData);
		}

		private void ReplaceInternalData (Bitmap otherImage, QBitmapData other)
		{
			Free ();
			mParent = otherImage;
			mTarget = other;
		}

		public void InitialiseBlankImage (int width, int height, PixelFormat format)
		{
			mParent = new Bitmap (width, height, format);
			var otherBitmap = mParent.LockBits (new Rectangle (0, 0, width, height), ImageLockMode.ReadWrite, format);
			mTarget = new QBitmapData (otherBitmap);
		}

		public void BlurAlpha(int radius, int passes)
		{
			var otherImage = new Bitmap (Width, Height, this.Format);
			var otherBitmap = otherImage.LockBits (new Rectangle (0, 0, Width, Height), ImageLockMode.ReadWrite, this.Format);

			var temporaryBitmap = new QBitmapData (otherBitmap);
			mTarget.BlurAlpha(radius, passes, temporaryBitmap, Width, Height);

			otherImage.UnlockBits (otherBitmap);
		}

		public void Clear32 (byte r, byte g, byte b, byte a)
		{
			mTarget.Clear32 (r, g, b, a);
		}

		public void Colour32 (byte r, byte g, byte b)
		{
			mTarget.Colour32 (r, g, b);
		}

		public void Free ()
		{
			mParent.UnlockBits (mTarget.GetBitmapData ());
			mParent = null;
			mTarget = null;
		}

		public void RetargetGlyphRectangleOutwards (QFontGlyph glyph, bool setYOffset, byte alphaTolerance)
		{
			mTarget.RetargetGlyphRectangleOutwards (glyph, setYOffset, alphaTolerance);
		}

		public void Save (string fileName, ImageFormat format)
		{
			throw new System.NotImplementedException ();
		}

		public bool IsEmptyAlphaPixel (int px, int py, byte alphaEmptyPixelTolerance)
		{
			return mTarget.IsEmptyAlphaPixel (px, py, alphaEmptyPixelTolerance);
		}

		public BitmapData GetBitmapData ()
		{
			return mTarget.GetBitmapData ();
		}

		public int Width {
			get {
				return mTarget.Width;
			}
		}

		public int Height {
			get {
				return mTarget.Height;
			}
		}

		public PixelFormat Format {
			get {
				return mParent.PixelFormat;
			}
		}

		#endregion
	}
}

