using System;
using System.Drawing.Text;
using System.Drawing;
using QuickFont;
using System.Collections.Concurrent;
using OpenTK;
using OpenTK.Graphics;
using System.Collections.Generic;
using System.IO;
using OpenTK.Graphics.OpenGL;

namespace NextFont
{
	public class NxFont : IFont
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

		private QFontData fontData;

		#region IFont implementation

		public void SetData (QFontData data)
		{
			fontData =  data;
		}

		#endregion

		public NxFont()
		{
			optionsStack = new ConcurrentStack<QFontRenderOptions> ();
		}

		public NxFont(string fileName, float size, float height, NxFontBuilderConfiguration config) : this(fileName, size, height, FontStyle.Regular, config)
		{
		
		}

		public NxFont DropShadow {get; set;}
		public NxFont (string fileName, float size, float height, FontStyle style, NxFontBuilderConfiguration config)
		{
			optionsStack = new ConcurrentStack<QFontRenderOptions> ();
			PrivateFontCollection pfc = new PrivateFontCollection();
			pfc.AddFontFile(fileName);
			var fontFamily = pfc.Families[0];

			if (!fontFamily.IsStyleAvailable(style))
				throw new ArgumentException("Font file: " + fileName + " does not support style: " +  style );

			if (config == null)
			{
				throw new ArgumentNullException ("config");
			}

			float fontScale;
			var transToVp = SetupTransformViewport (height, config.TransformToCurrentOrthogProjection, config.Transform, out fontScale);

			var internalConfig = new QFontBuilderConfiguration (config.AddDropShadow);
			internalConfig.SuperSampleLevels = config.SuperSampleLevels;
			if (internalConfig.ShadowConfig != null)
			{
				internalConfig.ShadowConfig.blurRadius = config.BlurRadius;
			}
			using(var font = new Font(fontFamily, size * fontScale * config.SuperSampleLevels, style)){
				var builder = new Builder<NxFont>(font, internalConfig);
				NxFont dropShadowFont;
				fontData = builder.BuildFontData(null, out dropShadowFont);
				DropShadow = dropShadowFont;
			}

			if (internalConfig.ShadowConfig != null)
				Options.DropShadowActive = true;
			if (transToVp != null)
				Options.TransformToViewport = transToVp;

			InitialiseGlyphRenderer(config.CharacterOutput, config.FontGlyphRenderer, config.DropShadowRenderer);
		}

		private IGlyphRenderer FontRenderer {get;set;}
		public void InitialiseGlyphRenderer(IDrawCommandList commandList, IGlyphRenderer mainFont, IGlyphRenderer dropShadow)
		{			
			if (commandList == null)
				return;

			FontRenderer = mainFont ?? new BufferedGlyphRenderer (commandList, fontData, Vector3.Zero, new Vector4(1,1,1,1));

			if (DropShadow != null)
			{
				DropShadow.FontRenderer = dropShadow ?? new BufferedGlyphRenderer (commandList, DropShadow.fontData, Vector3.Zero, new Vector4(1,1,1,1));
			}
		}

