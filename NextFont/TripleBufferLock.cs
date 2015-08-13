using System;

namespace NextFont
{
	public class TripleBufferLock : IDisposable
	{
		const int NO_OF_COPIES = 3;
		private readonly BufferLock[] mBufferLocks;
		private uint mNextBlockIndex;
		// Whether it's the CPU (true) that updates, or the GPU (false)
		private readonly bool mCPUUpdates;
		public TripleBufferLock (bool cpuUpdates)
		{
			mCPUUpdates = cpuUpdates;
			mBufferLocks = new BufferLock[NO_OF_COPIES];
			mNextBlockIndex = 0;
		}

		#region IDisposable implementation

		public void Dispose ()
		{
			Dispose(true);
			GC.SuppressFinalize(this); 
		}

		private void ReleaseUnmanagedResources()
		{

		}

		// Protected implementation of Dispose pattern. 
		bool disposed = false;
		protected virtual void Dispose(bool disposing)
		{
			if (disposed)
				return; 

			ReleaseUnmanagedResources ();

			if (disposing) {
				ReleaseOtherDisposableObjects();
			}
			disposed = true;
		}

		private void ReleaseOtherDisposableObjects()
		{
			for (int i = 0; i < NO_OF_COPIES; ++i)
			{
				if (mBufferLocks [i] != null)
				{
					mBufferLocks [i].Dispose ();
					mBufferLocks [i] = null;
				}
			}
		}

		#endregion

		public void WaitForLockedRange(uint index)
		{
			var currentLock = mBufferLocks [index];
			currentLock.Wait ();
			currentLock.Dispose ();
			mBufferLocks [index] = null;
		}

		public uint Lock()
		{
			uint current = mNextBlockIndex;
			mBufferLocks [current] = new BufferLock (mCPUUpdates);
			mNextBlockIndex = (mNextBlockIndex + 1) % NO_OF_COPIES;
			return current;
		}
	}
}

