//
// System.Web.HttpWriter.cs 
//
// 
// Author:
//	Miguel de Icaza (miguel@novell.com)
//
//
// Copyright (C) 2005-2009 Novell, Inc (http://www.novell.com)
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
using System.Text;
using System.Threading;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Permissions;
	
namespace System.Web
{	
	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class HttpWriter : TextWriter
	{
		const long MAX_TOTAL_BUFFERS_SIZE = 4 * 1024 * 1024;
		const uint SINGLE_BUFFER_SIZE = 128 * 1024;
		const uint MIN_SINGLE_BUFFER_SIZE = 32 * 1024;
		
		HttpResponseStream output_stream;
		HttpResponse response;
		Encoding encoding;

		[ThreadStatic]
		static byte [] _bytebuffer;
		static readonly uint byteBufferSize;
		
		static HttpWriter ()
		{
			int workerThreads, completionPortThreads;

			ThreadPool.GetMinThreads (out workerThreads, out completionPortThreads);
			workerThreads *= 3;

			uint bufferSize = (uint)(MAX_TOTAL_BUFFERS_SIZE / workerThreads);
			byteBufferSize = Math.Min (SINGLE_BUFFER_SIZE, bufferSize);
			if (byteBufferSize < MIN_SINGLE_BUFFER_SIZE)
				byteBufferSize = MIN_SINGLE_BUFFER_SIZE;
		}
		
		internal HttpWriter (HttpResponse response)
		{
			this.response = response;
			encoding = response.ContentEncoding;
			output_stream = response.output_stream;
		}

		byte [] GetByteBuffer (int length)
		{
			if (_bytebuffer == null)
				_bytebuffer = new byte [byteBufferSize];
			
			// We will reuse the buffer if its size is < 32K
			if (byteBufferSize >= length)
				return _bytebuffer;
			else
				return new byte [length];
		}

		public override Encoding Encoding {
			get {
				return encoding;
			}
		}

		internal void SetEncoding (Encoding new_encoding)
		{
			encoding = new_encoding;
		}

		public Stream OutputStream {
			get {
				return output_stream;
			}
		}

		internal HttpResponse Response {
			get { return response; }
		}
		//
		// Flush data, and closes socket
		//
		public override void Close ()
		{
			output_stream.Close ();
		}

		public override void Flush ()
		{
			output_stream.Flush ();
		}

		char [] chars = new char [1];
		public override void Write (char ch)
		{
			chars [0] = ch;
			Write (chars, 0, 1);
		}

		public override void Write (object obj)
		{
			if (obj == null)
				return;
			
			Write (obj.ToString ());
		}
		
		public override void Write (string s)
		{
			if (s != null)
				WriteString (s, 0, s.Length);
		}
		
		public override void Write (char [] buffer, int index, int count)
		{
			if (buffer == null || index < 0 || count < 0 || (buffer.Length - index) < count)
				throw new ArgumentOutOfRangeException ();
#if TARGET_JVM
			output_stream.Write (buffer, index, count);
#else
			int length = encoding.GetMaxByteCount (count);
			byte [] bytebuffer = GetByteBuffer (length);
			int realLength = encoding.GetBytes (buffer, index, count, bytebuffer, 0);
			output_stream.Write (bytebuffer, 0, realLength);
#endif
			if (response.buffer)
				return;

			response.Flush ();
		}

		static char [] newline = new char [2] { '\r', '\n' };
		
		public override void WriteLine ()
		{
			Write (newline, 0, 2);
		}

		public void WriteString (string s, int index, int count)
		{
			if (s == null)
				return;

			if (index < 0 || count < 0 || ((index + count > s.Length)))
				throw new ArgumentOutOfRangeException ();
#if TARGET_JVM
			output_stream.Write (s, index, count);
#else
			int length = encoding.GetMaxByteCount (count);
			byte [] bytebuffer = GetByteBuffer (length);
			int realLength = encoding.GetBytes (s, index, count, bytebuffer, 0);
			output_stream.Write (bytebuffer, 0, realLength);
#endif
			if (response.buffer)
				return;

			response.Flush ();
		}

		internal void WriteUTF8Ptr (IntPtr ptr, int length)
		{
			output_stream.WritePtr (ptr, length);
		}

		public void WriteBytes (byte [] buffer, int index, int count)
		{
			output_stream.Write (buffer, index, count);

			if (response.buffer)
				return;

			response.Flush ();
		}
	}
}

