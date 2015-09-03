using System;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;

namespace NextFont.ConsoleApplication
{
	public class SentanceBlockStorageBuffer : IDisposable
	{
		public int BufferId { get; private set; }

		public SentanceBlockStorageBuffer(SentanceBlock[] blocks, BufferUsageHint hint)
		{
			// Buffer for the linked list.
			BufferId = GL.GenBuffer();
			// manually set
			const int BUFFER_INDEX = 0;
			GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, BUFFER_INDEX, BufferId);

			var bufferSize = (IntPtr) (blocks.Length * Marshal.SizeOf (typeof(SentanceBlock)));
			GL.BufferData<SentanceBlock>(BufferTarget.ShaderStorageBuffer, bufferSize, blocks, hint);
		}

		public void Bind()
		{
			GL.BindBuffer(BufferTarget.ShaderStorageBuffer, BufferId);
		}

		public void Unbind()
		{
			GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
		}

		#region IDisposable implementation
		~SentanceBlockStorageBuffer()
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
			GL.DeleteBuffer (BufferId);
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

