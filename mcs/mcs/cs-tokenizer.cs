//
// cs-tokenizer.cs: The Tokenizer for the C# compiler
//                  This also implements the preprocessor
//
// Author: Miguel de Icaza (miguel@gnu.org)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)
//

/*
 * TODO:
 *   Make sure we accept the proper Unicode ranges, per the spec.
 *   Report error 1032
*/

using System;
using System.Text;
using System.Collections;
using System.IO;
using System.Globalization;

namespace Mono.CSharp
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
		bool handle_remove_add = false;
		bool handle_assembly = false;

		//
		// Whether tokens have been seen on this line
		//
		bool tokens_seen = false;
		
		//
		// Returns a verbose representation of the current location
		//
		public string location {
			get {
				string det;

				if (current_token == Token.ERROR)
					det = "detail: " + error_details;
				else
					det = "";
				
				// return "Line:     "+line+" Col: "+col + "\n" +
				//       "VirtLine: "+ref_line +
				//       " Token: "+current_token + " " + det;

				return ref_name + " " + "(" + line + "," + col + "), Token:" + current_token + " " + det;
			}
		}

		public bool PropertyParsing {
			get {
				return handle_get_set;
			}

			set {
				handle_get_set = value;
			}
                }

		public bool AssemblyTargetParsing {
			get {
				return handle_assembly;
			}

			set {
				handle_assembly = value;
			}
		}

		public bool EventParsing {
			get {
				return handle_remove_add;
			}

			set {
				handle_remove_add = value;
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
		// Pre-processor
		//
		Hashtable defines;

		const int TAKING        = 1;
		const int TAKEN_BEFORE  = 2;
		const int ELSE_SEEN     = 4;
		const int PARENT_TAKING = 8;
		
		//
		// pre-processor if stack state:
		//
		Stack ifstack;
		
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
		
		static void InitTokens ()
		{
			keywords = new Hashtable ();

			keywords.Add ("abstract", Token.ABSTRACT);
			keywords.Add ("as", Token.AS);
			keywords.Add ("add", Token.ADD);
			keywords.Add ("assembly", Token.ASSEMBLY);
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
			keywords.Add ("lock", Token.LOCK);
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
			keywords.Add ("remove", Token.REMOVE);
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
			keywords.Add ("volatile", Token.VOLATILE);
			keywords.Add ("while", Token.WHILE);
		}

		//
		// Class initializer
		// 
		static Tokenizer ()
		{
			InitTokens ();
			csharp_format_info = new NumberFormatInfo ();
			csharp_format_info.CurrencyDecimalSeparator = ".";
			styles = NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint;
		}

		bool is_keyword (string name)
		{
			bool res;
			
			res = keywords.Contains (name);
			if (handle_get_set == false && (name == "get" || name == "set"))
				return false;
			if (handle_remove_add == false && (name == "remove" || name == "add"))
				return false;
			if (handle_assembly == false && (name == "assembly"))
				return false;
			return res;
		}

		int GetKeyword (string name)
		{
			return (int) (keywords [name]);
		}

		public Location Location {
			get {
				return new Location (ref_line);
			}
		}
		
		public Tokenizer (System.IO.Stream input, string fname, ArrayList defs)
		{
			this.ref_name = fname;
			reader = new System.IO.StreamReader (input);
			putback_char = -1;

			if (defs != null){
				defines = new Hashtable ();
				foreach (string def in defs)
					defines [def] = true;
			}

			//
			// FIXME: This could be `Location.Push' but we have to
			// find out why the MS compiler allows this
			//
			Mono.CSharp.Location.Push (fname);
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
					t = Token.OP_PTR;
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
				
				if (Char.IsDigit (e) || (e >= 'A' && e <= 'F')){
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
			return t;
		}

		int integer_type_suffix (ulong ul, int c)
		{
			bool is_unsigned = false;
			bool is_long = false;

			if (c != -1){
				bool scanning = true;
				do {
					switch (c){
					case 'U': case 'u':
						if (is_unsigned)
							scanning = false;
						is_unsigned = true;
						getChar ();
						break;

					case 'l':
						if (!is_unsigned){
							//
							// if we have not seen anything in between
							// report this error
							//
							Report.Warning (
								78, Location,
							"the 'l' suffix is easily confused with digit `1'," +
							" use 'L' for clarity");
						}
						goto case 'L';
						
					case 'L': 
						if (is_long)
							scanning = false;
						is_long = true;
						getChar ();
						break;
						
					default:
						scanning = false;
						break;
					}
					c = peekChar ();
				} while (scanning);
			}

			if (is_long && is_unsigned){
				val = ul;
				return Token.LITERAL_INTEGER;
			} else if (is_unsigned){
				// uint if possible, or ulong else.

				if ((ul & 0xffffffff00000000) == 0)
					val = (uint) ul;
				else
					val = ul;
			} else if (is_long){
				// long if possible, ulong otherwise
				if ((ul & 0x8000000000000000) != 0)
					val = ul;
				else
					val = (long) ul;
			} else {
				// int, uint, long or ulong in that order
				if ((ul & 0xffffffff00000000) == 0){
					uint ui = (uint) ul;
					
					if ((ui & 0x80000000) != 0)
						val = ui;
					else
						val = (int) ui;
				} else {
					if ((ul & 0x8000000000000000) != 0)
						val = ul;
					else
						val = (long) ul;
				}
			}
			return Token.LITERAL_INTEGER;
		}
				
		//
		// given `c' as the next char in the input decide whether
		// we need to convert to a special type, and then choose
		// the best representation for the integer
		//
		int adjust_int (int c)
		{
			ulong ul = System.UInt64.Parse (number.ToString ());
			return integer_type_suffix (ul, c);
		}

		int adjust_real (int t)
		{
			string s = number.ToString ();

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
					ulong ul;
					getChar ();
					hex_digits (-1);

					string s = number.ToString ();

					ul = System.UInt64.Parse (s, NumberStyles.HexNumber);
					return integer_type_suffix (ul, peekChar ());
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
					c = getChar ();
				} else {
					putback ('.');
					number.Length -= 1;
					return adjust_int (-1);
				}
			}
			
			if (c == 'e' || c == 'E'){
				is_real = true;
				number.Append ("e");
				c = getChar ();
				
				if (c == '+'){
					number.Append ((char) c);
					c = getChar ();
				} else if (c == '-'){
					number.Append ((char) c);
					c = getChar ();
				}
				decimal_digits (-1);
				c = getChar ();
			}

			type = real_type_suffix (c);
			if (type == Token.NONE && !is_real){
				putback (c);
				return adjust_int (c);
			} else 
				is_real = true;

			if (type == Token.NONE){
				putback (c);
			}
			
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
				v = '\r'; break;
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

		static StringBuilder static_cmd_arg = new System.Text.StringBuilder ();
		
		void get_cmd_arg (out string cmd, out string arg)
		{
			int c;
			
			tokens_seen = false;
			arg = "";
			static_cmd_arg.Length = 0;
				
			while ((c = getChar ()) != -1 && (c != '\n') && ((c != ' ') || c == '\t')){
				if (c == '\r')
					continue;
				static_cmd_arg.Append ((char) c);
			}

			cmd = static_cmd_arg.ToString ();

			if (c == '\n'){
				line++;
				ref_line++;
				return;
			}

			// skip over white space
			while ((c = getChar ()) != -1 && (c != '\n') && ((c == ' ') || (c == '\t')))
				;

			if (c == '\n'){
				line++;
				ref_line++;
				return;
			}
			
			static_cmd_arg.Length = 0;
			static_cmd_arg.Append ((char) c);
			
			while ((c = getChar ()) != -1 && (c != '\n')){
				if (c == '\r')
					continue;
				static_cmd_arg.Append ((char) c);
			}

			if (c == '\n'){
				line++;
				ref_line++;
			}
			arg = static_cmd_arg.ToString ().Trim ();
		}

		//
		// Handles the #line directive
		//
		bool PreProcessLine (string arg)
		{
			if (arg == "")
				return false;

			if (arg == "default"){
				line = ref_line = line;
				return false;
			}
			
			try {
				int pos;

				if ((pos = arg.IndexOf (' ')) != -1 && pos != 0){
					ref_line = System.Int32.Parse (arg.Substring (0, pos));
					pos++;
					
					char [] quotes = { '\"' };
					
					ref_name = arg.Substring (pos);
					ref_name.TrimStart (quotes);
					ref_name.TrimEnd (quotes);
				} else {
					ref_line = System.Int32.Parse (arg);
				}
			} catch {
				return false;
			}
			
			return true;
		}

		//
		// Handles #define and #undef
		//
		void PreProcessDefinition (bool is_define, string arg)
		{
			if (arg == "" || arg == "true" || arg == "false"){
				Report.Error(1001, Location, "Missing identifer to pre-processor directive");
				return;
			}

			if (is_define){
				if (defines == null)
					defines = new Hashtable ();
				defines [arg] = 1;
			} else {
				if (defines == null)
					return;
				if (defines.Contains (arg))
					defines.Remove (arg);
			}
		}

		bool eval_val (string s)
		{
			if (s == "true")
				return true;
			if (s == "false")
				return false;
			
			if (defines == null)
				return false;
			if (defines.Contains (s))
				return true;

			return false;
		}

		bool pp_primary (ref string s)
		{
			s.Trim ();
			int len = s.Length;

			if (len > 0){
				char c = s [0];
				
				if (c == '('){
					s = s.Substring (1);
					bool val = pp_expr (ref s);
					if (s.Length > 0 && s [0] == ')')
						return val;
					report1517 ();
					return false;
				}
				
				if (Char.IsLetter (c) || c == '_'){
					int j = 1;

					while (j < len){
						c = s [j];
						
						if (Char.IsLetter (c) || Char.IsDigit (c) || c == '_'){
							j++;
							continue;
						}
						bool v = eval_val (s.Substring (0, j));
						s = s.Substring (j);
						return v;
					}
					bool vv = eval_val (s);
					s = "";
					return vv;
				}
			}
			report1517 ();
			return false;
		}
		
		bool pp_unary (ref string s)
		{
			s = s.Trim ();
			int len = s.Length;

			if (len > 0){
				if (s [0] == '!'){
					if (len > 1 && s [1] == '='){
						report1517 ();
						return false;
					}
					s = s.Substring (1);
					return ! pp_primary (ref s);
				} else
					return pp_primary (ref s);
			} else {
				report1517 ();
				return false;
			}
		}
		
		bool pp_eq (ref string s)
		{
			bool va = pp_unary (ref s);

			s = s.Trim ();
			int len = s.Length;
			if (len > 0){
				if (s [0] == '='){
					if (len > 2 && s [1] == '='){
						s = s.Substring (2);
						return va == pp_unary (ref s);
					} else {
						report1517 ();
						return false;
					}
				} else if (s [0] == '!' && len > 1 && s [1] == '='){
					s = s.Substring (2);

					return va != pp_unary (ref s);

				}
			}

			return va;
				
		}
		
		bool pp_and (ref string s)
		{
			bool va = pp_eq (ref s);

			s = s.Trim ();
			int len = s.Length;
			if (len > 0){
				if (s [0] == '&'){
					if (len > 2 && s [1] == '&'){
						s = s.Substring (2);
						return va && pp_eq (ref s);
					} else {
						report1517 ();
						return false;
					}
				} 
			}
			return va;
		}
		
		//
		// Evaluates an expression for `#if' or `#elif'
		//
		bool pp_expr (ref string s)
		{
			bool va = pp_and (ref s);

			s = s.Trim ();
			int len = s.Length;
			if (len > 0){
				if (s [0] == '|'){
					if (len > 2 && s [1] == '|'){
						s = s.Substring (2);
						return va || pp_and (ref s);
					} else {
						report1517 ();
						return false;
					}
				} else {
					report1517 ();
					return false;
				}
			}

			return va;
		}

		bool eval (string s)
		{
			bool v = pp_expr (ref s);

			return v;
		}
		
		void report1517 ()
		{
			Report.Error (1517, Location, "Invalid pre-processor directive");
		}

		void report1028 (string extra)
		{
			Report.Error (
				1028, Location,
				"Unexpected processor directive (" + extra + ")");
		}
		
		//
		// if true, then the code continues processing the code
		// if false, the code stays in a loop until another directive is
		// reached.
		//
		bool handle_preprocessing_directive ()
		{
			char [] blank = { ' ', '\t' };
			string cmd, arg;
			
			get_cmd_arg (out cmd, out arg);
			
			switch (cmd){
			case "line":
				if (!PreProcessLine (arg))
					Report.Error (
						1576, Location,
						"Argument to #line directive is missing or invalid");
				return true;

			case "define":
				PreProcessDefinition (true, arg);
				return true;

			case "undef":
				PreProcessDefinition (false, arg);
				return true;

			case "error":
				Report.Error (1029, Location, "#error: '" + arg + "'");
				return true;

			case "warning":
				Report.Warning (1030, Location, "#warning: '" + arg + "'");
				return true;

			case "region":
				arg = "true";
				goto case "if";

			case "endregion":
				goto case "endif";
				
			case "if":
				if (arg == ""){
					report1517 ();
					return true;
				}
				bool taking = false;
				if (ifstack == null)
					ifstack = new Stack ();

				if (ifstack.Count == 0){
					taking = true;
				} else {
					int state = (int) ifstack.Peek ();
					if ((state & TAKING) != 0)
						taking = true;
				}
					
				if (eval (arg) && taking){
					ifstack.Push (TAKING | TAKEN_BEFORE | PARENT_TAKING);
					return true;
				} else {
					ifstack.Push (taking ? PARENT_TAKING : 0);
					return false;
				}
				
			case "endif":
				if (ifstack == null || ifstack.Count == 0){
					report1028 ("no #if for this #endif");
					return true;
				} else {
					ifstack.Pop ();
					if (ifstack.Count == 0)
						return true;
					else {
						int state = (int) ifstack.Peek ();

						if ((state & TAKING) != 0)
							return true;
						else
							return false;
					}
				}

			case "elif":
				if (ifstack == null || ifstack.Count == 0){
					report1028 ("no #if for this #elif");
					return true;
				} else {
					int state = (int) ifstack.Peek ();

					if ((state & ELSE_SEEN) != 0){
						report1028 ("#elif not valid after #else");
						return true;
					}

					if ((state & (TAKEN_BEFORE | TAKING)) != 0)
						return false;

					if (eval (arg) && ((state & PARENT_TAKING) != 0)){
						state = (int) ifstack.Pop ();
						ifstack.Push (state | TAKING | TAKEN_BEFORE);
						return true;
					} else 
						return false;
				}

			case "else":
				if (ifstack == null || ifstack.Count == 0){
					Report.Error (
						1028, Location,
						"Unexpected processor directive (no #if for this #else)");
					return true;
				} else {
					int state = (int) ifstack.Peek ();

					if ((state & ELSE_SEEN) != 0){
						report1028 ("#else within #else");
						return true;
					}

					ifstack.Pop ();
					ifstack.Push (state | ELSE_SEEN);

					if ((state & TAKEN_BEFORE) == 0){
						if ((state & PARENT_TAKING) != 0)
							return true;
						else
							return false;
					}
					return false;
				}
			}
			
			Report.Error (1024, "Preprocessor directive expected (got: " + cmd + ")");
			return true;
		}
		
		public int xtoken ()
		{
			int t;
			bool allow_keyword_as_ident = false;
			bool doread = false;
			int c;

			val = null;
			// optimization: eliminate col and implement #directive semantic correctly.
			for (;(c = getChar ()) != -1; col++) {
				if (is_identifier_start_character ((char) c)){
					System.Text.StringBuilder id = new System.Text.StringBuilder ();
					string ids;

					tokens_seen = true;
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
						if (ids.Length > 512){
							Report.Error (
								645, Location,
								"Identifier too long (limit is 512 chars)");
						}
						allow_keyword_as_ident = false;
						return Token.IDENTIFIER;
					}

					// true, false and null are in the hash anyway.
					return GetKeyword (ids);

				}

				if (c == '.'){
					tokens_seen = true;
					if (Char.IsDigit ((char) peekChar ()))
						return is_number (c);
					return Token.DOT;
				}
				
				if (Char.IsDigit ((char) c)){
					tokens_seen = true;
					return is_number (c);
				}

				// Handle double-slash comments.
				if (c == '/'){
					int d = peekChar ();
				
					if (d == '/'){
						getChar ();
						while ((d = getChar ()) != -1 && (d != '\n'))
							col++;
						line++;
						ref_line++;
						col = 0;
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
								col = 0;
							}
						}
						continue;
					}
				}

				/* For now, ignore pre-processor commands */
				// FIXME: In C# the '#' is not limited to appear
				// on the first column.
				if (c == '#' && !tokens_seen){
				start_again:
					
					bool cont = handle_preprocessing_directive ();

					if (cont){
						col = 0;
						continue;
					}
					col = 1;

					bool skipping = false;
					for (;(c = getChar ()) != -1; col++){
						if (c == '\n'){
							col = 0;
							line++;
							ref_line++;
							skipping = false;
						} else if (c == ' ' || c == '\t' || c == '\v' || c == '\r')
							continue;
						else if (c != '#')
							skipping = true;
						if (c == '#' && !skipping)
							goto start_again;
					}
					tokens_seen = false;
					if (c == -1)
						Report.Error (1027, Location, "#endif expected");
					continue;
				}
				
				if ((t = is_punct ((char)c, ref doread)) != Token.ERROR){
					tokens_seen = true;
					if (doread){
						getChar ();
						col++;
					}
					return t;
				}
				
				if (c == '"'){
					System.Text.StringBuilder s = new System.Text.StringBuilder ();
					tokens_seen = true;
					
					while ((c = getChar ()) != -1){
						if (c == '"'){
							allow_keyword_as_ident = false;
							val = s.ToString ();
							return Token.LITERAL_STRING;
						}

						if (!allow_keyword_as_ident){
							c = escape (c);
							if (c == -1)
								return Token.ERROR;
						}
						s.Append ((char) c);
					}
				}

				if (c == '\''){
					c = getChar ();
					tokens_seen = true;
					if (c == '\''){
						error_details = "Empty character literal";
						Report.Error (1011, Location, error_details);
						return Token.ERROR;
					}
					c = escape (c);
					if (c == -1)
						return Token.ERROR;
					val = new System.Char ();
					val = (char) c;
					c = getChar ();

					if (c != '\''){
						error_details = "Too many characters in character literal";
						Report.Error (1012, Location, error_details);

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
					tokens_seen = false;
					continue;
				}

				if (c == ' ' || c == '\t' || c == '\f' || c == '\v' || c == '\r'){
					if (c == '\t')
						col = (((col + 8) / 8) * 8) - 1;
					continue;
				}

				if (c == '@'){
					tokens_seen = true;
					allow_keyword_as_ident = true;
					continue;
				}

				error_details = ((char)c).ToString ();
				
				return Token.ERROR;
			}

			if (ifstack != null && ifstack.Count > 1)
				Report.Error (1027, Location, "#endif expected");
			return Token.EOF;
		}
	}
}

