﻿using System;
using QuickFont;
using OpenTK;
using OpenTK.Graphics;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.IO;
using System.Collections.Generic;

namespace NextFont.ConsoleApplication
{
	public class Example : GameWindow
	{
		#region string constants

		static String introduction = @"Welcome to the QuickFont tutorial. All text in this tutorial (including headings!) is drawn with QuickFont, so it is also intended to showcase the library. :) If you want to get started immediately, you can skip the rest of this introduction by pressing [Right]. You can also press [Left] to go back to previous pages at any point" + Environment.NewLine + Environment.NewLine +
			"Why QuickFont? QuickFont is intended as a replacement (and improvement upon) OpenTK's TextPrinter library. My primary motivation for writing it was for practical reasons: I'm using OpenTK to write a game, and currently the most annoying bugs are all being caused by TextPrinter: it is slow, it is buggy, and no one wants to maintain it." + Environment.NewLine + Environment.NewLine +
			"I did consider simply fixing it, but then decided that it would be easier and more fun to write my own library from scratch. That is exactly what I've done." + Environment.NewLine + Environment.NewLine +
			"In fact it's turned out to be well worth it. It has only taken me a few days to write the library, and already it has quite a few really cool features which I will be using in my game.";

		const String usingQuickFontIsSuperEasy = @"Using QuickFont is super easy. To load a font: ";
		const String loadingAFont1 = "myFont = new QFont(\"HappySans.ttf\", 16);";
		const String andPrintWithIt = @"...and to print with it: ";
		static String printWithFont1 = "QFont.Begin();" + Environment.NewLine + "myFont.Print(\"Hello World!\")" + Environment.NewLine + "QFont.End();";
		const String itIsAlsoEasyToMeasure = "It is also very easy to measure text: ";
		const String measureText1 = "var bounds = myFont.Measure(\"Hello World\"); ";

		const String oneOfTheFirstGotchas = "One of the first \"gotchas\" that I experienced with the old TextPrinter was having to manage a private font collection. Unlike TextPrinter, QuickFont does not need the private font collection (or Font object for that matter) to exist after construction. QuickFont works out everything it needs at load time, hence you can just pass it a file name, it will load the pfc internally and then chuck it away immediately. If you still prefer to manage a font collection yourself, and you simply want to create a QuickFont from a font object, that's fine: QuickFont has a constructor for this:  ";
		const String loadingAFont2 = "myFont = new QFont(fontObject);";

		static String whenPrintingText = "When printing text, you can specify" + Environment.NewLine +
			"an alignment. Unbounded text can" + Environment.NewLine + 
			"be left-aligned, right-aligned " + Environment.NewLine +
			"or centered. You specify the " + Environment.NewLine + 
			"alignment as follows: ";


		const String printWithFont2 = "myFont.Print(\"Hello World!\",QFontAlignment.Right)";

		static String righAlignedText = "Right-aligned text will appear" + Environment.NewLine +
			"to the left of the original" + Environment.NewLine +
			"position, given by this red line.";


		static String centredTextAsYou = "Centred text, as you would expect, is centred" + Environment.NewLine +
			"around the current position. The default alignment" + Environment.NewLine +
			"is Left. As you can see, you can include " + Environment.NewLine +
			"line-breaks in unbounded text.";

		static String ofCourseItsNot = "Of course, it's not much fun having to insert your own line-breaks. A much better option is to simply specify the bounds of your text, and then let QuickFont decide where the line-breaks should go for you. You do this by specifying maxWidth. " + Environment.NewLine + Environment.NewLine +
			"You can still specify line-breaks for new paragraphs. For example, this is all written using a single print. QuickFont is also clever enough to spot where it might have accidentally inserted a line-break just before you have explicitly included one in the text. In this case, it will make sure that it does not insert a redundant line-break. :)" + Environment.NewLine + Environment.NewLine +
			"Another really cool feature of QuickFont, as you may have guessed already, is that it supports justified bounded text. It was quite tricky to get it all working pixel-perfectly, but as you can see, the results are pretty good. The precise justification settings are configurable in myFont.Options." + Environment.NewLine + Environment.NewLine +
			"You can press the [Up] and [Down] arrow keys to change the alignment on this block of bounded text. You can also press the [Enter] key to test some serious bounding! Note that the bound height is always ignored.";

