//
// System.IO.StringReader
//
// Author: Marcin Szczepanski (marcins@zipworld.com.au)
//


using System;

namespace System.IO {
	public class StringReader : TextReader {

		protected string source;
		protected char[] sourceChars;

		protected int nextChar;
		protected int sourceLength;

		public StringReader( string s ) {
			this.source = s;
			nextChar = 0;
			sourceLength = s.Length;
			sourceChars = s.ToCharArray();
		}

		public override void Close() {
			Dispose( true );
		}

		protected override void Dispose( bool disposing ) {
			return;
		}
	
		public override int Peek() {
			if( nextChar > sourceLength ) {
				return -1;
			} else {
				return (int)source[ nextChar ];
			}
		}

		public override int Read() {
			if( nextChar > sourceLength ) {
				return -1;
			} else {
				return (int)source[ nextChar++ ];
			}
		}

		
		// The method will read up to count characters from the StringReader 
		// into the buffer character array starting at position index. Returns 
		// the actual number of characters read, or zero if the end of the string 
		// has been reached and no characters are read.

		public override int Read( out char[] buffer, int index, int count ) {
			
			int charsToRead;

			if( nextChar + count > sourceLength ) {
				charsToRead = sourceLength - nextChar;
			} else {
				charsToRead = count;
			}
			
			buffer = new char [charsToRead];

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
                   
			
			int nextCR = source.IndexOf( '\r', nextChar );
                        int nextLF = source.IndexOf( '\n', nextChar );
        
                        if( nextCR == -1 && nextLF == -1 ) {
                                return ReadToEnd();
                        }
                                       
                        if( nextChar > sourceLength ) return null;
                        
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
                        string toEnd = source.Substring( nextChar, sourceLength - nextChar );
                        nextChar = sourceLength;
                        return toEnd;
                }

	}
}
