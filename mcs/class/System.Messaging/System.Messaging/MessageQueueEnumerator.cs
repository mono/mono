//
// System.Messaging
//
// Authors:
//      Peter Van Isacker (sclytrack@planetinternet.be)
//      Rafael Teixeira   (rafaelteixeirabr@hotmail.com)
//
// (C) 2003 Peter Van Isacker
//

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
using System;
using System.Collections;

namespace System.Messaging
{
	public class MessageQueueEnumerator : MarshalByRefObject, IEnumerator, IDisposable
	{
		private bool disposed;
		private ArrayList queueList;
		private int currentIndex;

		internal MessageQueueEnumerator (ArrayList queueList)
		{
			this.queueList = queueList;
			this.currentIndex = -1;
		}

		public MessageQueue Current
		{
			get
			{
				if (currentIndex < 0 || currentIndex >= queueList.Count)
					return null;
				return (MessageQueue) queueList[currentIndex];
			}
		}

		object IEnumerator.Current
		{
			get
			{
				if (currentIndex < 0 || currentIndex >= queueList.Count)
					return null;
				return queueList[currentIndex];
			}
		}

		public IntPtr LocatorHandle
		{
			[MonoTODO]
			get
			{
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public void Close ()
		{
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			Close ();
			disposed = true;
		}

		public bool MoveNext ()
		{
			return (++currentIndex) < queueList.Count;
		}

		public void Reset ()
		{
			currentIndex = -1;
		}

		~MessageQueueEnumerator ()
		{
			if (!disposed)
				Dispose (false);
		}
	}
}
