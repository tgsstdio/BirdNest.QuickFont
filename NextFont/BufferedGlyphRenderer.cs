using System;
using System.Collections.Generic;
using QuickFont;
using OpenTK;
using OpenTK.Graphics;

namespace NextFont
{
	public class BufferedGlyphRenderer : IGlyphRenderer
	{
		private readonly List<GlyphKey> mCharacters;
		private readonly QFontData mFontData;
		private Vector3 PrintOffset;
		private Color4 mFontColor;
		private readonly SentanceInfo[] mSentances;
		public BufferedGlyphRenderer (QFontData fontData, Vector3 printOffset, Color4 fontColor)
		{
			mFontData = fontData;
			mCharacters = new List<GlyphKey> ();
			PrintOffset = printOffset;
			mFontColor = fontColor;
			mSentances = new SentanceInfo[mFontData.Pages.Length];
		}

		private class SentanceInfo
		{
			public SentanceBlock BlockInfo;
			private readonly List<float> mVertices;
			public SentenceInfo()
			{
				mVertices = new List<float>();
			}

			public void Reset()
			{
				mVertices.Clear ();
			}

			public void AddVertex(Vector3 pos, Vector2 tx)
			{
				mVertices.Add (pos.X);
				mVertices.Add (pos.Y);
				mVertices.Add (pos.Z);
				mVertices.Add (tx.X);
				mVertices.Add (tx.Y);
			}
		}

		#region IGlyphRenderer implementation

		private struct GlyphKey
		{
			public float X { get; set; }
			public float Y { get; set; }
			public char Key{ get; set; }
		}


		public void RenderGlyph (float x, float y, char c, bool isDropShadow)
		{
			mCharacters.Add (new GlyphKey{ X = x, Y = y, Key = c });
		}

		public void Reset ()
		{
			mCharacters.Clear ();
		}

		public void Flush ()
		{
			foreach(var letter in mCharacters)
			{
				var x = letter.X;
				var y = letter.Y;
				var c = letter.Key;

				var glyph = mFontData.CharSetMapping[c];

				TexturePage sheet = mFontData.Pages[glyph.page];
				SentanceInfo dest = mSentances [glyph.page];

				float tx1 = (float)(glyph.rect.X) / sheet.Width;
				float ty1 = (float)(glyph.rect.Y) / sheet.Height;
				float tx2 = (float)(glyph.rect.X + glyph.rect.Width) / sheet.Width;
				float ty2 = (float)(glyph.rect.Y + glyph.rect.Height) / sheet.Height;

				var tv1 = new Vector2(tx1, ty1);
				var tv2 = new Vector2(tx1, ty2);
				var tv3 = new Vector2(tx2, ty2);
				var tv4 = new Vector2(tx2, ty1);

				var v1 = PrintOffset + new Vector3(x, y + glyph.yOffset, 0);
				var v2 = PrintOffset + new Vector3(x, y + glyph.yOffset + glyph.rect.Height, 0);
				var v3 = PrintOffset + new Vector3(x + glyph.rect.Width, y + glyph.yOffset + glyph.rect.Height, 0);
				var v4 = PrintOffset + new Vector3(x + glyph.rect.Width, y + glyph.yOffset, 0);

				var normal = new Vector3(0, 0, -1);

				int argb = mFontColor.ToArgb ();

//				var vbo = VertexBuffers[glyph.page];
//
				dest.BlockInfo.Color = new Vector4(mFontColor.R, mFontColor.G, mFontColor.B, mFontColor.A);
				dest.AddVertex(v1, tv1, argb);
				dest.AddVertex(v2, tv2, argb);
				dest.AddVertex(v3, tv3, argb);
//
//				vbo.AddVertex(v1, normal, tv1, argb);
//				vbo.AddVertex(v3, normal, tv3, argb);
//				vbo.AddVertex(v4, normal, tv4, argb);
			}

		}
		#endregion
	}
}

