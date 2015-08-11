using System;
using OpenTK;

namespace DynamicStreaming
{
	public interface IProblem
	{
		void Init();
		void Render();
		void Shutdown();
		string GetName();
		bool SetSolution(ISolution solution);
		ISolution mActiveSolution {get;set;}
		void GetClearValues(out Vector4 outCol, out float outDepth);
	}
}

