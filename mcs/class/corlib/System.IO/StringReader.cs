//
// System.IO.StringReader
//
// Author: Marcin Szczepanski (marcins@zipworld.com.au)
//


using System;

namespace System.IO {
	[Serializable]
	public class StringReader : TextReader {

		private string source;
		private char[] sourceChars;

		private int nextChar;
		private int sourceLength;
		private bool disposed = false;

		public StringReader( string s ) {

			if (s == null) 
				throw new ArgumentNullException ();

			this.source = s;
			nextChar = 0;
			sourceLength = s.Length;
			sourceChars = s.ToCharArray();
		}

		public override void Close() {
			Dispose( true );
			disposed = true;
		}

		protected override void Dispose (bool disposing)
		{
			sourceChars = null;
			base.Dispose (disposing);
		}

		public override int Peek() {

			if (disposed) 
				throw new ObjectDisposedException ("StringReader", "Cannot read from a closed StringReader");

			if( nextChar >= sourceLength ) {
				return -1;
			} else {
				return (int)source[ nextChar ];
			}
		}

		public override int Read() {

			if (disposed) 
				throw new ObjectDisposedException ("StringReader", "Cannot read from a closed StringReader");

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

		public override int Read( char[] buffer, int index, int count ) {

			if (disposed) 
				throw new ObjectDisposedException ("StringReader", "Cannot read from a closed StringReader");

			if( buffer == null ) {
				throw new ArgumentNullException();
			} else if( buffer.Length - index < count ) {
				throw new ArgumentException();
			} else if( index < 0 || count < 0 ) {
				throw new ArgumentOutOfRangeException();
			}

			int charsToRead;

			if( nextChar + count > sourceLength ) {
				charsToRead = sourceLength - nextChar;
			} else {
				charsToRead = count;
			}

			Array.Copy(sourceChars, nextChar, buffer, index, charsToRead );

			nextChar += count;

			return charsToRead;
		}

		public override string ReadLine() {
			// Reads until next \r or \n or \r\n, otherwise return null

                        // LAMESPEC:
                        // The Beta 2 SDK help says that the ReadLine method returns
                        // "The next line from the input stream [...] A line is defined as a sequence of
                        // characters followed by a carriage return (\r), a line feed (\n), or a carriage
                        // return immediately followed by a line feed (\r\n). [...]
                        // The returned value is a null reference if the end of the input stream has been reached."
                        //
                        // HOWEVER, the MS implementation returns the rest of the string if no \r and/or \n is found
                        // in the string

			if (disposed) 
				throw new ObjectDisposedException ("StringReader", "Cannot read from a closed StringReader");

			if (nextChar >= source.Length)
				return null;

			int nextCR = source.IndexOf( '\r', nextChar );
                        int nextLF = source.IndexOf( '\n', nextChar );

                        if( nextCR == -1 && nextLF == -1 ) {
                                return ReadToEnd();
                        }

                        int readTo;

                        if( nextCR == -1 ) {
                                readTo = nextLF;
                        } else {
                                readTo = nextCR;
                        }

                        string nextLine = source.Substring( nextChar, readTo - nextChar );

                        if( nextLF == nextCR + 1 ) {
		                nextChar = readTo + 2;
                        } else {
                                nextChar = readTo + 1;
                        }

			return nextLine;
		}

                public override string ReadToEnd() {

			if (disposed) 
				throw new ObjectDisposedException ("StringReader", "Cannot read from a closed StringReader");

                        string toEnd = source.Substring( nextChar, sourceLength - nextChar );
                        nextChar = sourceLength;
                        return toEnd;
                }

	}
}
