//
// Mainsoft.Web.Hosting.OutputStreamWrapper
//
// Authors:
//	Vladimir Krasnov (vladimirk@mainsoft.com)
//
// (C) 2006 Mainsoft
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
using System.Web;
using System.Collections.Generic;
using System.Text;

using java.io;
using vmw.common;


namespace Mainsoft.Web.Hosting
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class OutputStreamWrapper : java.io.Writer
	{
		enum OutputMode
		{
			CharWriter,
			ByteStream,
		}

		readonly PrintWriter _printWriter;
		readonly OutputStream _outputStream;
		readonly OutputMode _outputMode;

		private byte [] _bytebuffer;
		private char [] _charBuffer;
		//sbyte [] _oneByte = new sbyte [1];

		Encoding _encoding = null;

		/// <summary>
		/// Ctor from writer
		/// </summary>
		/// <param name="writer"></param>
		public OutputStreamWrapper (PrintWriter writer)
		{
			_printWriter = writer;
			_outputMode = OutputMode.CharWriter;
		}

		/// <summary>
		/// Ctor from stream
		/// </summary>
		/// <param name="outputStream"></param>
		public OutputStreamWrapper (OutputStream outputStream)
		{
			_outputStream = outputStream;
			_outputMode = OutputMode.ByteStream;
		}

		/// <summary>
		/// 
		/// </summary>
		public override void close ()
		{
			// BUGBUG
			// on WPS, closing the writer causes
			// any portlets rendered after us to be not shown...
			//_writer.close();
		}

		/// <summary>
		/// Flushes servers stream
		/// </summary>
		public override void flush ()
		{
			if (_outputMode == OutputMode.CharWriter)
				_printWriter.flush ();
			else
				_outputStream.flush();
		}

		/// <summary>
		/// Writes char array
		/// </summary>
		/// <param name="chars"></param>
		/// <param name="index"></param>
		/// <param name="count"></param>
		public override void write (char [] chars, int index, int count)
		{
			if (_outputMode == OutputMode.CharWriter) {
				_printWriter.write (chars, index, count);
			}
			else {
				int length = Encoding.GetMaxByteCount (count);
				byte [] bytebuffer = GetByteBuffer (length);
				int realLength = Encoding.GetBytes (chars, index, count, bytebuffer, 0);

				_outputStream.write (TypeUtils.ToSByteArray (bytebuffer), 0, realLength);
			}
				
		}

		/// <summary>
		/// Writes byte array
		/// </summary>
		/// <param name="sbytes"></param>
		/// <param name="index"></param>
		/// <param name="count"></param>
		public void write (sbyte [] sbytes, int index, int count)
		{
			if (_outputMode == OutputMode.ByteStream)
				_outputStream.write (sbytes, index, count);
			else {
				int length = Encoding.GetMaxCharCount (count);
				char [] charbuffer = GetCharBuffer (length);
				int realLength = Encoding.GetChars ((byte []) TypeUtils.ToByteArray (sbytes), index, count, charbuffer, 0);

				_printWriter.write (charbuffer, 0, realLength);
			}
		}

		private char [] GetCharBuffer (int length)
		{
			// We will reuse the buffer if its size is < 32K
			if (_charBuffer != null && _charBuffer.Length >= length)
				return _charBuffer;

			if (length > 32 * 1024)
				return new char [length];

			if (length < 1024)
				length = 1024;

			_charBuffer = new char [length];
			return _charBuffer;
		}

		private byte [] GetByteBuffer (int length)
		{
			// We will reuse the buffer if its size is < 32K
			if (_bytebuffer != null && _bytebuffer.Length >= length)
				return _bytebuffer;

			if (length > 32 * 1024)
				return new byte [length];

			if (length < 1024)
				length = 1024;

			_bytebuffer = new byte [length];
			return _bytebuffer;
		}
		
		private Encoding Encoding
		{
			get
			{
				if (_encoding == null) {
					HttpContext current = HttpContext.Current;
					_encoding = (current != null) ? current.Response.ContentEncoding : Encoding.UTF8;
				}
				return _encoding;
			}
		}
	}
}
