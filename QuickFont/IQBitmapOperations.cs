using System.Drawing;

namespace QuickFont
{
	public interface IQBitmapOperations<TBitmap>
		where TBitmap : class, IQBitmap, new()
	{
		void Blit(TBitmap source, Rectangle sourceRect, int px, int py);
		void Blit(TBitmap source, int srcPx, int srcPy, int srcW, int srcH, int destX, int destY);
		void BlitMask(TBitmap source, int srcPx, int srcPy, int srcW, int srcH, int px, int py);
	}
}