		public static TransformViewport? SetupTransformViewport (float height, bool transformToCurrentOrthogProjection, Matrix4 transform, out float fontScale)
		{
			TransformViewport? transToVp = null;
			float result = 1f;
			if (transformToCurrentOrthogProjection)
				transToVp = OrthogonalTransform (transform, height, out result);
			fontScale = result;
			return transToVp;
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

		public float LineSpacing
		{
			get { return (float)Math.Ceiling(fontData.maxGlyphHeight * Options.LineSpacing); }
		}

		public bool IsMonospacingActive
		{
			get { return fontData.IsMonospacingActive(Options); }
		}


		public float MonoSpaceWidth
		{
			get { return fontData.GetMonoSpaceWidth(Options); }
		}

		#region SafeMeasure

		public SizeF SafeMeasure(Rectangle clientRectangle, string text, float maxWidth, QFontAlignment alignment)
		{
			var processedText = SafeProcessText(clientRectangle, text, maxWidth, alignment);
			return SafePrintOrMeasure(processedText,  true);
		}

		#endregion

		#region SafePrint


		public void SafePrint(Matrix4 transform, Rectangle clientRectangle, string text, float maxWidth, QFontAlignment alignment)
		{
			var processedText = SafeProcessText(clientRectangle, text, maxWidth, alignment);
			BeginPrint (transform);
			SafePrintOrMeasure(processedText,  false);
			EndPrint ();
		}

		private SizeF SafePrintOrMeasure(ProcessedText processedText, bool measureOnly)
		{
			// init values we'll return
			float maxMeasuredWidth = 0f;

			float xPos = 0f;
			float yPos = 0f;

			float xOffset = xPos;
			float yOffset = yPos;

			//			if (!measureOnly && !UsingVertexBuffers && Options.UseDefaultBlendFunction)
			//				GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

			float maxWidth = processedText.maxWidth;
			var alignment = processedText.alignment;

			//TODO - use these instead of translate when rendering by position (at some point)

			var nodeList = processedText.textNodeList;
			for (TextNode node = nodeList.Head; node != null; node = node.Next)
				node.LengthTweak = 0f;  //reset tweaks


			if (alignment == QFontAlignment.Right)
				xOffset -= (float)Math.Ceiling(TextNodeLineLength(nodeList.Head, maxWidth) - maxWidth);
			else if (alignment == QFontAlignment.Centre)
				xOffset -= (float)Math.Ceiling(0.5f * TextNodeLineLength(nodeList.Head, maxWidth));
			else if (alignment == QFontAlignment.Justify)
				JustifyLine(nodeList.Head, maxWidth);


			bool atLeastOneNodeCosumedOnLine = false;
			float length = 0f;
			for (TextNode node = nodeList.Head; node != null; node = node.Next)
			{
				bool newLine = false;

				if (node.Type == TextNodeType.LineBreak)
				{
					newLine = true;
				}
				else
				{

					if (Options.WordWrap && SkipTrailingSpace(node, length, maxWidth) && atLeastOneNodeCosumedOnLine)
					{
						newLine = true;
					}
					else if (length + node.ModifiedLength <= maxWidth || !atLeastOneNodeCosumedOnLine)
					{
						atLeastOneNodeCosumedOnLine = true;

						if (!measureOnly)
							RenderWord(xOffset + length, yOffset, node);
						length += node.ModifiedLength;

						maxMeasuredWidth = Math.Max(length, maxMeasuredWidth);

					}
					else if (Options.WordWrap)
					{
						newLine = true;
						if (node.Previous != null)
							node = node.Previous;
					}
					else
						continue; // continue so we still read line breaks even if reached max width
				}

				if (newLine)
				{
					//                        if (yOffset + LineSpacing - yPos >= processedText.maxSize.Height)
					//                            break;

					yOffset += LineSpacing;
					xOffset = xPos;
					length = 0f;
					atLeastOneNodeCosumedOnLine = false;

					if (node.Next != null)
					{
						if (alignment == QFontAlignment.Right)
							xOffset -= (float)Math.Ceiling(TextNodeLineLength(node.Next, maxWidth) - maxWidth);
						else if (alignment == QFontAlignment.Centre)
							xOffset -= (float)Math.Ceiling(0.5f * TextNodeLineLength(node.Next, maxWidth));
						else if (alignment == QFontAlignment.Justify)
							JustifyLine(node.Next, maxWidth);
					}
				}
			}

			return new SizeF(maxMeasuredWidth, yOffset + LineSpacing - yPos);
		}

		private ProcessedText SafeProcessText(Rectangle clientRectangle, string text, float maxWidth, QFontAlignment alignment)
		{
			//TODO: bring justify and alignment calculations in here

			maxWidth = TransformWidthToViewport(clientRectangle, maxWidth);

			var nodeList = new TextNodeList(text);
			nodeList.MeasureNodes(fontData, Options);

			//we "crumble" words that are two long so that that can be split up
			var nodesToCrumble = new List<TextNode>();
			foreach (TextNode node in nodeList)
				if (node.Length >= maxWidth && node.Type == TextNodeType.Word)
					nodesToCrumble.Add(node);

			foreach (var node in nodesToCrumble)
				nodeList.Crumble(node, 1);

			//need to measure crumbled words
			nodeList.MeasureNodes(fontData, Options);


			var processedText = new ProcessedText();
			processedText.textNodeList = nodeList;
			processedText.maxWidth = maxWidth;
			processedText.alignment = alignment;

			return processedText;
		}

		private float TransformWidthToViewport(Rectangle clientRect, float input)
		{
			var v2 = Options.TransformToViewport;
			if (v2 == null)
			{
				return input;
			}

			return input * ((float)clientRect.Width / v2.Value.Width);
		}

		private float TextNodeLineLength(TextNode node, float maxLength)
		{

			if (node == null)
				return 0;

			bool atLeastOneNodeCosumedOnLine = false;
			float length = 0;
			for (; node != null; node = node.Next)
			{

				if (node.Type == TextNodeType.LineBreak)
					break;

				if (SkipTrailingSpace(node, length, maxLength) && atLeastOneNodeCosumedOnLine)
					break;

				if (length + node.Length <= maxLength || !atLeastOneNodeCosumedOnLine)
				{
					atLeastOneNodeCosumedOnLine = true;
					length += node.Length;
				}
				else
				{
					break;
				}


			}
			return length;
		}

		private void JustifyLine(TextNode node, float targetLength)
		{

			bool justifiable = false;

			if (node == null)
				return;

			var headNode = node; //keep track of the head node


			//start by finding the length of the block of text that we know will actually fit:

			int charGaps = 0;
			int spaceGaps = 0;

			bool atLeastOneNodeCosumedOnLine = false;
			float length = 0;
			var expandEndNode = node; //the node at the end of the smaller list (before adding additional word)
			for (; node != null; node = node.Next)
			{



				if (node.Type == TextNodeType.LineBreak)
					break;

				if (SkipTrailingSpace(node, length, targetLength) && atLeastOneNodeCosumedOnLine)
				{
					justifiable = true;
					break;
				}

				if (length + node.Length < targetLength || !atLeastOneNodeCosumedOnLine)
				{

					expandEndNode = node;

					if (node.Type == TextNodeType.Space)
						spaceGaps++;

					if (node.Type == TextNodeType.Word)
					{
						charGaps += (node.Text.Length - 1);

						//word was part of a crumbled word, so there's an extra char cap between the two words
						if (CrumbledWord(node))
							charGaps++;

					}

					atLeastOneNodeCosumedOnLine = true;
					length += node.Length;
				}
				else
				{
					justifiable = true;
					break;
				}



			}


			//now we check how much additional length is added by adding an additional word to the line
			float extraLength = 0f;
			int extraSpaceGaps = 0;
			int extraCharGaps = 0;
			bool contractPossible = false;
			TextNode contractEndNode = null;
			for (node = expandEndNode.Next; node != null; node = node.Next)
			{


				if (node.Type == TextNodeType.LineBreak)
					break;

				if (node.Type == TextNodeType.Space)
				{
					extraLength += node.Length;
					extraSpaceGaps++;
				} 
				else if (node.Type == TextNodeType.Word)
				{
					contractEndNode = node;
					contractPossible = true;
					extraLength += node.Length;
					extraCharGaps += (node.Text.Length - 1);
					break;
				}
			}



			if (justifiable)
			{

				//last part of this condition is to ensure that the full contraction is possible (it is all or nothing with contractions, since it looks really bad if we don't manage the full)
				bool contract = contractPossible && (extraLength + length - targetLength) * Options.JustifyContractionPenalty < (targetLength - length) &&
					((targetLength - (length + extraLength + 1)) / targetLength > -Options.JustifyCapContract); 

				if((!contract && length < targetLength) || (contract && length + extraLength > targetLength))  //calculate padding pixels per word and char
				{

					if (contract)
					{
						length += extraLength + 1; 
						charGaps += extraCharGaps;
						spaceGaps += extraSpaceGaps;
					}



					int totalPixels = (int)(targetLength - length); //the total number of pixels that need to be added to line to justify it
					int spacePixels = 0; //number of pixels to spread out amongst spaces
					int charPixels = 0; //number of pixels to spread out amongst char gaps





					if (contract)
					{

						if (totalPixels / targetLength < -Options.JustifyCapContract)
							totalPixels = (int)(-Options.JustifyCapContract * targetLength);
					}
					else
					{
						if (totalPixels / targetLength > Options.JustifyCapExpand)
							totalPixels = (int)(Options.JustifyCapExpand * targetLength);
					}


					//work out how to spread pixles between character gaps and word spaces
					if (charGaps == 0)
					{
						spacePixels = totalPixels;
					}
					else if (spaceGaps == 0)
					{
						charPixels = totalPixels;
					}
					else
					{

						if(contract)
							charPixels = (int)(totalPixels * Options.JustifyCharacterWeightForContract * charGaps / spaceGaps);
						else 
							charPixels = (int)(totalPixels * Options.JustifyCharacterWeightForExpand * charGaps / spaceGaps);


						if ((!contract && charPixels > totalPixels) ||
							(contract && charPixels < totalPixels) )
							charPixels = totalPixels;

						spacePixels = totalPixels - charPixels;
					}


					int pixelsPerChar = 0;  //minimum number of pixels to add per char
					int leftOverCharPixels = 0; //number of pixels remaining to only add for some chars

					if (charGaps != 0)
					{
						pixelsPerChar = charPixels / charGaps;
						leftOverCharPixels = charPixels - pixelsPerChar * charGaps;
					}


					int pixelsPerSpace = 0; //minimum number of pixels to add per space
					int leftOverSpacePixels = 0; //number of pixels remaining to only add for some spaces

					if (spaceGaps != 0)
					{
						pixelsPerSpace = spacePixels / spaceGaps;
						leftOverSpacePixels = spacePixels - pixelsPerSpace * spaceGaps;
					}

					//now actually iterate over all nodes and set tweaked length
					for (node = headNode; node != null; node = node.Next)
					{

						if (node.Type == TextNodeType.Space)
						{
							node.LengthTweak = pixelsPerSpace;
							if (leftOverSpacePixels > 0)
							{
								node.LengthTweak += 1;
								leftOverSpacePixels--;
							}
							else if (leftOverSpacePixels < 0)
							{
								node.LengthTweak -= 1;
								leftOverSpacePixels++;
							}


						}
						else if (node.Type == TextNodeType.Word)
						{
							int cGaps = (node.Text.Length - 1);
							if (CrumbledWord(node))
								cGaps++;

							node.LengthTweak = cGaps * pixelsPerChar;


							if (leftOverCharPixels >= cGaps)
							{
								node.LengthTweak += cGaps;
								leftOverCharPixels -= cGaps;
							}
							else if (leftOverCharPixels <= -cGaps)
							{
								node.LengthTweak -= cGaps;
								leftOverCharPixels += cGaps;
							} 
							else  
							{
								node.LengthTweak += leftOverCharPixels;
								leftOverCharPixels = 0;
							}
						}

						if ((!contract && node == expandEndNode) || (contract && node == contractEndNode))
							break;

					}

				}

			}


		}

		private bool SkipTrailingSpace(TextNode node, float lengthSoFar, float boundWidth)
		{
			if (node.Type == TextNodeType.Space && node.Next != null && node.Next.Type == TextNodeType.Word && node.ModifiedLength + node.Next.ModifiedLength + lengthSoFar > boundWidth)
			{
				return true;
			}
			return false;
		}

		private bool CrumbledWord(TextNode node)
		{
			return (node.Type == TextNodeType.Word && node.Next != null && node.Next.Type == TextNodeType.Word);  
		}

		private void RenderWord(float x, float y, TextNode node)
		{
			if (node.Type != TextNodeType.Word)
				return;

			int charGaps = node.Text.Length - 1;
			bool isCrumbleWord = CrumbledWord(node);
			if (isCrumbleWord)
				charGaps++;

			int pixelsPerGap = 0;
			int leftOverPixels = 0;

			if (charGaps != 0)
			{
				pixelsPerGap = (int)node.LengthTweak / charGaps;
				leftOverPixels = (int)node.LengthTweak - pixelsPerGap * charGaps;
			}

			for(int i = 0; i < node.Text.Length; i++){
				char c = node.Text[i];
				if(fontData.CharSetMapping.ContainsKey(c)){
					var glyph = fontData.CharSetMapping[c];

					// TODO : intercept and add drop shadow
					RenderDropShadow (x, y, c, glyph);
					RenderGlyph(x,y,c, false);


					if (IsMonospacingActive)
						x += MonoSpaceWidth;
					else
						x += (int)Math.Ceiling(glyph.rect.Width + fontData.meanGlyphWidth * Options.CharacterSpacing + fontData.GetKerningPairCorrection(i, node.Text, node));

					x += pixelsPerGap;
					if (leftOverPixels > 0)
					{
						x += 1.0f;
						leftOverPixels--;
					}
					else if (leftOverPixels < 0)
					{
						x -= 1.0f;
						leftOverPixels++;
					}


				}
			}
		}

		#endregion

		#region PrintAt

		public void PrintAt(Rectangle clientRectangle, string text, float maxWidth, QFontAlignment alignment, Vector2 position)
		{
			position = TransformPositionToViewport(clientRectangle, position);
			position = LockToPixel(position);

			//GL.PushMatrix();
				var offset = new Vector3(position.X, position.Y, 0f);
				var headShift = Matrix4.CreateTranslation(offset);
				SafePrint(headShift, clientRectangle, text, maxWidth, alignment);
			//GL.PopMatrix();
		}

		private Vector2 TransformPositionToViewport(Rectangle clientRectangle, Vector2 input)
		{
			var v2 = Options.TransformToViewport;
			if (v2 == null)
			{
				return input;
			}

			float X, Y;

			X = (input.X - v2.Value.X) * (clientRectangle.Width / v2.Value.Width);
			Y = (input.Y - v2.Value.Y) * (clientRectangle.Height / v2.Value.Height);

			return new Vector2(X, Y);
		}

		private Vector2 LockToPixel(Vector2 input)
		{
			if (Options.LockToPixel)
			{
				float r = Options.LockToPixelRatio;
				return new Vector2((1 - r) * input.X + r * ((int)Math.Round(input.X)), (1 - r) * input.Y + r * ((int)Math.Round(input.Y)));
			}
			return input;
		}

		#endregion

		#region Print

		void BeginPrint (Matrix4 transform)
		{
			if (FontRenderer != null)
			{
				FontRenderer.Reset ();
				FontRenderer.Colour = new Vector4(Options.Colour.R, Options.Colour.G, Options.Colour.B, Options.Colour.A);		
				FontRenderer.Transform = transform;
			}
			if (DropShadow != null && DropShadow.FontRenderer != null)
			{
				DropShadow.FontRenderer.Reset ();
				DropShadow.FontRenderer.Colour = new Vector4(1,1,1, Options.DropShadowOpacity);
				DropShadow.FontRenderer.Transform = transform;
			}
		}

		void EndPrint ()
		{
			if (FontRenderer != null)
			{
				FontRenderer.Flush ();
			}
			if (DropShadow != null && DropShadow.FontRenderer != null)
			{
				DropShadow.FontRenderer.Flush ();
			}
		}

		public SizeF Print(Matrix4 transform, string text, QFontAlignment alignment)
		{
			BeginPrint (transform);
			var result = PrintOrMeasure(text, alignment, false);
			EndPrint ();
			return result;
		}

		private SizeF PrintOrMeasure(string text, QFontAlignment alignment, bool measureOnly)
		{
			float maxWidth = 0f;
			float xOffset = 0f;
			float yOffset = 0f;

			float maxXpos = float.MinValue;
			float minXPos = float.MaxValue;

			text = text.Replace("\r\n", "\r");

			if (alignment == QFontAlignment.Right)
				xOffset -= MeasureNextlineLength(text);
			else if (alignment == QFontAlignment.Centre)
				xOffset -= (int)(0.5f * MeasureNextlineLength(text));

			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];

				//newline
				if (c == '\r' || c == '\n')
				{
					yOffset += LineSpacing;
					xOffset = 0f;

					if (alignment == QFontAlignment.Right)
						xOffset -= MeasureNextlineLength(text.Substring(i + 1));
					else if (alignment == QFontAlignment.Centre)
						xOffset -= (int)(0.5f * MeasureNextlineLength(text.Substring(i + 1)));

				}
				else
				{

					minXPos = Math.Min(xOffset, minXPos);

					//normal character
					if (c != ' ' && fontData.CharSetMapping.ContainsKey(c))
					{
						QFontGlyph glyph = fontData.CharSetMapping[c];
						if (!measureOnly)
						{
							RenderDropShadow (xOffset, yOffset, c, glyph);
							RenderGlyph (xOffset, yOffset, c, false);
						}
					}

					if (IsMonospacingActive)
						xOffset += MonoSpaceWidth;
					else
					{
						if (c == ' ')
							xOffset += (float)Math.Ceiling(fontData.meanGlyphWidth * Options.WordSpacing);
						//normal character
						else if (fontData.CharSetMapping.ContainsKey(c))
						{
							QFontGlyph glyph = fontData.CharSetMapping[c];
							xOffset += (float)Math.Ceiling(glyph.rect.Width + fontData.meanGlyphWidth * Options.CharacterSpacing + fontData.GetKerningPairCorrection(i, text, null));
						}
					}

					maxXpos = Math.Max(xOffset, maxXpos);
				}
			}

