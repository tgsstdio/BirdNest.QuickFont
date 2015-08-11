using System;
using System.Drawing.Text;
using System.Drawing;

namespace NextFont
{
	public class NxFont
	{
		public NxFont (string fileName, float size, FontStyle style, NxFontBuilderConfiguration config)
		{
			PrivateFontCollection pfc = new PrivateFontCollection();
			pfc.AddFontFile(fileName);
			var fontFamily = pfc.Families[0];

			if (!fontFamily.IsStyleAvailable(style))
				throw new ArgumentException("Font file: " + fileName + " does not support style: " +  style );

			if (config == null)
				config = new NxFontBuilderConfiguration();

			// TODO : remove viewport
//			TransformViewport? transToVp = null;
			float fontScale = 1f;
//			if (config.TransformToCurrentOrthogProjection)
//				transToVp = OrthogonalTransform(out fontScale);

			using(var font = new Font(fontFamily, size * fontScale * config.SuperSampleLevels, style)){
				fontData = BuildFont(font, config, null);
			}

			if (config.ShadowConfig != null)
				Options.DropShadowActive = true;
			if (transToVp != null)
				Options.TransformToViewport = transToVp;

			// Replace
//			if(config.UseVertexBuffer)
//				InitVBOs();
		}
	}
}

