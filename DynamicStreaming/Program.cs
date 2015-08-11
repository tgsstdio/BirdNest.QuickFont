using System;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;

namespace DynamicStreaming
{
	class MainClass
	{
		[STAThread]
		public static void Main (string[] args)
		{
			Console.WriteLine ("Hello World!");

			using (var game = new GameWindow ())
			{
				const int kVertsPerParticle = 6;
				const int kParticleCountX = 500;
				const int kParticleCountY = 320;

				var solution = new MapPersistent (kVertsPerParticle, game.Width, game.Height);
				var problem = new DynamicStreamingProblem (solution, kVertsPerParticle, kParticleCountX, kParticleCountY);

				game.Load += (sender, e) =>
				{
					// setup settings, load textures, sounds
					game.VSync = VSyncMode.On;

					problem.Init();
				};

				game.Unload += (sender, e) => 
				{
					problem.Shutdown();
				};

				game.KeyDown += (object sender, KeyboardKeyEventArgs e) => 
				{
					if (e.Key == Key.Space)
					{
						game.Exit();
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
					solution.SetSize(game.Width, game.Height);
				};

				game.Run(60.0);
			}
		}
	}
}
