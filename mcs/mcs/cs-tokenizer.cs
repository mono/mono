//
// cs-tokenizer.cs: The Tokenizer for the C# compiler
//                  This also implements the preprocessor
//
// Author: Miguel de Icaza (miguel@gnu.org)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001, 2002 Ximian, Inc (http://www.ximian.com)
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
using System.Reflection;

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
		// Whether a token has been seen on the file
		// This is needed because `define' is not allowed to be used
		// after a token has been seen.
		//
		bool any_token_seen = false;
		static Hashtable tokenValues;
		
		private static Hashtable TokenValueName
		{
			get {
				if (tokenValues == null)
					tokenValues = GetTokenValueNameHash ();

				return tokenValues;
			}
		}

		private static Hashtable GetTokenValueNameHash ()
		{
			Type t = typeof (Token);
			FieldInfo [] fields = t.GetFields ();
			Hashtable hash = new Hashtable ();
			foreach (FieldInfo field in fields) {
				if (field.IsLiteral && field.IsStatic && field.FieldType == typeof (int))
					hash.Add (field.GetValue (null), field.Name);
			}
			return hash;
		}
		
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
				string current_token_name = TokenValueName [current_token] as string;
				if (current_token_name == null)
					current_token_name = current_token.ToString ();

				return String.Format ("{0} ({1},{2}), Token: {3} {4}", ref_name,
										       line,
										       col,
										       current_token_name,
										       det);
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

		static System.Text.StringBuilder id_builder;
		static System.Text.StringBuilder string_builder;
		static System.Text.StringBuilder number_builder;
		
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
			keywords.Add ("stackalloc", Token.STACKALLOC);
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
			csharp_format_info = NumberFormatInfo.InvariantInfo;
			styles = NumberStyles.Float;
			
			id_builder = new System.Text.StringBuilder ();
			string_builder = new System.Text.StringBuilder ();
			number_builder = new System.Text.StringBuilder ();
		}

		int GetKeyword (string name)
		{
			object o = keywords [name];

			if (o == null)
				return -1;
			
			int res = (int) o;

			if (handle_get_set == false && (res == Token.GET || res == Token.SET))
				return -1;
			if (handle_remove_add == false && (res == Token.REMOVE || res == Token.ADD))
				return -1;
			if (handle_assembly == false && res == Token.ASSEMBLY)
				return -1;
			return res;
			
		}

		public Location Location {
			get {
				return new Location (ref_line);
			}
		}

		void define (string def)
		{
			if (!RootContext.AllDefines.Contains (def)){
				RootContext.AllDefines [def] = true;
			}
			if (defines.Contains (def))
				return;
			defines [def] = true;
		}
		
		public Tokenizer (StreamReader input, string fname, ArrayList defs)
		{
			this.ref_name = fname;
			reader = input;
			
			putback_char = -1;

			if (defs != null){
				defines = new Hashtable ();
				foreach (string def in defs)
					define (def);
			}

			//
			// FIXME: This could be `Location.Push' but we have to
			// find out why the MS compiler allows this
			//
			Mono.CSharp.Location.Push (fname);
		}

		bool is_identifier_start_character (char c)
		{
			return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '_' || Char.IsLetter (c);
		}

		bool is_identifier_part_character (char c)
		{
			return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_' || (c >= '0' && c <= '9') || Char.IsLetter (c);
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
				number_builder.Append ((char) c);

			//
			// We use peekChar2, because decimal_digits needs to do a 
			// 2-character look-ahead (5.ToString for example).
			//
			while ((d = peekChar2 ()) != -1){
				if (d >= '0' && d <= '9'){
					number_builder.Append ((char) d);
					getChar ();
					seen_digits = true;
				} else
					break;
			}
			
			return seen_digits;
		}

		bool is_hex (int e)
		{
			return (e >= '0' && e <= '9') || (e >= 'A' && e <= 'F') || (e >= 'a' && e <= 'f');
		}
		
		void hex_digits (int c)
		{
			int d;

			if (c != -1)
				number_builder.Append ((char) c);
			while ((d = peekChar ()) != -1){
				if (is_hex (d)){
					number_builder.Append ((char) d);
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
			try {
				ulong ul = System.UInt64.Parse (number_builder.ToString ());
				return integer_type_suffix (ul, c);
			} catch (OverflowException) {
				error_details = "Integral constant is too large";
				Report.Error (1021, Location, error_details);
				val = 0ul;
				return Token.LITERAL_INTEGER;
			}
		}
		
		int adjust_real (int t)
		{
			string s = number_builder.ToString ();

			switch (t){
			case Token.LITERAL_DECIMAL:
				try {
					val = System.Decimal.Parse (s, styles, csharp_format_info);
				} catch (OverflowException) {
					val = 0m;     
					error_details = "Floating-point constant is outside the range of the type 'decimal'";
					Report.Error (594, Location, error_details);
				}
				break;
			case Token.LITERAL_FLOAT:
				try {
					val = (float) System.Double.Parse (s, styles, csharp_format_info);
				} catch (OverflowException) {
					val = 0.0f;     
					error_details = "Floating-point constant is outside the range of the type 'float'";
					Report.Error (594, Location, error_details);
				}
				break;
				
			case Token.LITERAL_DOUBLE:
			case Token.NONE:
				t = Token.LITERAL_DOUBLE;
				try {
					val = System.Double.Parse (s, styles, csharp_format_info);
				} catch (OverflowException) {
					val = 0.0;     
					error_details = "Floating-point constant is outside the range of the type 'double'";
					Report.Error (594, Location, error_details);
				}
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
			int type;

			number_builder.Length = 0;

			if (c >= '0' && c <= '9'){
				if (c == '0' && peekChar () == 'x' || peekChar () == 'X'){
					ulong ul;
					getChar ();
					hex_digits (-1);

					string s = number_builder.ToString ();

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
					number_builder.Length -= 1;
					return adjust_int (-1);
				}
			}
			
			if (c == 'e' || c == 'E'){
				is_real = true;
				number_builder.Append ("e");
				c = getChar ();
				
				if (c == '+'){
					number_builder.Append ((char) c);
					c = -1;
				} else if (c == '-') {
					number_builder.Append ((char) c);
					c = -1;
				} else 
					number_builder.Append ('+');
					
				decimal_digits (c);
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

		//
		// Accepts exactly count (4 or 8) hex, no more no less
		//
		int getHex (int count, out bool error)
		{
			int [] buffer = new int [8];
			int i;
			int total = 0;
			int c;
			int top = count != -1 ? count : 4;
			
			getChar ();
			error = false;
			for (i = 0; i < top; i++){
				c = getChar ();
				
				if (c >= '0' && c <= '9')
					c = (int) c - (int) '0';
				else if (c >= 'A' && c <= 'F')
					c = (int) c - (int) 'A' + 10;
				else if (c >= 'a' && c <= 'f')
					c = (int) c - (int) 'a' + 10;
				else {
					error = true;
					return 0;
				}
				
				total = (total * 16) + c;
				if (count == -1){
					int p = peekChar ();
					if (p == -1)
						break;
					if (!is_hex ((char)p))
						break;
				}
			}
			return total;
		}

		int escape (int c)
		{
			bool error;
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
			case 'x':
				v = getHex (-1, out error);
				if (error)
					goto default;
				return v;
			case 'u':
				v = getHex (4, out error);
				if (error)
					goto default;
				return v;
			case 'U':
				v = getHex (8, out error);
				if (error)
					goto default;
				return v;
			default:
				Report.Error (1009, Location, "Unrecognized escape sequence in " + (char)d);
				return d;
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
			putback_char = reader.Read ();
			return putback_char;
		}

		int peekChar2 ()
		{
			if (putback_char != -1)
				return putback_char;
			return reader.Peek ();
		}
		
		void putback (int c)
		{
			if (putback_char != -1){
				Console.WriteLine ("Col: " + col);
				Console.WriteLine ("Row: " + line);
				Console.WriteLine ("Name: " + ref_name);
				Console.WriteLine ("Current [{0}] putting back [{1}]  ", putback_char, c);
				throw new Exception ("This should not happen putback on putback");
			}
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
				
			while ((c = getChar ()) != -1 && (c != '\n') && (c != ' ') && (c != '\t')){
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
					
					ref_name = arg.Substring (pos). Trim (quotes);
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
				Report.Error (1001, Location, "Missing identifer to pre-processor directive");
				return;
			}

			char[] whitespace = { ' ', '\t' };
			if (arg.IndexOfAny (whitespace) != -1){
				Report.Error (1025, Location, "Single-line comment or end-of-line expected");
				return;
			}

			foreach (char c in arg){
				if (!Char.IsLetter (c) && (c != '_')){
					Report.Error (1001, Location, "Identifier expected");
					return;
				}
			}

			if (is_define){
				if (defines == null)
					defines = new Hashtable ();
				define (arg);
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
			s = s.Trim ();
			int len = s.Length;

			if (len > 0){
				char c = s [0];
				
				if (c == '('){
					s = s.Substring (1);
					bool val = pp_expr (ref s);
					if (s.Length > 0 && s [0] == ')'){
						s = s.Substring (1);
						return val;
					}
					Error_InvalidDirective ();
					return false;
				}
				
				if (is_identifier_start_character (c)){
					int j = 1;

					while (j < len){
						c = s [j];
						
						if (is_identifier_part_character (c)){
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
			Error_InvalidDirective ();
			return false;
		}
		
		bool pp_unary (ref string s)
		{
			s = s.Trim ();
			int len = s.Length;

			if (len > 0){
				if (s [0] == '!'){
					if (len > 1 && s [1] == '='){
						Error_InvalidDirective ();
						return false;
					}
					s = s.Substring (1);
					return ! pp_primary (ref s);
				} else
					return pp_primary (ref s);
			} else {
				Error_InvalidDirective ();
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
						Error_InvalidDirective ();
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
						return (va & pp_eq (ref s));
					} else {
						Error_InvalidDirective ();
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
				char c = s [0];
				
				if (c == '|'){
					if (len > 2 && s [1] == '|'){
						s = s.Substring (2);
						return va | pp_and (ref s);
					} else {
						Error_InvalidDirective ();
						return false;
					}
				} 
			}

			return va;
		}

		bool eval (string s)
		{
			bool v = pp_expr (ref s);
			s = s.Trim ();
			if (s.Length != 0){
				Error_InvalidDirective ();
				return false;
			}

			return v;
		}
		
		void Error_InvalidDirective ()
		{
			Report.Error (1517, Location, "Invalid pre-processor directive");
		}

		void Error_UnexpectedDirective (string extra)
		{
			Report.Error (
				1028, Location,
				"Unexpected processor directive (" + extra + ")");
		}

		void Error_TokensSeen ()
		{
			Report.Error (
				1032, Location,
				"Cannot define or undefine pre-processor symbols after a token in the file");
		}
		
		//
		// if true, then the code continues processing the code
		// if false, the code stays in a loop until another directive is
		// reached.
		//
		bool handle_preprocessing_directive (bool caller_is_taking)
		{
			char [] blank = { ' ', '\t' };
			string cmd, arg;
			
			get_cmd_arg (out cmd, out arg);

			// Eat any trailing whitespaces and single-line comments
			if (arg.IndexOf ("//") != -1)
				arg = arg.Substring (0, arg.IndexOf ("//"));
			arg = arg.TrimEnd (' ', '\t');

			//
			// The first group of pre-processing instructions is always processed
			//
			switch (cmd){
			case "line":
				if (!PreProcessLine (arg))
					Report.Error (
						1576, Location,
						"Argument to #line directive is missing or invalid");
				return true;

			case "region":
				arg = "true";
				goto case "if";

			case "endregion":
				goto case "endif";
				
			case "if":
				if (arg == ""){
					Error_InvalidDirective ();
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
					Error_UnexpectedDirective ("no #if for this #endif");
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
					Error_UnexpectedDirective ("no #if for this #elif");
					return true;
				} else {
					int state = (int) ifstack.Peek ();

					if ((state & ELSE_SEEN) != 0){
						Error_UnexpectedDirective ("#elif not valid after #else");
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
						Error_UnexpectedDirective ("#else within #else");
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

			//
			// These are only processed if we are in a `taking' block
			//
			if (!caller_is_taking)
				return false;
					
			switch (cmd){
			case "define":
				if (any_token_seen){
					Error_TokensSeen ();
					return true;
				}
				PreProcessDefinition (true, arg);
				return true;

			case "undef":
				if (any_token_seen){
					Error_TokensSeen ();
					return true;
				}
				PreProcessDefinition (false, arg);
				return true;

			case "error":
				Report.Error (1029, Location, "#error: '" + arg + "'");
				return true;

			case "warning":
				Report.Warning (1030, Location, "#warning: '" + arg + "'");
				return true;
			}

			Report.Error (1024, Location, "Preprocessor directive expected (got: " + cmd + ")");
			return true;

		}

		private int consume_string (bool quoted) 
		{
			int c;
			string_builder.Length = 0;
								
			while ((c = getChar ()) != -1){
				if (c == '"'){
					if (quoted && peekChar () == '"'){
						string_builder.Append ((char) c);
						getChar ();
						continue;
					} else {
						val = string_builder.ToString ();
						return Token.LITERAL_STRING;
					}
				}

				if (c == '\n'){
					if (!quoted)
						Report.Error (1010, Location, "Newline in constant");
					line++;
					ref_line++;
					col = 0;
				} else
					col++;

				if (!quoted){
					c = escape (c);
					if (c == -1)
						return Token.ERROR;
				}
				string_builder.Append ((char) c);
			}

			Report.Error (1039, Location, "Unterminated string literal");
			return Token.EOF;
		}

		private int consume_identifier (int c, bool quoted) 
		{
			id_builder.Length = 0;

			id_builder.Append ((char) c);
					
			while ((c = reader.Read ()) != -1) {
				if (is_identifier_part_character ((char) c)){
					id_builder.Append ((char)c);
					putback_char = -1;
					col++;
				} else {
					putback_char = c;
					break;
				}
			}
					
			string ids = id_builder.ToString ();
			int keyword = GetKeyword (ids);
			
			if (keyword == -1 || quoted){
				val = ids;
				if (ids.Length > 512){
					Report.Error (
						645, Location,
						"Identifier too long (limit is 512 chars)");
				}
				return Token.IDENTIFIER;
			}

			return keyword;
		}
		
		public int xtoken ()
		{
			int t;
			bool doread = false;
			int c;

			val = null;
			// optimization: eliminate col and implement #directive semantic correctly.
			for (;(c = getChar ()) != -1; col++) {
				if (c == ' ' || c == '\t' || c == '\f' || c == '\v' || c == '\r' || c == 0xa0){
					
					if (c == '\t')
						col = (((col + 8) / 8) * 8) - 1;
					continue;
				}

				// Handle double-slash comments.
				if (c == '/'){
					int d = peekChar ();
				
					if (d == '/'){
						getChar ();
						while ((d = getChar ()) != -1 && (d != '\n') && d != '\r')
							col++;
						if (d == '\n'){
							line++;
							ref_line++;
							col = 0;
						}
						any_token_seen |= tokens_seen;
						tokens_seen = false;
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
								any_token_seen |= tokens_seen;
								tokens_seen = false;
							}
						}
						continue;
					}
					goto is_punct_label;
				}

				
				if (is_identifier_start_character ((char)c)){
					tokens_seen = true;
					return consume_identifier (c, false);
				}

			is_punct_label:
				if ((t = is_punct ((char)c, ref doread)) != Token.ERROR){
					tokens_seen = true;
					if (doread){
						getChar ();
						col++;
					}
					return t;
				}

				// white space
				if (c == '\n'){
					line++;
					ref_line++;
					col = 0;
					any_token_seen |= tokens_seen;
					tokens_seen = false;
					continue;
				}

				if (c >= '0' && c <= '9'){
					tokens_seen = true;
					return is_number (c);
				}

				if (c == '.'){
					tokens_seen = true;
					int peek = peekChar ();
					if (peek >= '0' && peek <= '9')
						return is_number (c);
					return Token.DOT;
				}
				
				/* For now, ignore pre-processor commands */
				// FIXME: In C# the '#' is not limited to appear
				// on the first column.
				if (c == '#' && !tokens_seen){
					bool cont = true;
					
				start_again:
					
					cont = handle_preprocessing_directive (cont);

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
						} else if (c == ' ' || c == '\t' || c == '\v' || c == '\r' || c == 0xa0)
							continue;
						else if (c != '#')
							skipping = true;
						if (c == '#' && !skipping)
							goto start_again;
					}
					any_token_seen |= tokens_seen;
					tokens_seen = false;
					if (c == -1)
						Report.Error (1027, Location, "#endif/#endregion expected");
					continue;
				}
				
				if (c == '"') 
					return consume_string (false);

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
							if (c == '\n' || c == '\''){
								line++;
								ref_line++;
								col = 0;
								break;
							} else
								col++;
							
						}
						return Token.ERROR;
					}
					return Token.LITERAL_CHARACTER;
				}
				
				if (c == '@') {
					c = getChar ();
					if (c == '"') {
						tokens_seen = true;
						return consume_string (true);
					} else if (is_identifier_start_character ((char) c)){
						return consume_identifier (c, true);
					} else {
						Report.Error (1033, Location, "'@' must be followed by string constant or identifier");
					}
				}

				error_details = ((char)c).ToString ();
				
				return Token.ERROR;
			}

			if (ifstack != null && ifstack.Count >= 1)
				Report.Error (1027, Location, "#endif/#endregion expected");

			return Token.EOF;
		}
	}
}

