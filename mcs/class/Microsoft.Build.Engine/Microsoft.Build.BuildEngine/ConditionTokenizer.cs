//
// ConditionTokenizer.cs
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//   Jaroslaw Kowalski <jaak@jkowalski.net>
// 
// (C) 2006 Marek Sieradzki
// (C) 2004-2006 Jaroslaw Kowalski
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

#if NET_2_0

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace Microsoft.Build.BuildEngine {

	internal sealed class ConditionTokenizer {
		string	inputString = null;
		int	position = 0;
		int	tokenPosition = 0;
		
		Token	token;
		
		bool	ignoreWhiteSpace = true;
		
		static TokenType[] charIndexToTokenType = new TokenType[128];
		static Hashtable keywordToTokenType = CollectionsUtil.CreateCaseInsensitiveHashtable ();

		static ConditionTokenizer ()
		{
			for (int i = 0; i < 128; i++)
				charIndexToTokenType [i] = TokenType.Invalid;
			
			foreach (CharToTokenType cht in charToTokenType)
				charIndexToTokenType [(int) cht.ch] = cht.tokenType;
			
		}
		
		public ConditionTokenizer ()
		{
			this.ignoreWhiteSpace = true;
		}
		
		public void Tokenize (string s)
		{
			this.inputString = s;
			this.position = 0;
			this.token = new Token (null, TokenType.BOF);

			GetNextToken ();
		}
		
		private void SkipWhiteSpace ()
		{
			int ch;
			
			while ((ch = PeekChar ()) != -1) {
				if (!Char.IsWhiteSpace ((char)ch))
					break;
				ReadChar ();
			}
		}
		
		private int PeekChar ()
		{
			if (position < inputString.Length)
				return (int) inputString [position];
			else
				return -1;
		}
		
		private int ReadChar ()
		{
			if (position < inputString.Length)
				return (int) inputString [position++];
			else
				return -1;
		}
		
		public void Expect (TokenType type)
		{
			if (token.Type != type)
				throw new ExpressionParseException ("Expected token type of type: " + type + ", got " + token.Type +
					" (" + token.Value + ") .");
			
			GetNextToken ();
		}
		
		public bool IsEOF ()
		{
			return token.Type == TokenType.EOF;
		}
		
		public bool IsNumber ()
		{
			return token.Type == TokenType.Number;
		}
		
		public bool IsToken (TokenType type)
		{
			return token.Type == type;
		}
		
		public bool IsPunctation ()
		{
			return (token.Type >= TokenType.FirstPunct && token.Type < TokenType.LastPunct);
		}
		
		// FIXME: add 'and' and 'or' tokens
		public void GetNextToken ()
		{
			if (token.Type == TokenType.EOF)
				throw new ExpressionParseException ("Cannot read past the end of stream.");
			
			if (IgnoreWhiteSpace)
				SkipWhiteSpace ();
			
			tokenPosition = position;
			
			int i = PeekChar ();
			
			if (i == -1) {
				token = new Token (null, TokenType.EOF);
				return;
			}
			
			char ch = (char) i;
			
			if (IgnoreWhiteSpace == false && Char.IsWhiteSpace (ch)) {
				StringBuilder sb = new StringBuilder ();
				int ch2;

				while ((ch2 = PeekChar ()) != -1)  {
					if (!Char.IsWhiteSpace ((char) ch2))
						break;

					sb.Append ((char)ch2);
					ReadChar();
				}
				
				token = new Token (sb.ToString (), TokenType.WhiteSpace);
				return;
			}
			
			if (Char.IsDigit (ch)) {
				StringBuilder sb = new StringBuilder ();
				
				sb.Append (ch);
				ReadChar ();
				
				while ((i = PeekChar ()) != -1) {
					ch = (char) i;
					
					if (Char.IsDigit (ch) || ch == '.')
						sb.Append ((char) ReadChar ());
					else
						break;
				}
				
				token = new Token (sb.ToString (), TokenType.Number);
				return;
			}
			
			if (ch == '\'') {
				StringBuilder sb = new StringBuilder ();
				string temp;
				
				sb.Append (ch);
				ReadChar ();
				
				while ((i = PeekChar ()) != -1) {
					ch = (char) i;
					
					sb.Append ((char) ReadChar ());
					
					if (ch == '\'')
						break;
				}
				
				temp = sb.ToString ();
				
				// FIXME: test extreme cases
				token = new Token (temp.Substring (1, temp.Length - 2), TokenType.String);
				
				return;
			}
			
			if (ch == '_' || Char.IsLetter (ch)) {
				StringBuilder sb = new StringBuilder ();
				
				sb.Append ((char) ch);
				ReadChar ();
				
				while ((i = PeekChar ()) != -1) {
					if ((char) i == '_' || Char.IsLetterOrDigit ((char) i))
						sb.Append ((char) ReadChar ());
					else
						break;
				}
				
				string temp = sb.ToString ();
				
				if (temp.ToLower () == "and")
					token = new Token (temp, TokenType.And);
				else if (temp.ToLower () == "or")
					token = new Token (temp, TokenType.Or);
				else
					token = new Token (sb.ToString (), TokenType.String);
				return;
			}
			
			ReadChar ();
			
			if (ch == '!' && PeekChar () == (int) '=') {
				token = new Token ("!=", TokenType.NotEqual);
				ReadChar ();
				return;
			}
			
			if (ch == '<' && PeekChar () == (int) '=') {
				token = new Token ("<=", TokenType.LessOrEqual);
				ReadChar ();
				return;
			}
			
			if (ch == '>' && PeekChar () == (int) '=') {
				token = new Token (">=", TokenType.GreaterThanOrEqual);
				ReadChar ();
				return;
			}
			
			if (ch == '=' && PeekChar () == (int) '=') {
				token = new Token ("==", TokenType.Equal);
				ReadChar ();
				return;
			}
			
			if (ch == '-' && PeekChar () == (int) '>') {
				token = new Token ("->", TokenType.Transform);
				ReadChar ();
				return;
			}
			
			if (ch >= 32 && ch < 128) {
				if (charIndexToTokenType [ch] != TokenType.Invalid) {
					token = new Token (new String (ch, 1), charIndexToTokenType [ch]);
					return;
				} else
					throw new ExpressionParseException (String.Format ("Invalid punctuation: {0}", ch));
			}
			
			throw new ExpressionParseException (String.Format ("Invalid token: {0}", ch));
		}
		
		public int TokenPosition {
			get { return tokenPosition; }
		}
		
		public Token Token {
			get { return token; }
		}
		
		public bool IgnoreWhiteSpace {
			get { return ignoreWhiteSpace; }
			set { ignoreWhiteSpace = value; }
		}
		
		struct CharToTokenType {
			public char ch;
			public TokenType tokenType;
			
			public CharToTokenType (char ch, TokenType tokenType)
			{
				this.ch = ch;
				this.tokenType = tokenType;
			}
		}
		
		static CharToTokenType[] charToTokenType = {
			new CharToTokenType ('<', TokenType.LessThan),
			new CharToTokenType ('>', TokenType.GreaterThan),
			new CharToTokenType ('=', TokenType.Equal),
			new CharToTokenType ('(', TokenType.LeftParen),
			new CharToTokenType (')', TokenType.RightParen),
			new CharToTokenType ('.', TokenType.Dot),
			new CharToTokenType (',', TokenType.Comma),
			new CharToTokenType ('!', TokenType.Not),
			new CharToTokenType ('@', TokenType.Item),
			new CharToTokenType ('$', TokenType.Property),
			new CharToTokenType ('%', TokenType.Metadata),
			new CharToTokenType ('\'', TokenType.Apostrophe),
		};
	}
}

#endif
