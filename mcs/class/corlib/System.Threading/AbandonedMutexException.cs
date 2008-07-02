//
// System.Threading.AbandonedMutexException.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.Runtime.Serialization;
using System.Runtime.InteropServices;

namespace System.Threading
{
	[Serializable]
	[ComVisible (false)]
	public class AbandonedMutexException : SystemException
	{
		Mutex mutex;
		int mutex_index = -1;
		
		public AbandonedMutexException()
			: base ("Mutex was abandoned")
		{
		}

		public AbandonedMutexException (string message)
			: base (message)
		{
		}

		public AbandonedMutexException (int location, WaitHandle handle)
			: base ("Mutex was abandoned")
		{
			mutex_index = location;
			mutex = handle as Mutex;
		}
		

		protected AbandonedMutexException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		public AbandonedMutexException (string message, Exception inner)
			: base (message, inner)
		{
		}

		public AbandonedMutexException (string message, int location, WaitHandle handle)
			: base (message)
		{
			mutex_index = location;
			mutex = handle as Mutex;
		}

		public AbandonedMutexException (string message, Exception inner, int location, WaitHandle handle)
			: base (message, inner)
		{
			mutex_index = location;
			mutex = handle as Mutex;
		}

		public Mutex Mutex
		{
			get {
				return(mutex);
			}
		}

		public int MutexIndex
		{
			get {
				return(mutex_index);
			}
		}
	}
}

#endif
