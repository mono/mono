//
// System.Threading.LockCookie.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//


namespace System.Threading
{
	[Serializable]
	public struct LockCookie
	{
		internal int ThreadId;
		internal int ReaderLocks;
		internal int WriterLocks;
		
		internal LockCookie (int thread_id)
		{
			ThreadId = thread_id;
			ReaderLocks = 0;
			WriterLocks = 0;
		}
		
		internal LockCookie (int thread_id, int reader_locks, int writer_locks)
		{
			ThreadId = thread_id;
			ReaderLocks = reader_locks;
			WriterLocks = writer_locks;
		}
	}
}

