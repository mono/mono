//
// System.IO.UnexceptionalStreamReader.cs
//
// Authors:
//   Dietmar Maurer (dietmar@ximian.com)
//   Miguel de Icaza (miguel@ximian.com)
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004 Novell (http://www.novell.com)
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


// This is a wrapper around StreamReader used by System.Console that
// catches IOException so that graphical applications don't suddenly
// get IO errors when their terminal vanishes.  See
// UnexceptionalStreamWriter too.

using System;
using System.Text;
using System.Runtime.InteropServices;

namespace System.IO {
	internal class UnexceptionalStreamReader : StreamReader {
		public UnexceptionalStreamReader(Stream stream)
			: base (stream)
		{
		}

		public UnexceptionalStreamReader(Stream stream, bool detect_encoding_from_bytemarks)
			: base (stream, detect_encoding_from_bytemarks)
		{
		}

		public UnexceptionalStreamReader(Stream stream, Encoding encoding)
			: base (stream, encoding)
		{
		}

		public UnexceptionalStreamReader(Stream stream, Encoding encoding, bool detect_encoding_from_bytemarks)
			: base (stream, encoding, detect_encoding_from_bytemarks)
		{
		}
		
		public UnexceptionalStreamReader(Stream stream, Encoding encoding, bool detect_encoding_from_bytemarks, int buffer_size)
			: base (stream, encoding, detect_encoding_from_bytemarks, buffer_size)
		{
		}

		public UnexceptionalStreamReader(string path)
			: base (path)
		{
		}

		public UnexceptionalStreamReader(string path, bool detect_encoding_from_bytemarks)
			: base (path, detect_encoding_from_bytemarks)
		{
		}

		public UnexceptionalStreamReader(string path, Encoding encoding)
			: base (path, encoding)
		{
		}

		public UnexceptionalStreamReader(string path, Encoding encoding, bool detect_encoding_from_bytemarks)
			: base (path, encoding, detect_encoding_from_bytemarks)
		{
		}
		
		public UnexceptionalStreamReader(string path, Encoding encoding, bool detect_encoding_from_bytemarks, int buffer_size)
			: base (path, encoding, detect_encoding_from_bytemarks, buffer_size)
		{
		}

		public override int Peek ()
		{
			try {
				return(base.Peek ());
			} catch (IOException) {
			}

			return(-1);
		}

		public override int Read ()
		{
			try {
				return(base.Read ());
			} catch (IOException) {
			}

			return(-1);
		}

		public override int Read ([In, Out] char[] dest_buffer,
					  int index, int count)
		{
			try {
				return(base.Read (dest_buffer, index, count));
			} catch (IOException) {
			}

			return(0);
		}

		public override string ReadLine()
		{
			try {
				return(base.ReadLine ());
			} catch (IOException) {
			}

			return(null);
		}

		public override string ReadToEnd()
		{
			try {
				return(base.ReadToEnd ());
			} catch (IOException) {
			}

			return(null);
		}
	}
}
