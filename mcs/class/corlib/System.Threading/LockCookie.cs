//
// System.Threading.LockCookie.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if NET_2_0
using System.Runtime.InteropServices;
#endif

namespace System.Threading
{
#if NET_2_0
	[ComVisible (true)]
#else
	[Serializable]
#endif
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

#if NET_2_0
		public override int GetHashCode ()
		{
			return(base.GetHashCode ());
		}

		public bool Equals (LockCookie obj)
		{
			if (this.ThreadId == obj.ThreadId &&
			    this.ReaderLocks == obj.ReaderLocks &&
			    this.WriterLocks == obj.WriterLocks) {
				return(true);
			} else {
				return(false);
			}
		}
		
		public override bool Equals (Object obj)
		{
			if (!(obj is LockCookie)) {
				return(false);
			}
			
			return(obj.Equals (this));
		}

		public static bool operator == (LockCookie a, LockCookie b)
		{
			return a.Equals (b);
		}

		public static bool operator != (LockCookie a, LockCookie b)
		{
			return !a.Equals (b);
		}
#endif

	}
}

