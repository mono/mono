//
// System.Xml.XPath.Tokenizer
//
// Author:
//   Piers Haken (piersh@friskit.com)
//
// (C) 2002 Piers Haken
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
using System.Globalization;
using System.IO;
using System.Text;
using System.Collections;
using Mono.Xml.XPath;
using Mono.Xml.XPath.yyParser;

namespace System.Xml.XPath
{
	internal class Tokenizer : Mono.Xml.XPath.yyParser.yyInput
	{
		private string m_rgchInput;
		private int m_ich;
		private int m_cch;
		private int m_iToken;
		private int m_iTokenPrev = Token.EOF;
		private Object m_objToken;
		private bool m_fPrevWasOperator = false;
		private bool m_fThisIsOperator = false;
		private static readonly Hashtable s_mapTokens = new Hashtable ();
		private static readonly Object [] s_rgTokenMap =
		{
		   Token.AND, "and",
		   Token.OR, "or",
		   Token.DIV, "div",
		   Token.MOD, "mod",
		   Token.ANCESTOR, "ancestor",
		   Token.ANCESTOR_OR_SELF, "ancestor-or-self",
		   Token.ATTRIBUTE, "attribute",
		   Token.CHILD, "child",
		   Token.DESCENDANT, "descendant",
		   Token.DESCENDANT_OR_SELF, "descendant-or-self",
		   Token.FOLLOWING, "following",
		   Token.FOLLOWING_SIBLING, "following-sibling",
		   Token.NAMESPACE, "namespace",
		   Token.PARENT, "parent",
		   Token.PRECEDING, "preceding",
		   Token.PRECEDING_SIBLING, "preceding-sibling",
		   Token.SELF, "self",
		   Token.COMMENT, "comment",
		   Token.TEXT, "text",
		   Token.PROCESSING_INSTRUCTION, "processing-instruction",
		   Token.NODE, "node",
		};
		private const char EOL = '\0';

		static Tokenizer ()
		{
			for (int i = 0; i < s_rgTokenMap.Length; i += 2)
				s_mapTokens.Add (s_rgTokenMap [i + 1], s_rgTokenMap [i]);
		}

		public Tokenizer (string strInput)
		{
			//Console.WriteLine ("Tokenizing: " + strInput);
			m_rgchInput = strInput;
			m_ich = 0;
			m_cch = strInput.Length;
			SkipWhitespace ();
		}

		private char Peek (int iOffset)
		{
			if (m_ich + iOffset>= m_cch)
				return EOL;
			return m_rgchInput [m_ich + iOffset];
		}

		private char Peek ()
		{
			return Peek (0);
		}

		private char GetChar ()
		{
			if (m_ich >= m_cch)
				return EOL;
			return m_rgchInput [m_ich++];
		}

		private char PutBack ()
		{
			if (m_ich == 0)
				throw new XPathException ("XPath parser returned an error status: invalid tokenizer state.");
			return m_rgchInput [--m_ich];
		}

		private bool SkipWhitespace ()	// returns trus if any whitespace was skipped
		{
			if (!IsWhitespace (Peek ()))
				return false;
					
			while (IsWhitespace (Peek ()))
				GetChar ();

			return true;
		}

		private int ParseNumber ()
		{
			StringBuilder sb = new StringBuilder ();

			while (IsDigit (Peek ()))
				sb.Append ((char) GetChar ());

			// don't handle '3.' as an error case (it is not. XPath 3.7 syntax [30])
			if (Peek () == '.')
			{
				sb.Append ((char) GetChar ());
				while (IsDigit (Peek ()))
					sb.Append ((char) GetChar ());
			}
			m_objToken = Double.Parse (sb.ToString (), NumberFormatInfo.InvariantInfo);
			return Token.NUMBER;
		}

		private int ParseLiteral ()
		{
			StringBuilder sb = new StringBuilder ();

			char chInit = GetChar ();
			char ch;
			while ((ch = Peek ()) != chInit)
			{
				if (ch == EOL)
					throw new XPathException ("unmatched "+chInit+" in expression");
				sb.Append ((char) GetChar ());
			}
			GetChar ();
			m_objToken = sb.ToString ();
			return Token.LITERAL;
		}

