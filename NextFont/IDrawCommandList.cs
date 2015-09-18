using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;

namespace NextFont
{
	public interface IDrawCommandList
	{
		void RenderChunk (TextureHandle handle, Vector4 fontColor, Matrix4 transform, List<float> vertexChunk, List<uint> indicesChunk);
		void Clear();
	}
}

