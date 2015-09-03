using System;
using OpenTK.Graphics.OpenGL;

namespace NextFont.ConsoleApplication
{
	public class IntAttributeBinding : IVertexAttributeBinding
	{
		public IntAttributeBinding (string fieldName, int location, int elements)
		{
			this.Name = fieldName;
			this.Location = location;
			this.Elements = elements;
		}

		#region IVertexAttributeBinding implementation

		public string Name {
			get;
			private set;
		}

		public void BindManually (int programID)
		{
			Location = GL.GetAttribLocation (programID, Name);
			if (Location != -1)
			{
				GL.BindBuffer (BufferTarget.ArrayBuffer, Buffer);
				GL.VertexAttribIPointer (Location, Elements, VertexAttribIntegerType.UnsignedInt, Stride, Offset);		
				if (Divisor.HasValue)
				{
					GL.VertexAttribDivisor (Location, Divisor.Value);
				}
				GL.EnableVertexAttribArray (Location);
			}
		}

		public void Initialise ()
		{
			GL.BindBuffer (BufferTarget.ArrayBuffer, Buffer);
			GL.EnableVertexAttribArray (Location);
			GL.VertexAttribIPointer (Location, Elements, VertexAttribIntegerType.UnsignedInt, Stride, Offset);
			if (Divisor.HasValue)
			{
				GL.VertexAttribDivisor (Location, Divisor.Value);
			}
			GL.BindBuffer (BufferTarget.ArrayBuffer, 0);
		}

		public int Buffer {
			get;
			set;
		}

		public int Location {
			get;
			set;
		}

		public int Elements {
			get;
			set;
		}

		public int Stride {
			get;
			set;
		}

		public IntPtr Offset {
			get;
			set;
		}

		#endregion

		public int? Divisor {
			get;
			set;
		}
	}
}

