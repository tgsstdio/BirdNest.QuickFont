using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace QuickFont
{
	public class QFontData
    {

        /// <summary>
        /// Mapping from a pair of characters to a pixel offset
        /// </summary>
        public Dictionary<String, int> KerningPairs;

        /// <summary>
        /// List of texture pages
        /// </summary>
        public TexturePage[] Pages;

        /// <summary>
        /// Mapping from character to glyph index
        /// </summary>
        public Dictionary<char, QFontGlyph> CharSetMapping; 

        /// <summary>
        /// The average glyph width
        /// </summary>
        public float meanGlyphWidth;

        /// <summary>
        /// The maximum glyph height
        /// </summary>
        public int maxGlyphHeight;


        /// <summary>
        /// Null if no dropShadow is available
        /// </summary>
		//public TFont dropShadow;


        /// <summary>
        /// Whether the original font (from ttf) was detected to be monospaced
        /// </summary>
        public bool naturallyMonospaced = false;


        public bool IsMonospacingActive(QFontRenderOptions options)
        {
            return (options.Monospacing == QFontMonospacing.Natural && naturallyMonospaced) || options.Monospacing == QFontMonospacing.Yes; 
        }



        public float GetMonoSpaceWidth(QFontRenderOptions options)
        {
            return (float)Math.Ceiling(1 + (1 + options.CharacterSpacing) * meanGlyphWidth);
        }




        public List<String> Serialize()
        {
            var data = new List<String>();


            data.Add("" + Pages.Length);
            data.Add("" + CharSetMapping.Count);

            foreach (var glyphChar in CharSetMapping)
            {
                var chr = glyphChar.Key;
                var glyph = glyphChar.Value;

                data.Add("" + chr + " " + 
                    glyph.page + " " +
                    glyph.rect.X + " " +
                    glyph.rect.Y + " " +
                    glyph.rect.Width + " " +
                    glyph.rect.Height + " " +
                    glyph.yOffset);
            }
            return data;
        }

		public QFontDataInformation LoadFromStream(Stream fs)
		{
			var lines = new List<String>();
			using (var reader = new StreamReader (fs))
			{
				string line;
				while ((line = reader.ReadLine ()) != null)
					lines.Add (line);

				return Deserialize(lines);
			}
		}

		public QFontDataInformation Deserialize(List<String> input)
        {
            try
            {
				CharSetMapping = new Dictionary<char, QFontGlyph>();
				var charSetList = new List<char>();

				int pageCount = int.Parse(input[0]);
                int glyphCount = int.Parse(input[1]);

                for (int i = 0; i < glyphCount; i++)
                {
                    var vals = input[2 + i].Split(' ');
                    var glyph = new QFontGlyph(int.Parse(vals[1]), new Rectangle(int.Parse(vals[2]), int.Parse(vals[3]), int.Parse(vals[4]), int.Parse(vals[5])), int.Parse(vals[6]), vals[0][0]);

                    CharSetMapping.Add(vals[0][0], glyph);
                    charSetList.Add(vals[0][0]);
                }

				return new QFontDataInformation (pageCount, charSetList.ToArray());;
            }
            catch (Exception e)
            {
                throw new Exception("Failed to parse qfont file. Invalid format.",e);
            }
        }

        public void CalculateMeanWidth()
        {
            meanGlyphWidth = 0f;
            foreach (var glyph in CharSetMapping)
                meanGlyphWidth += glyph.Value.rect.Width;

            meanGlyphWidth /= CharSetMapping.Count;

        }


        public void CalculateMaxHeight()
        {
            maxGlyphHeight = 0;
            foreach (var glyph in CharSetMapping)
                maxGlyphHeight = Math.Max(glyph.Value.rect.Height, maxGlyphHeight);

        }


        /// <summary>
        /// Returns the kerning length correction for the character at the given index in the given string.
        /// Also, if the text is part of a textNode list, the nextNode is given so that the following 
        /// node can be checked incase of two adjacent word nodes.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="text"></param>
        /// <param name="textNode"></param>
        /// <returns></returns>
        public int GetKerningPairCorrection(int index, string text, TextNode textNode)
        {
            if (KerningPairs == null)
                return 0;


            var chars = new char[2];

            if (index + 1 == text.Length)
            {
                if (textNode != null && textNode.Next != null && textNode.Next.Type == TextNodeType.Word)
                    chars[1] = textNode.Next.Text[0];
                else
                    return 0;
            }
            else
            {
                chars[1] = text[index + 1];
            }

            chars[0] = text[index];

            String str = new String(chars);


            if (KerningPairs.ContainsKey(str))
                return KerningPairs[str];

            return 0;
            
        }

		private static void CreateBitmapPerGlyph<TBitmap>(QFontGlyph[] sourceGlyphs, TBitmap[] sourceBitmaps, out QFontGlyph[]  destGlyphs, out TBitmap[] destBitmaps)
			where TBitmap : class, IQBitmap, IQBitmapOperations<TBitmap>, new()
		{
			destBitmaps = new TBitmap[sourceGlyphs.Length];
			destGlyphs = new QFontGlyph[sourceGlyphs.Length];
			for(int i = 0; i < sourceGlyphs.Length; i++){
				var sg = sourceGlyphs[i];
				destGlyphs[i] = new QFontGlyph(i,new Rectangle(0,0,sg.rect.Width,sg.rect.Height),sg.yOffset,sg.character);
				destBitmaps[i] = new TBitmap();
				destBitmaps[i].InitialiseBlankImage (sg.rect.Width, sg.rect.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
				destBitmaps[i].Blit(sourceBitmaps[sg.page],sg.rect,0,0);
			}
		}

		public List<QFontGlyph> InitialiseQFontData<TBitmap>(QFontDataInformation fontInfo, ref List<TBitmap> bitmapPages, float downSampleFactor, QFontLoaderConfiguration loaderConfig)
			where TBitmap : class, IQBitmap, IQBitmapOperations<TBitmap>, new()
		{
			foreach (var glyph in CharSetMapping.Values)
				bitmapPages[glyph.page].RetargetGlyphRectangleOutwards(glyph, false, loaderConfig.KerningConfig.alphaEmptyPixelTolerance);

			var intercept = Helper.FirstIntercept(CharSetMapping);
			if (intercept != null)
			{
				throw new Exception("Failed to load font from file. Glyphs '" + intercept[0] + "' and '" + intercept[1] + "' were overlapping. If you are texturing your font without locking pixel opacity, then consider using a larger glyph margin. This can be done by setting QFontBuilderConfiguration myQfontBuilderConfig.GlyphMargin, and passing it into CreateTextureFontFiles.");
			}

			int localPageCount = fontInfo.PageCount;

			if (downSampleFactor > 1.0f)
			{
				foreach (var page in bitmapPages)
					page.DownScale32((int)(page.Width * downSampleFactor), (int)(page.Height * downSampleFactor));

				foreach (var glyph in CharSetMapping.Values)
				{

					glyph.rect = new Rectangle((int)(glyph.rect.X * downSampleFactor),
						(int)(glyph.rect.Y * downSampleFactor),
						(int)(glyph.rect.Width * downSampleFactor),
						(int)(glyph.rect.Height * downSampleFactor));
					glyph.yOffset = (int)(glyph.yOffset * downSampleFactor);
				}
			}
			else if (downSampleFactor < 1.0f )
			{
				// If we were simply to shrink the entire texture, then at some point we will make glyphs overlap, breaking the font.
				// For this reason it is necessary to copy every glyph to a separate bitmap, and then shrink each bitmap individually.
				QFontGlyph[] shrunkGlyphs;
				TBitmap[] shrunkBitmapsPerGlyph;
				CreateBitmapPerGlyph<TBitmap>(Helper.ToArray(CharSetMapping.Values), bitmapPages.ToArray(), out shrunkGlyphs, out shrunkBitmapsPerGlyph);

				//shrink each bitmap
				for (int i = 0; i < shrunkGlyphs.Length; i++)
				{   
					var bmp = shrunkBitmapsPerGlyph[i];
					bmp.DownScale32(Math.Max((int)(bmp.Width * downSampleFactor),1), Math.Max((int)(bmp.Height * downSampleFactor),1));
					shrunkGlyphs[i].rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
					shrunkGlyphs[i].yOffset = (int)(shrunkGlyphs[i].yOffset * downSampleFactor);
				}

				//use roughly the same number of pages as before..
				int newWidth = (int)(bitmapPages[0].Width * (0.1f + downSampleFactor));
				int newHeight = (int)(bitmapPages[0].Height * (0.1f + downSampleFactor));

				//free old bitmap pages since we are about to chuck them away
				for (int i = 0; i < localPageCount; i++)
					bitmapPages[i].Free();

				QFontGlyph[] shrunkRepackedGlyphs;
				bitmapPages = Helper.GenerateBitmapSheetsAndRepack<TBitmap>(shrunkGlyphs, shrunkBitmapsPerGlyph, newWidth, newHeight, out shrunkRepackedGlyphs, 4, false);
				CharSetMapping = Helper.CreateCharGlyphMapping(shrunkRepackedGlyphs);

				foreach (var bmp in shrunkBitmapsPerGlyph)
					bmp.Free();

				localPageCount = bitmapPages.Count;
			}

			Pages = new TexturePage[localPageCount];
			for(int i = 0; i < localPageCount; i ++ )
				Pages[i] = new TexturePage(bitmapPages[i].GetBitmapData());


			if (Math.Abs (downSampleFactor - 1.0f) > float.Epsilon)
			{
				foreach (var glyph in CharSetMapping.Values)
					bitmapPages [glyph.page].RetargetGlyphRectangleOutwards (glyph, false, loaderConfig.KerningConfig.alphaEmptyPixelTolerance);


				intercept = Helper.FirstIntercept (CharSetMapping);
				if (intercept != null)
				{
					throw new Exception ("Failed to load font from file. Glyphs '" + intercept [0] + "' and '" + intercept [1] + "' were overlapping. This occurred only after resizing your texture font, implying that there is a bug in QFont. ");
				}
			}

			var glyphList = new List<QFontGlyph>();

			foreach (var c in fontInfo.CharSet)
				glyphList.Add(CharSetMapping[c]);

			return glyphList;
		}

		public void InitialiseKerningPairs<TBitmap>(QFontDataInformation fontInfo, List<TBitmap> bitmapPages, List<QFontGlyph> glyphList, QFontLoaderConfiguration loaderConfig)
			where TBitmap : class, IQBitmap, IQBitmapOperations<TBitmap>, new()
		{
			KerningPairs = KerningCalculator.CalculateKerning(Helper.ToArray(fontInfo.CharSet), glyphList.ToArray(), bitmapPages, loaderConfig.KerningConfig);

			CalculateMeanWidth();
			CalculateMaxHeight();

			for (int i = 0; i < bitmapPages.Count; i++)
				bitmapPages[i].Free();
		}

		public float MeasureTextNodeLength(TextNode node, QFontRenderOptions options)
		{

			bool monospaced = IsMonospacingActive(options);
			float monospaceWidth = GetMonoSpaceWidth(options);

			if (node.Type == TextNodeType.Space)
			{
				if (monospaced)
					return monospaceWidth;

				return (float)Math.Ceiling(meanGlyphWidth * options.WordSpacing);
			}


			float length = 0f;
			if (node.Type == TextNodeType.Word)
			{

				for (int i = 0; i < node.Text.Length; i++)
				{
					char c = node.Text[i];
					if (CharSetMapping.ContainsKey(c))
					{
						if (monospaced)
							length += monospaceWidth;
						else
							length += (float)Math.Ceiling(CharSetMapping[c].rect.Width + meanGlyphWidth * options.CharacterSpacing + GetKerningPairCorrection(i, node.Text, node));
					}
				}
			}
			return length;
		}

        public void Dispose()
        {
            foreach (var page in Pages)
                page.Dispose();
        }
    }
}
