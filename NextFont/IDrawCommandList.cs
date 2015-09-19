using System.Collections.Generic;
using OpenTK;

namespace NextFont
{
	public interface IDrawCommandList
	{
		void RenderChunk (TextureHandle handle, Vector4 fontColor, Matrix4 transform, uint noOfVertices, List<float> vertexChunk, List<uint> indicesChunk);
		void Clear();
		TextVertexBuffer AsStaticText();
		DrawElementsIndirectCommand[] GetCommands();
	}
}

