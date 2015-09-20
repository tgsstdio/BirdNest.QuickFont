using System;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;

namespace NextFont
{
	public class SentanceBlockStorageBuffer : IDisposable
	{
		public int BufferId { get; private set; }
		public int Index { get; private set; }

		public SentanceBlockStorageBuffer(SentanceBlock[] blocks, BufferUsageHint hint)
		{
			// Buffer for the linked list.
			BufferId = GL.GenBuffer();
			// manually set
			const int BUFFER_INDEX = 0;
			Index = BUFFER_INDEX;
			GL.BindBuffer(BufferTarget.ShaderStorageBuffer, BufferId);
			GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, Index, BufferId);
			var structSize = Marshal.SizeOf (typeof(SentanceBlock));

			var bufferSize = (IntPtr) (blocks.Length * structSize);
			GL.BufferData<SentanceBlock>(BufferTarget.ShaderStorageBuffer, bufferSize, blocks, hint);
			GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
		}

		public void Bind()
		{
			GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, Index, BufferId);
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

