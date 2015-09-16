using System.Runtime.InteropServices;

namespace NextFont.ConsoleApplication
{
	[StructLayout(LayoutKind.Sequential)]
	public struct DrawElementsIndirectCommand
	{
		public uint Count {get;set;}
		public uint InstanceCount {get;set;}
		public uint FirstIndex {get;set;}
		public uint BaseVertex {get;set;}
		public uint BaseInstance {get;set;}
	};
}

