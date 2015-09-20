using System;
using OpenTK.Graphics.OpenGL;

namespace NextFont
{
	public class TextVertexBuffer : IDisposable
	{
		public TextVertexBuffer ()
		{
			in_position = new FloatAttributeBinding ("in_position", 0, 3);
			in_texCoords = new FloatAttributeBinding ("in_texCoords", 1, 2);
			in_drawID = new IntAttributeBinding ("in_drawId", 2, 1);
			ArrayId = GL.GenVertexArray();
		}

		public void Bind()
		{
			GL.BindVertexArray (ArrayId);
			GL.BindBuffer (BufferTarget.ElementArrayBuffer, ElementBufferId);
		}

		public void Unbind()
		{
			GL.BindVertexArray (0);
			GL.BindBuffer (BufferTarget.ElementArrayBuffer, 0);
		}

		#region IDisposable implementation

		~ TextVertexBuffer(){
			Dispose(false);
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize(this);
		}

		void ReleaseUnmanagedResources ()
		{
			GL.DeleteVertexArray (ArrayId);
		}

		void ReleaseManagedResources()
		{
			in_position.Dispose ();
			in_texCoords.Dispose ();
			in_drawID.Dispose ();
		}

		private bool mDisposed = false;
		protected virtual void Dispose(bool disposing)
		{
			if (mDisposed)
			{
				return;
			}

			ReleaseUnmanagedResources ();
			if (disposing)
			{
				ReleaseManagedResources ();
			}

			mDisposed = true;
		}

		#endregion

		public int ArrayId { get; private set; }
		public int ElementBufferId { get; private set; }
		public void Initialise(int elementBuffer)
		{
			GL.BindVertexArray (ArrayId);
			in_position.Initialise ();
			in_texCoords.Initialise ();
			in_drawID.Initialise ();
			ElementBufferId = elementBuffer;
			GL.BindBuffer (BufferTarget.ElementArrayBuffer, ElementBufferId);
			GL.BindVertexArray (0);
		}

		private static void BindFloatArrayAttribute (FloatAttributeBinding binding)
		{
			GL.BindBuffer (BufferTarget.ArrayBuffer, binding.Buffer);
			GL.EnableVertexAttribArray (binding.Location);
			GL.VertexAttribPointer (binding.Location, binding.Elements, VertexAttribPointerType.Float, false, binding.Stride, binding.Offset);
			GL.BindBuffer (BufferTarget.ArrayBuffer, 0);
		}

		public void BindManually(int programID)
		{
			GL.BindVertexArray (ArrayId);
			in_position.BindManually (programID);
			in_texCoords.BindManually (programID);
			in_drawID.BindManually (programID);
			GL.BindBuffer (BufferTarget.ArrayBuffer, 0);
		}

		public FloatAttributeBinding in_position { get; private set; }
		public FloatAttributeBinding in_texCoords { get; private set; }
		public IntAttributeBinding in_drawID { get; private set; }
	}
}

