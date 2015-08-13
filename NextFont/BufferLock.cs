using System;
using OpenTK.Graphics.OpenGL;

namespace NextFont
{
	public class BufferLock : IDisposable
	{
		private IntPtr mSyncObj;
		private bool mCPUUpdates;
		public BufferLock (bool isCPUUpdates)
		{
			mCPUUpdates = isCPUUpdates;
			mSyncObj = GL.FenceSync(SyncCondition.SyncGpuCommandsComplete, 0);
		}

		public void Wait()
		{
			const ulong ONE_SECOND_IN_NANOSECS = 1000000000;

			if  (mCPUUpdates) 
			{
				ClientWaitSyncFlags waitFlags = ClientWaitSyncFlags.None;
				ulong waitDuration = 0;
				while (true) {
					WaitSyncStatus waitRet = GL.ClientWaitSync(mSyncObj, waitFlags, waitDuration);
					if (waitRet == WaitSyncStatus.AlreadySignaled || waitRet == WaitSyncStatus.ConditionSatisfied) {
						return;
					}

					if (waitRet == WaitSyncStatus.WaitFailed) {
						//!"Not sure what to do here. Probably raise an exception or something."
						throw new TimeoutException("GL FenceSync failed");
					}

					// After the first time, need to start flushing, and wait for a looong time.
					waitFlags = ClientWaitSyncFlags.SyncFlushCommandsBit;
					waitDuration = ONE_SECOND_IN_NANOSECS;
				}
			} else {
				GL.WaitSync(mSyncObj, WaitSyncFlags.None, (long) All.TimeoutIgnored);
			}
		}

		#region IDisposable implementation

		public void Dispose ()
		{
			Dispose(true);
			GC.SuppressFinalize(this); 
		}

		private void ReleaseUnmanagedResources()
		{
			GL.DeleteSync(mSyncObj);
		}

		// Protected implementation of Dispose pattern. 
		private bool disposed = false;
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

		}

		#endregion
	};
}

