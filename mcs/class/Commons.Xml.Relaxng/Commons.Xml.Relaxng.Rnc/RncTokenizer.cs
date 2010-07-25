//
// RELAX NG Compact Syntax parser
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// (C)2003 Atsushi Enomoto
// (C)2004 Novell Inc.
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

using System;
using System.Collections;
using System.IO;
using System.Xml;
using Commons.Xml.Relaxng;

namespace Commons.Xml.Relaxng.Rnc
{
	internal class RncTokenizer : Commons.Xml.Relaxng.Rnc.yyParser.yyInput
	{
		TextReader source;

		int currentToken;
		object tokenValue;
		int peekChar;
		string peekString;
		bool isElement;
		bool isLiteralNsUri;

		int line = 1;
		int column;
		int savedLineNumber = 1;
		int savedLinePosition;
		bool nextIncrementLine;
		string baseUri;

		public RncTokenizer (TextReader source, string baseUri)
		{
			this.source = source;
			this.baseUri = baseUri;
		}

		public bool IsElement {
			get { return isElement; }
		}

		public int Line {
			get { return savedLineNumber; }
		}

		public int Column {
			get { return savedLinePosition; }
		}

		public string BaseUri {
			get { return baseUri; }
		}

		// jay interface implementation

		public int token ()
		{
			return currentToken;
		}

		public bool advance ()
		{
			tokenValue = null;
			currentToken = ParseToken (false);
			savedLineNumber = line;
			savedLinePosition = column;
			return currentToken != Token.EOF;
		}

		public object value ()
		{
			return tokenValue;
		}

		// private methods

		private int ReadEscapedHexNumber (int current)
		{
			int i = source.Read ();
			switch (i) {
			case '0':
			case '1':
			case '2':
			case '3':
			case '4':
			case '5':
			case '6':
			case '7':
			case '8':
			case '9':
				current = current * 16 + (i - '0');
				return ReadEscapedHexNumber (current);
			case 'A':
			case 'B':
			case 'C':
			case 'D':
			case 'E':
			case 'F':
				current = current * 16 + (i - 'A') + 10;
				return ReadEscapedHexNumber (current);
			case 'a':
			case 'b':
			case 'c':
			case 'd':
			case 'e':
			case 'f':
				current = current * 16 + (i - 'a' + 10);
				return ReadEscapedHexNumber (current);
			}
			peekChar = i;
			return current;
		}

		private int ReadFromStream ()
		{
			int ret = source.Read ();
			if (ret != '\\')
				return ret;
			ret = source.Read ();
			switch (ret) {
			case 'x':
				int tmp;
				int xcount = 0;
				do {
					xcount++;
					tmp = source.Read ();
				} while (tmp == 'x');
				if (tmp != '{') {
					peekString = new string ('x', xcount);
					if (tmp >= 0)
						peekString += (char) tmp;
					return '\\';
				}
				ret = ReadEscapedHexNumber (0);
				if (peekChar != '}')
					break;
				peekChar = 0;
				return ret;
			}
			peekString = new string ((char) ret, 1);
			return '\\';
		}

		private int PeekChar ()
		{
			if (peekChar == 0) {
				if (peekString != null) {
					peekChar = peekString [0];
					peekString = peekString.Length == 1 ?
						null : peekString.Substring (1);
				}
				else
					peekChar = ReadFromStream ();
			}

			return peekChar;
		}

		private int ReadChar ()
		{
			int ret;
			if (peekChar != 0) {
				ret = peekChar;
				peekChar = 0;
			}
			else if (peekString != null) {
				ret = peekString [0];
				peekString = peekString.Length == 1 ?
					null : peekString.Substring (1);
			}
			else
				ret = ReadFromStream ();

			if (nextIncrementLine) {
				line++;
				column = 1;
				nextIncrementLine = false;
			}
			switch (ret) {
			case '\r':
				break;
			case '\n':
				nextIncrementLine = true;
				goto default;
			default:
				column++;
				break;
			}

			return ret;
		}

		private void SkipWhitespaces ()
		{
			while (true) {
				switch (PeekChar ()) {
				case ' ':
				case '\t':
				case '\r':
				case '\n':
					ReadChar ();
					continue;
				default:
					return;
				}
			}
		}

		char [] nameBuffer = new char [30];

		private string ReadQuoted (char quoteChar)
		{
			int index = 0;
			bool loop = true;
			while (loop) {
				int c = ReadChar ();
				switch (c) {
				case -1:
				case '\'':
				case '\"':
					if (quoteChar != c)
						goto default;
					loop = false;
					break;
				default:
					if (c < 0)
						throw new RelaxngException ("Unterminated quoted literal.");
					if (XmlChar.IsInvalid (c))
						throw new RelaxngException ("Invalid character in literal.");
					AppendNameChar (c, ref index);
					break;
				}
			}

			return new string (nameBuffer, 0, index);
		}

		private void AppendNameChar (int c, ref int index)
		{
			if (nameBuffer.Length == index) {
				char [] arr = new char [index * 2];
				Array.Copy (nameBuffer, arr, index);
				nameBuffer = arr;
			}
			if (c > 0x10000) {
				AppendNameChar ((c - 0x10000) / 0x400 + 0xD800, ref index);
				AppendNameChar ((c - 0x10000) % 0x400 + 0xDC00, ref index);
			}
			else
				nameBuffer [index++] = (char) c;
		}

