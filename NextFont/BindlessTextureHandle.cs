using System.Runtime.InteropServices;

namespace NextFont
{
	[StructLayout(LayoutKind.Sequential)]	
	public struct TextureHandle
	{
		public long TextureId {get;set;}
		public uint Index {get;set;}
		public float Slice {get;set;}
	}
}

