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
		internal bool IsReader;
		internal bool IsWriter;
		
		internal LockCookie (int thread_id, bool is_reader, bool is_writer)
		{
			ThreadId = thread_id;
			IsReader = is_reader;
			IsWriter = is_writer;
		}
	}
}