		private string ReadIdentifier ()
		{
			StringBuilder sb = new StringBuilder ();

			char ch = Peek ();
			if (!Char.IsLetter (ch) && ch != '_')
				return null;

			sb.Append ((char) GetChar ());

			while ((ch = Peek ()) == '_' || ch == '-' || ch == '.' || Char.IsLetterOrDigit (ch))
				sb.Append ((char) GetChar ());

			SkipWhitespace ();
			return sb.ToString ();
		}

		private int ParseIdentifier ()
		{
			string strToken = ReadIdentifier ();
			Object objToken = s_mapTokens [strToken];

			int iToken = (objToken != null) ? (int) objToken : Token.QName;
			m_objToken = strToken;

			char ch = Peek ();
			if (ch == ':')
			{
				if (Peek (1) == ':')
				{
					// If the two characters following an NCName (possibly
					// after intervening ExprWhitespace) are ::, then the
					// token must be recognized as an AxisName.
					if (objToken == null || !IsAxisName (iToken))
						throw new XPathException ("invalid axis name: '"+strToken+"'");
					return iToken;
				}

				GetChar ();
				SkipWhitespace ();
				ch = Peek ();

				if (ch == '*')
				{
					GetChar ();
					m_objToken = new XmlQualifiedName ("", strToken);
					return Token.QName;
				}
				string strToken2 = ReadIdentifier ();
				if (strToken2 == null)
					throw new XPathException ("invalid QName: "+strToken+":"+(char)ch);

				ch = Peek ();
				m_objToken = new XmlQualifiedName (strToken2, strToken);
				if (ch == '(')
					return Token.FUNCTION_NAME;
				return Token.QName;
			}

			// If there is a preceding token and the preceding
			// token is not one of @, ::, (, [, , or an Operator,
			// then a * must be recognized as a MultiplyOperator
			// and an NCName must be recognized as an OperatorName.
			if (!IsFirstToken && !m_fPrevWasOperator)
			{
				if (objToken == null || !IsOperatorName (iToken))
					throw new XPathException ("invalid operator name: '"+strToken+"'");
				return iToken;
			}

			if (ch == '(')
			{
				// If the character following an NCName (possibly
				// after intervening ExprWhitespace) is (, then the
				// token must be recognized as a NodeType or a FunctionName.
				if (objToken == null)
				{
					m_objToken = new XmlQualifiedName (strToken, "");
					return Token.FUNCTION_NAME;
				}
				if (IsNodeType (iToken))
					return iToken;
				throw new XPathException ("invalid function name: '"+strToken+"'");
			}

			m_objToken = new XmlQualifiedName (strToken, "");
			return Token.QName;
		}

		private static bool IsWhitespace (char ch)
		{
			// return Char.IsWhiteSpace (ch);
			return (ch == ' ' || ch == '\t' || ch == '\n' || ch == '\r');
		}

		private static bool IsDigit (char ch)
		{
			// return Char.IsDigit (ch);
			return ch >= '0' && ch <= '9';
		}


