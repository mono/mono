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
		bool isElement;
		bool isLiteralNsUri;

		int line = 1;
		int column;
		bool nextIncrementLine;
		string prefixName;

		public RncTokenizer (TextReader source)
		{
			this.source = source;
		}

		public bool IsElement {
			get { return isElement; }
		}

		public int Line {
			get { return line; }
		}

		public int Column {
			get { return column; }
		}

		// jay interface implementation

		public int token ()
		{
			return currentToken;
		}

		public bool advance ()
		{
			if (prefixName != null)
				throw new RelaxngException ("Invalid prefix was found.");
			tokenValue = null;
			currentToken = ParseToken ();
			return currentToken != Token.EOF;
		}

		public object value ()
		{
			return tokenValue;
		}

		// private methods

		private int PeekChar ()
		{
			if (peekChar == 0)
				peekChar = source.Read ();
			return peekChar;
		}

		private int ReadChar ()
		{
			int ret;
			if (peekChar != 0) {
				ret = peekChar;
				peekChar = 0;
			}
			else
				ret = source.Read ();

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

		// TODO: parse three quoted
		private string ReadQuoted (char quoteChar)
		{
			int index = 0;
			bool loop = true;
			do {
				int c = ReadChar ();
				switch (c) {
				case -1:
				case '\"':
					loop = false;
					break;
				default:
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

		private string ReadOneToken ()
		{
			int index = 0;
			bool loop = true;
			do {
				int c = PeekChar ();
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
					if (!IsTokenContinuable (c)) {
						if (c == ':') {
							if (prefixName != null)
								throw new RelaxngException ("Invalid colon was found.");
							prefixName = new string (nameBuffer, 0, index);
						}
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

		private bool IsTokenContinuable (int c)
		{
			switch (c) {
			case '=':
			case ':':
			case ',':
			case '{':
			case '}':
			case '(':
			case ')':
			case '[':
			case ']':
			case '&':
			case '|':
			case '?':
			case '*':
			case '\\':
			case '+':
			case '-':
			case '>':
			case '#':
			case '\'':
			case '\"':
				return false;
			}
			return true;
		}

		private int ParseToken ()
		{
			SkipWhitespaces ();
			int c = ReadChar ();
			string name;
			switch (c) {
			case -1:
				return Token.EOF;
			case '=':
				return Token.Equal;
			case ':':
				// return CName
				if (prefixName == null)
					throw new RelaxngException ("Invalid character ':' was found.");
				if (PeekChar () == '*') {
					ReadChar ();
					tokenValue = prefixName;
					prefixName = null;
					return Token.NsName;
				}
				tokenValue = prefixName + ":" + ReadOneToken ();
				prefixName = null;
				return Token.CName;
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
				return Token.BackSlash;
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
				// NOTE: This interpretation is expanded against the spec
//				if (ReadChar () != '#')
//					throw new RelaxngException ("Invalid character after '#'.");
				tokenValue = ReadLine ();
				return Token.Documentation;
			case '\'':
			case '\"':
				name = ReadQuoted ((char) c);
				tokenValue = name;
				return Token.LiteralSegment;
			default:
				peekChar = c;
				name = ReadOneToken ();
				if (prefixName != null)
					return ParseToken ();
				tokenValue = name;
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
					return Token.NCNameButKeyword;
				}
			}
		}

	}
}
