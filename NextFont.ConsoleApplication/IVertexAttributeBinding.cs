using System;

namespace NextFont.ConsoleApplication
{
	public interface IVertexAttributeBinding
	{
		string Name  {get; }
		int Buffer { get; set; }
		int Location { get; set; }
		int Elements { get; set; }
		int Stride { get; set; }
		IntPtr Offset { get; set; }		

		void Initialise();
		void BindManually(int programID);
	}

}

