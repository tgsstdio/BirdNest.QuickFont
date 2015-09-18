using System.Runtime.InteropServices;

namespace NextFont
{
	[StructLayout(LayoutKind.Sequential)]	
	public struct TextureHandle
	{
		public long Texture	{get;set;}
		public uint Index {get;set;}
		public float Slice {get;set;}
	}
}

