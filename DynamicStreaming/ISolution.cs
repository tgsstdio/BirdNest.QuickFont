using System;
using OpenTK;

namespace DynamicStreaming
{
	public interface ISolution
	{
		void ShutDown();
		string GetName();
		string GetProblemName();
		bool SupportsApi();
		void SetSize(int width, int height);
	}
}

