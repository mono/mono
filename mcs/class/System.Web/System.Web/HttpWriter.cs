// 
// System.Web.HttpWriter
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
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
using System.IO;
using System.Text;
using System.Web.Util;

namespace System.Web
{
	public sealed class HttpWriter : TextWriter
	{
		internal const int MaxBufferSize = 32768;
		
		HttpResponse _Response;

		HttpResponseStream _ResponseStream;

		MemoryStream _OutputStream;
		StreamWriter _OutputHelper;
		Encoding _Encoding;	

		Stream _OutputFilter;
		HttpResponseStreamProxy _OutputProxy;

		internal HttpWriter (HttpResponse Response)
		{
			_Response = Response;

			_Encoding = _Response.ContentEncoding;
			_OutputStream = new MemoryStream (MaxBufferSize);
			_OutputHelper = new StreamWriter (_OutputStream, _Response.ContentEncoding);
			_ResponseStream = new HttpResponseStream (this);
		}  

		internal void Dispose ()
		{
			_OutputHelper.Close ();
			_OutputStream.Close ();
			_OutputFilter.Close ();
		}

		internal Stream GetActiveFilter ()
		{
			if (null == _OutputFilter) {
				// Create a filter proxy to allow us to know if we have a valid filter
				if (null == _OutputProxy)
					_OutputProxy = new HttpResponseStreamProxy (this);

				return _OutputProxy;
			}
			return _OutputFilter;
		}

		internal void ActivateFilter (Stream OutputFilter)
		{
			if (null == _OutputProxy)
				throw new HttpException ("Invalid filter usage");

			_OutputFilter = OutputFilter;
		}

		internal void FilterData (bool CloseStream)
		{
			// Check if we have any filter at all
			if (null == _OutputFilter)
				return;

			FlushBuffers ();

			// Save our current data
			byte [] arrData = _OutputStream.GetBuffer ();
			int size = (int) _OutputStream.Length;

			// Remove our internal data
			Clear ();

			// If we have a filter then we have a proxy
			_OutputProxy.Active = true;

			try {
				// Call the filter (it does a callback into our HttpWriter again)
				_OutputFilter.Write (arrData, 0, size);
				_OutputFilter.Flush ();

				if (CloseStream)
					_OutputFilter.Close ();
			} finally {
				_OutputProxy.Active = false;
			}
		}

		internal void Clear ()
		{
			// This clears all the buffers that are around
			_OutputHelper.Flush ();
			_OutputStream.Position = 0;
		}

		internal void SendContent (HttpWorkerRequest Handler)
		{
			FlushBuffers();

			int l = (int)_OutputStream.Length;
			if (l > 0) {
				byte [] arrContent = _OutputStream.GetBuffer ();
				Handler.SendResponseFromMemory (arrContent, l);
			}
		}

		internal void Update ()
		{
			_Encoding = _Response.ContentEncoding;
			_OutputHelper.Flush ();
			_OutputHelper = new StreamWriter (_OutputStream, _Encoding);
		}

		internal long BufferSize
		{
			get {
				FlushBuffers ();
				return _OutputStream.Length;
			}
		}

		internal void FlushBuffers ()
		{
			_OutputHelper.Flush ();
			_OutputStream.Flush ();
		}

		internal byte[] GetBuffer () {
			return _OutputStream.GetBuffer ();
		}
		
		public override Encoding Encoding
		{
			get { return _Encoding; }
		}

		public Stream OutputStream
		{
			get { return _ResponseStream; }
		}

		public override void Close ()
		{
			_Response.Flush ();
			_Response.Close ();
		}

		public override void Flush ()
		{
		}

		private void CheckIfFlush ()
		{
			if (!_Response.BufferOutput) {
				FlushBuffers ();
			}
		}

		public override void Write (char ch)
		{
			_OutputHelper.Write (ch);
			CheckIfFlush ();
		}

		public override void Write (object obj)
		{
			if (obj != null)
			{
				_OutputHelper.Write (obj.ToString ());
				CheckIfFlush ();
			}
		}

		public override void Write (string s)
		{
			_OutputHelper.Write (s);
			CheckIfFlush ();
		}

		public override void Write (char [] buffer, int index, int count)
		{
			_OutputHelper.Write (buffer, index, count);
			CheckIfFlush ();
		}

		public void WriteBytes (byte [] buffer, int index, int count)
		{
			_OutputStream.Write (buffer, index, count);
			CheckIfFlush ();
		}

		public override void WriteLine ()
		{
			_OutputHelper.Write ("\r\n");
			CheckIfFlush ();
		}

		public void WriteString (string s, int index, int count)
		{
			_OutputHelper.Write (s.Substring (index, count));
			CheckIfFlush ();
		}
	}
}

