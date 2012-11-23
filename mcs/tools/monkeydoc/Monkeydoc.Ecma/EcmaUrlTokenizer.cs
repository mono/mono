using System;
using System.IO;
using System.Text;
using System.Globalization;

namespace Monkeydoc.Ecma
{
	public class EcmaUrlTokenizer : yyParser.yyInput
	{
		TextReader input;
		object val;
		int current_token;
		int current_pos;
		StringBuilder ident = new StringBuilder (20);

		public EcmaUrlTokenizer (TextReader input)
		{
			this.input = input;
		}

		static bool is_identifier_start_character (int c)
		{
			return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_' || Char.IsLetter ((char)c);
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
			return input.Peek () != -1;
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
			if (token == Token.ERROR)
				Console.WriteLine ("Problem at pos {0} after token {1}", current_pos, current_token);
			current_token = token;
			return token;
		}

		int xtoken ()
		{
			char next = (char)input.Read ();
			while (char.IsWhiteSpace (next))
				next = (char)input.Read ();
			current_pos++;
			val = null;

			switch (next) {
			case ',':
				return Token.COMMA;
			case '.':
				return Token.DOT;
			case '<':
				return Token.OP_GENERICS_LT;
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
				ident.Clear ();
				ident.Append (current);
				int peek;

				while ((peek = input.Peek ()) != -1 && is_identifier_part_character ((char)peek)) {
					ident.Append ((char)input.Read ());
					current_pos++;
				}

				val = ident.ToString ();
				return Token.IDENTIFIER;
			} else if (char.IsDigit (current)) {
				val = current - '0';
				return Token.DIGIT;
			} else {
				val = null;
				return Token.ERROR;
			}
		}
	}
}
