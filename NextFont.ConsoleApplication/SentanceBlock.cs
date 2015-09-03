using System.Runtime.InteropServices;
using OpenTK;

namespace NextFont.ConsoleApplication
{
	[StructLayout(LayoutKind.Sequential)]
	public struct SentanceBlock
	{
		public Vector4 Color { get; set; }
		public Matrix4 Transform { get; set; }
	}
}