		static String anotherCoolFeature = "QuickFont works by using the System.Drawing to render to a bitmap, and then measuring and targeting each glyph before packing them into another bitmap which is then turned into an OpenGL texture. So essentially all fonts are \"texture\" fonts. However, QuickFont also allows you to get at the bitmaps before they are turned into OpenGL textures, save them to png file(s), modify them and then load (and retarget/remeasure) them again as QFonts. Sound complicated? - Don't worry, it's really easy. :)" + Environment.NewLine + Environment.NewLine +
			"Firstly, you need to create your new silhouette files from an existing font. You only want to call this code once, as calling it again will overwrite your modified .png, so take care. :) ";

		const String textureFontCode1 = "QFont.CreateTextureFontFiles(\"HappySans.ttf\",16,\"myTextureFont\");";


		const String thisWillHaveCreated = "This will have created two files: \"myTextureFont.qfont\" and \"myTextureFont.png\" (or possibly multiple png files if your font is very large - I will explain how to configure this later). The next step is to actually texture your font. The png file(s) contain packed font silhouettes, perfect for layer effects in programs such as photoshop or GIMP. I suggest locking the alpha channel first, because QuickFont will complain if you merge two glyphs. You can enlarge glyphs at this stage, and QuickFont will automatically retarget each glyph when you next load the texture; however, it will fail if glyphs are merged...    ";

		const String ifYouDoIntend = "...if you do intend to increase the size of the glyphs, then you can configure the silhouette texture to be generated with larger glyph margins to avoid glyphs merging. Here, I've also configured the texture sheet size a bit larger because the font is large and I want it all on one sheet for convenience: ";

		static String textureFontCode2 = "QFontBuilderConfiguration config = new QFontBuilderConfiguration();" + Environment.NewLine +
			"config.GlyphMargin = 6;" + Environment.NewLine +
			"config.PageWidth = 1024;" + Environment.NewLine +
			"config.PageHeight = 1024;" + Environment.NewLine +
			"QFont.CreateTextureFontFiles(\"HappySans.ttf\",48,config,\"myTextureFont\");";


		static String actuallyTexturing = "Actually texturing the glyphs is really going to come down to how skilled you are in photoshop, and how good the original font was that you chose as a silhouette. To give you an idea: this very cool looking font I'm using for headings only took me 3 minutes to texture in photoshop because I did it with layer affects that did all glyphs at once. :)" + Environment.NewLine + Environment.NewLine +
			"Anyway, once you've finished texturing your font, save the png file. Now you can load the font and write with it just like any other font!";

		const String textureFontCode3 = "myTexureFont = QFont.FromQFontFile(\"myTextureFont.qfont\");";

		const String asIhaveleant = "As I have learnt, trying to create drop-shadows as part of the glyphs themselves gives very poor results because the glyphs become larger than usual and the font looks poor when printed. To do drop-shadows properly, they need to be rendered separately underneath each glyph. This is what QuickFont does. In fact it does a lot more: it will generate the drop-shadow textures for you. It's super-easy to create a font with a drop-shadow: ";
		const String dropShadowCode1 = "myFont = new QFont(\"HappySans.ttf\", 16, new QFontBuilderConfiguration(true));";
		const String thisWorksFine = "This works fine for texture fonts too: ";
		const String dropShadowCode2 = "myTexureFont = QFont.FromQFontFile(\"myTextureFont.qfont\", new QFontLoaderConfiguration(true));";
		const String onceAFont = "Once a font has been loaded with a drop-shadow, it will automatically be rendered with a shadow. However, you can turn this off or customise the drop-shadow in myFont.options when rendering (I am rotating the drop shadow here, which looks kind of cool but is now giving me a headache xD). I've turned drop-shadows on for this font on this page; however, they are very subtle because the font is so tiny. If you want the shadow to be more visible for tiny fonts like this, you could modify the DropShadowConfiguration object passed into the font constructor to blur the shadow less severely during creation. ";

		static String thereAreActually = "There are actually a lot more interesting config values and neat things that QuickFont does. Now that I look back at it, it's a bit crazy that I got this all done in a few days, but this tutorial is getting a bit tedious to write and I'm dying to get on with making my game, so I'm going to leave it at this. " + Environment.NewLine + Environment.NewLine +
			"I suppose I should also mention that there are almost certainly going to be a few bugs. Let me know if you find any and I will get them fixed asap. :) " + Environment.NewLine + Environment.NewLine +
			"I should probably also say something about the code: it's not unit tested and it probably would need a good few hours of refactoring before it would be clean enough to be respectable. I will do this at some point. Also, feel free to berate me if I'm severely breaking any conventions. I'm a programmer by profession and really should know better. ;)" + Environment.NewLine + Environment.NewLine +
			"With regard to features: I'm probably not going to add many more to this library. It really is intended for rendering cool-looking text quickly for things like games. If you want highly formatted text, for example, then it probably isn't the right tool. I hope you find it useful; I know I already do! :P" + Environment.NewLine + Environment.NewLine +
			"A tiny disclaimer: all of QuickFont is written from scratch apart from ~100 lines I stole from TextPrinter for setting the correct perspective. Obviously the example itself is just a hacked around version of the original example that comes with OpenTK.";

