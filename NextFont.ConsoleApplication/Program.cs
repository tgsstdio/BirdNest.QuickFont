using System;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;

namespace NextFont.ConsoleApplication
{
	class MainClass
	{
		[STAThread]
		public static void Main (string[] args)
		{
			using (var game = new GameWindow (800, 600, GraphicsMode.Default, "OpenTK Quick Start Sample"))
			{
				NxFont heading1 = null;

				game.Load += (sender, e) =>
				{
					// setup settings, load textures, sounds
					game.VSync = VSyncMode.On;

					var buildConfig = new NxFontBuilderConfiguration();

					buildConfig.Transform = Matrix4.CreateOrthographicOffCenter(game.ClientRectangle.X, game.Width, game.Height, game.ClientRectangle.Y, -1, 1);
					heading1 = new NxFont("Fonts/HappySans.ttf", 72, game.Height, buildConfig);

				};

				game.Unload += (sender, e) => 
				{

				};

				game.KeyDown += (sender, e) => {
					if (e.Key == Key.Space)
					{
						game.Exit ();
					}
				};

				game.UpdateFrame += (sender, e) =>
				{
					// add game logic, input handling

					// update shader uniforms

					// update shader mesh
				};

				game.RenderFrame += (sender, e) =>
				{
					game.SwapBuffers();
				};

				game.Resize += (sender, e) =>
				{
					//GL.Viewport(0, 0, game.Width, game.Height);
				};

				game.Run(60.0);
			}
		}
	}
}
