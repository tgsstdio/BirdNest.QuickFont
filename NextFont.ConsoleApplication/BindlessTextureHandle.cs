using System.Runtime.InteropServices;

namespace NextFont.ConsoleApplication
{
	[StructLayout(LayoutKind.Sequential)]
	public struct BindlessTextureHandle
	{
		public ulong Handle {get;set;}
		public float Slice {get;set;}
		public uint Index { get; set; }
	}
}