		const String hereIsSomeMono = "Here is some mononspaced text.  Monospaced fonts will automatically be rendered in monospace mode; however, you can render monospaced fonts ordinarily " +
			"or ordinary fonts in monospace mode using the render option:";
		const String monoCode1 = " myFont.Options.Monospacing = QFontMonospacing.Yes; ";
		const String theDefaultMono = "The default value for this is QFontMonospacing.Natural which simply means that if the underlying font was monospaced, then use monospacing. ";

		static String mono =           " **   **   **   *  *   **  " + Environment.NewLine +                  
			" * * * *  *  *  ** *  *  * " + Environment.NewLine +                
			" *  *  *  *  *  * **  *  * " + Environment.NewLine +             
			" *     *   **   *  *   **  ";         
		#endregion

		public Example()
			: base(800, 600, GraphicsMode.Default, "OpenTK Quick Start Sample")
		{
			VSync = VSyncMode.On;
			this.WindowBorder = WindowBorder.Fixed;
		}

		NxFont heading1 = null;
		NxFont heading2;
		NxFont mainText;
		NxFont codeText;
		NxFont controlsText;
		NxFont monoSpaced;

		int currentDemoPage = 0;
		const int FIRST_PAGE = 0;
		const int LAST_PAGE = 8;

		QFontAlignment cycleAlignment = QFontAlignment.Left;
		double cnt = 0;
		double boundsAnimationCnt = 1.0f;

		bool useShader = true;

		private void InitialiseKeyDown()
		{
			KeyDown += (sender, ke) => {
				if (ke.Key == Key.Escape)
				{
					this.Exit ();
				}
			};

			KeyDown += (sender, ke) => {

				switch (ke.Key)
				{
				case Key.Space:
					useShader = !useShader;
					break;
				case Key.Right:
					currentDemoPage++;
					break;

				case Key.BackSpace:
				case Key.Left:
					currentDemoPage--;
					break;

				case Key.Enter:
					{
						if (currentDemoPage == 4)
							boundsAnimationCnt = 0f;

					}
					break;

				case Key.Up:
					{
						if (currentDemoPage == 4)
						{
							if (cycleAlignment == QFontAlignment.Justify)
								cycleAlignment = QFontAlignment.Left;
							else
								cycleAlignment++;    
						}


					}
					break;


				case Key.Down:
					{
						if (currentDemoPage == 4)
						{
							if (cycleAlignment == QFontAlignment.Left)
								cycleAlignment = QFontAlignment.Justify;
							else
								cycleAlignment--;    
						}


					}
					break;
				case Key.F9:

					break;

				}

				if (currentDemoPage > LAST_PAGE)
					currentDemoPage = LAST_PAGE;

				if (currentDemoPage < FIRST_PAGE)
					currentDemoPage = FIRST_PAGE;
			};
		}

