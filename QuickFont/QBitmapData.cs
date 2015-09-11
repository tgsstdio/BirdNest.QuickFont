using System;
using System.Drawing.Imaging;
using System.Drawing;

namespace QuickFont
{
	public class QBitmapData
	{
		private readonly BitmapData mBitmapData;
		public BitmapData GetBitmapData()
		{
			return mBitmapData;
		}

		public QBitmapData (BitmapData data)
		{
			mBitmapData = data;
		}

		public int Width {
			get
			{
				return mBitmapData.Width;
			}				
		}

		public int Height {
			get
			{
				return mBitmapData.Height;
			}
		}

		/// <summary>
		/// Sets colour without touching alpha values
		/// </summary>
		/// <param name="r"></param>
		/// <param name="g"></param>
		/// <param name="b"></param>
		public void Colour32(byte r, byte g, byte b)
		{
			unsafe
			{
				byte* addr;
				for (int i = 0; i < mBitmapData.Width; i++)
				{
					for (int j = 0; j < mBitmapData.Height; j++)
					{
						addr = (byte*)(mBitmapData.Scan0) + mBitmapData.Stride * j + i * 4;
						*addr = b;
						*(addr + 1) = g;
						*(addr + 2) = r;
					}
				}
			}
		}

		public void Clear32(byte r, byte g, byte b, byte a)
		{
			unsafe
			{
				byte* sourcePtr = (byte*)(mBitmapData.Scan0);

				for (int i = 0; i < mBitmapData.Height; i++)
				{
					for (int j = 0; j < mBitmapData.Width; j++)
					{
						*(sourcePtr) = b;
						*(sourcePtr + 1) = g;
						*(sourcePtr + 2) = r;
						*(sourcePtr + 3) = a;

						sourcePtr += 4;
					}
					sourcePtr += mBitmapData.Stride - mBitmapData.Width * 4; //move to the end of the line (past unused space)
				}
			}
		}

		public unsafe void PutPixel32(int px, int py, byte r, byte g, byte b, byte a)
		{
			byte* addr = (byte*)(mBitmapData.Scan0) + mBitmapData.Stride * py + px * 4;

			*addr = b;
			*(addr + 1) = g;
			*(addr + 2) = r;
			*(addr + 3) = a;
		}

		public unsafe void GetPixel32(int px, int py, ref byte r, ref byte g, ref byte b, ref byte a)
		{
			byte* addr = (byte*)(mBitmapData.Scan0) + mBitmapData.Stride * py + px * 4;

			b = *addr;
			g = *(addr + 1);
			r = *(addr + 2);
			a = *(addr + 3); 
		}

		/// <summary>
		/// Returns try if the given pixel is empty (i.e. alpha is zero)
		/// </summary>
		public static unsafe bool EmptyAlphaPixel(BitmapData bitmapData, int px, int py, byte alphaEmptyPixelTolerance)
		{
			byte* addr = (byte*)(bitmapData.Scan0) + bitmapData.Stride * py + px * 4;
			return (*(addr + 3) <= alphaEmptyPixelTolerance);
		}

		public unsafe void PutAlpha32(int px, int py, byte a)
		{
			*((byte*)(mBitmapData.Scan0) + mBitmapData.Stride * py + px * 4 + 3) = a;
		}

		public unsafe void GetAlpha32(int px, int py, ref byte a)
		{
			a = *((byte*)(mBitmapData.Scan0) + mBitmapData.Stride * py + px * 4 + 3);
		}

		public bool IsEmptyAlphaPixel(int px, int py, byte alphaEmptyPixelTolerance)
		{
			return EmptyAlphaPixel (mBitmapData, px, py, alphaEmptyPixelTolerance);
		}

