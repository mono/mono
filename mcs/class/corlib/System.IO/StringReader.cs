//
// System.IO.StringReader
//
// Author: Marcin Szczepanski (marcins@zipworld.com.au)
//
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

using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace System.IO {
	[Serializable]
	public class StringReader : TextReader {

		string source;
		int nextChar;
		int sourceLength;

		public StringReader( string s ) {

			if (s == null) 
				throw new ArgumentNullException ("s");

			this.source = s;
			nextChar = 0;
			sourceLength = s.Length;
		}

		public override void Close ()
		{
			Dispose (true);
		}

		protected override void Dispose (bool disposing)
		{
			source = null;
			base.Dispose (disposing);
		}

		public override int Peek() {

			CheckObjectDisposedException ();

			if( nextChar >= sourceLength ) {
				return -1;
			} else {
				return (int)source[ nextChar ];
			}
		}

		public override int Read() {

			CheckObjectDisposedException ();

			if( nextChar >= sourceLength ) {
				return -1;
			} else {
				return (int)source[ nextChar++ ];
			}
		}


		// The method will read up to count characters from the StringReader
		// into the buffer character array starting at position index. Returns
		// the actual number of characters read, or zero if the end of the string
		// has been reached and no characters are read.

		public override int Read ([In, Out] char[] buffer, int index, int count)
		{
			CheckObjectDisposedException ();

			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (buffer.Length - index < count)
				throw new ArgumentException ();
			if (index < 0 || count < 0)
				throw new ArgumentOutOfRangeException ();

			int charsToRead;

			// reordered to avoir possible integer overflow
			if (nextChar > sourceLength - count)
				charsToRead = sourceLength - nextChar;
			else
				charsToRead = count;
			
			source.CopyTo (nextChar, buffer, index, charsToRead);

			nextChar += charsToRead;

			return charsToRead;
		}

		public override string ReadLine ()
		{
			// Reads until next \r or \n or \r\n, otherwise return null

			// LAMESPEC:
			// The Beta 2 SDK help says that the ReadLine method
			// returns "The next line from the input stream [...] A
			// line is defined as a sequence of characters followed by
			// a carriage return (\r), a line feed (\n), or a carriage
			// return immediately followed by a line feed (\r\n).
			// [...] The returned value is a null reference if the end
			// of the input stream has been reached."
			//
			// HOWEVER, the MS implementation returns the rest of
			// the string if no \r and/or \n is found in the string

			CheckObjectDisposedException ();

			if (nextChar >= source.Length)
				return null;

			int nextCR = source.IndexOf ('\r', nextChar);
			int nextLF = source.IndexOf ('\n', nextChar);
			int readTo;
			bool consecutive = false;

			if (nextCR == -1) {
				if (nextLF == -1)
					return ReadToEnd ();

				readTo = nextLF;
			} else if (nextLF == -1) {
				readTo = nextCR;
			} else {
				readTo = (nextCR > nextLF) ? nextLF : nextCR;
				consecutive = (nextCR + 1 == nextLF || nextLF + 1 == nextCR);
			}

			string nextLine = source.Substring (nextChar, readTo - nextChar);
			nextChar = readTo + ((consecutive) ? 2 : 1);
			return nextLine;
		}

                public override string ReadToEnd() {

			CheckObjectDisposedException ();
                        string toEnd = source.Substring( nextChar, sourceLength - nextChar );
                        nextChar = sourceLength;
                        return toEnd;
                }

		private void CheckObjectDisposedException ()
		{
			if (source == null)
				throw new ObjectDisposedException ("StringReader", 
					Locale.GetText ("Cannot read from a closed StringReader"));
		}
	}
}