		private void CheckGLError()
		{
			var error = GL.GetError ();
			if (error != ErrorCode.NoError)
			{
  				throw new Exception (error.ToString());
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			InitialiseKeyDown ();

			// setup settings, load textures, sounds
			this.VSync = VSyncMode.On;
			this.WindowBorder = WindowBorder.Fixed;

			var screenMatrix = Matrix4.CreateOrthographicOffCenter(ClientRectangle.X, Width, Height, ClientRectangle.Y, -1, 1);

			var singleDrawCommand = new DrawCommandList ();

			var fontFileLoader = new NxFontFileLoader ();

			var heading2Config = new NxFontLoaderConfiguration (singleDrawCommand, screenMatrix, true);
			heading2 = fontFileLoader.Load("woodenFont.qfont", Height, 1.0f, heading2Config);

			var builderConfig = new NxFontBuilderConfiguration(singleDrawCommand, screenMatrix, true);
			builderConfig.BlurRadius = 1; //reduce blur radius because font is very small
			builderConfig.TextGenerationRenderHint = TextGenerationRenderHint.ClearTypeGridFit; //best render hint for this font
			mainText = new NxFont("Fonts/times.ttf", 14,  Height, builderConfig);

			var heading1Config = new NxFontBuilderConfiguration(singleDrawCommand, screenMatrix, true);
			heading1Config.Transform = screenMatrix;
			heading1 = new NxFont("Fonts/HappySans.ttf", 72, Height, heading1Config);

			var buildConfig = new NxFontBuilderConfiguration (singleDrawCommand, screenMatrix, true);
			controlsText = new NxFont("Fonts/HappySans.ttf", 32,  Height, buildConfig);

			var noShadowConfig = new NxFontBuilderConfiguration (singleDrawCommand, screenMatrix);
			codeText = new NxFont("Fonts/Comfortaa-Regular.ttf", 12,  Height, FontStyle.Regular, noShadowConfig);

			heading1.Options.Colour = new Color4(0.2f, 0.2f, 0.2f, 1.0f);
			mainText.Options.Colour = new Color4(0.1f, 0.1f, 0.1f, 1.0f);
			mainText.Options.DropShadowActive = false;
			codeText.Options.Colour = new Color4(0.0f, 0.0f, 0.4f, 1.0f);

			//					var config2 = new NxFontBuilderConfiguration(screenMatrix);
			//					config2.SuperSampleLevels = 1;
			//   font = new QFont("Fonts/times.ttf", 16,config2);
			//   font.Options.Colour = new Color4(0.1f, 0.1f, 0.1f, 1.0f);
			//   font.Options.CharacterSpacing = 0.1f;

			monoSpaced = new NxFont("Fonts/Anonymous.ttf", 10, Height, noShadowConfig);
			monoSpaced.Options.Colour = new Color4(0.1f, 0.1f, 0.1f, 1.0f);

			Console.WriteLine(" Monospaced : " + monoSpaced.IsMonospacingActive);

			GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
			GL.Disable(EnableCap.DepthTest);

			mOrthographicMatrix = Matrix4.CreateOrthographicOffCenter (0f, ClientRectangle.Width, ClientRectangle.Height, 0f, -1.0f, 1f);

			GenerateRightArrow ();
			var rightArrowCommands = singleDrawCommand.GetCommands ();
			singleDrawCommand.ClearCommandsOnly ();

			GenerateLeftArrow ();
			var leftArrowCommands = singleDrawCommand.GetCommands ();
			singleDrawCommand.ClearCommandsOnly ();


			var pageCommands = new List<DrawElementsIndirectCommand[]> ();
			for (int i = FIRST_PAGE; i < MAX_PAGES; ++i)
			{
				currentDemoPage = i;
				GenerateText ();
				pageCommands.Add (singleDrawCommand.GetCommands ());
				singleDrawCommand.ClearCommandsOnly ();
			}
			currentDemoPage = FIRST_PAGE;

			mSharedVertexBuffer = singleDrawCommand.AsStaticText ();
			var bufferArray = singleDrawCommand.Blocks.ToArray ();
			mConstantBuffer = new SentanceBlockStorageBuffer (bufferArray, BufferUsageHint.StaticDraw);


			screenPages = new TextOutput[MAX_PAGES];
			for (int i = 0; i < MAX_PAGES; ++i)
			{
				screenPages[i] = new TextOutput (mSharedVertexBuffer, pageCommands[i], mConstantBuffer); 
			}

			leftArrow = new TextOutput (mSharedVertexBuffer, leftArrowCommands, mConstantBuffer);
			rightArrow = new TextOutput (mSharedVertexBuffer, rightArrowCommands, mConstantBuffer);



			CheckGLError ();

			InitialiseUnload ();

			using (var vert = File.OpenRead (@"Shaders/BindlessTex.vert"))
			using (var frag = File.OpenRead (@"Shaders/BindlessTex.frag"))				
			{
				var manager = new ShaderManager ();
				programID = manager.CreateFragmentProgram (vert, frag, "");

				GL.UseProgram (programID);
					mSharedVertexBuffer.BindManually (programID);					
				GL.BindVertexArray (0);
				GL.UseProgram (0);
			}
		}

		const int MAX_PAGES = 9;

		void InitialiseUnload ()
		{
			Unload += (sender, e) =>  {
				GL.DeleteProgram(programID);
				for (int i = 0; i < MAX_PAGES; ++i)
				{
					screenPages[i].Dispose();
				}
				leftArrow.Dispose();
				rightArrow.Dispose();
				mSharedVertexBuffer.Dispose();
				mConstantBuffer.Dispose();
			};
		}

		public int programID;

		public Matrix4 mOrthographicMatrix;
		public SentanceBlockStorageBuffer mConstantBuffer;
		public TextVertexBuffer mSharedVertexBuffer;

		public TextOutput[] screenPages;

		public TextOutput leftArrow;
		public TextOutput rightArrow;

		private float[] vertexData = {
			-1f, -1f, 0,
			1.0f, 1.0f,

			0.9f, -1f, 0,
			0.9f, 0.9f,

			1f, 1f, 0,
			0.9f, 0.9f,

			-1f, 0.9f, 0,
			0.9f, 0.9f,
		};

		public uint[] drawIDData = {
			0, 1, 2, 3, 0, 0,
		};

		public uint[] elementData = {
			0, 1, 2,
			2, 3, 0,
		};



		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			base.OnUpdateFrame(e);

			// add game logic, input handling

			// update shader uniforms

			// update shader mesh

			//GenerateText ();

			//	QFont.End();
		}

