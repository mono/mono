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
		private int m_iToken;
		private Object m_objToken;
		private bool m_fPrevWasSpecial = false;
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
		private static readonly Hashtable s_mapfPrevWasSpecial = new Hashtable ();
		private static readonly int [] s_rgfPrevWasSpecial =
		{
			Token.AT,
			Token.COLON2,
			Token.PAREN_OPEN,
			Token.BRACKET_OPEN,
			Token.COMMA,

			Token.AND,
			Token.OR,
			Token.DIV,
			Token.MOD,

			Token.SLASH,
			Token.SLASH2,
			Token.BAR,
			Token.PLUS,
			Token.MINUS,
			Token.EQ,
			Token.NE,
			Token.LE,
			Token.LT,
			Token.GE,
			Token.GT,

			Token.ASTERISK,
		};
		private const char EOL = '\0';

		static Tokenizer ()
		{
			for (int i = 0; i < s_rgTokenMap.Length; i += 2)
				s_mapTokens.Add (s_rgTokenMap [i + 1], s_rgTokenMap [i]);
			object objTmp = new Object ();
			for (int i = 0; i < s_rgfPrevWasSpecial.Length; i++)
				s_mapfPrevWasSpecial.Add (s_rgfPrevWasSpecial [i], null);
		}

		public Tokenizer (string strInput)
		{
			m_rgchInput = strInput.ToCharArray ();
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
				throw new XPathException ("invalid tokenizer state");	// TODO: better description
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

			char chInit = GetChar ();
			char ch;
			while ((ch = Peek ()) != chInit)
			{
				if (ch == EOL)
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

			char ch;
			while ((ch = Peek ()) == '_' || ch == '-' || ch == '.' || Char.IsLetterOrDigit (ch))
				sb.Append ((char) GetChar ());

			String strToken = sb.ToString ();
			Object objToken = s_mapTokens [strToken];

			if (!m_fPrevWasSpecial && objToken != null)
				return (int) objToken;

			SkipWhitespace ();

			ch = Peek ();
			if (ch == '(')					
			{
				if (objToken != null)
					return (int) objToken;
				m_objToken = strToken;
				return Token.FUNCTION_NAME;
			}
			else if (ch == ':' && Peek (1) == ':')
			{
				if (objToken != null)
					return (int) objToken;
			}

			m_objToken = strToken;
			return Token.NCName;
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
						else if (Char.IsLetter (ch) || ch == '_')	 // NCName
						{
							return ParseIdentifier ();
						}
						break;
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
			bool fWhitespace = SkipWhitespace ();
			m_fPrevWasSpecial = (!fWhitespace && s_mapfPrevWasSpecial.Contains (m_iToken));
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
