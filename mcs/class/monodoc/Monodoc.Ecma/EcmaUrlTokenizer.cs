using System;
using System.Text;
using System.Globalization;

namespace Monodoc.Ecma
{
	public class EcmaUrlTokenizer : yyParser.yyInput
	{
		const char EndOfStream = (char)0;
		string input;
		object val;
		int current_token;
		int current_pos;
		int real_current_pos;
		int identCount = 0;

		public EcmaUrlTokenizer (string input)
		{
			this.input = input;
		}

		static bool is_identifier_start_character (char c)
		{
			return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_' || Char.IsLetter (c);
		}

		static bool is_identifier_part_character (char c)
		{
			if (c >= 'a' && c <= 'z')
				return true;

			if (c >= 'A' && c <= 'Z')
				return true;

			if (c == '_' || (c >= '0' && c <= '9'))
				return true;

			if (c < 0x80)
				return false;

			return Char.IsLetter (c) || Char.GetUnicodeCategory (c) == UnicodeCategory.ConnectorPunctuation;
		}

		public bool advance ()
		{
			return Peek () != EndOfStream;
		}

		public Object Value {
			get {
				return val;
			}
		}

		public Object value ()
		{
			return val;
		}

		public int token ()
		{
			int token = xtoken ();
			//Console.WriteLine ("Current token {0} with value {1}", token, val == null ? "(none)" : val.ToString ());
			if (token == Token.ERROR) {
				throw new Exception (string.Format ("Error at position {0} parsing url '{0}'",  current_pos, input));
			}
			current_token = token;
			return token;
		}

		int xtoken ()
		{
			char next = Read ();
			while (char.IsWhiteSpace (next))
				next = Read ();
			current_pos++;
			val = null;

			switch (next) {
			case ',':
				return Token.COMMA;
			case '.':
				return Token.DOT;
			case '{':
			case '<':
				return Token.OP_GENERICS_LT;
			case '}':
			case '>':
				return Token.OP_GENERICS_GT;
			case '`':
				return Token.OP_GENERICS_BACKTICK;
			case '(':
				return Token.OP_OPEN_PAREN;
			case ')':
				return Token.OP_CLOSE_PAREN;
			case '+':
				return Token.INNER_TYPE_SEPARATOR;
			case ':':
				return Token.COLON;
			case '/':
				return Token.SLASH_SEPARATOR;
			case '[':
				return Token.OP_ARRAY_OPEN;
			case ']':
				return Token.OP_ARRAY_CLOSE;
			case '*':
				return Token.STAR;
			case '&':
				return Token.REF_ARG;
			case '@':
				return Token.OUT_ARG;
			case '$':
				return Token.EXPLICIT_IMPL_SEP;
			default:
				return TokenizeIdentifierOrNumber (next);
			}
		}

		int TokenizeIdentifierOrNumber (char current)
		{
			// We must first return the expression type which is a uppercase letter and a colon
			if (current_pos < 2) {
				val = null;
				return (int)current;
			}

			if (is_identifier_start_character (current) || current == '*') {
				unsafe {
					// identifier length is artificially limited to 1024 bytes by implementations
					char* pIdent = stackalloc char[512];
					*pIdent = current;
					identCount = 1;

					char peek;
					while ((peek = Peek ()) != EndOfStream && is_identifier_part_character (peek)) {
						*(pIdent + identCount) = Read ();
						++current_pos;
						++identCount;
					}

					val = new string ((char*)pIdent, 0, identCount);
					return Token.IDENTIFIER;
				}
			} else if (char.IsDigit (current)) {
				val = current - '0';
				return Token.DIGIT;
			} else {
				val = null;
				return Token.ERROR;
			}
		}

		char Read ()
		{
			try {
				return input[real_current_pos++];
			} catch {
				return EndOfStream;
			}
		}

		char Peek ()
		{
			try {
				return input[real_current_pos];
			} catch {
				return EndOfStream;
			}
		}
	}
}
