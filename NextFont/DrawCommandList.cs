using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;

namespace NextFont
{
	public class DrawCommandList : IDrawCommandList
	{
		private readonly List<DrawElementsIndirectCommand> mDrawCommands;
		public List<SentanceBlock> Blocks { get; private set; }
		private readonly List<float> mVertexData;
		private readonly List<uint> mIndices;
		private readonly List<uint> mDrawIDs;

		private uint mTotalVertices;
		public DrawCommandList ()
		{
			mDrawCommands = new List<DrawElementsIndirectCommand> ();
			Blocks = new List<SentanceBlock> ();
			mVertexData = new List<float> ();
			mIndices = new List<uint> ();
			mDrawIDs = new List<uint> ();
		}

		public void Clear()
		{
			mDrawCommands.Clear ();
			mVertexData.Clear ();
			mIndices.Clear ();
			mDrawIDs.Clear ();
			mTotalVertices = 0;
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
			Blocks.Add (block);
		}

		void AddVertices (List<float> vertexChunk)
		{
			mVertexData.AddRange (vertexChunk);
		}

		void AddIndices (List<uint> indicesChunk)
		{
			mIndices.AddRange (indicesChunk);
		}

		void AddDrawIDs (uint currentBlock, int triCount)
		{
			mDrawIDs.Add (currentBlock);
		}

		public void RenderChunk(TextureHandle handle, Vector4 fontColor, Matrix4 transform, uint noOfVertices, List<float> vertexChunk, List<uint> indicesChunk)
		{
			uint firstIndex = (uint)mIndices.Count;
			uint baseVertex = mTotalVertices;
			uint currentBlock = (uint)Blocks.Count;

			AddVertices (vertexChunk);
			AddIndices (indicesChunk);

			AddDrawIDs (currentBlock, indicesChunk.Count / 3);
			AddSentanceBlock (handle, fontColor, transform);
			AddDrawCommand (indicesChunk, firstIndex, baseVertex, currentBlock);
			mTotalVertices += noOfVertices;
		}

		public TextVertexBuffer AsStaticText ()
		{
			int vertexBuffer = GL.GenBuffer ();
			GL.BindBuffer (BufferTarget.ArrayBuffer, vertexBuffer);
			var vertexData = mVertexData.ToArray ();
			GL.BufferData<float> (BufferTarget.ArrayBuffer, (IntPtr)(sizeof(float) * vertexData.Length), vertexData, BufferUsageHint.StaticDraw);
			GL.BindBuffer (BufferTarget.ArrayBuffer, 0);

			int drawIDBuffer = GL.GenBuffer ();
			GL.BindBuffer (BufferTarget.ArrayBuffer, drawIDBuffer);
			var drawIDData = mDrawIDs.ToArray ();
			GL.BufferData<uint> (BufferTarget.ArrayBuffer, (IntPtr)(sizeof(uint) * drawIDData.Length), drawIDData, BufferUsageHint.StaticDraw);
			GL.BindBuffer (BufferTarget.ArrayBuffer, 0);

			int elementBuffer = GL.GenBuffer ();
			GL.BindBuffer (BufferTarget.ElementArrayBuffer, elementBuffer);
			var elementData = mIndices.ToArray ();
			GL.BufferData<uint> (BufferTarget.ElementArrayBuffer, (IntPtr)(sizeof(uint) * elementData.Length), elementData, BufferUsageHint.StaticDraw);
			GL.BindBuffer (BufferTarget.ElementArrayBuffer, 0);

			const int POSITION = 0;
			const int IN_TEXTURE = 1;
			const int DRAW_ID = 2;

			int offset = 0;
			/// VERTEX
			//vbo = GL.GenVertexArray();
			//GL.BindVertexArray (vbo);
			//CheckGLError ();

			int elementCount = 3;
			int size = elementCount * sizeof(float);
			int location = POSITION;

			var vbo = new TextVertexBuffer ();
			vbo.in_position.Buffer = vertexBuffer;
			vbo.in_position.Location = location;
			vbo.in_position.Elements =	elementCount;
			vbo.in_position.Offset = (IntPtr)offset;

			offset += size;
			elementCount = 2;
			size = elementCount * sizeof(float);
			location = IN_TEXTURE;

			vbo.in_texCoords.Buffer = vertexBuffer;
			vbo.in_texCoords.Location = location;
			vbo.in_texCoords.Elements =	elementCount;
			vbo.in_texCoords.Offset = (IntPtr)offset;

			// SHARED BUFFER AT END
			offset += size;
			int stride = offset;
			vbo.in_position.Stride = stride;
			vbo.in_texCoords.Stride = stride;

			offset = 0;
			stride = sizeof(uint);
			elementCount = 1;
			size = elementCount * sizeof(uint);
			location = DRAW_ID;

			vbo.in_drawID.Buffer = drawIDBuffer;
			vbo.in_drawID.Location = location;
			vbo.in_drawID.Elements = elementCount;
			vbo.in_drawID.Stride = stride;
			vbo.in_drawID.Offset = (IntPtr)offset;
			vbo.in_drawID.Divisor = 3;

			vbo.Initialise (elementBuffer);

			return vbo;
		}

		public DrawElementsIndirectCommand[] GetCommands ()
		{
			return mDrawCommands.ToArray ();
		}
	}
}

