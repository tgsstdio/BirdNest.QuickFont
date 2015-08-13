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
		public NxFontLoaderConfiguration (Matrix4 transform, bool addDropShadow)
		{
			Transform = transform;

			if (addDropShadow)
				this.ShadowConfig = new QFontShadowConfiguration();
		}
	}
}

