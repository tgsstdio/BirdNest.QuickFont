using System.Collections.Generic;
using OpenTK;

namespace NextFont
{
	public class DrawCommandList : IDrawCommandList
	{
		private List<DrawElementsIndirectCommand> mDrawCommands;
		private readonly List<SentanceBlock> mBlocks;
		private readonly List<float> mVertexData;
		private readonly List<uint> mIndices;

		public DrawCommandList ()
		{
			mDrawCommands = new List<DrawElementsIndirectCommand> ();
			mBlocks = new List<SentanceBlock> ();
			mVertexData = new List<float> ();
			mIndices = new List<uint> ();
		}

		public void Clear()
		{
			mDrawCommands.Clear ();
			mBlocks.Clear ();
			mVertexData.Clear ();
			mIndices.Clear ();
		}

		void AddDrawCommand (IList<uint> indicesChunk, uint firstIndex, uint baseVertex, uint currentBlock)
		{
			var command = new DrawElementsIndirectCommand ();
			command.Count = (uint)indicesChunk.Count;
			command.InstanceCount = 1;
			command.FirstIndex = firstIndex;
			command.BaseVertex = baseVertex;
			// IMPORTANT - controls material index
			command.BaseInstance = currentBlock;
			mDrawCommands.Add (command);
		}

		void AddSentanceBlock (TextureHandle handle, Vector4 fontColor, Matrix4 transform)
		{
			var block = new SentanceBlock {
				Colour = fontColor,
				Transform = transform,
				Handle = handle,
			};
			mBlocks.Add (block);
		}

		void AddVertices (List<float> vertexChunk)
		{
			mVertexData.AddRange (vertexChunk);
		}

		void AddIndices (List<uint> indicesChunk)
		{
			mIndices.AddRange (indicesChunk);
		}

		public void RenderChunk(TextureHandle handle, Vector4 fontColor, Matrix4 transform, List<float> vertexChunk, List<uint> indicesChunk)
		{
			uint firstIndex = (uint)mIndices.Count;
			uint baseVertex = (uint)mVertexData.Count;
			uint currentBlock = (uint)mBlocks.Count;

			AddVertices (vertexChunk);
			AddIndices (indicesChunk);

			AddSentanceBlock (handle, fontColor, transform);
			AddDrawCommand (indicesChunk, firstIndex, baseVertex, currentBlock);
		}
	}
}

