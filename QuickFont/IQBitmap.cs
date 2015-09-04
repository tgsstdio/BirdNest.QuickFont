using System.Drawing.Imaging;
using System;

namespace QuickFont
{
	public interface IQBitmap
	{
		int Width { get; }
		int Height { get; }
		PixelFormat Format { get; }
		void InitialiseBlankImage(int width, int height, PixelFormat format);
		void DownScale32 (int newWidth, int newHeight);
		void BlurAlpha (int radius, int passes);
		void Clear32 (byte r, byte g, byte b, byte a);
		void Colour32 (byte r, byte g, byte b);
		void Free ();
		void RetargetGlyphRectangleOutwards (QFontGlyph glyph, bool setYOffset, byte alphaTolerance);
		void Save(string fileName, ImageFormat format);
		bool IsEmptyAlphaPixel (int px, int py, byte alphaEmptyPixelTolerance);
		BitmapData GetBitmapData();
	}
}
