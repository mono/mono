//
// System.CStreamReader
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
#if NET_2_0
using System.Text;
using System.Runtime.InteropServices;

namespace System.IO {
	class CStreamReader : StreamReader {
		public CStreamReader(Stream stream, Encoding encoding)
			: base (stream, encoding)
		{
		}

		public override int Peek ()
		{
			try {
				return base.Peek ();
			} catch (IOException) {
			}

			return -1;
		}

		public override int Read ()
		{
			try {
				ConsoleKeyInfo key = Console.ReadKey ();
				return key.KeyChar;
			} catch (IOException) {
			}

			return(-1);
		}

		public override int Read ([In, Out] char [] dest_buffer, int index, int count)
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
			while (count > 0) {
				int c = Read ();
				if (c < 0)
					break;
				chars_read++;
				count--;

				dest_buffer [index] = (char) c;
				//if (CheckEOL (dest_buffer [index++]))
				//	return chars_read;
			}
			return chars_read;
		}

		public override string ReadLine ()
		{
			try {
				return ConsoleDriver.driver.ReadLine ();
			} catch (IOException) {
			}

			return null;
		}

		public override string ReadToEnd ()
		{
			try {
				return (base.ReadToEnd ());
			} catch (IOException) {
			}

			return null;
		}
	}
}
#endif

