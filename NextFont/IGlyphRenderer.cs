using OpenTK;
using OpenTK.Graphics;

namespace NextFont
{
	public interface IGlyphRenderer
	{
		Color4 Colour {get;set;}
		Matrix4 Transform { get; set; }
		void RenderGlyph (float x, float y, char c, bool isDropShadow);
		void Reset();
		void Flush();
	}
}