		void GenerateLeftArrow ()
		{
			//	GL.PushMatrix();
			var offset = new Vector3 (10 + 16 * (float)(1 + Math.Sin (cnt * 4)), Height - controlsText.Measure (ClientRectangle, "P").Height - 10f, 0f);
			var transform = Matrix4.CreateTranslation (offset);
			controlsText.Options.Colour = new Color4 (0.8f, 0.1f, 0.1f, 1.0f);
			var mvp = Matrix4.Mult (transform, mOrthographicMatrix);
			controlsText.Print (mvp, "<- Press [Left]", QFontAlignment.Left);
			//	GL.PopMatrix();
		}

		void GenerateRightArrow ()
		{
			var offset = new Vector3 (Width - 10 - 16 * (float)(1 + Math.Sin (cnt * 4)), Height - controlsText.Measure (ClientRectangle, "P").Height - 10f, 0f);
			var transform = Matrix4.CreateTranslation (offset);
			controlsText.Options.Colour = new Color4 (0.8f, 0.1f, 0.1f, 1.0f);
			var mvp = Matrix4.Mult (transform, mOrthographicMatrix);
			controlsText.Print (mvp, "Press [Right] ->", QFontAlignment.Right);
		}

		void GenerateText ()
		{
			switch (currentDemoPage)
			{
			case 0:
				{
					float yOffset = 0;

					var offset1 = new Vector3 (Width * 0.5f, yOffset, 0f);
					var transform1 = Matrix4.CreateTranslation (offset1);

					var mvp = Matrix4.Mult (transform1, mOrthographicMatrix);
					heading1.Print (mvp, "QuickFont", QFontAlignment.Centre);
					yOffset += heading1.Measure (ClientRectangle, "QuickFont").Height;

					var offset2 = new Vector3 (20f, yOffset, 0f);
					var transform2 = Matrix4.CreateTranslation (offset2);

					var mvp2 = Matrix4.Mult (transform2, mOrthographicMatrix);
					heading2.Print (mvp2, "Introduction", QFontAlignment.Left);
					yOffset += heading2.Measure (ClientRectangle, "Introduction").Height;

//
					var offset3 = new Vector3 (30f, yOffset + 20, 0f);
					var transform3 = Matrix4.CreateTranslation (offset3);
					var mvp3 = Matrix4.Mult (transform3, mOrthographicMatrix);
					mainText.SafePrint (mvp3, ClientRectangle, introduction, Width - 60, QFontAlignment.Justify);
					//GL.PopMatrix();
					//QFont.End();

				}
				break;
			case 1:
				{
					float yOffset = 20;
					//QFont.Begin();
					//GL.PushMatrix();
					var offset = new Vector3 (20f, yOffset, 0f);
					var transform = Matrix4.CreateTranslation (offset);
					var mvp = Matrix4.Mult (transform, mOrthographicMatrix);

					heading2.Print (mvp, "Easy as ABC!", QFontAlignment.Left);
					yOffset += heading2.Measure (ClientRectangle, "Easy as ABC!").Height;

					//GL.PopMatrix();
					PrintComment (usingQuickFontIsSuperEasy, ref yOffset);
					PrintCode (loadingAFont1, ref yOffset);
					PrintComment (andPrintWithIt, ref yOffset);
					PrintCode (printWithFont1, ref yOffset);
					PrintComment (itIsAlsoEasyToMeasure, ref yOffset);
					PrintCode (measureText1, ref yOffset);
					PrintComment (oneOfTheFirstGotchas, ref yOffset);
					PrintCode (loadingAFont2, ref yOffset);
					//QFont.End();
				}
				break;
			case 2:
				{
					float yOffset = 20;
					//QFont.Begin();
					//GL.PushMatrix();
					var offset = new Vector3 (20f, yOffset, 0f);
					var transform = Matrix4.CreateTranslation (offset);
					var mvp = Matrix4.Mult (transform, mOrthographicMatrix);
					heading2.Print (mvp, "Alignment", QFontAlignment.Left);
					yOffset += heading2.Measure (ClientRectangle, "Easy as ABC!").Height;
					//GL.PopMatrix();
					PrintCommentWithLine (whenPrintingText, QFontAlignment.Left, Width * 0.5f, ref yOffset);
					PrintCode (printWithFont2, ref yOffset);
					PrintCommentWithLine (righAlignedText, QFontAlignment.Right, Width * 0.5f, ref yOffset);
					yOffset += 10f;
					PrintCommentWithLine (centredTextAsYou, QFontAlignment.Centre, Width * 0.5f, ref yOffset);
					//QFont.End();
				}
				break;
			case 3:
				{
					float yOffset = 20;
					//QFont.Begin();
					//GL.PushMatrix();
					var offset = new Vector3 (20f, yOffset, 0f);
					var transform = Matrix4.CreateTranslation (offset);
					var mvp = Matrix4.Mult (transform, mOrthographicMatrix);

					heading2.Print (mvp, "Bounds and Justify", QFontAlignment.Left);
					yOffset += heading2.Measure (ClientRectangle, "Easy as ABC!").Height;
					//GL.PopMatrix();
					//GL.PushMatrix();
					yOffset += 20;
					var offset2 = new Vector3 ((int)(Width * 0.5), yOffset, 0f);
					var transform2 = Matrix4.CreateTranslation (offset2);
					var mvp2 = Matrix4.Mult (transform2, mOrthographicMatrix);
					controlsText.Print (mvp2, "Press [Up], [Down] or [Enter]!", QFontAlignment.Centre);
					yOffset += controlsText.Measure (ClientRectangle, "[]").Height;
					//GL.PopMatrix();
					float boundShrink = (int)(350 * (1 - Math.Cos (boundsAnimationCnt * Math.PI * 2)));
					yOffset += 15;

					PrintWithBounds (mainText, ofCourseItsNot, new RectangleF (30f + boundShrink * 0.5f, yOffset, Width - 60 - boundShrink, 350f), cycleAlignment, ref yOffset);
					string printWithBounds = "myFont.Print(text,400f,QFontAlignment." + cycleAlignment + ");";
					yOffset += 15f;
					PrintCode (printWithBounds, ref yOffset);
					//QFont.End();
				}
				break;
			case 4:
				{
					float yOffset = 20;
					//QFont.Begin();
					//GL.PushMatrix();
					var offset = new Vector3 (20f, yOffset, 0f);
					var transform = Matrix4.CreateTranslation (offset);
					var mvp = Matrix4.Mult (transform, mOrthographicMatrix);
					heading2.Print (mvp, "Your own Texture Fonts", QFontAlignment.Left);
					yOffset += heading2.Measure (ClientRectangle, "T").Height;
					//GL.PopMatrix();
					PrintComment (anotherCoolFeature, ref yOffset);
					PrintCode (textureFontCode1, ref yOffset);
					PrintComment (thisWillHaveCreated, ref yOffset);
					//QFont.End();
				}
				break;
			case 5:
				{
					float yOffset = 20;
					//QFont.Begin();
					//GL.PushMatrix();
					var offset = new Vector3 (20f, yOffset, 0f);
					var transform = Matrix4.CreateTranslation (offset);
					var mvp = Matrix4.Mult (transform, mOrthographicMatrix);
					heading2.Print (mvp, "Your own Texture Fonts", QFontAlignment.Left);
					yOffset += heading2.Measure (ClientRectangle, "T").Height;
					//GL.PopMatrix();
					PrintComment (ifYouDoIntend, ref yOffset);
					PrintCode (textureFontCode2, ref yOffset);
					PrintComment (actuallyTexturing, ref yOffset);
					PrintCode (textureFontCode3, ref yOffset);
					//	QFont.End();
				}
				break;
			case 6:
				{
					float yOffset = 20;
					//QFont.Begin();
					heading2.Options.DropShadowOffset = new Vector2 (0.1f + 0.2f * (float)Math.Sin (cnt), 0.1f + 0.2f * (float)Math.Cos (cnt));
					//GL.PushMatrix();
					var offset = new Vector3 (20f, yOffset, 0f);
					var transform = Matrix4.CreateTranslation (offset);
					var mvp = Matrix4.Mult (transform, mOrthographicMatrix);
					heading2.Print (mvp, "Drop Shadows", QFontAlignment.Left);
					yOffset += heading2.Measure (ClientRectangle, "T").Height;
					//	GL.PopMatrix();
					heading2.Options.DropShadowOffset = new Vector2 (0.16f, 0.16f);
					//back to default
					mainText.Options.DropShadowActive = true;
					mainText.Options.DropShadowOpacity = 0.7f;
					mainText.Options.DropShadowOffset = new Vector2 (0.1f + 0.2f * (float)Math.Sin (cnt), 0.1f + 0.2f * (float)Math.Cos (cnt));
					PrintComment (asIhaveleant, ref yOffset);
					PrintCode (dropShadowCode1, ref yOffset);
					PrintComment (thisWorksFine, ref yOffset);
					PrintCode (dropShadowCode2, ref yOffset);
					PrintComment (onceAFont, ref yOffset);
					mainText.Options.DropShadowActive = false;
					//	QFont.End();
				}
				break;
			case 7:
				{
					float yOffset = 20;
					//	QFont.Begin();
					monoSpaced.Options.CharacterSpacing = 0.05f;
					//	GL.PushMatrix();
					var offset = new Vector3 (20f, yOffset, 0f);
					var transform = Matrix4.CreateTranslation (offset);
					var mvp = Matrix4.Mult (transform, mOrthographicMatrix);
					heading2.Print (mvp, "Monospaced Fonts", QFontAlignment.Left);
					yOffset += heading2.Measure (ClientRectangle, "T").Height;
					//	GL.PopMatrix();
					WriteComment (monoSpaced, hereIsSomeMono, QFontAlignment.Left, ref yOffset);
					PrintCode (monoCode1, ref yOffset);
					WriteComment (monoSpaced, theDefaultMono, QFontAlignment.Left, ref yOffset);
					WriteCommentWithLine (monoSpaced, mono, QFontAlignment.Left, Width * 0.5f, ref yOffset);
					yOffset += 2f;
					WriteCommentWithLine (monoSpaced, mono, QFontAlignment.Right, Width * 0.5f, ref yOffset);
					yOffset += 2f;
					WriteCommentWithLine (monoSpaced, mono, QFontAlignment.Centre, Width * 0.5f, ref yOffset);
					yOffset += 2f;
					monoSpaced.Options.CharacterSpacing = 0.5f;
					const string AS_USUAL_COMMENT = "As usual, you can adjust character spacing with myFont.Options.CharacterSpacing.";
					WriteComment (monoSpaced, AS_USUAL_COMMENT, QFontAlignment.Left, ref yOffset);
					//	QFont.End();
				}
				break;
			case 8:
				{
					float yOffset = 20;
					//	QFont.Begin();
					//	GL.PushMatrix();
					var offset = new Vector3 (20f, yOffset, 0f);
					var transform = Matrix4.CreateTranslation (offset);
					var mvp = Matrix4.Mult (transform, mOrthographicMatrix);
					heading2.Print (mvp, "In Conclusion", QFontAlignment.Left);
					yOffset += heading2.Measure (ClientRectangle, "T").Height;
					//	GL.PopMatrix();
					PrintComment (thereAreActually, ref yOffset);
					//	QFont.End();
				}
				break;
			}
//			//	QFont.Begin();
//			if (currentDemoPage != LAST_PAGE)
//			{
//				//	GL.PushMatrix();
//				GenerateRightArrow ();
//
//				//	GL.PopMatrix();
//			}
//			if (currentDemoPage != 1)
//			{
//				GenerateLeftArrow ();
//			}
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			base.OnRenderFrame(e);
		//	GL.ClearColor(0.25f, 0.75f, 0.5f, 1f);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			//GL.UseProgram(programID);
	//		GL.BindVertexArray (0);

			GL.Begin(PrimitiveType.Quads);
				GL.Color3(1.0f, 1.0f, 1.0);
				GL.Vertex2(-1f, -1f);
				GL.Color3(0.9f, 0.9f, 0.9f);
				GL.Vertex2(-1f, 1f);
				GL.Color3(0.9f, 0.9f, 0.9f);
				GL.Vertex2(1f, 1f);
				GL.Color3(0.9f, 0.9f, 0.9f);
				GL.Vertex2(1f, -1f);
			GL.End();

			var commands = new DrawElementsIndirectCommand[1];

			int stride = System.Runtime.InteropServices.Marshal.SizeOf (typeof(DrawElementsIndirectCommand));

			commands [0].Count = (uint)elementData.Length;
			commands [0].InstanceCount = 1;
			commands [0].FirstIndex = 0;
			commands [0].BaseVertex = 0;
			// IMPORTANT - controls material index
			commands [0].BaseInstance = 0;

			if (useShader)
			{
				GL.UseProgram (programID);
			}

			GL.Enable (EnableCap.Blend);
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

			mSharedVertexBuffer.Bind ();
			mConstantBuffer.Bind ();

			screenPages [currentDemoPage].Render ();

			if (currentDemoPage != LAST_PAGE)
			{
				rightArrow.Render ();
			}
			if (currentDemoPage != FIRST_PAGE)
			{
				leftArrow.Render ();
			}

			mSharedVertexBuffer.Unbind ();
			mConstantBuffer.Unbind ();

			GL.Disable (EnableCap.Blend);

			GL.UseProgram(0);

			SwapBuffers();
		}

