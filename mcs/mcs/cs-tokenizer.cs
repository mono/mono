//
// cs-tokenizer.cs: The Tokenizer for the C# compiler
//
// Author: Miguel de Icaza (miguel@gnu.org)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)
//

/*
  Todo:

  Do something with the integer and float suffixes, pass full datatype?
  Make sure we accept the proper Unicode ranges, per the spec.

  Open issues:

  * Data type handling
  
	  Currently I am returning different tokens for the various
	  kinds of floating point types (float, double, decimal) and I
	  am only returning a single token for all integer values
	  (integer, unsigned int, etc) as an experiment as to see
	  which mechanism is better.
	
	  I do not know yet how I will be doing the mapping of "int"
	  to things like System.Int32 and so on.  I am confused.  MAN
	  I AM C
	
	  Indeed, this might be the core of the problem, I should
	  *probably* just return a TYPE token and have the value of
	  the token be stuff like `System.Int32', `System.UInt32',
	  `System.Double' and so on.  I will see.

  * Error reporting.

          I was returning Token.ERROR on errors and setting an
          internal error string with the details, but it might make sense
	  to just use exceptions.

	  Change of mind: I think I want to keep returning errors *UNLESS* the
	  parser is catching errors from the tokenizer (at that point, there is
	  not really any reason to use exceptions) so that I can continue the
	  parsing 

  * IDEA

          I think I have solved the problem.  The idea is to not even *bother*
	  about handling data types a lot here (except for fitting data into
	  the proper places), but let the upper layer handle it.

	  Ie, treat LITERAL_CHARACTER, LITERAL_INTEGER, LITERAL_FLOAT, LITERAL_DOUBLE, and
	  return then as `LITERAL_LITERAL' with maybe subdetail information

*/

using System;
using System.Text;
using System.Collections;
using System.IO;
using System.Globalization;

namespace CIR
{
	/// <summary>
	///    Tokenizer for C# source code. 
	/// </summary>
	
	public class Tokenizer : yyParser.yyInput
	{
		StreamReader reader;
		public string ref_name;
		public int ref_line = 1;
		public int line = 1;
		public int col = 1;
		public int current_token;
		bool handle_get_set = false;

		public string location {
			get {
				string det;

				if (current_token == Token.ERROR)
					det = "detail: " + error_details;
				else
					det = "";
				
				return "Line:     "+line+" Col: "+col + "\n" +
				       "VirtLine: "+ref_line +
				       " Token: "+current_token + " " + det;
			}
		}

		public bool properties {
			get {
				return handle_get_set;
			}

			set {
				handle_get_set = value;
			}
                }
		
		//
		// Class variables
		// 
		static Hashtable keywords;
		static NumberStyles styles;
		static NumberFormatInfo csharp_format_info;
		
		//
		// Values for the associated token returned
		//
		System.Text.StringBuilder number;
		int putback_char;
		Object val;
		
		//
		// Details about the error encoutered by the tokenizer
		//
		string error_details;
		
		public string error {
			get {
				return error_details;
			}
		}
		
		public int Line {
			get {
				return line;
			}
		}

		public int Col {
			get {
				return col;
			}
		}
		
