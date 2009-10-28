//
// SqlWhereClauseTokenizer.cs
//
// Author:
//   Juraj Skripsky (juraj@hotfeet.ch)
//
// (C) 2004 HotFeet GmbH (http://www.hotfeet.ch)
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
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Collections;

namespace Mono.Data.SqlExpressions {
	internal class Tokenizer : yyParser.yyInput {
		private static readonly IDictionary tokenMap = new Hashtable ();
		private static readonly Object [] tokens = {
			Token.AND, "and",
			Token.OR, "or",
			Token.NOT, "not",
			
			Token.TRUE, "true",
			Token.FALSE, "false",
			Token.NULL, "null",
			
			Token.PARENT, "parent",
			Token.CHILD, "child",
			
			Token.IS, "is",
			Token.IN, "in",
			Token.NOT_IN, "not in",
			Token.LIKE, "like",
			Token.NOT_LIKE, "not like",
			
			Token.COUNT, "count",
			Token.SUM, "sum",
			Token.AVG, "avg",
			Token.MAX, "max",
			Token.MIN, "min",
			Token.STDEV, "stdev",
			Token.VAR, "var",
			
			Token.IIF, "iif",
			Token.SUBSTRING, "substring",
			Token.ISNULL, "isnull",
			Token.LEN, "len",
			Token.TRIM, "trim",
			Token.CONVERT, "convert"
		};
		private char[] input;
		private int pos;

		private int tok;
		private object val;

		static Tokenizer ()
		{
			for (int i = 0; i < tokens.Length; i += 2)
				tokenMap.Add (tokens [i + 1], tokens [i]);
		}

		public Tokenizer (string strInput)
		{
			input = strInput.ToCharArray ();
			pos = 0;
		}

		private char Current() {
			return input [pos];
		}

		private char Next() {
			if (pos + 1 >= input.Length)
				return (char)0;
			return input [pos + 1];
		}

		private bool MoveNext() {
			pos++;
			if (pos >= input.Length)
				return false;

			return true;
		}
		
		private bool SkipWhiteSpace ()
		{
			if (pos >= input.Length)
				return false;

			while (Char.IsWhiteSpace (Current ())) {
				if (!MoveNext ())
					return false;
			}

			return true;
		}

		private object ReadNumber ()
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append (Current ());

			char next;
			while (Char.IsDigit (next = Next ()) || next == '.') {
				sb.Append (next);
				if (!MoveNext ())
					break;
			}

			string str = sb.ToString ();

			if (str.IndexOf(".") == -1)
				return Int64.Parse (str);
			else
				return double.Parse (str);
		}

		private char ProcessEscapes(char c)
		{
			if (c == '\\') {
				if (MoveNext())
					c = Current ();
				else
					c = '\0';

				switch (c) {
				case 'n':
					c = '\n';
					break;
				case 'r':
					c = '\r';
					break;
				case 't':
					c = '\t';
					break;

				case '\\':
					c = '\\';
					break;
					
				default:
					throw new SyntaxErrorException (String.Format ("Invalid escape sequence: '\\{0}'.", c));
				}
			}
			return c;
		}

		private string ReadString (char terminator)
		{
			return ReadString (terminator, false /* canEscape */);
		}

		private string ReadString (char terminator, 
					   bool canEscape // twice the terminator is not a terminator
					   )
		{
			bool terminated = false;
			StringBuilder sb = new StringBuilder ();
			while (MoveNext ()) {
				if (Current () == terminator) {
					if (Next () == terminator) {
						sb.Append (ProcessEscapes (Current ()));
						MoveNext ();
						continue;
					}
					terminated = true;
					break;
				}
				sb.Append (ProcessEscapes (Current ()));
			}
			
			if (! terminated)
				throw new SyntaxErrorException (String.Format ("invalid string at {0}{1}<--",
									       terminator,
									       sb.ToString ())
								);
			return sb.ToString ();			
		}

		private string ReadIdentifier ()
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append (Current ());

			char next;
			string ret;
			while ((next = Next ()) == '_' || Char.IsLetterOrDigit (next) || next == '\\') {
				sb.Append (ProcessEscapes (next));				
				if (!MoveNext ())
					break;
			}
			ret = sb.ToString ();
			if (String.Compare (ret,
					    "not",
#if NET_2_0
					    StringComparison.OrdinalIgnoreCase
#else
					    true, CultureInfo.InvariantCulture
#endif
			    ) == 0) {
				int savedPos = pos;
				while (Char.IsWhiteSpace ((next = Next ()))) {
					if (!MoveNext ()) {
						pos = savedPos;
						return ret;
					}
				}
				MoveNext ();
				
				string target;
				switch (Current ()) {
					case 'i':
					case 'I':
						target = "in";
						break;

					case 'l':
					case 'L':
						target = "like";
						break;
					
					default:
						pos = savedPos;
						return ret;
				}

				int tlen = target.Length;
				int idx = 1;
				while (tlen-- > 0 && Char.IsLetter ((next = Next ()))) {
					char lc;
#if NET_2_0
					lc = Char.ToLowerInvariant (next);
#else
					lc = Char.ToLower (next);
#endif
					if (target [idx++] != lc) {
						pos = savedPos;
						return ret;
					}
					MoveNext ();
				}

				sb.Append (' ');
				sb.Append (target);
				ret = sb.ToString ();
			}

			return ret;
		}

		private int ParseIdentifier ()
		{
			string strToken = ReadIdentifier ();
			object tokenObj = tokenMap[strToken.ToLower()];

			if(tokenObj != null)
				return (int)tokenObj;
			
			val = strToken;
			return Token.Identifier;
		}

		private int ParseToken ()
		{
			char cur;
			switch (cur = Current ()) {
			case '(':
				return Token.PAROPEN;

			case ')':
				return Token.PARCLOSE;

			case '.':
				return Token.DOT;

			case ',':
				return Token.COMMA;

			case '+':
				return Token.PLUS;

			case '-':
				return Token.MINUS;

			case '*':
				return Token.MUL;

			case '/':
				return Token.DIV;

			case '%':
				return Token.MOD;
				
			case '=':
				return Token.EQ;

			case '<':
				return Token.LT;

			case '>':
				return Token.GT;

			case '[':
				val = ReadString (']');
				return Token.Identifier;

			case '#':
				string date = ReadString ('#');
				val = DateTime.Parse (date);
				return Token.DateLiteral;

			case '\'':
			case '\"':
				val = ReadString (cur, true);
				return Token.StringLiteral;

			default:
				if (Char.IsDigit (cur)) {				
					val = ReadNumber ();
					return Token.NumberLiteral;
				} else if (Char.IsLetter (cur) || cur == '_')
					return ParseIdentifier ();
				break;
			}
			throw new SyntaxErrorException ("invalid token: '" + cur + "'");
		}

		///////////////////////////
		// yyParser.yyInput methods
		///////////////////////////

		/** move on to next token.
		  @return false if positioned beyond tokens.
		  @throws IOException on input error.
		  */
		public bool advance ()
		{
			if (!SkipWhiteSpace())
				return false;
			tok = ParseToken();
			MoveNext ();
			return true;
		}

		/** classifies current token.
		  Should not be called if advance() returned false.
		  @return current %token or single character.
		  */
		public int token ()
		{
			return tok;
		}

		/** associated with current token.
		  Should not be called if advance() returned false.
		  @return value for token().
		  */
		public Object value ()
		{
			return val;
		}
	}
}
