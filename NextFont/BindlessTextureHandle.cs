using System.Runtime.InteropServices;

namespace NextFont
{
	[StructLayout(LayoutKind.Sequential)]	
	public struct BindlessTextureHandle
	{
		public long TextureId {get;set;}
	}
}

