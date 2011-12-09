//
// Lexer.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2011 Xamarin Inc (http://www.xamarin.com)
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

namespace System.Net.Http.Headers
{
	struct Token
	{
		public enum Type
		{
			Error,
			End,
			Token,
			QuotedString,
			SeparatorEqual,
			SeparatorSemicolon,
			SeparatorSlash,
		}

		readonly Type type;

		public Token (Type type, int startPosition, int endPosition)
			: this ()
		{
			this.type = type;
			StartPosition = startPosition;
			EndPosition = endPosition;
		}

		public int StartPosition { get; private set; }
		public int EndPosition { get; private set; }

		public Type Kind {
			get {
				return type;
			}
		}

		public static implicit operator Token.Type (Token token)
		{
			return token.type;
		}

		public override string ToString ()
		{
			return type.ToString ();
		}
	}

	class Lexer
	{
		static readonly bool[] token_chars = {
				false, false, false, false, false, false, false, false, false, false, false, false, false, false,
				false, false, false, false, false, false, false, false, false, false, false, false, false, false,
				false, false, false, false, false, true, false, true, true, true, true, false, false, false, true,
				true, false, true, true, false, true, true, true, true, true, true, true, true, true, true, false,
				false, false, false, false, false, false, true, true, true, true, true, true, true, true, true,
				true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true,
				false, false, false, true, true, true, true, true, true, true, true, true, true, true, true, true,
				true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true,
				false, true, false
			};

		static readonly int last_token_char = token_chars.Length;

		readonly string s;
		int pos;

		public Lexer (string stream)
		{
			this.s = stream;
		}

		public string GetStringValue (Token token)
		{
			return s.Substring (token.StartPosition, token.EndPosition - token.StartPosition);
		}

		public static bool IsValidToken (string input)
		{
			int i = 0;
			//
			// any CHAR except CTLs or separator
			//
			for (; i < input.Length; ++i) {
				char s = input[i];
				if (s > last_token_char || !token_chars[s])
					return false;
			}

			return i > 0;
		}

		public Token Scan ()
		{
			int start = pos;
			if (s == null)
				return new Token (Token.Type.Error, 0, 0);

			Token.Type ttype;
			if (pos >= s.Length) {
				ttype = Token.Type.End;
			} else {
				ttype = Token.Type.Error;
			start:
				char ch = s[pos++];
				switch (ch) {
				case ' ':
				case '\t':
					if (pos == s.Length) {
						ttype = Token.Type.End;
						break;
					}

					goto start;
				case '=':
					ttype = Token.Type.SeparatorEqual;
					break;
				case ';':
					ttype = Token.Type.SeparatorSemicolon;
					break;
				case '/':
					ttype = Token.Type.SeparatorSlash;
					break;
				case '"':
					// Quoted string
					start = pos - 1;
					while (pos < s.Length) {
						ch = s[pos];
						if (ch == '"') {
							++pos;
							ttype = Token.Type.QuotedString;
							break;
						}

						// any OCTET except CTLs, but including LWS
						if (ch < 32 || ch > 126)
							break;

						++pos;
					}

					break;
				case '(':
					break;
				default:
					if (ch <= last_token_char && token_chars[ch]) {
						start = pos - 1;

						ttype = Token.Type.Token;
						while (pos < s.Length) {
							ch = s[pos];
							if (ch > last_token_char || !token_chars[ch]) {
								break;
							}

							++pos;
						}
					}

					break;
				}
			}

			return new Token (ttype, start, pos);
		}
	}
}
