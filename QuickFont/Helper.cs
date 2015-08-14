using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace QuickFont
{
    public static class Helper
    {
		public static void IsMatrixOrthogonal (out bool isOrthog, out float left, out float right, out float bottom, out float top, Matrix4 matrix)
		{		
			if (System.Math.Abs (matrix.M11) < float.Epsilon || System.Math.Abs (matrix.M22) < float.Epsilon)
			{
				isOrthog = false;
				left = right = bottom = top = 0;
				return;
			}
			left = -(1f + matrix.M41) / (matrix.M11);
			right = (1f - matrix.M41) / (matrix.M11);
			bottom = -(1 + matrix.M42) / (matrix.M22);
			top = (1 - matrix.M42) / (matrix.M22);
			isOrthog = Math.Abs (matrix.M12) < float.Epsilon && Math.Abs (matrix.M13) < float.Epsilon && Math.Abs (matrix.M14) < float.Epsilon && Math.Abs (matrix.M21) < float.Epsilon && Math.Abs (matrix.M23) < float.Epsilon && Math.Abs (matrix.M24) < float.Epsilon && Math.Abs (matrix.M31) < float.Epsilon && Math.Abs (matrix.M32) < float.Epsilon && Math.Abs (matrix.M34) < float.Epsilon && Math.Abs (matrix.M44 - 1f) < float.Epsilon;
		}

        public static T[] ToArray<T>(ICollection<T> collection)
        {
            T[] output = new T[collection.Count];
            collection.CopyTo(output, 0);
            return output;
        }

        /// <summary>
        /// Ensures GL.End() is called
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="code"></param>
        public static void SafeGLBegin(BeginMode mode, Action code)
        {
            GL.Begin(mode);

            code();

            GL.End();
        }

        /// <summary>
        /// Ensures that state is disabled
        /// </summary>
        /// <param name="cap"></param>
        /// <param name="code"></param>
        public static void SafeGLEnable(EnableCap cap, Action code)
        {
            GL.Enable(cap);

            code();

            GL.Disable(cap);
        }

        /// <summary>
        /// Ensures that multiple states are disabled
        /// </summary>
        /// <param name="cap"></param>
        /// <param name="code"></param>
        public static void SafeGLEnable(EnableCap[] caps, Action code)
        {
            foreach(var cap in caps)
                GL.Enable(cap);

            code();

            foreach (var cap in caps)
                GL.Disable(cap);
        }

        public static void SafeGLEnableClientStates(ArrayCap[] caps, Action code)
        {
            foreach (var cap in caps)
                GL.EnableClientState(cap);

            code();

            foreach (var cap in caps)
                GL.DisableClientState(cap);
        }

		public static void SaveQFontDataToFile(QFontData data, string filePath) 
		{
			var lines = data.Serialize();
			StreamWriter writer = new StreamWriter(filePath + ".qfont");
			foreach (var line in lines)
				writer.WriteLine(line);

			writer.Close();

		}

		private delegate bool EmptyDel(BitmapData data, int x, int y);

		public static void RetargetGlyphRectangleOutwards(BitmapData bitmapData, QFontGlyph glyph, bool setYOffset, byte alphaTolerance)
		{
			int startX,endX;
			int startY,endY;

			var rect = glyph.rect;

			EmptyDel emptyPix;

			if (bitmapData.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb)
				emptyPix = delegate(BitmapData data, int x, int y) { return QBitmap.EmptyAlphaPixel(data, x, y, alphaTolerance); };
			else
				emptyPix = delegate(BitmapData data, int x, int y) { return QBitmap.EmptyPixel(data, x, y); };


			unsafe
			{

				for (startX = rect.X; startX >= 0; startX--)
				{
					bool foundPix = false;
					for (int j = rect.Y; j <= rect.Y + rect.Height; j++)
					{
						if (!emptyPix(bitmapData, startX, j))
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


				for (endX = rect.X + rect.Width; endX < bitmapData.Width; endX++)
				{
					bool foundPix = false;
					for (int j = rect.Y; j <= rect.Y + rect.Height; j++)
					{
						if (!emptyPix(bitmapData, endX, j))
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
						if (!emptyPix(bitmapData, i, startY))
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



				for (endY = rect.Y + rect.Height; endY < bitmapData.Height; endY++)
				{
					bool foundPix = false;
					for (int i = startX; i <= endX; i++)
					{
						if (!emptyPix(bitmapData, i, endY))
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

		private static bool RectangleIntersect(Rectangle r1, Rectangle r2)
		{
			return (r1.X < r2.X + r2.Width && r1.X + r1.Width > r2.X &&
				r1.Y < r2.Y + r2.Height && r1.Y + r1.Height > r2.Y);

		}

		public static char[] FirstIntercept(Dictionary<char,QFontGlyph> charSet)
		{
			char[] keys = Helper.ToArray(charSet.Keys);

			for (int i = 0; i < keys.Length; i++)
			{
				for (int j = i + 1; j < keys.Length; j++)
				{
					if (charSet[keys[i]].page == charSet[keys[j]].page && RectangleIntersect(charSet[keys[i]].rect, charSet[keys[j]].rect))
					{
						return new char[2] { keys[i], keys[j] };
					}

				}
			}
			return null;
		}

		public static QFontDataInformation LoadQFontInformation<TFont>(Stream fs)
		{
			var lines = new List<String>();
			using (var reader = new StreamReader (fs))
			{
				string line;
				while ((line = reader.ReadLine ()) != null)
					lines.Add (line);

				var data = new QFontData();
				return data.Deserialize(lines);
			}
		}

		static List<QBitmap> LoadBitmaps (string filePath, int pageCount)
		{
			string namePrefix = filePath.Replace (".qfont", "").Replace (" ", "");
			var bitmapPages = new List<QBitmap> ();
			if (pageCount == 1)
			{
				bitmapPages.Add (new QBitmap (namePrefix + ".png"));
			}
			else
			{
				for (int i = 0; i < pageCount; i++)
					bitmapPages.Add (new QBitmap (namePrefix + "_sheet_" + i));
			}
			return bitmapPages;
		}

		public static List<QBitmap> GenerateBitmapSheetsAndRepack(QFontGlyph[] sourceGlyphs, BitmapData[] sourceBitmaps, int destSheetWidth, int destSheetHeight, out QFontGlyph[] destGlyphs, int destMargin, bool usePowerOfTwo)
		{
			var pages = new List<QBitmap>();
			destGlyphs = new QFontGlyph[sourceGlyphs.Length];

			QBitmap currentPage = null;


			int maxY = 0;
			foreach (var glph in sourceGlyphs)
				maxY = Math.Max(glph.rect.Height, maxY);


			int finalPageIndex = 0;
			int finalPageRequiredWidth = 0;
			int finalPageRequiredHeight = 0;


			for (int k = 0; k < 2; k++)
			{
				bool pre = k == 0;  //first iteration is simply to determine the required size of the final page, so that we can crop it in advance


				int xPos = 0;
				int yPos = 0;
				int maxYInRow = 0;
				int totalTries = 0;

				for (int i = 0; i < sourceGlyphs.Length; i++)
				{


					if(!pre && currentPage == null){

						if (finalPageIndex == pages.Count)
						{
							int width = Math.Min(destSheetWidth, usePowerOfTwo ? PowerOfTwo(finalPageRequiredWidth) : finalPageRequiredWidth);
							int height = Math.Min(destSheetHeight, usePowerOfTwo ? PowerOfTwo(finalPageRequiredHeight) : finalPageRequiredHeight);

							currentPage = new QBitmap(new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb));
							currentPage.Clear32(255, 255, 255, 0); //clear to white, but totally transparent
						}
						else
						{
							currentPage = new QBitmap(new Bitmap(destSheetWidth, destSheetHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb));
							currentPage.Clear32(255, 255, 255, 0); //clear to white, but totally transparent
						}
						pages.Add(currentPage);

					}



					totalTries++;

					if (totalTries > 10 * sourceGlyphs.Length)
						throw new Exception("Failed to fit font into texture pages");


					var rect = sourceGlyphs[i].rect;

					if (xPos + rect.Width + 2 * destMargin <= destSheetWidth && yPos + rect.Height + 2 * destMargin <= destSheetHeight)
					{
						if (!pre)
						{
							//add to page
							if(sourceBitmaps[sourceGlyphs[i].page].PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb)
								QBitmap.Blit(sourceBitmaps[sourceGlyphs[i].page], currentPage.bitmapData, rect.X, rect.Y, rect.Width, rect.Height, xPos + destMargin, yPos + destMargin);
							else
								QBitmap.BlitMask(sourceBitmaps[sourceGlyphs[i].page], currentPage.bitmapData, rect.X, rect.Y, rect.Width, rect.Height, xPos + destMargin, yPos + destMargin);

							destGlyphs[i] = new QFontGlyph(pages.Count - 1, new Rectangle(xPos + destMargin, yPos + destMargin, rect.Width, rect.Height), sourceGlyphs[i].yOffset, sourceGlyphs[i].character);
						}
						else
						{
							finalPageRequiredWidth = Math.Max(finalPageRequiredWidth, xPos + rect.Width + 2 * destMargin);
							finalPageRequiredHeight = Math.Max(finalPageRequiredHeight, yPos + rect.Height + 2 * destMargin);
						}


						xPos += rect.Width + 2 * destMargin;
						maxYInRow = Math.Max(maxYInRow, rect.Height);

						continue;
					}


					if (xPos + rect.Width + 2 * destMargin > destSheetWidth)
					{
						i--;

						yPos += maxYInRow + 2 * destMargin;
						xPos = 0;

						if (yPos + maxY + 2 * destMargin > destSheetHeight)
						{
							yPos = 0;

							if (!pre)
							{
								currentPage = null;
							}
							else
							{
								finalPageRequiredWidth = 0;
								finalPageRequiredHeight = 0;
								finalPageIndex++;
							}
						}
						continue;
					}

				}

			}


			return pages;


		}

		public static Dictionary<char, QFontGlyph> CreateCharGlyphMapping(QFontGlyph[] glyphs)
		{
			var dict = new Dictionary<char, QFontGlyph>();
			for (int i = 0; i < glyphs.Length; i++)
				dict.Add(glyphs[i].character, glyphs[i]);

			return dict;
		}

		public static void ScaleSheetsAndGlyphs(List<QBitmap> pages, QFontGlyph[] glyphs, float scale)
		{
			foreach (var page in pages)
				page.DownScale32((int)(page.bitmap.Width * scale), (int)(page.bitmap.Height * scale));


			foreach (var glyph in glyphs)
			{
				glyph.rect = new Rectangle((int)(glyph.rect.X * scale), (int)(glyph.rect.Y * scale), (int)(glyph.rect.Width * scale), (int)(glyph.rect.Height * scale));
				glyph.yOffset = (int)(glyph.yOffset * scale);

			}
		}

		public static TFont BuildDropShadow<TFont>(List<QBitmap> sourceFontSheets, QFontGlyph[] sourceFontGlyphs, QFontShadowConfiguration shadowConfig, char[] charSet, byte alphaTolerance)
			where TFont : IFont, new()
		{

			QFontGlyph[] newGlyphs;

			var sourceBitmapData = new List<BitmapData>();
			foreach(var sourceSheet in sourceFontSheets)
				sourceBitmapData.Add(sourceSheet.bitmapData);


			//GenerateBitmapSheetsAndRepack(QFontGlyph[] sourceGlyphs, BitmapData[] sourceBitmaps, int destSheetWidth, int destSheetHeight, out QFontGlyph[] destGlyphs, int destMargin, bool usePowerOfTwo)

			var bitmapSheets = GenerateBitmapSheetsAndRepack(sourceFontGlyphs, sourceBitmapData.ToArray(), shadowConfig.PageWidth, shadowConfig.PageHeight, out newGlyphs, shadowConfig.GlyphMargin + shadowConfig.blurRadius*3, shadowConfig.ForcePowerOfTwo);

			//scale up in case we wanted bigger/smaller shadows
			if (Math.Abs (shadowConfig.Scale - 1.0f) > float.Epsilon)
				ScaleSheetsAndGlyphs (bitmapSheets, newGlyphs, shadowConfig.Scale); //no point in retargeting yet, since we will do it after blur


			//blacken and blur
			foreach (var bitmapSheet in bitmapSheets)
			{
				bitmapSheet.Colour32(0, 0, 0);
				bitmapSheet.BlurAlpha(shadowConfig.blurRadius, shadowConfig.blurPasses);

			}




			//retarget after blur and scale
			RetargetAllGlyphs(bitmapSheets, newGlyphs, alphaTolerance);

			//create list of texture pages
			var newTextureSheets = new List<TexturePage>();
			foreach (var page in bitmapSheets)
				newTextureSheets.Add(new TexturePage(page.bitmapData));


			var fontData = new QFontData();
			fontData.CharSetMapping = new Dictionary<char, QFontGlyph>();
			for(int i = 0; i < charSet.Length; i++)
				fontData.CharSetMapping.Add(charSet[i],newGlyphs[i]);

			fontData.Pages = newTextureSheets.ToArray();
			fontData.CalculateMeanWidth();
			fontData.CalculateMaxHeight();


			foreach (var sheet in bitmapSheets)
				sheet.Free();

			var result = new TFont ();
			result.SetData (fontData);
			return result;
		}

		public static void RetargetAllGlyphs(List<QBitmap> pages, QFontGlyph[] glyphs, byte alphaTolerance)
		{
			foreach (var glyph in glyphs)
				RetargetGlyphRectangleOutwards(pages[glyph.page].bitmapData, glyph, false, alphaTolerance);
		}

		/// <summary>
		/// Returns the power of 2 that is closest to x, but not smaller than x.
		/// </summary>
		public static int PowerOfTwo(int x)
		{
			int shifts = 0;
			uint val = (uint)x;

			if (x < 0)
				return 0;

			while (val > 0)
			{
				val = val >> 1;
				shifts++;
			}

			val = (uint)1 << (shifts - 1);
			if (val < x)
			{
				val = val << 1;
			}

			return (int)val;
		}

		public static void RetargetGlyphRectangleInwards(BitmapData bitmapData, QFontGlyph glyph, bool setYOffset, byte alphaTolerance)
		{
			int startX, endX;
			int startY, endY;

			var rect = glyph.rect;

			EmptyDel emptyPix;

			if (bitmapData.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb)
				emptyPix = delegate(BitmapData data, int x, int y) { return QBitmap.EmptyAlphaPixel(data, x, y, alphaTolerance); };
			else
				emptyPix = delegate(BitmapData data, int x, int y) { return QBitmap.EmptyPixel(data, x, y); };


			unsafe
			{

				for (startX = rect.X; startX < bitmapData.Width; startX++)
					for (int j = rect.Y; j < rect.Y + rect.Height; j++)
						if (!emptyPix(bitmapData, startX, j))
							goto Done1;
				Done1:

				for (endX = rect.X + rect.Width; endX >= 0; endX--)
					for (int j = rect.Y; j < rect.Y + rect.Height; j++)
						if (!emptyPix(bitmapData, endX, j))
							goto Done2;
				Done2:

				for (startY = rect.Y; startY < bitmapData.Height; startY++)
					for (int i = startX; i < endX; i++)
						if (!emptyPix(bitmapData, i, startY))
							goto Done3;

				Done3:

				for (endY = rect.Y + rect.Height; endY >= 0; endY--)
					for (int i = startX; i < endX; i++)
						if (!emptyPix(bitmapData, i, endY))
							goto Done4;
				Done4:;


			}

			if (endY < startY)
				startY = endY = rect.Y;

			if (endX < startX)
				startX = endX = rect.X;

			glyph.rect = new Rectangle(startX, startY, endX - startX + 1, endY - startY + 1);

			if (setYOffset)
				glyph.yOffset = glyph.rect.Y;

		}
    }
}
