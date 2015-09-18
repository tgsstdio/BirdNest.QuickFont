using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics;

namespace NextFont
{
	[StructLayout(LayoutKind.Sequential)]
	public struct SentanceBlock
	{
		public TextureHandle Handle {get;set;}
		public Vector4 Colour { get; set; }
		public Matrix4 Transform { get; set; }
	}
}

