﻿using System;
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

			var bitmapPages = new List<QBitmap> ();
			foreach (var bitmapFileName in bitmapFiles)
			{
				// TODO : STREAM BASED REPLACEMENT 
				// GDI+ require the bitmap files to be locked as indexed image
				// during the lifetime i.e. maybe reloaded from disk				
				//using (var fs = File.OpenRead (bitmapFileName))
				//using (var b = new Bitmap(fs))				
			//	{
					var qb = new QBitmap (bitmapFileName);
					bitmapPages.Add (qb);
			//	}
			}
			var glyphList = fontData.InitialiseQFontData (fontInfo, ref bitmapPages, downSampleFactor, internalConfig);

			if (loaderConfig.ShadowConfig != null)
			{
				qfont.DropShadow = Helper.BuildDropShadow<NxFont> (
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

			return qfont;
		}
	}
}

