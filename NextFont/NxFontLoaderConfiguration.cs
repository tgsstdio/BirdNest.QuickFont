using System;
using OpenTK;
using QuickFont;

namespace NextFont
{
	public class NxFontLoaderConfiguration
	{
		public QFontShadowConfiguration ShadowConfig = null;
		public bool TransformToCurrentOrthogProjection = false;		
		public Matrix4 Transform { get; private set;}
		public IDrawCommandList CharacterOutput { get; private set;}
		public IGlyphRenderer FontGlyphRenderer {get;set;}
		public IGlyphRenderer DropShadowRenderer {get;set;}

		public NxFontLoaderConfiguration (IDrawCommandList charOutput, Matrix4 transform, bool addDropShadow)
		{
			CharacterOutput = charOutput;
			Transform = transform;

			if (addDropShadow)
				this.ShadowConfig = new QFontShadowConfiguration();
		}
	}
}