		#region Helper functions

		private void PrintWithBounds(NxFont font, string text, RectangleF bounds, QFontAlignment alignment, ref float yOffset)
		{
//			GL.Disable(EnableCap.Texture2D);
//			GL.Color4(1.0f, 0f, 0f, 1.0f);

			float maxWidth = bounds.Width;

			float height = font.SafeMeasure(ClientRectangle, text, maxWidth, alignment).Height;

//			GL.Begin(BeginMode.LineLoop);
//				GL.Vertex3(bounds.X, bounds.Y, 0f);
//				GL.Vertex3(bounds.X + bounds.Width, bounds.Y, 0f);
//				GL.Vertex3(bounds.X + bounds.Width, bounds.Y + height, 0f);
//				GL.Vertex3(bounds.X, bounds.Y + height, 0f);
//			GL.End();

			font.PrintAt(ClientRectangle, text, maxWidth, alignment, new Vector2(bounds.X,bounds.Y));

			yOffset += height;

		}

		public void PrintComment(string comment, ref float yOffset)
		{
			WriteComment(mainText, comment, QFontAlignment.Justify, ref yOffset);
		}	

		private void WriteComment(NxFont font, string comment,QFontAlignment alignment, ref float yOffset)
		{
			//GL.PushMatrix();
				yOffset += 20;
				var offset = new Vector3(30f, yOffset, 0f);
				var transform = Matrix4.CreateTranslation(offset);

				var mvp = Matrix4.Mult (transform, mOrthographicMatrix);

				font.SafePrint(mvp, ClientRectangle, comment, Width - 60, alignment);
				yOffset += font.SafeMeasure(ClientRectangle, comment, Width - 60, alignment).Height;
			//GL.PopMatrix();
		}

