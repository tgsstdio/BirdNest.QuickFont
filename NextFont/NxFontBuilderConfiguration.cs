using System;
using OpenTK;
using QuickFont;

namespace NextFont
{
	public class NxFontBuilderConfiguration
	{
		public Matrix4 Transform {get;set;}
		public int SuperSampleLevels = 1;
		public int BlurRadius = 3;
		public bool TransformToCurrentOrthogProjection = false;
		public bool AddDropShadow = false;
		public QFontKerningConfiguration KerningConfig = new QFontKerningConfiguration();
		public TextGenerationRenderHint TextGenerationRenderHint = TextGenerationRenderHint.SizeDependent;
		public IDrawCommandList CharacterOutput { get; private set;}
		public IGlyphRenderer FontGlyphRenderer {get;set;}
		public IGlyphRenderer DropShadowRenderer {get;set;}

		public NxFontBuilderConfiguration (IDrawCommandList charOutput,  Matrix4 transform) : this(charOutput, transform, false)
		{
			
		}

		public NxFontBuilderConfiguration(IDrawCommandList charOutput, Matrix4 transform, bool addDropShadow) 
		{
			CharacterOutput = charOutput;
			Transform = transform;
			AddDropShadow = addDropShadow;
		}
	}
}

