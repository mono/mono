/******************************************************************************
* The MIT License
* Copyright (c) 2003 Novell Inc.  www.novell.com
* 
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
* copies of the Software, and to  permit persons to whom the Software is 
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in 
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/
//
// Novell.Directory.Ldap.Utilclass.SchemaTokenCreator.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;

namespace Novell.Directory.Ldap.Utilclass
{
	
	public class SchemaTokenCreator
	{
		private string basestring;
		private bool cppcomments=false;	// C++ style comments enabled
		private bool ccomments=false;	// C style comments enabled
		private bool iseolsig=false;
		private bool cidtolower;
		private bool pushedback;
		private int peekchar;	
		private sbyte[] ctype;
		private int linenumber=1;
		private int ichar=1;
		private char[] buf;

		private System.IO.StreamReader reader = null;
		private System.IO.StringReader sreader = null;
		private System.IO.Stream input = null;

		public System.String StringValue;
		public double NumberValue;
		public int lastttype;	

		private void Initialise()
		{
			ctype = new sbyte[256];
			buf = new char[20];
			peekchar=System.Int32.MaxValue;
			WordCharacters('a', 'z');
			WordCharacters('A', 'Z');
			WordCharacters(128 + 32, 255);
			WhitespaceCharacters(0, ' ');
			CommentCharacter('/');
			QuoteCharacter('"');
			QuoteCharacter('\'');
			parseNumbers();
		}

		public SchemaTokenCreator(System.IO.Stream instream)
		{
			Initialise();
			if (instream == null)
			{
				throw new System.NullReferenceException();
			}
			input = instream;
		}

		public SchemaTokenCreator(System.IO.StreamReader r)
		{
			Initialise();
			if (r == null)
			{
				throw new System.NullReferenceException();
			}
			reader = r;
		}

		public SchemaTokenCreator(System.IO.StringReader r)
		{
			Initialise();
			if (r == null)
			{
				throw new System.NullReferenceException();
			}
			sreader = r;
		}

		public void  pushBack()
		{
				pushedback = true;
		}

		public int CurrentLine
		{
			get
			{
			return linenumber;
			}
		}

		public System.String ToStringValue()
		{
			System.String strval;
			switch (lastttype)
			{
				
				case (int)TokenTypes.EOF: 
					strval = "EOF";
					break;
				
				case (int) TokenTypes.EOL: 
					strval = "EOL";
					break;
				
				case (int) TokenTypes.WORD: 
					strval = StringValue;
					break;

				case (int) TokenTypes.STRING: 
					strval = StringValue;
					break;
				
				case (int) TokenTypes.NUMBER: 
				case (int) TokenTypes.REAL: 
					strval = "n=" + NumberValue;
					break;
				
				default:  
				{
					if (lastttype < 256 && ((ctype[lastttype] & (sbyte)CharacterTypes.STRINGQUOTE) != 0))
					{
						strval = StringValue;
						break;
					}
						
					char[] s = new char[3];
					s[0] = s[2] = '\'';
					s[1] = (char) lastttype;
					strval = new System.String(s);
					break;
				}
				
			}
			return  strval;
		}

		public void  WordCharacters(int min, int max)
		{
			if (min < 0)
				min = 0;
			if (max >= ctype.Length)
				max = ctype.Length - 1;
			while (min <= max)
				ctype[min++] |= (sbyte)CharacterTypes.ALPHABETIC;
		}

		public void  WhitespaceCharacters(int min, int max)
		{
			if (min < 0)
				min = 0;
			if (max >= ctype.Length)
				max = ctype.Length - 1;
			while (min <= max)
				ctype[min++] = (sbyte)CharacterTypes.WHITESPACE;
		}
		
		public void  OrdinaryCharacters(int min, int max)
		{
			if (min < 0)
				min = 0;
			if (max >= ctype.Length)
				max = ctype.Length - 1;
			while (min <= max)
				ctype[min++] = 0;
		}

		public void  OrdinaryCharacter(int ch)
		{
			if (ch >= 0 && ch < ctype.Length)
				ctype[ch] = 0;
		}

		public void  CommentCharacter(int ch)
		{
			if (ch >= 0 && ch < ctype.Length)
				ctype[ch] = (sbyte)CharacterTypes.COMMENTCHAR;
		}
		
		public void InitTable()
		{
			for (int i = ctype.Length; --i >= 0; )
				ctype[i] = 0;
		}

		public void  QuoteCharacter(int ch)
		{
			if (ch >= 0 && ch < ctype.Length)
				ctype[ch] = (sbyte)CharacterTypes.STRINGQUOTE;
		}

		public void  parseNumbers()
		{
			for (int i = '0'; i <= '9'; i++)
				ctype[i] |= (sbyte)CharacterTypes.NUMERIC;
			ctype['.'] |= (sbyte)CharacterTypes.NUMERIC;
			ctype['-'] |= (sbyte)CharacterTypes.NUMERIC;
		}

		private int read()
		{
			if (sreader !=null )
			{
				return sreader.Read();
			}
			else if (reader != null)
			{
				return reader.Read();
			}
			else if (input != null)
				return input.ReadByte();
			else
				throw new System.SystemException();
		}

		public int nextToken()
		{
			if (pushedback)
			{
				pushedback = false;
				return lastttype;
			}

			StringValue = null;
			
			int curc = peekchar;
			if (curc < 0)
				curc = System.Int32.MaxValue;
			if (curc == (System.Int32.MaxValue - 1))
			{
				curc = read();
				if (curc < 0)
					return lastttype = (int) TokenTypes.EOF;
				if (curc == '\n')
					curc = System.Int32.MaxValue;
			}
			if (curc == System.Int32.MaxValue)
			{
				curc = read();
				if (curc < 0)
					return lastttype = (int) TokenTypes.EOF;
			}
			lastttype = curc; 
			peekchar = System.Int32.MaxValue;
			
			int ctype = curc < 256?this.ctype[curc]:(sbyte)CharacterTypes.ALPHABETIC;
			while ((ctype & (sbyte)CharacterTypes.WHITESPACE) != 0)
			{
				if (curc == '\r')
				{
					linenumber++;
					if (iseolsig)
					{
						peekchar = (System.Int32.MaxValue - 1);
						return lastttype = (int) TokenTypes.EOL;
					}
					curc = read();
					if (curc == '\n')
						curc = read();
				}
				else
				{
					if (curc == '\n')
					{
						linenumber++;
						if (iseolsig)
						{
							return lastttype = (int) TokenTypes.EOL;
						}
					}
					curc = read();
				}
				if (curc < 0)
					return lastttype = (int) TokenTypes.EOF;
				ctype = curc < 256?this.ctype[curc]:(sbyte)CharacterTypes.ALPHABETIC;
			}
			
			if ((ctype & (sbyte)CharacterTypes.NUMERIC) != 0)
			{
				bool checkb = false;
				if (curc == '-')
				{
					curc = read();
					if (curc != '.' && (curc < '0' || curc > '9'))
					{
						peekchar = curc;
						return lastttype = '-';
					}
					checkb = true;
				}
				double dvar = 0;
				int tempvar = 0;
				int checkdec = 0;
				while (true)
				{
					if (curc == '.' && checkdec == 0)
						checkdec = 1;
					else if ('0' <= curc && curc <= '9')
					{
						dvar = dvar * 10 + (curc - '0');
						tempvar += checkdec;
					}
					else
						break;
					curc = read();
				}
				peekchar = curc;
				if (tempvar != 0)
				{
					double divby = 10;
					tempvar--;
					while (tempvar > 0)
					{
						divby *= 10;
						tempvar--;
					}
					dvar = dvar / divby;
				}
				NumberValue = checkb?- dvar:dvar;
				return lastttype = (int) TokenTypes.NUMBER;
			}
			
			if ((ctype & (sbyte)CharacterTypes.ALPHABETIC) != 0)
			{
				int i = 0;
				do 
				{
					if (i >= buf.Length)
					{
						char[] nb = new char[buf.Length * 2];
						Array.Copy((System.Array) buf, 0, (System.Array) nb, 0, buf.Length);
						buf = nb;
					}
					buf[i++] = (char) curc;
					curc = read();
					ctype = curc < 0?(sbyte)CharacterTypes.WHITESPACE:curc < 256?this.ctype[curc]:(sbyte)CharacterTypes.ALPHABETIC;
				}
				while ((ctype & ((sbyte)CharacterTypes.ALPHABETIC | (sbyte)CharacterTypes.NUMERIC)) != 0);
				peekchar = curc;
				StringValue = new String(buf, 0, i);
				if (cidtolower)
					StringValue = StringValue.ToLower();
				return lastttype = (int) TokenTypes.WORD;
			}
			
			if ((ctype & (sbyte)CharacterTypes.STRINGQUOTE) != 0)
			{
				lastttype = curc;
				int i = 0;
				int rc = read();
				while (rc >= 0 && rc != lastttype && rc != '\n' && rc != '\r')
				{
					if (rc == '\\')
					{
						curc = read();
						int first = curc; 
						if (curc >= '0' && curc <= '7')
						{
							curc = curc - '0';
							int loopchar = read();
							if ('0' <= loopchar && loopchar <= '7')
							{
								curc = (curc << 3) + (loopchar - '0');
								loopchar = read();
								if ('0' <= loopchar && loopchar <= '7' && first <= '3')
								{
									curc = (curc << 3) + (loopchar - '0');
									rc = read();
								}
								else
									rc = loopchar;
							}
							else
								rc = loopchar;
						}
						else
						{
							switch (curc)
							{
								
								case 'f': 
									curc = 0xC;
									break;

								case 'a': 
									curc = 0x7;
									break;
								
								case 'b': 
									curc = '\b';
									break;

								case 'v': 
									curc = 0xB;
									break;
							
								case 'n': 
									curc = '\n';
									break;
								
								case 'r': 
									curc = '\r';
									break;
								
								case 't': 
									curc = '\t';
									break;

								default:
									break;
								
							}
							rc = read();
						}
					}
					else
					{
						curc = rc;
						rc = read();
					}
					if (i >= buf.Length)
					{
						char[] nb = new char[buf.Length * 2];
						Array.Copy((System.Array) buf, 0, (System.Array) nb, 0, buf.Length);
						buf = nb;
					}
					buf[i++] = (char) curc;
				}
				
				peekchar = (rc == lastttype)?System.Int32.MaxValue:rc;
				
				StringValue = new String(buf, 0, i);
				return lastttype;
			}
			
			if (curc == '/' && (cppcomments || ccomments))
			{
				curc = read();
				if (curc == '*' && ccomments)
				{
					int prevc = 0;
					while ((curc = read()) != '/' || prevc != '*')
					{
						if (curc == '\r')
						{
							linenumber++;
							curc = read();
							if (curc == '\n')
							{
								curc = read();
							}
						}
						else
						{
							if (curc == '\n')
							{
								linenumber++;
								curc = read();
							}
						}
						if (curc < 0)
							return lastttype = (int) TokenTypes.EOF;
						prevc = curc;
					}
					return nextToken();
				}
				else if (curc == '/' && cppcomments)
				{
					while ((curc = read()) != '\n' && curc != '\r' && curc >= 0)
						;
					peekchar = curc;
					return nextToken();
				}
				else
				{
					if ((this.ctype['/'] & (sbyte)CharacterTypes.COMMENTCHAR) != 0)
					{
						while ((curc = read()) != '\n' && curc != '\r' && curc >= 0)
							;
						peekchar = curc;
						return nextToken();
					}
					else
					{
						peekchar = curc;
						return lastttype = '/';
					}
				}
			}
			
			if ((ctype & (sbyte)CharacterTypes.COMMENTCHAR) != 0)
			{
				while ((curc = read()) != '\n' && curc != '\r' && curc >= 0)
					;
				peekchar = curc;
				return nextToken();
			}
			
			return lastttype = curc;
		}

	}
}
