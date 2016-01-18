using System;
using System.Collections.Generic;
using QuickFont;
using OpenTK;
using OpenTK.Graphics;

namespace NextFont
{
	public class BufferedGlyphRenderer : IGlyphRenderer
	{
		public Matrix4 Transform {
			get;
			set;
		}

		private readonly List<GlyphKey> mCharacters;
		private readonly QFontData mFontData;
		private Vector3 PrintOffset;

		public Vector4 Colour {get;set;}

		private readonly SentanceBlockInfo[] mDestinations;
		private readonly IDrawCommandList mCommandBuffer;
		public BufferedGlyphRenderer (IDrawCommandList commandBuffer, QFontData fontData, Vector3 printOffset, Vector4 fontColor)
		{
			mFontData = fontData;
			mCharacters = new List<GlyphKey> ();
			PrintOffset = printOffset;
			Colour = fontColor;
			mCommandBuffer = commandBuffer;
			mDestinations = new SentanceBlockInfo[mFontData.Pages.Length];
			InitialiseDestinations ();
		}

		private void InitialiseDestinations()
		{
			for (int i = 0; i < mFontData.Pages.Length; ++i)
			{
				mDestinations [i] = new SentanceBlockInfo ();
				var handle = new TextureHandle ();
				handle.Texture = mFontData.Pages [i].Resident;
				handle.Slice = 0;
				handle.Index = 0;
				mDestinations[i].Handle = handle;
			}
		}

		private class SentanceBlockInfo
		{
			public TextureHandle Handle;
			public List<float> Vertices { get; private set; }
			public List<uint> Indices { get; private set; }
			public uint VertexIndex { get; private set; }
			public uint NoOfTriangles { get; private set; }
			public SentanceBlockInfo()
			{
				Vertices = new List<float>();
				Indices = new List<uint>();
			}

			public void Reset()
			{
				Vertices.Clear ();
				Indices.Clear ();
				VertexIndex = 0;
				NoOfTriangles = 0;
			}

			public void AddTriangle(uint a, uint b, uint c)
			{
				Indices.Add (a);
				Indices.Add (b);
				Indices.Add (c);
				++NoOfTriangles;
			}

			public uint AddVertex(Vector3 pos, Vector2 tx)
			{
				Vertices.Add (pos.X);
				Vertices.Add (pos.Y);
				Vertices.Add (pos.Z);
				Vertices.Add (tx.X);
				Vertices.Add (tx.Y);
				var currentIndex = VertexIndex;
				++VertexIndex;
				return currentIndex;
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
			if (isDropShadow) 
			{
				var glyph = mFontData.CharSetMapping[c];
				x -= (int)(glyph.rect.Width * 0.5f);
				y -= (int)(glyph.rect.Height * 0.5f + glyph.yOffset);
			}

			mCharacters.Add (new GlyphKey{ X = x, Y = y, Key = c });
		}

		public void Reset ()
		{
			mCharacters.Clear ();
			foreach (var dest in mDestinations)
			{
				dest.Reset ();
			}
		}

		void ShaderlessTransform (ref Vector3 v0, ref Vector3 v1, ref Vector3 v2, ref Vector3 v3)
		{
			var other0 = Vector4.Transform (new Vector4 (v0, 1), Transform);
			var other1 = Vector4.Transform (new Vector4 (v1, 1), Transform);
			var other2 = Vector4.Transform (new Vector4 (v2, 1), Transform);
			var other3 = Vector4.Transform (new Vector4 (v3, 1), Transform);
			v0 = other0.Xyz;
			v1 = other1.Xyz;
			v2 = other2.Xyz;
			v3 = other3.Xyz;
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
				SentanceBlockInfo dest = mDestinations [glyph.page];

				float tx1 = (float)(glyph.rect.X) / sheet.Width;
				float ty1 = (float)(glyph.rect.Y) / sheet.Height;
				float tx2 = (float)(glyph.rect.X + glyph.rect.Width) / sheet.Width;
				float ty2 = (float)(glyph.rect.Y + glyph.rect.Height) / sheet.Height;

				var tv1 = new Vector2(tx1, ty1);
				var tv2 = new Vector2(tx1, ty2);
				var tv3 = new Vector2(tx2, ty2);
				var tv4 = new Vector2(tx2, ty1);

				var v0 = PrintOffset + new Vector3(x, y + glyph.yOffset, 0);
				var v1 = PrintOffset + new Vector3(x, y + glyph.yOffset + glyph.rect.Height, 0);
				var v2 = PrintOffset + new Vector3(x + glyph.rect.Width, y + glyph.yOffset + glyph.rect.Height, 0);
				var v3 = PrintOffset + new Vector3(x + glyph.rect.Width, y + glyph.yOffset, 0);

				// UNCOMMENT TO WHEN RENDERING WITHOUT VERTEX SHADER
				//ShaderlessTransform (ref v0, ref v1, ref v2, ref v3);

				var t1_0 = dest.AddVertex(v0, tv1);
				var t1_1 = dest.AddVertex(v1, tv2);
				var t1_2 = dest.AddVertex(v2, tv3);

				dest.AddTriangle (t1_0, t1_1, t1_2);

				var t1_3 = dest.AddVertex(v3, tv4);

				dest.AddTriangle (t1_0, t1_2, t1_3);
			}

			foreach(var dest in mDestinations)
			{
				mCommandBuffer.RenderChunk (dest.Handle,
					Colour,
					Transform,
					dest.VertexIndex,
					dest.Vertices,
					dest.Indices);
					
			}
		}
		#endregion
	}
}

