// 
// System.IO.MonoSyncFileStream.cs: Synchronous FileStream with
//     asynchronous BeginRead/Write methods.
//
// Authors:
//     Robert Jordan (robertj@gmx.net)
//
// Copyright (C) 2007 Novell, Inc. (http://www.novell.com)
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
using System.Runtime.Remoting.Messaging;

namespace System.IO
{
	internal class MonoSyncFileStream : FileStream
	{
		public MonoSyncFileStream (IntPtr handle, FileAccess access, bool ownsHandle, int bufferSize)
			: base (handle, access, ownsHandle, bufferSize, false)
		{
		}

		delegate void WriteDelegate (byte [] buffer, int offset, int count);

		public override IAsyncResult BeginWrite (byte [] buffer, int offset, int count,
							AsyncCallback cback, object state)
		{
			if (!CanWrite)
				throw new NotSupportedException ("This stream does not support writing");

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (count < 0)
				throw new ArgumentOutOfRangeException ("count", "Must be >= 0");

			if (offset < 0)
				throw new ArgumentOutOfRangeException ("offset", "Must be >= 0");

			WriteDelegate d = new WriteDelegate (this.Write);
			return d.BeginInvoke (buffer, offset, count, cback, state);
		}

		public override void EndWrite (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");

			AsyncResult ar = asyncResult as AsyncResult;
			if (ar == null)
				throw new ArgumentException ("Invalid IAsyncResult", "asyncResult");

			WriteDelegate d = ar.AsyncDelegate as WriteDelegate;
			if (d == null)
				throw new ArgumentException ("Invalid IAsyncResult", "asyncResult");

			d.EndInvoke (asyncResult);
		}

		delegate int ReadDelegate (byte [] buffer, int offset, int count);

		public override IAsyncResult BeginRead (byte [] buffer, int offset, int count,
							AsyncCallback cback, object state)
		{
			if (!CanRead)
				throw new NotSupportedException ("This stream does not support reading");

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (count < 0)
				throw new ArgumentOutOfRangeException ("count", "Must be >= 0");

			if (offset < 0)
				throw new ArgumentOutOfRangeException ("offset", "Must be >= 0");

			ReadDelegate d = new ReadDelegate (this.Read);
			return d.BeginInvoke (buffer, offset, count, cback, state);
		}

		public override int EndRead (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");

			AsyncResult ar = asyncResult as AsyncResult;
			if (ar == null)
				throw new ArgumentException ("Invalid IAsyncResult", "asyncResult");

			ReadDelegate d = ar.AsyncDelegate as ReadDelegate;
			if (d == null)
				throw new ArgumentException ("Invalid IAsyncResult", "asyncResult");

			return d.EndInvoke (asyncResult);
		}

	}
}
