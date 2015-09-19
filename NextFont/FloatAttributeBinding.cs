using System;
using OpenTK.Graphics.OpenGL;

namespace NextFont
{
	public class FloatAttributeBinding : IVertexAttributeBinding
	{
		public FloatAttributeBinding (string fieldName, int location, int elements)
		{
			this.Name = fieldName;
			this.Location = location;
			this.Elements = elements;
		}

		public string Name {
			get;
			private set;
		}

		public void Initialise ()
		{
			GL.BindBuffer (BufferTarget.ArrayBuffer, Buffer);
			GL.EnableVertexAttribArray (Location);
			GL.VertexAttribPointer (Location, Elements, VertexAttribPointerType.Float, false, Stride, Offset);
			GL.BindBuffer (BufferTarget.ArrayBuffer, 0);			
		}

		public void BindManually (int programID)
		{
			Location = GL.GetAttribLocation (programID, Name);
			if (Location != -1)
			{
				GL.BindBuffer (BufferTarget.ArrayBuffer, Buffer);
				GL.VertexAttribPointer (Location, Elements, VertexAttribPointerType.Float, false, Stride, Offset);		
				GL.EnableVertexAttribArray (Location);
			}
		}

		public int Buffer { get; set; }
		public int Location { get; set; }
		public int Elements { get; set; }
		public int Stride { get; set; }
		public IntPtr Offset { get; set; }

		#region IDisposable implementation

		~ FloatAttributeBinding(){
			Dispose(false);
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize(this);
		}

		void ReleaseUnmanagedResources ()
		{
			GL.DeleteBuffer (Buffer);
		}

		void ReleaseManagedResources()
		{

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
	}
}