			if (Math.Abs (minXPos - float.MaxValue) > float.Epsilon)
				maxWidth = maxXpos - minXPos;
	
			return new SizeF(maxWidth, yOffset + LineSpacing);
		}

		private float MeasureNextlineLength(string text)
		{

			float xOffset = 0;

			for(int i=0; i < text.Length;i++)
			{
				char c = text[i];

				if (c == '\r' || c == '\n')
				{
					break;
				}


				if (IsMonospacingActive)
				{
					xOffset += MonoSpaceWidth;
				}
				else
				{
					//space
					if (c == ' ')
					{
						xOffset += (float)Math.Ceiling(fontData.meanGlyphWidth * Options.WordSpacing);
					}
					//normal character
					else if (fontData.CharSetMapping.ContainsKey(c))
					{
						QFontGlyph glyph = fontData.CharSetMapping[c];
						xOffset += (float)Math.Ceiling(glyph.rect.Width + fontData.meanGlyphWidth * Options.CharacterSpacing + fontData.GetKerningPairCorrection(i, text, null));
					}
				}
			}
			return xOffset;
		}

		private void RenderGlyph(float x, float y, char c, bool isDropShadow)
		{
			if (FontRenderer != null)
			{
				FontRenderer.RenderGlyph (x, y, c, isDropShadow);
			}
		}

		private void RenderDropShadow(float x, float y, char c, QFontGlyph nonShadowGlyph)
		{
			//note can cast drop shadow offset to int, but then you can't move the shadow smoothly...
			if (DropShadow != null && Options.DropShadowActive)
			{
				//make sure fontdata font's options are synced with the actual options
				if (DropShadow.Options != Options)
					DropShadow.Options = Options;
				
				DropShadow.RenderGlyph(
					x + (fontData.meanGlyphWidth * Options.DropShadowOffset.X + nonShadowGlyph.rect.Width * 0.5f),
					y + (fontData.meanGlyphWidth * Options.DropShadowOffset.Y + nonShadowGlyph.rect.Height * 0.5f + nonShadowGlyph.yOffset), c, true);
			}
		}

		#endregion

		#region Measure

		public SizeF Measure(Rectangle clientRectangle, string text, QFontAlignment alignment = QFontAlignment.Left)
		{
			return TransformMeasureFromViewport(clientRectangle, PrintOrMeasure(text, alignment, true));
		}

		private SizeF TransformMeasureFromViewport(Rectangle clientRectangle, SizeF input)
		{
			var v2 = Options.TransformToViewport;
			if (v2 == null)
			{
				return input;
			}
			var v1 = clientRectangle;

			float X, Y;

			X = input.Width * ((float)v2.Value.Width / v1.Width);
			Y = input.Height * ((float)v2.Value.Height / v1.Height);

			return new SizeF(X, Y);
		}

		#endregion
	}
}

