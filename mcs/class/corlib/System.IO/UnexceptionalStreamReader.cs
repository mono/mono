//
// System.IO.UnexceptionalStreamReader.cs
//
// Authors:
//   Dietmar Maurer (dietmar@ximian.com)
//   Miguel de Icaza (miguel@ximian.com)
//   Dick Porter (dick@ximian.com)
//   Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System.Text;
using System.Runtime.InteropServices;

namespace System.IO {
	internal class UnexceptionalStreamReader : StreamReader {

		private static bool[] newline = new bool [Environment.NewLine.Length];

		private static char newlineChar;

		static UnexceptionalStreamReader () {
			string n = Environment.NewLine;
			if (n.Length == 1)
				newlineChar = n [0];
		}
/*
		public UnexceptionalStreamReader(Stream stream)
			: base (stream)
		{
		}

		public UnexceptionalStreamReader(Stream stream, bool detect_encoding_from_bytemarks)
			: base (stream, detect_encoding_from_bytemarks)
		{
		}
*/
		public UnexceptionalStreamReader(Stream stream, Encoding encoding)
			: base (stream, encoding)
		{
		}
/*
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
*/
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
			if (dest_buffer == null)
				throw new ArgumentNullException ("dest_buffer");
			if (index < 0)
				throw new ArgumentOutOfRangeException ("index", "< 0");
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count", "< 0");
			// ordered to avoid possible integer overflow
			if (index > dest_buffer.Length - count)
				throw new ArgumentException ("index + count > dest_buffer.Length");

			int chars_read = 0;
			char nl = newlineChar;
			try {
				while (count > 0) {
					int c = base.Read ();
					if (c < 0)
						break;
					chars_read++;
					count--;

					dest_buffer [index] = (char) c;
					// shortcut when a new line is only one character (e.g. Linux, Mac)
					if (nl != (char)0) {
						if ((char)c == nl)
							return chars_read;
					} else {
						if (CheckEOL ((char)c))
							return chars_read;
					}
					index ++;
				}
			} catch (IOException) {
			}
			
			return chars_read;
		}

		private bool CheckEOL (char current)
		{
			// general case for any length (e.g. Windows)
			for (int i=0; i < newline.Length; i++) {
				if (!newline [i]) {
					if (current == Environment.NewLine [i]) {
						newline [i] = true;
						return (i == newline.Length - 1);
					}
					break;
				}
			}
			for (int j=0; j < newline.Length; j++)
				newline [j] = false;
			return false;
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
