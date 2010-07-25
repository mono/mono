/*
 * Copyright 2004 The Apache Software Foundation
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
namespace Monodoc.Lucene.Net.QueryParsers
{
	
	/// <summary>An efficient implementation of JavaCC's CharStream interface.  <p>Note that
	/// this does not do line-number counting, but instead keeps track of the
	/// character position of the token in the input, as required by Lucene's {@link
	/// Monodoc.Lucene.Net.Analysis.Token} API. 
	/// </summary>
	public sealed class FastCharStream : CharStream
	{
		internal char[] buffer = null;
		
		internal int bufferLength = 0; // end of valid chars
		internal int bufferPosition = 0; // next char to read
		
		internal int tokenStart = 0; // offset in buffer
		internal int bufferStart = 0; // position in file of buffer
		
		internal System.IO.TextReader input; // source of chars
		
		/// <summary>Constructs from a Reader. </summary>
		public FastCharStream(System.IO.TextReader r)
		{
			input = r;
		}
		
		public char ReadChar()
		{
			if (bufferPosition >= bufferLength)
				Refill();
			return buffer[bufferPosition++];
		}
		
		private void  Refill()
		{
			int newPosition = bufferLength - tokenStart;
			
			if (tokenStart == 0)
			{
				// token won't fit in buffer
				if (buffer == null)
				{
					// first time: alloc buffer
					buffer = new char[2048];
				}
				else if (bufferLength == buffer.Length)
				{
					// grow buffer
					char[] newBuffer = new char[buffer.Length * 2];
					Array.Copy(buffer, 0, newBuffer, 0, bufferLength);
					buffer = newBuffer;
				}
			}
			else
			{
				// shift token to front
				Array.Copy(buffer, tokenStart, buffer, 0, newPosition);
			}
			
			bufferLength = newPosition; // update state
			bufferPosition = newPosition;
			bufferStart += tokenStart;
			tokenStart = 0;
            
            int charsRead = 0;
            
            try
            {
                charsRead = input.Read(buffer, newPosition, buffer.Length - newPosition);
            }
            catch 
            {
            }
            
			if (charsRead <= 0)
				throw new System.IO.IOException("read past eof");
			else
				bufferLength += charsRead;
		}
		
		public char BeginToken()
		{
			tokenStart = bufferPosition;
			return ReadChar();
		}
		
		public void  Backup(int amount)
		{
			bufferPosition -= amount;
		}
		
		public System.String GetImage()
		{
			return new System.String(buffer, tokenStart, bufferPosition - tokenStart);
		}
		
		public char[] GetSuffix(int len)
		{
			char[] value_Renamed = new char[len];
			Array.Copy(buffer, bufferPosition - len, value_Renamed, 0, len);
			return value_Renamed;
		}
		
		public void  Done()
		{
			try
			{
				input.Close();
			}
			catch (System.IO.IOException e)
			{
				System.Console.Error.WriteLine("Caught: " + e + "; ignoring.");
			}
		}
		
		public int GetColumn()
		{
			return bufferStart + bufferPosition;
		}
		public int GetLine()
		{
			return 1;
		}
		public int GetEndColumn()
		{
			return bufferStart + bufferPosition;
		}
		public int GetEndLine()
		{
			return 1;
		}
		public int GetBeginColumn()
		{
			return bufferStart + tokenStart;
		}
		public int GetBeginLine()
		{
			return 1;
		}
	}
}