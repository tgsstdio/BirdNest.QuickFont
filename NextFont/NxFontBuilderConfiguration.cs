using System;
using OpenTK;
using QuickFont;

namespace NextFont
{
	public class NxFontBuilderConfiguration
	{
		public Matrix4 Transform {get;set;}
		public int SuperSampleLevels = 1;
		public bool TransformToCurrentOrthogProjection = false;
		public QFontShadowConfiguration ShadowConfig = null;
		public QFontKerningConfiguration KerningConfig = new QFontKerningConfiguration();
		public TextGenerationRenderHint TextGenerationRenderHint = TextGenerationRenderHint.SizeDependent;

		public NxFontBuilderConfiguration (Matrix4 transform) : this(transform, false)
		{
			
		}

		public NxFontBuilderConfiguration(Matrix4 transform, bool addDropShadow) 
		{
			Transform = transform;

			if (addDropShadow)
				this.ShadowConfig = new QFontShadowConfiguration();
		}
	}
}

