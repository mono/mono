//
// System.Net.WebAsyncResult
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
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

using System.IO;
using System.Threading;

namespace System.Net
{
	class WebAsyncResult : SimpleAsyncResult
	{
		int nbytes;
		IAsyncResult innerAsyncResult;
		HttpWebResponse response;
		Stream writeStream;
		byte [] buffer;
		int offset;
		int size;
		public bool EndCalled;
		public bool AsyncWriteAll;

		public WebAsyncResult (AsyncCallback cb, object state)
			: base (cb, state)
		{
		}

		public WebAsyncResult (HttpWebRequest request, AsyncCallback cb, object state)
			: base (cb, state)
		{
		}

		public WebAsyncResult (AsyncCallback cb, object state, byte [] buffer, int offset, int size)
			: base (cb, state)
		{
			this.buffer = buffer;
			this.offset = offset;
			this.size = size;
		}

		internal void Reset ()
		{
			this.nbytes = 0;
			this.response = null;
			this.buffer = null;
			this.offset = 0;
			this.size = 0;
			Reset_internal ();
		}

		internal void SetCompleted (bool synch, int nbytes)
		{
			this.nbytes = nbytes;
			SetCompleted_internal (synch);
		}
		
		internal void SetCompleted (bool synch, Stream writeStream)
		{
			this.writeStream = writeStream;
			SetCompleted_internal (synch);
		}
		
		internal void SetCompleted (bool synch, HttpWebResponse response)
		{
			this.response = response;
			SetCompleted_internal (synch);
		}

		internal void DoCallback ()
		{
			DoCallback_internal ();
		}
		
		internal int NBytes {
			get { return nbytes; }
			set { nbytes = value; }
		}

		internal IAsyncResult InnerAsyncResult {
			get { return innerAsyncResult; }
			set { innerAsyncResult = value; }
		}

		internal Stream WriteStream {
			get { return writeStream; }
		}

		internal HttpWebResponse Response {
			get { return response; }
		}

		internal byte [] Buffer {
			get { return buffer; }
		}

		internal int Offset {
			get { return offset; }
		}

		internal int Size {
			get { return size; }
		}
	}
}

