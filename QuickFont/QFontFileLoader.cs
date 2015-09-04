using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;

namespace QuickFont
{
	public class QFontFileLoader : IQFontLoader
	{
		public QFont Load(string path)
		{
			return Load(path, 1.0f, null); 
		}

		public QFont Load(string path, QFontLoaderConfiguration loaderConfig) 
		{
			return Load(path, 1.0f, loaderConfig); 
		}

		public QFont Load(string path, float downSampleFactor) 
		{
			return Load(path, downSampleFactor,null); 
		}

		public QFont Load(string path, float downSampleFactor, QFontLoaderConfiguration loaderConfig)
		{
			if (loaderConfig == null)
				loaderConfig = new QFontLoaderConfiguration();

			TransformViewport? transToVp = null;
			float fontScale = 1f;
			if (loaderConfig.TransformToCurrentOrthogProjection)
				transToVp = QFont.OrthogonalTransform(out fontScale);

			QFont qfont = new QFont();
			qfont.fontData = new QFontData ();

			QFontDataInformation fontInfo = null;
			using (var fs = File.OpenRead (path))
			{
				fontInfo = qfont.fontData.LoadFromStream (fs);
			}
			var bitmapFiles = fontInfo.GenerateBitmapPageNames (path);

			var bitmapPages = new List<QBitmap> ();
			foreach (var bitmapFileName in bitmapFiles)
			{
				// TODO : GDI+ require the bitmap files to be locked as indexed image
				// during the lifetime i.e. maybe reloaded from disk
				using (var fs = File.OpenRead (bitmapFileName))
				using (var b = new Bitmap(fs))				
				{
					var qb = new QBitmap (bitmapFileName);
					bitmapPages.Add (qb);
				}
			}
			var glyphList = qfont.fontData.InitialiseQFontData (fontInfo, ref bitmapPages, downSampleFactor, loaderConfig);

			if (loaderConfig.ShadowConfig != null)
			{
				qfont.DropShadow = Helper.BuildDropShadow<QFont, QBitmap> (
					bitmapPages,
					glyphList.ToArray (),
					loaderConfig.ShadowConfig,
					Helper.ToArray (fontInfo.CharSet),
					loaderConfig.KerningConfig.alphaEmptyPixelTolerance);
			}
			qfont.fontData.InitialiseKerningPairs (fontInfo, bitmapPages, glyphList, loaderConfig);

			if (loaderConfig.ShadowConfig != null)
				qfont.Options.DropShadowActive = true;
			if (transToVp != null)
				qfont.Options.TransformToViewport = transToVp;

			return qfont;
		}

}
}

