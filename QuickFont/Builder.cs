using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Imaging;
using System.IO;

namespace QuickFont
{

    /// <summary>
    /// Class for building a Quick Font, given a Font
    /// and a configuration object.
    /// </summary>
	public class Builder<TFont> where TFont : class, IFont, new()
    {

        private string charSet;
        private QFontBuilderConfiguration config;
        private Font font;

        public Builder(Font font, QFontBuilderConfiguration config)
        {
            this.charSet = config.charSet;
            this.config = config;
            this.font = font;
            
        }

        //these do not affect the actual width of glyphs (we measure widths pixel-perfectly ourselves), but is used to detect whether a font is monospaced
        private List<SizeF> GetGlyphSizes(Font font)
        {
            Bitmap bmp = new Bitmap(512, 512, PixelFormat.Format24bppRgb);
            Graphics graph = Graphics.FromImage(bmp);
            List<SizeF> sizes = new List<SizeF>();

            for (int i = 0; i < charSet.Length; i++)
            {
                var charSize = graph.MeasureString("" + charSet[i], font);
                sizes.Add(new SizeF(charSize.Width, charSize.Height));
            }

            graph.Dispose();
            bmp.Dispose();

            return sizes;
        }

        private SizeF GetMaxGlyphSize(List<SizeF> sizes)
        {
            SizeF maxSize = new SizeF(0f, 0f);
            for (int i = 0; i < charSet.Length; i++)
            {
                if (sizes[i].Width > maxSize.Width)
                    maxSize.Width = sizes[i].Width;

                if (sizes[i].Height > maxSize.Height)
                    maxSize.Height = sizes[i].Height;
            }

            return maxSize;
        }

        private SizeF GetMinGlyphSize(List<SizeF> sizes)
        {
            SizeF minSize = new SizeF(float.MaxValue, float.MaxValue);
            for (int i = 0; i < charSet.Length; i++)
            {
                if (sizes[i].Width < minSize.Width)
                    minSize.Width = sizes[i].Width;

                if (sizes[i].Height < minSize.Height)
                    minSize.Height = sizes[i].Height;
            }

            return minSize;
        }


        /// <summary>
        /// Returns true if all glyph widths are within 5% of each other
        /// </summary>
        /// <param name="sizes"></param>
        /// <returns></returns>
        private bool IsMonospaced(List<SizeF> sizes)
        {
            var min = GetMinGlyphSize(sizes);
            var max = GetMaxGlyphSize(sizes);

            if (max.Width - min.Width < max.Width * 0.05f)
                return true;

            return false;
        }

        /*
        private SizeF GetMaxGlyphSize(Font font)
        {
            Bitmap bmp = new Bitmap(256, 256, PixelFormat.Format24bppRgb);
            Graphics graph = Graphics.FromImage(bmp);

            SizeF maxSize = new SizeF(0f, 0f);
            for (int i = 0; i < charSet.Length; i++)
            {
                var charSize = graph.MeasureString("" + charSet[i], font);

                if (charSize.Width > maxSize.Width)
                    maxSize.Width = charSize.Width;

                if (charSize.Height > maxSize.Height)
                    maxSize.Height = charSize.Height;
            }

            graph.Dispose();
            bmp.Dispose();

            return maxSize;
        }*/

        //The initial bitmap is simply a long thin strip of all glyphs in a row
		private Bitmap CreateInitialBitmap(Font font, SizeF maxSize, int initialMargin, out QFontGlyph[] glyphs, TextGenerationRenderHint renderHint)
        {
            glyphs = new QFontGlyph[charSet.Length];

            int spacing = (int)Math.Ceiling(maxSize.Width) + 2 * initialMargin;
            Bitmap bmp = new Bitmap(spacing * charSet.Length, (int)Math.Ceiling(maxSize.Height) + 2 * initialMargin, PixelFormat.Format24bppRgb);
            Graphics graph = Graphics.FromImage(bmp);

            switch(renderHint){
                case TextGenerationRenderHint.SizeDependent: 
                    graph.TextRenderingHint = font.Size <= 12.0f  ? TextRenderingHint.ClearTypeGridFit : TextRenderingHint.AntiAlias; 
                    break;
                case TextGenerationRenderHint.AntiAlias: 
                    graph.TextRenderingHint = TextRenderingHint.AntiAlias; 
                    break;
                case TextGenerationRenderHint.AntiAliasGridFit: 
                    graph.TextRenderingHint = TextRenderingHint.AntiAliasGridFit; 
                    break;
                case TextGenerationRenderHint.ClearTypeGridFit:
                    graph.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                    break;
                case TextGenerationRenderHint.SystemDefault:
                    graph.TextRenderingHint = TextRenderingHint.SystemDefault;
                    break;
            }

  
                

            int xOffset = initialMargin;
            for (int i = 0; i < charSet.Length; i++)
            {
                graph.DrawString("" + charSet[i], font, Brushes.White, xOffset, initialMargin);
                var charSize = graph.MeasureString("" + charSet[i], font);
                glyphs[i] = new QFontGlyph(0, new Rectangle(xOffset - initialMargin, 0, (int)charSize.Width + initialMargin * 2, (int)charSize.Height + initialMargin * 2), 0, charSet[i]);
                xOffset += (int)charSize.Width + initialMargin * 2;
            }

            graph.Flush();
            graph.Dispose();

            return bmp;
        }