		public void PrintCode(string code, ref float yOffset)
		{
		//	GL.PushMatrix();
				yOffset += 20;
				var offset = new Vector3(50f, yOffset, 0f);
				var transform = Matrix4.CreateTranslation(offset);
				var mvp = Matrix4.Mult (transform, mOrthographicMatrix);
				codeText.SafePrint(mvp, ClientRectangle, code, Width - 50, QFontAlignment.Left);
				yOffset += codeText.SafeMeasure(ClientRectangle, code, Width - 50, QFontAlignment.Left).Height;
			//GL.PopMatrix();
		}

		private void PrintCommentWithLine(string comment, QFontAlignment alignment, float xOffset, ref float yOffset)
		{
			WriteCommentWithLine(mainText, comment, alignment, xOffset, ref yOffset);
		}

		private void WriteCommentWithLine(NxFont font, string comment, QFontAlignment alignment, float xOffset, ref float yOffset)
		{
			//GL.PushMatrix();
				yOffset += 20;
				var offset = new Vector3((int)xOffset, yOffset, 0f);
				var transform = Matrix4.CreateTranslation(offset);
				var mvp = Matrix4.Mult (transform, mOrthographicMatrix);
				font.Print(mvp, comment, alignment);
				var bounds = font.SafeMeasure(ClientRectangle, comment, Width-60, alignment);

//				GL.Disable(EnableCap.Texture2D);
//				GL.Begin(BeginMode.Lines);
//					GL.Color4(1.0f, 0f, 0f, 1f); 
//					GL.Vertex2(0f, 0f);
//					GL.Color4(1.0f, 0f, 0f, 1f); 
//					GL.Vertex2(0f, bounds.Height + 20f);
//				GL.End();

				yOffset += bounds.Height;

		//	GL.PopMatrix();
		}

		#endregion
	}
}

