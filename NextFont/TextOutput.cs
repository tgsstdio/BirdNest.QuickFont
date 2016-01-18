using System;
using OpenTK.Graphics.OpenGL;

namespace NextFont
{
	public class TextOutput : IDisposable
	{
		private TextVertexBuffer TextBuffer;
		private DrawElementsIndirectCommand[] DrawCommands;
		private SentanceBlockStorageBuffer Storage;
		private int Stride;

		public TextOutput (
			TextVertexBuffer textBuffer,
			DrawElementsIndirectCommand[] drawCommands,
			SentanceBlockStorageBuffer storage)
		{
			TextBuffer = textBuffer;
			DrawCommands = drawCommands;
			Storage = storage;
			Stride = System.Runtime.InteropServices.Marshal.SizeOf (typeof(DrawElementsIndirectCommand));
		}

		public void Bind()
		{
			Storage.Bind ();
			TextBuffer.Bind ();
		}

		public void BindManually(int programID)
		{
			TextBuffer.BindManually (programID);
		}

		public void Render()
		{
			GL.MultiDrawElementsIndirect<DrawElementsIndirectCommand> (
				All.Triangles,
				All.UnsignedInt,
				DrawCommands,
				DrawCommands.Length,
				Stride);
		}

		public void Unbind()
		{
			Storage.Unbind ();
			TextBuffer.Unbind ();
		}

		#region IDisposable implementation
		~TextOutput()
		{
			Dispose (false);
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		void ReleaseUnmanagedResources()
		{

		}

		void ReleaseManagedResources()
		{
			TextBuffer = null;
			DrawCommands = null;
			Storage = null;
		}

		private bool mDisposed = false;
		protected virtual void Dispose(bool disposing)
		{
			if (mDisposed)
				return;

			ReleaseUnmanagedResources ();

			if (disposing)
			{
				ReleaseManagedResources ();
			}
			mDisposed = true;
		}

		#endregion
	}
}