		public void DownScaleThis (int newWidth, int newHeight, QBitmapData dest, PixelFormat format)
		{
			var srcWidth = mBitmapData.Width;
			var srcHeight = mBitmapData.Height;

			if (format != PixelFormat.Format32bppArgb)
				throw new Exception ("DownsScale32 only works on 32 bit images");
			float xscale = (float)srcWidth / newWidth;
			float yscale = (float)srcHeight / newHeight;
			byte r = 0, g = 0, b = 0, a = 0;
			float summedR = 0f;
			float summedG = 0f;
			float summedB = 0f;
			float summedA = 0f;
			int left, right, top, bottom;
			//the area of old pixels covered by the new bitmap
			float targetStartX, targetEndX;
			float targetStartY, targetEndY;
			float leftF, rightF, topF, bottomF;
			//edges of new pixel in old pixel coords
			float weight;
			float weightScale = xscale * yscale;
			float totalColourWeight = 0f;
			for (int m = 0; m < newHeight; m++)
			{
				for (int n = 0; n < newWidth; n++)
				{
					leftF = n * xscale;
					rightF = (n + 1) * xscale;
					topF = m * yscale;
					bottomF = (m + 1) * yscale;
					left = (int)leftF;
					right = (int)rightF;
					top = (int)topF;
					bottom = (int)bottomF;
					if (left < 0)
						left = 0;
					if (top < 0)
						top = 0;
					if (right >= srcWidth)
						right = srcWidth - 1;
					if (bottom >= srcHeight)
						bottom = srcHeight - 1;
					summedR = 0f;
					summedG = 0f;
					summedB = 0f;
					summedA = 0f;
					totalColourWeight = 0f;
					for (int j = top; j <= bottom; j++)
					{
						for (int i = left; i <= right; i++)
						{
							targetStartX = Math.Max (leftF, i);
							targetEndX = Math.Min (rightF, i + 1);
							targetStartY = Math.Max (topF, j);
							targetEndY = Math.Min (bottomF, j + 1);
							weight = (targetEndX - targetStartX) * (targetEndY - targetStartY);
							this.GetPixel32 (i, j, ref r, ref g, ref b, ref a);
							summedA += weight * a;
							if (a != 0)
							{
								summedR += weight * r;
								summedG += weight * g;
								summedB += weight * b;
								totalColourWeight += weight;
							}
						}
					}
					summedR /= totalColourWeight;
					summedG /= totalColourWeight;
					summedB /= totalColourWeight;
					summedA /= weightScale;
					if (summedR < 0)
						summedR = 0f;
					if (summedG < 0)
						summedG = 0f;
					if (summedB < 0)
						summedB = 0f;
					if (summedA < 0)
						summedA = 0f;
					if (summedR >= 256)
						summedR = 255;
					if (summedG >= 256)
						summedG = 255;
					if (summedB >= 256)
						summedB = 255;
					if (summedA >= 256)
						summedA = 255;
					dest.PutPixel32 (n, m, (byte)summedR, (byte)summedG, (byte)summedB, (byte)summedA);
				}
			}
		}

		public void BlurAlpha (int radius, int passes, QBitmapData tmp, int width, int height)
		{
			byte a = 0;
			int summedA;
			int weight = 0;
			int xpos, ypos, x, y, kx, ky;
			for (int pass = 0; pass < passes; pass++)
			{
				//horizontal pass
				for (y = 0; y < height; y++)
				{
					for (x = 0; x < width; x++)
					{
						summedA = weight = 0;
						for (kx = -radius; kx <= radius; kx++)
						{
							xpos = x + kx;
							if (xpos >= 0 && xpos < width)
							{
								this.GetAlpha32 (xpos, y, ref a);
								summedA += a;
								weight++;
							}
						}
						summedA /= weight;
						tmp.PutAlpha32 (x, y, (byte)summedA);
					}
				}
				//vertical pass
				for (x = 0; x < width; ++x)
				{
					for (y = 0; y < height; ++y)
					{
						summedA = weight = 0;
						for (ky = -radius; ky <= radius; ky++)
						{
							ypos = y + ky;
							if (ypos >= 0 && ypos < height)
							{
								tmp.GetAlpha32 (x, ypos, ref a);
								summedA += a;
								weight++;
							}
						}
						summedA /= weight;
						this.PutAlpha32 (x, y, (byte)summedA);
					}
				}
			}
		}