		static void initTokens ()
		{
			keywords = new Hashtable ();

			keywords.Add ("abstract", Token.ABSTRACT);
			keywords.Add ("as", Token.AS);
			keywords.Add ("base", Token.BASE);
			keywords.Add ("bool", Token.BOOL);
			keywords.Add ("break", Token.BREAK);
			keywords.Add ("byte", Token.BYTE);
			keywords.Add ("case", Token.CASE);
			keywords.Add ("catch", Token.CATCH);
			keywords.Add ("char", Token.CHAR);
			keywords.Add ("checked", Token.CHECKED);
			keywords.Add ("class", Token.CLASS);
			keywords.Add ("const", Token.CONST);
			keywords.Add ("continue", Token.CONTINUE);
			keywords.Add ("decimal", Token.DECIMAL);
			keywords.Add ("default", Token.DEFAULT);
			keywords.Add ("delegate", Token.DELEGATE);
			keywords.Add ("do", Token.DO);
			keywords.Add ("double", Token.DOUBLE);
			keywords.Add ("else", Token.ELSE);
			keywords.Add ("enum", Token.ENUM);
			keywords.Add ("event", Token.EVENT);
			keywords.Add ("explicit", Token.EXPLICIT);
			keywords.Add ("extern", Token.EXTERN);
			keywords.Add ("false", Token.FALSE);
			keywords.Add ("finally", Token.FINALLY);
			keywords.Add ("fixed", Token.FIXED);
			keywords.Add ("float", Token.FLOAT);
			keywords.Add ("for", Token.FOR);
			keywords.Add ("foreach", Token.FOREACH);
			keywords.Add ("goto", Token.GOTO);
			keywords.Add ("get", Token.GET);
			keywords.Add ("if", Token.IF);
			keywords.Add ("implicit", Token.IMPLICIT);
			keywords.Add ("in", Token.IN);
			keywords.Add ("int", Token.INT);
			keywords.Add ("interface", Token.INTERFACE);
			keywords.Add ("internal", Token.INTERNAL);
			keywords.Add ("is", Token.IS);
			keywords.Add ("lock ", Token.LOCK );
			keywords.Add ("long", Token.LONG);
			keywords.Add ("namespace", Token.NAMESPACE);
			keywords.Add ("new", Token.NEW);
			keywords.Add ("null", Token.NULL);
			keywords.Add ("object", Token.OBJECT);
			keywords.Add ("operator", Token.OPERATOR);
			keywords.Add ("out", Token.OUT);
			keywords.Add ("override", Token.OVERRIDE);
			keywords.Add ("params", Token.PARAMS);
			keywords.Add ("private", Token.PRIVATE);
			keywords.Add ("protected", Token.PROTECTED);
			keywords.Add ("public", Token.PUBLIC);
			keywords.Add ("readonly", Token.READONLY);
			keywords.Add ("ref", Token.REF);
			keywords.Add ("return", Token.RETURN);
			keywords.Add ("sbyte", Token.SBYTE);
			keywords.Add ("sealed", Token.SEALED);
			keywords.Add ("set", Token.SET);
			keywords.Add ("short", Token.SHORT);
			keywords.Add ("sizeof", Token.SIZEOF);
			keywords.Add ("static", Token.STATIC);
			keywords.Add ("string", Token.STRING);
			keywords.Add ("struct", Token.STRUCT);
			keywords.Add ("switch", Token.SWITCH);
			keywords.Add ("this", Token.THIS);
			keywords.Add ("throw", Token.THROW);
			keywords.Add ("true", Token.TRUE);
			keywords.Add ("try", Token.TRY);
			keywords.Add ("typeof", Token.TYPEOF);
			keywords.Add ("uint", Token.UINT);
			keywords.Add ("ulong", Token.ULONG);
			keywords.Add ("unchecked", Token.UNCHECKED);
			keywords.Add ("unsafe", Token.UNSAFE);
			keywords.Add ("ushort", Token.USHORT);
			keywords.Add ("using", Token.USING);
			keywords.Add ("virtual", Token.VIRTUAL);
			keywords.Add ("void", Token.VOID);
			keywords.Add ("while", Token.WHILE);
		}

		//
		// Class initializer
		// 
		static Tokenizer ()
		{
			initTokens ();
			csharp_format_info = new NumberFormatInfo ();
			csharp_format_info.CurrencyDecimalSeparator = ".";
			styles = NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint;
		}

		bool is_keyword (string name)
		{
			bool res;
			
			res = keywords.Contains (name);
			if ((name == "get" || name == "set") && handle_get_set == false)
				return false;
			return res;
		}

		int getKeyword (string name)
		{
			return (int) (keywords [name]);
		}
		
		public Tokenizer (System.IO.Stream input, string fname)
		{
			this.ref_name = fname;
			reader = new System.IO.StreamReader (input);
			putback_char = -1;
		}

		bool is_identifier_start_character (char c)
		{
			return Char.IsLetter (c) || c == '_' ;
		}

		bool is_identifier_part_character (char c)
		{
			return (Char.IsLetter (c) || Char.IsDigit (c) || c == '_');
		}

