//
// System.Xml.XPath.Tokenizer
//
// Author:
//   Piers Haken (piersh@friskit.com)
//
// (C) 2002 Piers Haken
//
using System;
using System.IO;
using System.Text;
using System.Collections;
using Mono.Xml.XPath;
using Mono.Xml.XPath.yyParser;

namespace System.Xml.XPath
{
	internal class Tokenizer : Mono.Xml.XPath.yyParser.yyInput
	{
		private char [] m_rgchInput;
		private int m_ich;
		private int m_cch;
//		private System.IO.StreamReader m_input;
		private int m_iToken;
		private Object m_objToken;
		private static Hashtable m_mapTokens = new Hashtable ();
		private static readonly Object [] rgTokenMap =
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

		static Tokenizer ()
		{
			for (int i = 0; i < rgTokenMap.Length; i += 2)
				m_mapTokens.Add (rgTokenMap [i + 1], rgTokenMap [i]);
		}

		public Tokenizer (string strInput)
		{
			m_rgchInput = strInput.ToCharArray ();
			m_ich = 0;
			m_cch = strInput.Length;
			SkipWhitespace ();
		}

		private int Peek ()
		{
			if (m_ich >= m_cch)
				return -1;
			return m_rgchInput [m_ich];
		}

		private int GetChar ()
		{
			if (m_ich >= m_cch)
				return -1;
			return m_rgchInput [m_ich++];
		}

		private int PutBack ()
		{
			if (m_ich == 0)
				throw new XPathException ("invalid tokenizer state");	// TODO: better description
			return m_rgchInput [--m_ich];
		}

		private void SkipWhitespace ()
		{
			while (IsWhitespace (Peek ()))
				GetChar ();
		}

		[MonoTODO]
		private int ParseNumber ()
		{
			StringBuilder sb = new StringBuilder ();

			while (IsDigit (Peek ()))
				sb.Append ((char) GetChar ());

			// TODO: doesn't handle '3.' error case
			if (Peek () == '.')
			{
				sb.Append ((char) GetChar ());
				while (IsDigit (Peek ()))
					sb.Append ((char) GetChar ());
			}
			m_objToken = Double.Parse (sb.ToString ());
			return Token.NUMBER;
		}

		private int ParseLiteral ()
		{
			StringBuilder sb = new StringBuilder ();

			int chInit = GetChar ();
			int ch;
			while ((ch = Peek ()) != chInit)
			{
				if (ch == -1)
					return Token.ERROR;
				sb.Append ((char) GetChar ());
			}
			GetChar ();
			m_objToken = sb.ToString ();
			return Token.LITERAL;
		}

		private int ParseIdentifier ()
		{
			StringBuilder sb = new StringBuilder ();

			// FIXME: if it may be NCName, then many other characters should be allowed (e.g. unicode multibyte character).
			while (true)
			{
				int ch = Peek ();
				if (ch == '_' || ch == '-' ||
						(ch >= 'a' && ch <= 'z') ||
						(ch >= 'A' && ch <= 'Z') ||
						(ch >= '0' && ch <= '9'))
				{
					sb.Append ((char) GetChar ());
				}
				else
					break;
			}
			String strToken = sb.ToString ();
			Object objToken = m_mapTokens [strToken];
			if (objToken != null)
			{
				return (int) objToken;
			}
			else
			{
				m_objToken = strToken;

				SkipWhitespace ();
				if (Peek () == '(')					
					return Token.FUNCTION_NAME;
				return Token.NCName;
			}
		}

		private static bool IsWhitespace (int ch)
		{
			return (ch == ' ' || ch == '\t' || ch == '\n' || ch == '\r');
		}

		private static bool IsDigit (int ch)
		{
			return ch >= '0' && ch <= '9';
		}


		int ParseToken ()
		{
			int ch = Peek ();
			switch (ch)
			{
				case -1:
					return Token.EOF;

				case '/':
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
						GetChar ();
						return Token.COLON2;
					}
					return Token.COLON;

				case ',':
					GetChar ();
					return Token.COMMA;

				case '@':
					GetChar ();
					return Token.AT;

				case '[':
					GetChar ();
					return Token.BRACKET_OPEN;

				case ']':
					GetChar ();
					return Token.BRACKET_CLOSE;

				case '(':
					GetChar ();
					return Token.PAREN_OPEN;

				case ')':
					GetChar ();
					return Token.PAREN_CLOSE;

				case '+':
					GetChar ();
					return Token.PLUS;

				case '-':
					GetChar ();
					return Token.MINUS;

				case '*':
					GetChar ();
					return Token.ASTERISK;

				case '$':
					GetChar ();
					return Token.DOLLAR;

				case '|':
					GetChar ();
					return Token.BAR;

				case '=':
					GetChar ();
					return Token.EQ;

				case '!':
					GetChar ();
					if (Peek () == '=')
					{
						GetChar ();
						return Token.NE;
					}
					break;

				case '>':
					GetChar ();
					if (Peek () == '=')
					{
						GetChar ();
						return Token.GE;
					}
					return Token.GT;

				case '<':
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
					{
						if (IsDigit (ch))
						{
							return ParseNumber ();
						}
						else
						{
							return ParseIdentifier ();
						}
					}
			}
			return Token.ERROR;
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
			m_objToken = null;
			m_iToken = ParseToken ();
			SkipWhitespace ();
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
	}
}
