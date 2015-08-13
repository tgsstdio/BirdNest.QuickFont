using System;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using QuickFont;
using System.Drawing;

namespace NextFont.ConsoleApplication
{
	class MainClass
	{
		[STAThread]
		public static void Main (string[] args)
		{
			using (var game = new Example())
			{
				game.Run(30.0);
			}
		}
	}
}