		int is_punct (char c, ref bool doread)
		{
			int idx = "{}[](),:;~+-*/%&|^!=<>?".IndexOf (c);
			int d;
			int t;

			doread = false;

			switch (c){
			case '{':
				return Token.OPEN_BRACE;
			case '}':
				return Token.CLOSE_BRACE;
			case '[':
				return Token.OPEN_BRACKET;
			case ']':
				return Token.CLOSE_BRACKET;
			case '(':
				return Token.OPEN_PARENS;
			case ')':
				return Token.CLOSE_PARENS;
			case ',':
				return Token.COMMA;
			case ':':
				return Token.COLON;
			case ';':
				return Token.SEMICOLON;
			case '~':
				return Token.TILDE;
			case '?':
				return Token.INTERR;
			}

			d = peekChar ();
			if (c == '+'){
				
				if (d == '+')
					t = Token.OP_INC;
				else if (d == '=')
					t = Token.OP_ADD_ASSIGN;
				else
					return Token.PLUS;
				doread = true;
				return t;
			}
			if (c == '-'){
				if (d == '-')
					t = Token.OP_DEC;
				else if (d == '=')
					t = Token.OP_SUB_ASSIGN;
				else if (d == '>')
					return Token.OP_PTR;
				else
					return Token.MINUS;
				doread = true;
				return t;
			}

			if (c == '!'){
				if (d == '='){
					doread = true;
					return Token.OP_NE;
				}
				return Token.BANG;
			}

			if (c == '='){
				if (d == '='){
					doread = true;
					return Token.OP_EQ;
				}
				return Token.ASSIGN;
			}

			if (c == '&'){
				if (d == '&'){
					doread = true;
					return Token.OP_AND;
				} else if (d == '='){
					doread = true;
					return Token.OP_AND_ASSIGN;
				}
				return Token.BITWISE_AND;
			}

			if (c == '|'){
				if (d == '|'){
					doread = true;
					return Token.OP_OR;
				} else if (d == '='){
					doread = true;
					return Token.OP_OR_ASSIGN;
				}
				return Token.BITWISE_OR;
			}

			if (c == '*'){
				if (d == '='){
					doread = true;
					return Token.OP_MULT_ASSIGN;
				}
				return Token.STAR;
			}

			if (c == '/'){
				if (d == '='){
					doread = true;
					return Token.OP_DIV_ASSIGN;
				}
				return Token.DIV;
			}

			if (c == '%'){
				if (d == '='){
					doread = true;
					return Token.OP_MOD_ASSIGN;
				}
				return Token.PERCENT;
			}

			if (c == '^'){
				if (d == '='){
					doread = true;
					return Token.OP_XOR_ASSIGN;
				}
				return Token.CARRET;
			}

			if (c == '<'){
				if (d == '<'){
					getChar ();
					d = peekChar ();

					if (d == '='){
						doread = true;
						return Token.OP_SHIFT_LEFT_ASSIGN;
					}
					return Token.OP_SHIFT_LEFT;
				} else if (d == '='){
					doread = true;
					return Token.OP_LE;
				}
				return Token.OP_LT;
			}

			if (c == '>'){
				if (d == '>'){
					getChar ();
					d = peekChar ();

					if (d == '='){
						doread = true;
						return Token.OP_SHIFT_RIGHT_ASSIGN;
					}
					return Token.OP_SHIFT_RIGHT;
				} else if (d == '='){
					doread = true;
					return Token.OP_GE;
				}
				return Token.OP_GT;
			}
			return Token.ERROR;
		}

		bool decimal_digits (int c)
		{
			int d;
			bool seen_digits = false;
			
			if (c != -1)
				number.Append ((char) c);
			
			while ((d = peekChar ()) != -1){
				if (Char.IsDigit ((char)d)){
					number.Append ((char) d);
					getChar ();
					seen_digits = true;
				} else
					break;
			}
			return seen_digits;
		}

		void hex_digits (int c)
		{
			int d;

			if (c != -1)
				number.Append ((char) c);
			while ((d = peekChar ()) != -1){
				char e = Char.ToUpper ((char) d);
				
				if (Char.IsDigit (e) ||
				    (e >= 'A' && e <= 'F')){
					number.Append ((char) e);
					getChar ();
				} else
					break;
			}
		}
		
		int real_type_suffix (int c)
		{
			int t;
			
			switch (c){
			case 'F': case 'f':
				t =  Token.LITERAL_FLOAT;
				break;
			case 'D': case 'd':
				t = Token.LITERAL_DOUBLE;
				break;
			case 'M': case 'm':
				 t= Token.LITERAL_DECIMAL;
				break;
			default:
				return Token.NONE;
			}
			getChar ();
			return t;
		}

