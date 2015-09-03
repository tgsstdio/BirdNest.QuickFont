using System.Runtime.InteropServices;

namespace NextFont.ConsoleApplication
{
	[StructLayout(LayoutKind.Sequential)]
	public struct BindlessTextureHandle
	{
		public long TextureId {get;set;}
	}
}

