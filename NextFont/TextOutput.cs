using System;
using NextFont.ConsoleApplication;
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
			TextBuffer.Bind ();
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
			TextBuffer.Dispose ();
			DrawCommands = null;
			Storage.Dispose ();
		}

		void ReleaseManagedResources()
		{

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

