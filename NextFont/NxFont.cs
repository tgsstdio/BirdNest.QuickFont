using System;
using System.Drawing.Text;
using System.Drawing;
using QuickFont;
using System.Collections.Concurrent;
using OpenTK;

namespace NextFont
{
	public class NxFont
	{
		private ConcurrentStack<QFontRenderOptions> optionsStack;

		public QFontRenderOptions Options
		{
			get {

				if (optionsStack.Count == 0)
				{
					optionsStack.Push(new QFontRenderOptions());
				}

				QFontRenderOptions top = null;
				if (!optionsStack.TryPeek (out top))
				{
					throw new InvalidOperationException ();
				}

				return top; 
			}
			private set { //not sure if we should even allow this...

				QFontRenderOptions top = null;
				if (!optionsStack.TryPop (out top))
				{
					throw new InvalidOperationException ();
				}
				optionsStack.Push(value);
			}
		}

		private readonly QFontData fontData;

		public NxFont(string fileName, float size, float height) : this(fileName, size, height, FontStyle.Regular, null)
		{
		
		}

		public NxFont(string fileName, float size, float height, FontStyle style) : this(fileName, size, height, style, null)
		{
		
		}

		public NxFont(string fileName, float size, float height, NxFontBuilderConfiguration config) : this(fileName, size, height, FontStyle.Regular, config)
		{
		
		}

		public NxFont (string fileName, float size, float height, FontStyle style, NxFontBuilderConfiguration config)
		{
			optionsStack = new ConcurrentStack<QFontRenderOptions> ();
			PrivateFontCollection pfc = new PrivateFontCollection();
			pfc.AddFontFile(fileName);
			var fontFamily = pfc.Families[0];

			if (!fontFamily.IsStyleAvailable(style))
				throw new ArgumentException("Font file: " + fileName + " does not support style: " +  style );

			if (config == null)
				config = new NxFontBuilderConfiguration();

			// TODO : remove viewport
			TransformViewport? transToVp = null;
			float fontScale = 1f;
			if (config.TransformToCurrentOrthogProjection)
				transToVp = OrthogonalTransform(config.Transform, height, out fontScale);

			var internalConfig = new QFontBuilderConfiguration ();
			internalConfig.SuperSampleLevels = config.SuperSampleLevels;
			using(var font = new Font(fontFamily, size * fontScale * config.SuperSampleLevels, style)){
				var builder = new Builder(font, internalConfig);
				fontData = builder.BuildFontData(null);
			}

			if (config.ShadowConfig != null)
				Options.DropShadowActive = true;
			if (transToVp != null)
				Options.TransformToViewport = transToVp;

			// Replace
//			if(config.UseVertexBuffer)
//				InitVBOs();
		}

		const string ORTHOGONAL_ERROR = "Current projection matrix was not Orthogonal. Please ensure that you have set an orthogonal projection before attempting to create a font with the TransformToOrthogProjection flag set to true.";

		private static TransformViewport OrthogonalTransform(Matrix4 transform, float height, out float fontScale)
		{
			bool isOrthog;
			float left,right,bottom,top;
			Helper.IsMatrixOrthogonal(out isOrthog,out left,out right,out bottom,out top, transform);

			if (!isOrthog)
				throw new ArgumentOutOfRangeException(ORTHOGONAL_ERROR);

			var viewportTransform = new TransformViewport(left, top, right - left, bottom - top);
			fontScale = Math.Abs(height / viewportTransform.Height);
			return viewportTransform;
		}
	}
}