		int ParseToken ()
		{
			char ch = Peek ();
			switch (ch)
			{
				case EOL:
					return Token.EOF;

				case '/':
					m_fThisIsOperator = true;
					GetChar ();
					if (Peek () == '/')
					{
						GetChar ();
						return Token.SLASH2;
					}
					return Token.SLASH;

				case '.':
					GetChar ();
					if (Peek () == '.')
					{
						GetChar ();
						return Token.DOT2;
					}
					else if (IsDigit (Peek ()))
					{
						PutBack ();
						return ParseNumber ();
					}
					return Token.DOT;

				case ':':
					GetChar ();
					if (Peek () == ':')
					{
						m_fThisIsOperator = true;
						GetChar ();
						return Token.COLON2;
					}
					return Token.ERROR;

				case ',':
					m_fThisIsOperator = true;
					GetChar ();
					return Token.COMMA;

				case '@':
					m_fThisIsOperator = true;
					GetChar ();
					return Token.AT;

				case '[':
					m_fThisIsOperator = true;
					GetChar ();
					return Token.BRACKET_OPEN;

				case ']':
					GetChar ();
					return Token.BRACKET_CLOSE;

				case '(':
					m_fThisIsOperator = true;
					GetChar ();
					return Token.PAREN_OPEN;

				case ')':
					GetChar ();
					return Token.PAREN_CLOSE;

				case '+':
					m_fThisIsOperator = true;
					GetChar ();
					return Token.PLUS;

				case '-':
					m_fThisIsOperator = true;
					GetChar ();
					return Token.MINUS;

				case '*':
					GetChar ();
					if (!IsFirstToken && !m_fPrevWasOperator)
					{
						m_fThisIsOperator = true;
						return Token.MULTIPLY;
					}
					return Token.ASTERISK;

				case '$':
					GetChar ();
					m_fThisIsOperator = true;
					return Token.DOLLAR;

				case '|':
					m_fThisIsOperator = true;
					GetChar ();
					return Token.BAR;

				case '=':
					m_fThisIsOperator = true;
					GetChar ();
					return Token.EQ;

				case '!':
					GetChar ();
					if (Peek () == '=')
					{
						m_fThisIsOperator = true;
						GetChar ();
						return Token.NE;
					}
					break;

				case '>':
					m_fThisIsOperator = true;
					GetChar ();
					if (Peek () == '=')
					{
						GetChar ();
						return Token.GE;
					}
					return Token.GT;

				case '<':
					m_fThisIsOperator = true;
					GetChar ();
					if (Peek () == '=')
					{
						GetChar ();
						return Token.LE;
					}
					return Token.LT;

				case '\'':
					return ParseLiteral ();

				case '\"':
					return ParseLiteral ();

				default:
					if (IsDigit (ch))
					{
						return ParseNumber ();
					}
					else if (Char.IsLetter (ch) || ch == '_')	 // NCName
					{
						int iToken = ParseIdentifier ();
						if (IsOperatorName (iToken))
							m_fThisIsOperator = true;
						return iToken;
					}
					break;
			}
			throw new XPathException ("invalid token: '"+ch+"'");
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
			m_fThisIsOperator = false;
			m_objToken = null;
			m_iToken = ParseToken ();
			SkipWhitespace ();
			m_iTokenPrev = m_iToken;
			m_fPrevWasOperator = m_fThisIsOperator;
			return (m_iToken != Token.EOF);
		}

		/** classifies current token.
		  Should not be called if advance() returned false.
		  @return current %token or single character.
		  */
		public int token ()
		{
			return m_iToken;
		}

		/** associated with current token.
		  Should not be called if advance() returned false.
		  @return value for token().
		  */
		public Object value ()
		{
			return m_objToken;
		}
		private bool IsFirstToken { get { return m_iTokenPrev == Token.EOF; } }

		private bool IsNodeType (int iToken)
		{
			switch (iToken)
			{
				case Token.COMMENT:
				case Token.TEXT:
				case Token.PROCESSING_INSTRUCTION:
				case Token.NODE:
					return true;
				default:
					return false;
			}
		}
		private bool IsOperatorName (int iToken)
		{
			switch (iToken)
			{
				case Token.AND:
				case Token.OR:
				case Token.MOD:
				case Token.DIV:
					return true;
				default:
					return false;
			}
		}
		private bool IsAxisName (int iToken)
		{
			switch (iToken)
			{
				case Token.ATTRIBUTE:
				case Token.ANCESTOR:
				case Token.ANCESTOR_OR_SELF:
				case Token.CHILD:
				case Token.DESCENDANT:
				case Token.DESCENDANT_OR_SELF:
				case Token.FOLLOWING:
				case Token.FOLLOWING_SIBLING:
				case Token.NAMESPACE:
				case Token.PARENT:
				case Token.PRECEDING:
				case Token.PRECEDING_SIBLING:
				case Token.SELF:
					return true;
				default:
					return false;
			}
		}
	}
}
