using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace DynamicStreaming
{
	public class BufferRange
	{
		public int mStartOffset {get;set;}
		public int mLength { get; set; }

		public bool Overlaps(BufferRange _rhs) {
			return mStartOffset < (_rhs.mStartOffset + _rhs.mLength)
				&& _rhs.mStartOffset < (mStartOffset + mLength);
		}
	};

	public class BufferLock
	{
		public BufferRange mRange {get;set;}
		public IntPtr mSyncObj {get;set;}
	};

	public class BufferLockManager : IDisposable
	{
		bool disposed = false;

		public BufferLockManager (bool _cpuUpdates)
		{
			mCPUUpdates = _cpuUpdates;
			mBufferLocks = new List<BufferLock> ();
		}

		#region IDisposable implementation

		public void Dispose ()
		{
			Dispose(true);
			GC.SuppressFinalize(this); 
		}

		// Protected implementation of Dispose pattern. 
		protected virtual void Dispose(bool disposing)
		{
			if (disposed)
				return; 

			if (disposing) {
				// Free any other managed objects here. 
				//

				foreach (var it in mBufferLocks)
				{
					cleanup(it);
				}
				mBufferLocks.Clear();
			}

			// Free any unmanaged objects here. 
			//
			disposed = true;
		}

		#endregion


		public void WaitForLockedRange(int _lockBeginBytes, int _lockLength)
		{
			var testRange = new BufferRange{mStartOffset = _lockBeginBytes,mLength = _lockLength };
			var swapLocks = new List<BufferLock>();
			foreach (var it in mBufferLocks)
			{
				if (testRange.Overlaps(it.mRange)) {
					wait(it.mSyncObj);
					cleanup(it);
				} else {
					swapLocks.Add(it);
				}
			}

			mBufferLocks = swapLocks;
		}

		public void LockRange(int _lockBeginBytes, int _lockLength)
		{
			var newRange = new BufferRange{ mStartOffset = _lockBeginBytes, mLength = _lockLength };
			IntPtr syncName = GL.FenceSync(SyncCondition.SyncGpuCommandsComplete, 0);
			var newLock = new BufferLock{mRange= newRange, mSyncObj = syncName };

			mBufferLocks.Add(newLock);
		}

		private void wait(IntPtr _syncObj)
		{
			const ulong kOneSecondInNanoSeconds = 1000000000;

			if  (mCPUUpdates) 
			{
				ClientWaitSyncFlags waitFlags = ClientWaitSyncFlags.None;
				ulong waitDuration = 0;
				while (true) {
					WaitSyncStatus waitRet = GL.ClientWaitSync(_syncObj, waitFlags, waitDuration);
					if (waitRet == WaitSyncStatus.AlreadySignaled || waitRet == WaitSyncStatus.ConditionSatisfied) {
						return;
					}

					if (waitRet == WaitSyncStatus.WaitFailed) {
						//!"Not sure what to do here. Probably raise an exception or something."
						throw new TimeoutException("GL FenceSync failed");
					}

					// After the first time, need to start flushing, and wait for a looong time.
					waitFlags = ClientWaitSyncFlags.SyncFlushCommandsBit;
					waitDuration = kOneSecondInNanoSeconds;
				}
			} else {
				GL.WaitSync(_syncObj, WaitSyncFlags.None, (long) All.TimeoutIgnored);
			}
		}

		private void cleanup(BufferLock _bufferLock)
		{
			GL.DeleteSync(_bufferLock.mSyncObj);
		}

		private List<BufferLock> mBufferLocks;

		// Whether it's the CPU (true) that updates, or the GPU (false)
		private bool mCPUUpdates;
	}
}