		int integer_type_suffix (int c)
		{
			// FIXME: Handle U and L suffixes.
			// We also need to see in which kind of
			// Int the thing fits better according to the spec.
			return Token.LITERAL_INTEGER;
		}
		
		void adjust_int (int t)
		{
			val = new System.Int32();
			val = System.Int32.Parse (number.ToString (), 0);
		}

		int adjust_real (int t)
		{
			string s = number.ToString ();

			Console.WriteLine (s);
			switch (t){
			case Token.LITERAL_DECIMAL:
				val = new System.Decimal ();
				val = System.Decimal.Parse (
					s, styles, csharp_format_info);
				break;
			case Token.LITERAL_DOUBLE:
				val = new System.Double ();
				val = System.Double.Parse (
					s, styles, csharp_format_info);
				break;
			case Token.LITERAL_FLOAT:
				val = new System.Double ();
				val = (float) System.Double.Parse (
					s, styles, csharp_format_info);
				break;

			case Token.NONE:
				val = new System.Double ();
				val = System.Double.Parse (
					s, styles, csharp_format_info);
				t = Token.LITERAL_DOUBLE;
				break;
			}
			return t;
		}

		//
		// Invoked if we know we have .digits or digits
		//
		int is_number (int c)
		{
			bool is_real = false;
			number = new System.Text.StringBuilder ();
			int type;

			number.Length = 0;

			if (Char.IsDigit ((char)c)){
				if (c == '0' && peekChar () == 'x' || peekChar () == 'X'){
					getChar ();
					hex_digits (-1);
					val = new System.Int32 ();
					val = System.Int32.Parse (number.ToString (), NumberStyles.HexNumber);
					return integer_type_suffix (peekChar ());
				}
				decimal_digits (c);
				c = getChar ();
			}

			//
			// We need to handle the case of
			// "1.1" vs "1.string" (LITERAL_FLOAT vs NUMBER DOT IDENTIFIER)
			//
			if (c == '.'){
				if (decimal_digits ('.')){
					is_real = true;
					c = peekChar ();
				} else {
					putback ('.');
					number.Length -= 1;
					adjust_int (Token.LITERAL_INTEGER);
					return Token.LITERAL_INTEGER;
				}
			}
			
			if (c == 'e' || c == 'E'){
				is_real = true;
				number.Append ("e");
				getChar ();
				
				c = peekChar ();
				if (c == '+'){
					number.Append ((char) c);
					getChar ();
					c = peekChar ();
				} else if (c == '-'){
					number.Append ((char) c);
					getChar ();
					c = peekChar ();
				}
				decimal_digits (-1);
				c = peekChar ();
			}

			type = real_type_suffix (c);
			if (type == Token.NONE && !is_real){
				type = integer_type_suffix (c);
				adjust_int (type);
				putback (c);
				return type;
			} else
				is_real = true;

			if (is_real)
				return adjust_real (type);

			Console.WriteLine ("This should not be reached");
			throw new Exception ("Is Number should never reach this point");
		}
			
		int escape (int c)
		{
			int d;
			int v;

			d = peekChar ();
			if (c != '\\')
				return c;
			
			switch (d){
			case 'a':
				v = '\a'; break;
			case 'b':
				v = '\b'; break;
			case 'n':
				v = '\n'; break;
			case 't':
				v = '\t'; break;
			case 'v':
				v = '\v'; break;
			case 'r':
				v = 'c'; break;
			case '\\':
				v = '\\'; break;
			case 'f':
				v = '\f'; break;
			case '0':
				v = 0; break;
			case '"':
				v = '"'; break;
			case '\'':
				v = '\''; break;
			default:
				error_details = "cs1009: Unrecognized escape sequence " + (char)d;
				return -1;
			}
			getChar ();
			return v;
		}

		int getChar ()
		{
			if (putback_char != -1){
				int x = putback_char;
				putback_char = -1;

				return x;
			}
			return reader.Read ();
		}

		int peekChar ()
		{
			if (putback_char != -1)
				return putback_char;
			return reader.Peek ();
		}

		void putback (int c)
		{
			if (putback_char != -1)
				throw new Exception ("This should not happen putback on putback");
			putback_char = c;
		}

