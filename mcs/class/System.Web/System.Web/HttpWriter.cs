// 
// System.Web.HttpWriter
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//
using System;
using System.IO;
using System.Text;

namespace System.Web
{
	public sealed class HttpWriter : TextWriter
	{
		HttpResponse _Response;

		HttpResponseStream _ResponseStream;

		MemoryStream _OutputStream;
		StreamWriter _OutputHelper;
		Encoder _Encoder;
		Encoding _Encoding;	

		Stream _OutputFilter;
		HttpResponseStreamProxy _OutputProxy;

		internal HttpWriter (HttpResponse Response)
		{
			_Response = Response;

			_OutputStream = new MemoryStream (32768);
			_OutputHelper = new StreamWriter (_OutputStream, Encoding.Unicode);
			_ResponseStream = new HttpResponseStream (this);

			Update ();
		}  

		internal void Dispose ()
		{
			_OutputHelper.Close ();
			_OutputStream.Close ();
			_OutputFilter.Close ();
		}

		internal Stream GetActiveFilter()
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
			byte [] arrData = _OutputStream.ToArray ();

			// Remove our internal data
			Clear ();

			// If we have a filter then we have a proxy
			_OutputProxy.Active = true;

			try {
				// Call the filter (it does a callback into our HttpWriter again)
				_OutputFilter.Write (arrData, 0, arrData.Length);

				if (CloseStream)
					_OutputFilter.Close ();
			} finally {
				_OutputProxy.Active = false;
			}
		}

		internal void Clear ()
		{
			_OutputHelper.Close ();
			_OutputStream.Close ();
					
			// Quick way of doing cleanup
			_OutputStream = new MemoryStream (32768);
			_OutputHelper = new StreamWriter (_OutputStream, Encoding.Unicode);
		}

		internal byte [] ToArray ()
		{
			if (_OutputStream.Length == 0)
				return null;

			// TODO: Optimize
			byte [] arrStream = _OutputStream.ToArray ();
			byte [] arrData = new byte [arrStream.Length - 2];

			Buffer.BlockCopy (arrStream, 2, arrData, 0, arrStream.Length - 2);

			byte [] arrContent = _Encoding.GetBytes (Encoding.Unicode.GetString (arrData));

			return arrContent;
		}

		internal void SendContent (HttpWorkerRequest Handler)
		{
			FlushBuffers();

			if (_OutputStream.Length > 0) {
				byte [] arrContent = ToArray ();
				Handler.SendResponseFromMemory (arrContent, arrContent.Length);
			}
		}

		internal void Update ()
		{
			_Encoder = _Response.ContentEncoder;
			_Encoding = _Response.ContentEncoding;
		}

		internal long BufferSize
		{
			get {
				FlushBuffers ();

				if (_OutputStream.Length == 0)
					return 0;

				return ToArray ().Length;
			}
		}

		internal void FlushBuffers ()
		{
			_OutputHelper.Flush ();
			_OutputStream.Flush ();
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
			FlushBuffers ();
			_Response.Flush ();
			_Response.Close ();
		}

		public override void Flush ()
		{
			FlushBuffers ();
			_Response.Flush ();
		}

		public override void Write (char ch)
		{
			_OutputHelper.Write (ch);
			if (!_Response.BufferOutput) {
				FlushBuffers ();
				_Response.Flush ();
			}
		}

		public override void Write (object obj)
		{
			_OutputHelper.Write (obj.ToString ());
			if (!_Response.BufferOutput) {
				FlushBuffers ();
				_Response.Flush ();
			}
		}

		public override void Write (string s)
		{
			_OutputHelper.Write (s);
			if (!_Response.BufferOutput) {
				FlushBuffers ();
				_Response.Flush ();
			}
		}

		public override void Write (char [] buffer, int index, int count)
		{
			_OutputHelper.Write (buffer, index, count);
			if (!_Response.BufferOutput) {
				FlushBuffers ();
				_Response.Flush ();
			}
		}

		public void WriteBytes (byte[] buffer, int index, int count)
		{
			_OutputStream.Write (buffer, index, count);
			if (!_Response.BufferOutput) {
				FlushBuffers ();
				_Response.Flush ();
			}
		}

		public override void WriteLine ()
		{
			_OutputHelper.Write ("\r\n");
			if (!_Response.BufferOutput) {
				FlushBuffers ();
				_Response.Flush ();
			}
		}

		public void WriteString (string s, int index, int count)
		{
			_OutputHelper.Write (s.Substring (index, count));
			if (!_Response.BufferOutput) {
				FlushBuffers ();
				_Response.Flush ();
			}
		}
	}
}