		public QFontData BuildFontData(out TFont dropShadowFont)
        {			
			return BuildFontData(null, out dropShadowFont);
        }

		const string SUPERSAMPLE_LEVELS_MUST_BE_A_POWER_OF_TWO_ERROR = "SuperSampleLevels must be a power of two when using ForcePowerOfTwo.";

		public QFontData BuildFontData(string saveName, out TFont dropShadowFont)
        {
            if (config.ForcePowerOfTwo && config.SuperSampleLevels != Helper.PowerOfTwo(config.SuperSampleLevels))
            {
				throw new ArgumentOutOfRangeException (SUPERSAMPLE_LEVELS_MUST_BE_A_POWER_OF_TWO_ERROR);
            }

            if (config.SuperSampleLevels <= 0 || config.SuperSampleLevels > 8)
            {
                throw new ArgumentOutOfRangeException("SuperSampleLevels = [" + config.SuperSampleLevels + "] is an unsupported value. Please use values in the range [1,8]"); 
            }

            int margin = 2; //margin in initial bitmap (don't bother to make configurable - likely to cause confusion
            int pageWidth = config.PageWidth * config.SuperSampleLevels; //texture page width
            int pageHeight = config.PageHeight * config.SuperSampleLevels; //texture page height
            bool usePowerOfTwo = config.ForcePowerOfTwo;
            int glyphMargin = config.GlyphMargin * config.SuperSampleLevels;

            QFontGlyph[] initialGlyphs;
            var sizes = GetGlyphSizes(font);
            var maxSize = GetMaxGlyphSize(sizes);
			var initialBmp = CreateInitialBitmap(font, maxSize, margin, out initialGlyphs,config.TextGenerationRenderHint);
            var initialBitmapData = initialBmp.LockBits(new Rectangle(0, 0, initialBmp.Width, initialBmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            int minYOffset = int.MaxValue;
            foreach (var glyph in initialGlyphs){
				Helper.RetargetGlyphRectangleInwards (initialBitmapData, glyph, true, config.KerningConfig.alphaEmptyPixelTolerance);
                minYOffset = Math.Min(minYOffset,glyph.yOffset);
            }
            minYOffset--; //give one pixel of breathing room?

            foreach (var glyph in initialGlyphs)
                glyph.yOffset -= minYOffset;
           

            QFontGlyph[] glyphs; 

			// Override 
			var imageHack = new QBitmap ();
			imageHack.bitmap = initialBmp;
			imageHack.bitmapData = initialBitmapData;

			var bitmapPages = Helper.GenerateBitmapSheetsAndRepack<QBitmap>( initialGlyphs,new [] { imageHack},pageWidth, pageHeight, out glyphs, glyphMargin, usePowerOfTwo);

            initialBmp.UnlockBits(initialBitmapData);
            initialBmp.Dispose();

            if (config.SuperSampleLevels != 1)
            {
                Helper.ScaleSheetsAndGlyphs(bitmapPages, glyphs, 1.0f / config.SuperSampleLevels);
                Helper.RetargetAllGlyphs(bitmapPages, glyphs,config.KerningConfig.alphaEmptyPixelTolerance);
            }


            //create list of texture pages
            var pages = new List<TexturePage>();
            foreach (var page in bitmapPages)
				pages.Add(new TexturePage(page.GetBitmapData()));

			var fontData = new QFontData();
            fontData.CharSetMapping = Helper.CreateCharGlyphMapping(glyphs);
            fontData.Pages = pages.ToArray();
            fontData.CalculateMeanWidth();
            fontData.CalculateMaxHeight();
            fontData.KerningPairs = KerningCalculator.CalculateKerning(charSet.ToCharArray(), glyphs, bitmapPages,config.KerningConfig);
            fontData.naturallyMonospaced = IsMonospaced(sizes);

            if (saveName != null)
            {
                if (bitmapPages.Count == 1)
                    bitmapPages[0].Save(saveName + ".png", System.Drawing.Imaging.ImageFormat.Png);
                else
                {
                    for (int i = 0; i < bitmapPages.Count; i++)
                        bitmapPages[i].Save(saveName + "_sheet_" + i + ".png", System.Drawing.Imaging.ImageFormat.Png);
                }
            }


			if (config.ShadowConfig != null)
			{
				dropShadowFont = Helper.BuildDropShadow<TFont, QBitmap> (bitmapPages, glyphs, config.ShadowConfig, charSet.ToCharArray (), config.KerningConfig.alphaEmptyPixelTolerance);
			}
			else
			{
				dropShadowFont = null;
			}

            foreach (var page in bitmapPages)
                page.Free();


            //validate glyphs
            var intercept = Helper.FirstIntercept(fontData.CharSetMapping);
            if (intercept != null)
                throw new Exception("Failed to create glyph set. Glyphs '" + intercept[0] + "' and '" + intercept[1] + "' were overlapping. This is could be due to an error in the font, or a bug in Graphics.MeasureString().");
            



            return fontData;

        }

    }
}
