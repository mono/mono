//
// XmlSignatureStreamReader.cs: Wrap TextReader and eliminate \r
//
// Author:
//	Atsushi Enomoto (atsushi@ximian.com)
//
// (C) 2005 Novell Inc.
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
//
// Use it to distinguish &#xD; and \r. \r is removed, while &#xD; is not.
//
//

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace System.Security.Cryptography.Xml
{
	internal class XmlSignatureStreamReader : TextReader
	{
		TextReader source;
		int cache = int.MinValue;

		public XmlSignatureStreamReader (TextReader input)
		{
			source =input;
		}

		public override void Close ()
		{
			source.Close ();
		}

		public override int Peek ()
		{
			// If source TextReader does not support Peek(),
			// it does not support too. Or it just returns EOF.
			if (source.Peek () == -1)
				return -1;

			if (cache != int.MinValue)
				return cache;
			cache = source.Read ();
			if (cache != '\r')
				return cache;
			// cache must be '\r' here.
			if (source.Peek () != '\n')
				return '\r';
			// Now Peek() returns '\n', so clear cache.
			cache = int.MinValue;
			return '\n';
		}

		public override int Read ()
		{
			if (cache != int.MinValue) {
				int ret = cache;
				cache = int.MinValue;
				return ret;
			}
			int i = source.Read ();
			if (i != '\r')
				return i;
			// read one more char (after '\r')
			cache = source.Read ();
			if (cache != '\n')
				return '\r';
			cache = int.MinValue;
			return '\n';
		}

		public override int ReadBlock (
			[In, Out] char [] buffer, int index, int count)
		{
			char [] tmp = new char [count];
			source.ReadBlock (tmp, 0, count);
			int j = index;
			for (int i = 0; i < count; j++) {
				if (tmp [i] == '\r') {
					if (++i < tmp.Length && tmp [i] == '\n')
						buffer [j] = tmp [i++];
					else
						buffer [j] = '\r';
				}
				else
					buffer [j] = tmp [i];
			}
			while (j < count) {
				int d = Read ();
				if (d < 0)
					break;
				buffer [j++] = (char) d;
			}
			return j;
		}

		// I have no idea what to do here, but I don't think it 
		// makes sense.
		public override string ReadLine ()
		{
			return source.ReadLine ();
		}

		public override string ReadToEnd ()
		{
			return source.ReadToEnd ().Replace ("\r\n", "\n");
		}
	}
}
