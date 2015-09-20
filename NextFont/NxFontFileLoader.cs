using System;
using QuickFont;
using System.IO;
using System.Collections.Generic;
using System.Drawing;

namespace NextFont
{
	public class NxFontFileLoader : INxFontLoader
	{
		public NxFont Load(string path, float height, NxFontLoaderConfiguration loaderConfig)
		{
			return Load(path, height, 1.0f, loaderConfig); 
		}

		public NxFont Load(string filePath, float height, float downSampleFactor, NxFontLoaderConfiguration loaderConfig)
		{
			if (loaderConfig == null)
			{
				throw new ArgumentNullException ("loaderConfig");
			}

			float fontScale;
			TransformViewport? transToVp = NxFont.SetupTransformViewport (height, loaderConfig.TransformToCurrentOrthogProjection, loaderConfig.Transform, out fontScale);

			var qfont = new NxFont();
			var internalConfig = new QFontLoaderConfiguration();		
			var fontData = new QFontData ();
			qfont.SetData (fontData);

			QFontDataInformation fontInfo = null;
			using (var fs = File.OpenRead (filePath))
			{
				fontInfo = fontData.LoadFromStream (fs);
			}
			var bitmapFiles = fontInfo.GenerateBitmapPageNames (filePath);

			var bitmapPages = new List<NxBitmap> ();
			foreach (var bitmapFileName in bitmapFiles)
			{
				// TODO : STREAM BASED REPLACEMENT 
				// https://support.microsoft.com/en-us/kb/814675
				// GDI+ require the bitmap files to be locked as indexed image
				// during the lifetime i.e. maybe reloaded from disk				
				using (var fs = File.OpenRead (bitmapFileName))	
				{
					var parent = new Bitmap (fs);
					var data = parent.LockBits (
							new Rectangle(0,0, parent.Width, parent.Height)
							,System.Drawing.Imaging.ImageLockMode.ReadWrite
							,parent.PixelFormat);
					var target = new QBitmapData (data);
					var qb = new NxBitmap (parent, target);
					bitmapPages.Add (qb);
				}
			}
			var glyphList = fontData.InitialiseQFontData (fontInfo, ref bitmapPages, downSampleFactor, internalConfig);

			if (loaderConfig.ShadowConfig != null)
			{
				qfont.DropShadow = Helper.BuildDropShadow<NxFont, NxBitmap> (
					bitmapPages,
					glyphList.ToArray (),
					loaderConfig.ShadowConfig,
					Helper.ToArray (fontInfo.CharSet),
					internalConfig.KerningConfig.alphaEmptyPixelTolerance);
			}
			fontData.InitialiseKerningPairs (fontInfo, bitmapPages, glyphList, internalConfig);

			if (loaderConfig.ShadowConfig != null)
				qfont.Options.DropShadowActive = true;
			if (transToVp != null)
				qfont.Options.TransformToViewport = transToVp;

			qfont.InitialiseGlyphRenderer(loaderConfig.CharacterOutput, loaderConfig.FontGlyphRenderer, loaderConfig.DropShadowRenderer);

			return qfont;
		}
	}
}