		private delegate bool EmptyDel(BitmapData data, int x, int y);

		/// <summary>
		/// Returns try if the given pixel is empty (i.e. black)
		/// </summary>
		public static unsafe bool EmptyPixel(BitmapData bitmapData, int px, int py)
		{

			byte* addr = (byte*)(bitmapData.Scan0) + bitmapData.Stride * py + px * 3;
			return (*addr == 0 && *(addr + 1) == 0 && *(addr + 2) == 0);

		}

		public void RetargetGlyphRectangleOutwards (QFontGlyph glyph, bool setYOffset, byte alphaTolerance)
		{
			int startX,endX;
			int startY,endY;

			var rect = glyph.rect;

			EmptyDel emptyPix;

			if (mBitmapData.PixelFormat == PixelFormat.Format32bppArgb)
				emptyPix = delegate(BitmapData data, int x, int y) { return QBitmapData.EmptyAlphaPixel(data, x, y, alphaTolerance); };
			else
				emptyPix = delegate(BitmapData data, int x, int y) { return QBitmapData.EmptyPixel(data, x, y); };


			unsafe
			{

				for (startX = rect.X; startX >= 0; startX--)
				{
					bool foundPix = false;
					for (int j = rect.Y; j <= rect.Y + rect.Height; j++)
					{
						if (!emptyPix(mBitmapData, startX, j))
						{
							foundPix = true;
							break;
						}
					}

					if (!foundPix)
					{
						startX++;
						break;
					}
				}


				for (endX = rect.X + rect.Width; endX < mBitmapData.Width; endX++)
				{
					bool foundPix = false;
					for (int j = rect.Y; j <= rect.Y + rect.Height; j++)
					{
						if (!emptyPix(mBitmapData, endX, j))
						{
							foundPix = true;
							break; 
						}
					}

					if (!foundPix)
					{
						endX--;
						break;
					}
				}



				for (startY = rect.Y; startY >= 0; startY--)
				{
					bool foundPix = false;
					for (int i = startX; i <= endX; i++)
					{
						if (!emptyPix(mBitmapData, i, startY))
						{
							foundPix = true;
							break;
						}
					}

					if (!foundPix)
					{
						startY++;
						break;
					}
				}



				for (endY = rect.Y + rect.Height; endY < mBitmapData.Height; endY++)
				{
					bool foundPix = false;
					for (int i = startX; i <= endX; i++)
					{
						if (!emptyPix(mBitmapData, i, endY))
						{
							foundPix = true;
							break;
						}
					}

					if (!foundPix)
					{
						endY--;
						break;
					}
				}



			}



			glyph.rect = new Rectangle(startX, startY, endX - startX + 1, endY - startY + 1);

			if (setYOffset)
				glyph.yOffset = glyph.rect.Y;

		}

		public void BlitMask (BitmapData source, int srcPx, int srcPy, int srcW, int srcH, int px, int py)
		{
			BlitMask (source, this.mBitmapData, srcPx, srcPy, srcW, srcH, px, py);
		}

