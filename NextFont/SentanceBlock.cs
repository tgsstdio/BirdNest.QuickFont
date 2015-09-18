using System.Runtime.InteropServices;
using OpenTK;

namespace NextFont
{
	[StructLayout(LayoutKind.Sequential)]
	public struct SentanceBlock
	{
		public TextureHandle Texture {get;set;}
		public Vector4 Color { get; set; }
		public Matrix4 Transform { get; set; }
	}
}

