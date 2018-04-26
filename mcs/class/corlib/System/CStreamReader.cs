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
#if MONO_FEATURE_CONSOLE
using System.Text;
using System.Runtime.InteropServices;

namespace System.IO {
	class CStreamReader : StreamReader {
		TermInfoDriver driver;

		public CStreamReader(Stream stream, Encoding encoding)
			: base (stream, encoding)
		{
			driver = (TermInfoDriver) ConsoleDriver.driver;
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

		public override int Read ([In, Out] char [] dest, int index, int count)
		{
			if (dest == null)
				throw new ArgumentNullException ("dest");
			if (index < 0)
				throw new ArgumentOutOfRangeException ("index", "< 0");
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count", "< 0");
			// ordered to avoid possible integer overflow
			if (index > dest.Length - count)
				throw new ArgumentException ("index + count > dest.Length");

			try {
				return driver.Read (dest, index, count);
			} catch (IOException) {
			}

			return 0;
		}

		public override string ReadLine ()
		{
			try {
				return driver.ReadLine ();
			} catch (IOException) {
			}

			return null;
		}

		public override string ReadToEnd ()
		{
			try {
				return driver.ReadToEnd ();
			} catch (IOException) {
			}

			return null;
		}
	}
}
#endif