		/// <summary>
		/// Blits a block of a bitmap data from source to destination, using the luminance of the source to determine the 
		/// alpha of the target. Source must be 24-bit, target must be 32-bit.
		/// </summary>
		private static void BlitMask(BitmapData source, BitmapData target, int srcPx, int srcPy, int srcW, int srcH, int px, int py)
		{

			const int SOURCE_BPP = 3;
			const int TARGET_BPP = 4;

			int targetStartX, targetEndX;
			int targetStartY, targetEndY;
			int copyW, copyH;

			targetStartX = Math.Max(px, 0);
			targetEndX = Math.Min(px + srcW, target.Width);

			targetStartY = Math.Max(py, 0);
			targetEndY = Math.Min(py + srcH, target.Height);

			copyW = targetEndX - targetStartX;
			copyH = targetEndY - targetStartY;

			if (copyW < 0)
			{
				return;
			}

			if (copyH < 0)
			{
				return;
			}

			int sourceStartX = srcPx + targetStartX - px;
			int sourceStartY = srcPy + targetStartY - py;


			unsafe
			{
				byte* sourcePtr = (byte*)(source.Scan0);
				byte* targetPtr = (byte*)(target.Scan0);


				byte* targetY = targetPtr + targetStartY * target.Stride;
				byte* sourceY = sourcePtr + sourceStartY * source.Stride;
				for (int y = 0; y < copyH; y++, targetY += target.Stride, sourceY += source.Stride)
				{

					byte* targetOffset = targetY + targetStartX * TARGET_BPP;
					byte* sourceOffset = sourceY + sourceStartX * SOURCE_BPP;
					for (int x = 0; x < copyW; x++, targetOffset += TARGET_BPP, sourceOffset += SOURCE_BPP)
					{
						int lume = *(sourceOffset) + *(sourceOffset + 1) + *(sourceOffset + 2);

						lume /= 3;

						if (lume > 255)
							lume = 255;

						*(targetOffset + 3) = (byte)lume;

					}

				}
			}
		}

		public void Blit (BitmapData source, Rectangle sourceRect, int px, int py)
		{
			Blit(source, this.mBitmapData, sourceRect, px, py);
		}

		public void Blit (BitmapData source, int srcPx, int srcPy, int srcW, int srcH, int destX, int destY)
		{
			Blit(source, this.mBitmapData, srcPx, srcPy, srcW, srcH, destX, destY);
		}

        /// <summary>
        /// Blits from source to target. Both source and target must be 32-bit
        /// </summary>
        private void Blit(BitmapData source, BitmapData target, Rectangle sourceRect, int px, int py)
        {
            Blit(source, target, sourceRect.X, sourceRect.Y, sourceRect.Width, sourceRect.Height, px, py);
        }

		/// <summary>
		/// Blits from source to target. Both source and target must be 32-bit
		/// </summary>
		private static void Blit(BitmapData source, BitmapData target, int srcPx, int srcPy, int srcW, int srcH, int destX, int destY)
		{

			const int BPP = 4;

			int targetStartX, targetEndX;
			int targetStartY, targetEndY;
			int copyW, copyH;

			targetStartX = Math.Max(destX, 0);
			targetEndX = Math.Min(destX + srcW, target.Width);

			targetStartY = Math.Max(destY, 0);
			targetEndY = Math.Min(destY + srcH, target.Height);

			copyW = targetEndX - targetStartX;
			copyH = targetEndY - targetStartY;

			if (copyW < 0)
			{
				return;
			}

			if (copyH < 0)
			{
				return;
			}

			int sourceStartX = srcPx + targetStartX - destX;
			int sourceStartY = srcPy + targetStartY - destY;


			unsafe
			{
				byte* sourcePtr = (byte*)(source.Scan0);
				byte* targetPtr = (byte*)(target.Scan0);


				byte* targetY = targetPtr + targetStartY * target.Stride;
				byte* sourceY = sourcePtr + sourceStartY * source.Stride;
				for (int y = 0; y < copyH; y++, targetY += target.Stride, sourceY += source.Stride)
				{

					byte* targetOffset = targetY + targetStartX * BPP;
					byte* sourceOffset = sourceY + sourceStartX * BPP;
					for (int x = 0; x < copyW*BPP; x++, targetOffset ++, sourceOffset ++)
						*(targetOffset) = *(sourceOffset);

				}
			}
		}
	}
}

