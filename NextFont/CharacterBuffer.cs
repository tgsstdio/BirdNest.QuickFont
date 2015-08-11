using System;

namespace NextFont
{
	public class CharacterBuffer : IDisposable
	{
		const int TRIPLE_BUFFERING = 3;
		const int FLOATS_PER_VERTEX = 5;
		protected float[] VertexBuffer { get; private set;}
		public int MaximumNoOfCharacters {get; private set;}
		public CharacterBuffer (int noOfCharacters)
		{
			this.MaximumNoOfCharacters = noOfCharacters;			
			VertexBuffer = new float[TRIPLE_BUFFERING * FLOATS_PER_VERTEX * noOfCharacters];
		}

		#region IDisposable implementation

		public void Dispose ()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		bool disposed = false;
		protected virtual void Dispose(bool disposing)
		{
			if(disposed) return;
			ReleaseUnmanagedCode ();

			if (disposing){
				ReleaseOtherDisposableObjects();
				VertexBuffer = null;
			}

			disposed = true;
		}

		void ReleaseUnmanagedCode ()
		{

		}

		void ReleaseOtherDisposableObjects ()
		{
	
		}
		#endregion
	}
}