		private string ReadTripleQuoted (char quoteChar)
		{
			int index = 0;
			bool loop = true;
			do {
				int c = ReadChar ();
				switch (c) {
				case -1:
				case '\'':
				case '\"':
					// 1
					if (quoteChar != c)
						goto default;
					// 2
					if ((c = PeekChar ()) != quoteChar) {
						AppendNameChar (quoteChar, ref index);
						goto default;
					}
					ReadChar ();
					// 3
					if ((c = PeekChar ()) == quoteChar) {
						ReadChar ();
						loop = false;
						break;
					}
					AppendNameChar (quoteChar, ref index);
					AppendNameChar (quoteChar, ref index);
					break;
				default:
					if (c < 0)
						throw new RelaxngException ("Unterminated triple-quoted literal.");
					if (XmlChar.IsInvalid (c))
						throw new RelaxngException ("Invalid character in literal.");
					AppendNameChar (c, ref index);
					break;
				}
			} while (loop);

			return new string (nameBuffer, 0, index);
		}

		private string ReadOneName ()
		{
			int index = 0;
			bool loop = true;
			int c = PeekChar ();
			if (!XmlChar.IsFirstNameChar (c) || !XmlChar.IsNCNameChar (c))
				throw new RelaxngException (String.Format ("Invalid NCName start character: {0}", c));
			do {
				c = PeekChar ();
				switch (c) {
				case -1:
				case ' ':
				case '\t':
				case '\r':
				case '\n':
					ReadChar ();
					loop = false;
					break;
				default:
					if (!XmlChar.IsNCNameChar (c)) {
						loop = false;
						break;
					}

					ReadChar ();
					if (nameBuffer.Length == index) {
						char [] arr = new char [index * 2];
						Array.Copy (nameBuffer, arr, index);
						nameBuffer = arr;
					}
					nameBuffer [index++] = (char) c;
					break;
				}
			} while (loop);

			return new string (nameBuffer, 0, index);
		}

		private string ReadLine ()
		{
			string s = source.ReadLine ();
			line++;
			column = 1;
			return s;
		}

		private int ParseToken (bool backslashed)
		{
			SkipWhitespaces ();
			int c = ReadChar ();
			string name;
			switch (c) {
			case -1:
				return Token.EOF;
			case '=':
				return Token.Equal;
			case '~':
				return Token.Tilde;
			case ',':
				return Token.Comma;
			case '{':
				return Token.OpenCurly;
			case '}':
				return Token.CloseCurly;
			case '(':
				return Token.OpenParen;
			case ')':
				return Token.CloseParen;
			case '[':
				return Token.OpenBracket;
			case ']':
				return Token.CloseBracket;
			case '&':
				if (PeekChar () != '=')
					return Token.Amp;
				ReadChar ();
				return Token.AndEquals;
			case '|':
				if (PeekChar () != '=')
					return Token.Bar;
				ReadChar ();
				return Token.OrEquals;
			case '?':
				return Token.Question;
			case '*':
				// See also ':' for NsName
				return Token.Asterisk;
			case '\\':
				if (backslashed)
					return Token.ERROR;
				return ParseToken (true);
			case '+':
				return Token.Plus;
			case '-':
				return Token.Minus;
			case '>':
				if (PeekChar () == '>') {
					ReadChar ();
					return Token.TwoGreaters;
				}
				peekChar = '>';
				goto default;
			case '#':
//				tokenValue = ReadLine ();
//				return Token.Documentation;
				ReadLine ();
				return ParseToken (false);
			case '\'':
			case '\"':
				if (PeekChar () != c)
					name = ReadQuoted ((char) c);
				else {
					ReadChar ();
					if (PeekChar () == c) {
						ReadChar ();
						name = ReadTripleQuoted ((char) c);
					} // else '' or ""
					name = String.Empty;
				}
				int invidx = XmlChar.IndexOfInvalid (name, true) ;
				if (invidx >= 0)
					throw new RelaxngException (String.Format ("Invalid XML character in compact syntax literal segment at {0:X}", (int) name [invidx]));
				tokenValue = name;
				return Token.LiteralSegment;
			default:
				if (!XmlChar.IsNCNameChar (c))
					throw new RelaxngException ("Invalid NCName character.");
				peekChar = c;
				name = ReadOneName ();
				if (PeekChar () == ':') {
					ReadChar ();
					if (PeekChar () == '*') {
						ReadChar ();
						tokenValue = name;
						return Token.NsName;
					}
					tokenValue = name + ":" + ReadOneName ();
					return Token.CName;

				}
				tokenValue = name;
				if (backslashed)
					return Token.QuotedIdentifier;
				switch (name) {
				case "attribute":
					isElement = false;
					return Token.KeywordAttribute;
				case "element":
					isElement = true;
					return Token.KeywordElement;
				case "datatypes":
					return Token.KeywordDatatypes;
				case "default":
					return Token.KeywordDefault;
				case "div":
					return Token.KeywordDiv;
				case "empty":
					return Token.KeywordEmpty;
				case "external":
					return Token.KeywordExternal;
				case "grammar":
					return Token.KeywordGrammar;
				case "include":
					return Token.KeywordInclude;
				case "inherit":
					return Token.KeywordInherit;
				case "list":
					return Token.KeywordList;
				case "mixed":
					return Token.KeywordMixed;
				case "namespace":
					return Token.KeywordNamespace;
				case "notAllowed":
					return Token.KeywordNotAllowed;
				case "parent":
					return Token.KeywordParent;
				case "start":
					return Token.KeywordStart;
				case "string":
					return Token.KeywordString;
				case "text":
					return Token.KeywordText;
				case "token":
					return Token.KeywordToken;
				default:
					return Token.NCName;
				}
			}
		}

	}
}
