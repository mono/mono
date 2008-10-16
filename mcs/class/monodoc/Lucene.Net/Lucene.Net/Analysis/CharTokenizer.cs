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
namespace Monodoc.Lucene.Net.Analysis
{
	
	/// <summary>An abstract base class for simple, character-oriented tokenizers.</summary>
	public abstract class CharTokenizer : Tokenizer
	{
		public CharTokenizer(System.IO.TextReader input) : base(input)
		{
		}
		
		private int offset = 0, bufferIndex = 0, dataLen = 0;
		private const int MAX_WORD_LEN = 255;
		private const int IO_BUFFER_SIZE = 1024;
		private char[] buffer = new char[MAX_WORD_LEN];
		private char[] ioBuffer = new char[IO_BUFFER_SIZE];
		
		/// <summary>Returns true iff a character should be included in a token.  This
		/// tokenizer generates as tokens adjacent sequences of characters which
		/// satisfy this predicate.  Characters for which this is false are used to
		/// define token boundaries and are not included in tokens. 
		/// </summary>
		protected internal abstract bool IsTokenChar(char c);
		
		/// <summary>Called on each token character to normalize it before it is added to the
		/// token.  The default implementation does nothing.  Subclasses may use this
		/// to, e.g., lowercase tokens. 
		/// </summary>
		protected internal virtual char Normalize(char c)
		{
			return c;
		}
		
		/// <summary>Returns the next token in the stream, or null at EOS. </summary>
		public override Token Next()
		{
			int length = 0;
			int start = offset;
			while (true)
			{
				char c;
				
				offset++;
				if (bufferIndex >= dataLen)
				{
					dataLen = input.Read((System.Char[]) ioBuffer, 0, ioBuffer.Length);
					bufferIndex = 0;
				}
				;
				if (dataLen <= 0)
				{
					if (length > 0)
						break;
					else
						return null;
				}
				else
					c = ioBuffer[bufferIndex++];
				
				if (IsTokenChar(c))
				{
					// if it's a token char
					
					if (length == 0)
					// start of token
						start = offset - 1;
					
					buffer[length++] = Normalize(c); // buffer it, normalized
					
					if (length == MAX_WORD_LEN)
					// buffer overflow!
						break;
				}
				else if (length > 0)
				// at non-Letter w/ chars
					break; // return 'em
			}
			
			return new Token(new System.String(buffer, 0, length), start, start + length);
		}
	}
}