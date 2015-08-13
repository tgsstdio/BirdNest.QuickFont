using System.Runtime.InteropServices;
using OpenTK;

namespace NextFont
{
	[StructLayout(LayoutKind.Sequential)]
	public struct SentenceBlock
	{
		public BindlessTextureHandle Handle { get; set; }
		public Matrix4 Transform {get;set;}
		public Vector4 Color {get;set;}
	}
}