		public bool advance ()
		{
			return peekChar () != -1;
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
			current_token = xtoken ();
			return current_token;
		}
		
		public int xtoken ()
		{
			int t;
			bool allow_keyword_as_ident = false;
			bool doread = false;
			int c;

			val = null;
			for (;(c = getChar ()) != -1; col++) {
			
				if (is_identifier_start_character ((char) c)){
					System.Text.StringBuilder id = new System.Text.StringBuilder ();
					string ids;
					
					id.Append ((char) c);
					
					while ((c = peekChar ()) != -1) {
						if (is_identifier_part_character ((char) c)){
							id.Append ((char)getChar ());
							col++;
						} else 
							break;
					}
					
					ids = id.ToString ();

					if (!is_keyword (ids) || allow_keyword_as_ident) {
						val = ids;
						return Token.IDENTIFIER;
					}

					// true, false and null are in the hash anyway.
					return getKeyword (ids);

				}

				if (c == '.'){
					if (Char.IsDigit ((char) peekChar ()))
						return is_number (c);
					return Token.DOT;
				}
				
				if (Char.IsDigit ((char) c))
					return is_number (c);

				// Handle double-slash comments.
				if (c == '/'){
					int d = peekChar ();
				
					if (d == '/'){
						getChar ();
						while ((d = getChar ()) != -1 && (d != '\n'))
							col++;
						line++;
						ref_line++;
						continue;
					} else if (d == '*'){
						getChar ();

						while ((d = getChar ()) != -1){
							if (d == '*' && peekChar () == '/'){
								getChar ();
								col++;
								break;
							}
							if (d == '\n'){
								line++;
								ref_line++;
							}
							col++;
						}
						continue;
					}
				}

				/* For now, ignore pre-processor commands */
				if (col == 1 && c == '#'){
					System.Text.StringBuilder s = new System.Text.StringBuilder ();
					
					while ((c = getChar ()) != -1 && (c != '\n')){
						s.Append ((char) c);
					}
					if (String.Compare (s.ToString (), 0, "line", 0, 4) == 0){
						string arg = s.ToString ().Substring (5);
						int pos;

						if ((pos = arg.IndexOf (' ')) != -1 && pos != 0){
							ref_line = System.Int32.Parse (arg.Substring (0, pos));
							pos++;

							char [] quotes = { '\"' };

							ref_name = arg.Substring (pos);
							ref_name.TrimStart (quotes);
							ref_name.TrimEnd (quotes);
						} else
							ref_line = System.Int32.Parse (arg);
					}
					line++;
					ref_line++;
					continue;
				}
				
				if ((t = is_punct ((char)c, ref doread)) != Token.ERROR){
					if (doread){
						getChar ();
						col++;
					}
					return t;
				}
				
				if (c == '"'){
					System.Text.StringBuilder s = new System.Text.StringBuilder ();

					while ((c = getChar ()) != -1){
						if (c == '"'){
							val = s.ToString ();
							return Token.LITERAL_STRING;
						}

						c = escape (c);
						if (c == -1)
							return Token.ERROR;
						s.Append ((char) c);
					}
				}

				if (c == '\''){
					c = getChar ();
					if (c == '\''){
						error_details = "CS1011: Empty character literal";
						return Token.ERROR;
					}
					c = escape (c);
					if (c == -1)
						return Token.ERROR;
					val = new System.Char ();
					val = (char) c;
					c = getChar ();
					if (c != '\''){
						error_details = "CS1012: Too many characters in character literal";
						// Try to recover, read until newline or next "'"
						while ((c = getChar ()) != -1){
							if (c == '\n' || c == '\'')
								break;
							
						}
						return Token.ERROR;
					}
					return Token.LITERAL_CHARACTER;
				}
				
				// white space
				if (c == '\n'){
					line++;
					ref_line++;
					col = 0;
					continue;
				}
				if (c == ' ' || c == '\t' || c == '\f' || c == '\v' || c == '\r'){
					if (c == '\t')
						col = (((col + 8) / 8) * 8) - 1;
					
					continue;
				}

				if (c == '@'){
					allow_keyword_as_ident = true;
					continue;
				}

				error_details = ((char)c).ToString ();
				
				return Token.ERROR;
			}

			return Token.EOF;
		}
	}
}
