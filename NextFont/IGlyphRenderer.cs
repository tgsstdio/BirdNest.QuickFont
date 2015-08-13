namespace NextFont
{
	public interface IGlyphRenderer
	{
		void RenderGlyph (float x, float y, char c, bool isDropShadow);
		void Reset();
		void Flush();
	}
}

