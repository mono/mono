//
// cs-tokenizer.cs: The Tokenizer for the C# compiler
//                  This also implements the preprocessor
//
// Author: Miguel de Icaza (miguel@gnu.org)
//         Marek Safar (marek.safar@seznam.cz)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001, 2002 Ximian, Inc (http://www.ximian.com)
// (C) 2004 Novell, Inc
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
		SeekableStreamReader reader;
		SourceFile ref_name;
		SourceFile file_name;
		int ref_line = 1;
		int line = 1;
		int col = 0;
		int previous_col;
		int current_token;
		bool handle_get_set = false;
		bool handle_remove_add = false;
		bool handle_assembly = false;
		bool handle_constraints = false;
		bool handle_typeof = false;
		Location current_location;
		Location current_comment_location = Location.Null;
		ArrayList escapedIdentifiers = new ArrayList ();

		//
		// XML documentation buffer. The save point is used to divide
		// comments on types and comments on members.
		//
		StringBuilder xml_comment_buffer;

		//
		// See comment on XmlCommentState enumeration.
		//
		XmlCommentState xmlDocState = XmlCommentState.Allowed;

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

				return String.Format ("{0} ({1},{2}), Token: {3} {4}", ref_name.Name,
										       ref_line,
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

		public bool ConstraintsParsing {
			get {
				return handle_constraints;
			}

			set {
				handle_constraints = value;
			}
		}

		public bool TypeOfParsing {
			get {
				return handle_typeof;
			}

			set {
				handle_typeof = value;
			}
		}

		public XmlCommentState doc_state {
			get { return xmlDocState; }
			set {
				if (value == XmlCommentState.Allowed) {
					check_incorrect_doc_comment ();
					reset_doc_comment ();
				}
				xmlDocState = value;
			}
		}

		public bool IsEscapedIdentifier (Location loc)
		{
			foreach (LocatedToken lt in escapedIdentifiers)
				if (lt.Location.Equals (loc))
					return true;
			return false;
		}

		//
		// Class variables
		// 
		static CharArrayHashtable[] keywords;
		static Hashtable keywordStrings = new Hashtable ();
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
		const int REGION        = 16;		

		//
		// pre-processor if stack state:
		//
		Stack ifstack;

		static System.Text.StringBuilder string_builder;

		const int max_id_size = 512;
		static char [] id_builder = new char [max_id_size];

		static CharArrayHashtable [] identifiers = new CharArrayHashtable [max_id_size + 1];

		const int max_number_size = 512;
		static char [] number_builder = new char [max_number_size];
		static int number_pos;
		
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
				return ref_line;
			}
		}

		public int Col {
			get {
				return col;
			}
		}

		//
		// This is used when the tokenizer needs to save
		// the current position as it needs to do some parsing
		// on its own to deamiguate a token in behalf of the
		// parser.
		//
		Stack position_stack = new Stack ();
		class Position {
			public int position;
			public int ref_line;
			public int col;
			public int putback_char;
			public int previous_col;
			public int parsing_generic_less_than;
			
			public Position (Tokenizer t)
			{
				position = t.reader.Position;
				ref_line = t.ref_line;
				col = t.col;
				putback_char = t.putback_char;
				previous_col = t.previous_col;
				parsing_generic_less_than = t.parsing_generic_less_than;
			}
		}
		
		public void PushPosition ()
		{
			position_stack.Push (new Position (this));
		}

		public void PopPosition ()
		{
			Position p = (Position) position_stack.Pop ();

			reader.Position = p.position;
			ref_line = p.ref_line;
			col = p.col;
			putback_char = p.putback_char;
			previous_col = p.previous_col;

		}

		// Do not reset the position, ignore it.
		public void DiscardPosition ()
		{
			position_stack.Pop ();
		}
		
		static void AddKeyword (string kw, int token) {
			keywordStrings.Add (kw, kw);
			if (keywords [kw.Length] == null) {
				keywords [kw.Length] = new CharArrayHashtable (kw.Length);
			}
			keywords [kw.Length] [kw.ToCharArray ()] = token;
		}

		static void InitTokens ()
		{
			keywords = new CharArrayHashtable [64];

			AddKeyword ("__arglist", Token.ARGLIST);
			AddKeyword ("abstract", Token.ABSTRACT);
			AddKeyword ("as", Token.AS);
			AddKeyword ("add", Token.ADD);
			AddKeyword ("assembly", Token.ASSEMBLY);
			AddKeyword ("base", Token.BASE);
			AddKeyword ("bool", Token.BOOL);
			AddKeyword ("break", Token.BREAK);
			AddKeyword ("byte", Token.BYTE);
			AddKeyword ("case", Token.CASE);
			AddKeyword ("catch", Token.CATCH);
			AddKeyword ("char", Token.CHAR);
			AddKeyword ("checked", Token.CHECKED);
			AddKeyword ("class", Token.CLASS);
			AddKeyword ("const", Token.CONST);
			AddKeyword ("continue", Token.CONTINUE);
			AddKeyword ("decimal", Token.DECIMAL);
			AddKeyword ("default", Token.DEFAULT);
			AddKeyword ("delegate", Token.DELEGATE);
			AddKeyword ("do", Token.DO);
			AddKeyword ("double", Token.DOUBLE);
			AddKeyword ("else", Token.ELSE);
			AddKeyword ("enum", Token.ENUM);
			AddKeyword ("event", Token.EVENT);
			AddKeyword ("explicit", Token.EXPLICIT);
			AddKeyword ("extern", Token.EXTERN);
			AddKeyword ("false", Token.FALSE);
			AddKeyword ("finally", Token.FINALLY);
			AddKeyword ("fixed", Token.FIXED);
			AddKeyword ("float", Token.FLOAT);
			AddKeyword ("for", Token.FOR);
			AddKeyword ("foreach", Token.FOREACH);
			AddKeyword ("goto", Token.GOTO);
			AddKeyword ("get", Token.GET);
			AddKeyword ("if", Token.IF);
			AddKeyword ("implicit", Token.IMPLICIT);
			AddKeyword ("in", Token.IN);
			AddKeyword ("int", Token.INT);
			AddKeyword ("interface", Token.INTERFACE);
			AddKeyword ("internal", Token.INTERNAL);
			AddKeyword ("is", Token.IS);
			AddKeyword ("lock", Token.LOCK);
			AddKeyword ("long", Token.LONG);
			AddKeyword ("namespace", Token.NAMESPACE);
			AddKeyword ("new", Token.NEW);
			AddKeyword ("null", Token.NULL);
			AddKeyword ("object", Token.OBJECT);
			AddKeyword ("operator", Token.OPERATOR);
			AddKeyword ("out", Token.OUT);
			AddKeyword ("override", Token.OVERRIDE);
			AddKeyword ("params", Token.PARAMS);
			AddKeyword ("private", Token.PRIVATE);
			AddKeyword ("protected", Token.PROTECTED);
			AddKeyword ("public", Token.PUBLIC);
			AddKeyword ("readonly", Token.READONLY);
			AddKeyword ("ref", Token.REF);
			AddKeyword ("remove", Token.REMOVE);
			AddKeyword ("return", Token.RETURN);
			AddKeyword ("sbyte", Token.SBYTE);
			AddKeyword ("sealed", Token.SEALED);
			AddKeyword ("set", Token.SET);
			AddKeyword ("short", Token.SHORT);
			AddKeyword ("sizeof", Token.SIZEOF);
			AddKeyword ("stackalloc", Token.STACKALLOC);
			AddKeyword ("static", Token.STATIC);
			AddKeyword ("string", Token.STRING);
			AddKeyword ("struct", Token.STRUCT);
			AddKeyword ("switch", Token.SWITCH);
			AddKeyword ("this", Token.THIS);
			AddKeyword ("throw", Token.THROW);
			AddKeyword ("true", Token.TRUE);
			AddKeyword ("try", Token.TRY);
			AddKeyword ("typeof", Token.TYPEOF);
			AddKeyword ("uint", Token.UINT);
			AddKeyword ("ulong", Token.ULONG);
			AddKeyword ("unchecked", Token.UNCHECKED);
			AddKeyword ("unsafe", Token.UNSAFE);
			AddKeyword ("ushort", Token.USHORT);
			AddKeyword ("using", Token.USING);
			AddKeyword ("virtual", Token.VIRTUAL);
			AddKeyword ("void", Token.VOID);
			AddKeyword ("volatile", Token.VOLATILE);
			AddKeyword ("where", Token.WHERE);
			AddKeyword ("while", Token.WHILE);
			AddKeyword ("partial", Token.PARTIAL);
		}

		//
		// Class initializer
		// 
		static Tokenizer ()
		{
			InitTokens ();
			csharp_format_info = NumberFormatInfo.InvariantInfo;
			styles = NumberStyles.Float;
			
			string_builder = new System.Text.StringBuilder ();
		}

		int GetKeyword (char[] id, int id_len)
		{
			/*
			 * Keywords are stored in an array of hashtables grouped by their
			 * length.
			 */

			if ((id_len >= keywords.Length) || (keywords [id_len] == null))
				return -1;
			object o = keywords [id_len] [id];

			if (o == null)
				return -1;
			
			int res = (int) o;

			if (handle_get_set == false && (res == Token.GET || res == Token.SET))
				return -1;
			if (handle_remove_add == false && (res == Token.REMOVE || res == Token.ADD))
				return -1;
			if (handle_assembly == false && res == Token.ASSEMBLY)
				return -1;
			if (handle_constraints == false && res == Token.WHERE)
				return -1;

			return res;
			
		}

		public Location Location {
			get { return current_location; }
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
		
		public Tokenizer (SeekableStreamReader input, SourceFile file, ArrayList defs)
		{
			this.ref_name = file;
			this.file_name = file;
			reader = input;
			
			putback_char = -1;

			if (defs != null){
				defines = new Hashtable ();
				foreach (string def in defs)
					define (def);
			}

			xml_comment_buffer = new StringBuilder ();

			//
			// FIXME: This could be `Location.Push' but we have to
			// find out why the MS compiler allows this
			//
			Mono.CSharp.Location.Push (file);
		}

		static bool is_identifier_start_character (char c)
		{
			return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '_' || Char.IsLetter (c);
		}

		static bool is_identifier_part_character (char c)
		{
			return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_' || (c >= '0' && c <= '9') || Char.IsLetter (c);
		}

		public static bool IsKeyword (string s)
		{
			return keywordStrings [s] != null;
		}

		public static bool IsValidIdentifier (string s)
		{
			if (s == null || s.Length == 0)
				return false;

			if (!is_identifier_start_character (s [0]))
				return false;
			
			for (int i = 1; i < s.Length; i ++)
				if (! is_identifier_part_character (s [i]))
					return false;
			
			return true;
		}

		bool parse_generic_dimension (out int dimension)
		{
			dimension = 1;

		again:
			int the_token = token ();
			if (the_token == Token.OP_GENERICS_GT)
				return true;
			else if (the_token == Token.COMMA) {
				dimension++;
				goto again;
			}

			return false;
		}

		bool parse_less_than ()
		{
		start:
			int the_token = token ();
			if (the_token == Token.OPEN_BRACKET) {
				do {
					the_token = token ();
				} while (the_token != Token.CLOSE_BRACKET);
				the_token = token ();
			}
			switch (the_token) {
			case Token.IDENTIFIER:
			case Token.OBJECT:
			case Token.STRING:
			case Token.BOOL:
			case Token.DECIMAL:
			case Token.FLOAT:
			case Token.DOUBLE:
			case Token.SBYTE:
			case Token.BYTE:
			case Token.SHORT:
			case Token.USHORT:
			case Token.INT:
			case Token.UINT:
			case Token.LONG:
			case Token.ULONG:
			case Token.CHAR:
			case Token.VOID:
				break;

			default:
				return false;
			}
		again:
			the_token = token ();

			if (the_token == Token.OP_GENERICS_GT)
				return true;
			else if ((the_token == Token.COMMA) || (the_token == Token.DOT))
				goto start;
			else if (the_token == Token.INTERR || the_token == Token.STAR)
				goto again;
			else if (the_token == Token.OP_GENERICS_LT) {
				if (!parse_less_than ())
					return false;
				goto again;
			} else if (the_token == Token.OPEN_BRACKET) {
			rank_specifiers:
				the_token = token ();
				if (the_token == Token.CLOSE_BRACKET)
					goto again;
				else if (the_token == Token.COMMA)
					goto rank_specifiers;
				return false;
			}

			return false;
		}

		int parsing_generic_less_than = 0;

		int is_punct (char c, ref bool doread)
		{
			int d;
			int t;

			doread = false;

			switch (c){
			case '{':
				val = Location;
				return Token.OPEN_BRACE;
			case '}':
				val = Location;
				return Token.CLOSE_BRACE;
			case '[':
				// To block doccomment inside attribute declaration.
				if (doc_state == XmlCommentState.Allowed)
					doc_state = XmlCommentState.NotAllowed;
				return Token.OPEN_BRACKET;
			case ']':
				return Token.CLOSE_BRACKET;
			case '(':
				return Token.OPEN_PARENS;
			case ')': {
				if (deambiguate_close_parens == 0)
					return Token.CLOSE_PARENS;

				--deambiguate_close_parens;

				PushPosition ();
				int new_token = token ();
				PopPosition ();

				if (new_token == Token.OPEN_PARENS)
					return Token.CLOSE_PARENS_OPEN_PARENS;
				else if (new_token == Token.MINUS)
					return Token.CLOSE_PARENS_MINUS;
				else if (IsCastToken (new_token))
					return Token.CLOSE_PARENS_CAST;
				else
					return Token.CLOSE_PARENS_NO_CAST;
			}

			case ',':
				return Token.COMMA;
			case ';':
				val = Location;
				return Token.SEMICOLON;
			case '~':
				val = Location;
				return Token.TILDE;
			case '?':
				return Token.INTERR;
			}

			if (c == '<') {
				if (parsing_generic_less_than++ > 0)
					return Token.OP_GENERICS_LT;

				if (handle_typeof) {
					int dimension;
					PushPosition ();
					if (parse_generic_dimension (out dimension)) {
						val = dimension;
						DiscardPosition ();
						return Token.GENERIC_DIMENSION;
					}
					PopPosition ();
				}

				// Save current position and parse next token.
				PushPosition ();
				bool is_generic_lt = parse_less_than ();
				PopPosition ();

				if (is_generic_lt) {
					parsing_generic_less_than++;
					return Token.OP_GENERICS_LT;
				} else
					parsing_generic_less_than = 0;

				d = peekChar ();
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
			} else if (c == '>') {
				if (parsing_generic_less_than > 0) {
					parsing_generic_less_than--;
					return Token.OP_GENERICS_GT;
				}

				d = peekChar ();
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

			d = peekChar ();
			if (c == '+'){
				
				if (d == '+') {
					val = Location;
					t = Token.OP_INC;
				}
				else if (d == '=')
					t = Token.OP_ADD_ASSIGN;
				else {
					val = Location;
					return Token.PLUS;
				}
				doread = true;
				return t;
			}
			if (c == '-'){
				if (d == '-') {
					val = Location;
					t = Token.OP_DEC;
				}
				else if (d == '=')
					t = Token.OP_SUB_ASSIGN;
				else if (d == '>')
					t = Token.OP_PTR;
				else {
					val = Location;
					return Token.MINUS;
				}
				doread = true;
				return t;
			}

			if (c == '!'){
				if (d == '='){
					doread = true;
					return Token.OP_NE;
				}
				val = Location;
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
				val = Location;
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
				val = Location;
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

			if (c == ':'){
				if (d == ':'){
					doread = true;
					return Token.DOUBLE_COLON;
				}
				val = Location;
				return Token.COLON;
			}

			return Token.ERROR;
		}

		int deambiguate_close_parens = 0;

		public void Deambiguate_CloseParens (object expression)
		{
			putback (')');

			// When any binary operation is used we are sure it is not a cast
			if (expression is Binary)
				return;

			deambiguate_close_parens++;
		}

		public void PutbackNullable ()
		{
			if (nullable_pos < 0)
				throw new Exception ();

			current_token = -1;
			val = null;
			reader.Position = nullable_pos;

			putback_char = '?';
		}

		public void PutbackCloseParens ()
		{
			putback_char = ')';
		}

		void Error_NumericConstantTooLong ()
		{
			Report.Error (1021, Location, "Numeric constant too long");			
		}

		int nullable_pos = -1;

		public void CheckNullable (bool is_nullable)
		{
			if (is_nullable)
				nullable_pos = reader.Position;
			else
				nullable_pos = -1;
		}

		bool decimal_digits (int c)
		{
			int d;
			bool seen_digits = false;
			
			if (c != -1){
				if (number_pos == max_number_size)
					Error_NumericConstantTooLong ();
				number_builder [number_pos++] = (char) c;
			}
			
			//
			// We use peekChar2, because decimal_digits needs to do a 
			// 2-character look-ahead (5.ToString for example).
			//
			while ((d = peekChar2 ()) != -1){
				if (d >= '0' && d <= '9'){
					if (number_pos == max_number_size)
						Error_NumericConstantTooLong ();
					number_builder [number_pos++] = (char) d;
					getChar ();
					seen_digits = true;
				} else
					break;
			}
			
			return seen_digits;
		}

		static bool is_hex (int e)
		{
			return (e >= '0' && e <= '9') || (e >= 'A' && e <= 'F') || (e >= 'a' && e <= 'f');
		}
				
		static int real_type_suffix (int c)
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
						if (!is_unsigned && (RootContext.WarningLevel >= 4)){
							//
							// if we have not seen anything in between
							// report this error
							//
							Report.Warning (78, 4, Location, "The 'l' suffix is easily confused with the digit '1' (use 'L' for clarity)");
						}
						//
						// This goto statement causes the MS CLR 2.0 beta 1 csc to report an error, so
						// work around that.
						//
						//goto case 'L';
						if (is_long)
							scanning = false;
						is_long = true;
						getChar ();
						break;

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
				if (number_pos > 9){
					ulong ul = (uint) (number_builder [0] - '0');

					for (int i = 1; i < number_pos; i++){
						ul = checked ((ul * 10) + ((uint)(number_builder [i] - '0')));
					}
					return integer_type_suffix (ul, c);
				} else {
					uint ui = (uint) (number_builder [0] - '0');

					for (int i = 1; i < number_pos; i++){
						ui = checked ((ui * 10) + ((uint)(number_builder [i] - '0')));
					}
					return integer_type_suffix (ui, c);
				}
			} catch (OverflowException) {
				error_details = "Integral constant is too large";
				Report.Error (1021, Location, error_details);
				val = 0ul;
				return Token.LITERAL_INTEGER;
			}
			catch (FormatException) {
				Report.Error (1013, Location, "Invalid number");
				val = 0ul;
				return Token.LITERAL_INTEGER;
			}
		}
		
		int adjust_real (int t)
		{
			string s = new String (number_builder, 0, number_pos);
			const string error_details = "Floating-point constant is outside the range of type `{0}'";

			switch (t){
			case Token.LITERAL_DECIMAL:
				try {
					val = System.Decimal.Parse (s, styles, csharp_format_info);
				} catch (OverflowException) {
					val = 0m;     
					Report.Error (594, Location, error_details, "decimal");
				}
				break;
			case Token.LITERAL_FLOAT:
				try {
					val = float.Parse (s, styles, csharp_format_info);
				} catch (OverflowException) {
					val = 0.0f;     
					Report.Error (594, Location, error_details, "float");
				}
				break;
				
			case Token.LITERAL_DOUBLE:
			case Token.NONE:
				t = Token.LITERAL_DOUBLE;
				try {
					val = System.Double.Parse (s, styles, csharp_format_info);
				} catch (OverflowException) {
					val = 0.0;     
					Report.Error (594, Location, error_details, "double");
				}
				break;
			}
			return t;
		}

		int handle_hex ()
		{
			int d;
			ulong ul;
			
			getChar ();
			while ((d = peekChar ()) != -1){
				if (is_hex (d)){
					number_builder [number_pos++] = (char) d;
					getChar ();
				} else
					break;
			}
			
			string s = new String (number_builder, 0, number_pos);
			try {
				if (number_pos <= 8)
					ul = System.UInt32.Parse (s, NumberStyles.HexNumber);
				else
					ul = System.UInt64.Parse (s, NumberStyles.HexNumber);
			} catch (OverflowException){
				error_details = "Integral constant is too large";
				Report.Error (1021, Location, error_details);
				val = 0ul;
				return Token.LITERAL_INTEGER;
			}
			catch (FormatException) {
				Report.Error (1013, Location, "Invalid number");
				val = 0ul;
				return Token.LITERAL_INTEGER;
			}
			
			return integer_type_suffix (ul, peekChar ());
		}

		//
		// Invoked if we know we have .digits or digits
		//
		int is_number (int c)
		{
			bool is_real = false;
			int type;

			number_pos = 0;

			if (c >= '0' && c <= '9'){
				if (c == '0'){
					int peek = peekChar ();

					if (peek == 'x' || peek == 'X')
						return handle_hex ();
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
					number_pos--;
					return adjust_int (-1);
				}
			}
			
			if (c == 'e' || c == 'E'){
				is_real = true;
				if (number_pos == max_number_size)
					Error_NumericConstantTooLong ();
				number_builder [number_pos++] = 'e';
				c = getChar ();
				
				if (c == '+'){
					if (number_pos == max_number_size)
						Error_NumericConstantTooLong ();
					number_builder [number_pos++] = '+';
					c = -1;
				} else if (c == '-') {
					if (number_pos == max_number_size)
						Error_NumericConstantTooLong ();
					number_builder [number_pos++] = '-';
					c = -1;
				} else {
					if (number_pos == max_number_size)
						Error_NumericConstantTooLong ();
					number_builder [number_pos++] = '+';
				}
					
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
				Report.Error (1009, Location, "Unrecognized escape sequence `\\{0}'", ((char)d).ToString ());
				return d;
			}
			getChar ();
			return v;
		}

		int getChar ()
		{
			int x;
			if (putback_char != -1) {
				x = putback_char;
				putback_char = -1;
			} else
				x = reader.Read ();
			if (x == '\n') {
				line++;
				ref_line++;
				previous_col = col;
				col = 0;
			}
			else
				col++;
			return x;
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
				Console.WriteLine ("Name: " + ref_name.Name);
				Console.WriteLine ("Current [{0}] putting back [{1}]  ", putback_char, c);
				throw new Exception ("This should not happen putback on putback");
			}
			if (c == '\n' || col == 0) {
				// It won't happen though.
				line--;
				ref_line--;
				col = previous_col;
			}
			else
				col--;
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

		static bool IsCastToken (int token)
		{
			switch (token) {
			case Token.BANG:
			case Token.TILDE:
			case Token.IDENTIFIER:
			case Token.LITERAL_INTEGER:
			case Token.LITERAL_FLOAT:
			case Token.LITERAL_DOUBLE:
			case Token.LITERAL_DECIMAL:
			case Token.LITERAL_CHARACTER:
			case Token.LITERAL_STRING:
			case Token.BASE:
			case Token.CHECKED:
			case Token.DELEGATE:
			case Token.FALSE:
			case Token.FIXED:
			case Token.NEW:
			case Token.NULL:
			case Token.SIZEOF:
			case Token.THIS:
			case Token.THROW:
			case Token.TRUE:
			case Token.TYPEOF:
			case Token.UNCHECKED:
			case Token.UNSAFE:

				//
				// These can be part of a member access
				//
			case Token.INT:
			case Token.UINT:
			case Token.SHORT:
			case Token.USHORT:
			case Token.LONG:
			case Token.ULONG:
			case Token.DOUBLE:
			case Token.FLOAT:
			case Token.CHAR:
				return true;

			default:
				return false;
			}
		}

		public int token ()
                {
			current_token = xtoken ();

			if (current_token != Token.DEFAULT)
				return current_token;

			int c = consume_whitespace ();
			if (c == -1)
				current_token = Token.ERROR;
			else if (c == '(')
				current_token = Token.DEFAULT_OPEN_PARENS;
			else
				putback (c);

			return current_token;
                }

		static StringBuilder static_cmd_arg = new System.Text.StringBuilder ();
		
		void get_cmd_arg (out string cmd, out string arg)
		{
			int c;
			
			tokens_seen = false;
			arg = "";
			static_cmd_arg.Length = 0;

			// skip over white space
			while ((c = getChar ()) != -1 && (c != '\n') && ((c == '\r') || (c == ' ') || (c == '\t')))
				;
				
			while ((c != -1) && (c != '\n') && (c != ' ') && (c != '\t') && (c != '\r')){
 				if (is_identifier_part_character ((char) c)){
					static_cmd_arg.Append ((char) c);
					c = getChar ();
 				} else {
 					putback (c);
 					break;
 				}
			}

			cmd = static_cmd_arg.ToString ();

			if (c == '\n'){
				return;
			}

			// skip over white space
			while ((c = getChar ()) != -1 && (c != '\n') && ((c == '\r') || (c == ' ') || (c == '\t')))
				;

			if (c == '\n'){
				return;
			} else if (c == '\r'){
				return;
			} else if (c == -1){
				arg = "";
				return;
			}
			
			static_cmd_arg.Length = 0;
			static_cmd_arg.Append ((char) c);
			
			while ((c = getChar ()) != -1 && (c != '\n') && (c != '\r')){
				static_cmd_arg.Append ((char) c);
			}

			arg = static_cmd_arg.ToString ().Trim ();
		}

		//
		// Handles the #line directive
		//
		bool PreProcessLine (string arg)
		{
			if (arg.Length == 0)
				return false;

			if (arg == "default"){
				ref_line = line;
				ref_name = file_name;
				Location.Push (ref_name);
				return true;
			} else if (arg == "hidden"){
				//
				// We ignore #line hidden
				//
				return true;
			}
			
			try {
				int pos;

				if ((pos = arg.IndexOf (' ')) != -1 && pos != 0){
					ref_line = System.Int32.Parse (arg.Substring (0, pos));
					pos++;
					
					char [] quotes = { '\"' };
					
					string name = arg.Substring (pos). Trim (quotes);
					ref_name = Location.LookupFile (name);
					file_name.HasLineDirective = true;
					ref_name.HasLineDirective = true;
					Location.Push (ref_name);
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
			if (arg.Length == 0 || arg == "true" || arg == "false"){
				Report.Error (1001, Location, "Missing identifer to pre-processor directive");
				return;
			}

			char[] whitespace = { ' ', '\t' };
			if (arg.IndexOfAny (whitespace) != -1){
				Report.Error (1025, Location, "Single-line comment or end-of-line expected");
				return;
			}

			if (!is_identifier_start_character (arg [0]))
				Report.Error (1001, Location, "Identifier expected: " + arg);
			
			foreach (char c in arg.Substring (1)){
				if (!is_identifier_part_character (c)){
					Report.Error (1001, Location, "Identifier expected: " + arg);
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

		/// <summary>
		/// Handles #pragma directive
		/// </summary>
		void PreProcessPragma (string arg)
		{
			const string warning = "warning";
			const string w_disable = "warning disable";
			const string w_restore = "warning restore";

			if (arg == w_disable) {
				Report.RegisterWarningRegion (Location).WarningDisable (line);
				return;
			}

			if (arg == w_restore) {
				Report.RegisterWarningRegion (Location).WarningEnable (line);
				return;
			}

			if (arg.StartsWith (w_disable)) {
				int[] codes = ParseNumbers (arg.Substring (w_disable.Length));
				foreach (int code in codes) {
					if (code != 0)
						Report.RegisterWarningRegion (Location).WarningDisable (Location, code);
				}
				return;
			}

			if (arg.StartsWith (w_restore)) {
				int[] codes = ParseNumbers (arg.Substring (w_restore.Length));
				Hashtable w_table = Report.warning_ignore_table;
				foreach (int code in codes) {
					if (w_table != null && w_table.Contains (code))
						Report.Warning (1635, 1, Location, String.Format ("Cannot restore warning `CS{0:0000}' because it was disabled globally", code));
					Report.RegisterWarningRegion (Location).WarningEnable (Location, code);
				}
				return;
			}

			if (arg.StartsWith (warning)) {
				Report.Warning (1634, 1, Location, "Expected disable or restore");
				return;
			}

			Report.Warning (1633, 1, Location, "Unrecognized #pragma directive");
		}

		int[] ParseNumbers (string text)
		{
			string[] string_array = text.Split (',');
			int[] values = new int [string_array.Length];
			int index = 0;
			foreach (string string_code in string_array) {
				try {
					values[index++] = int.Parse (string_code, System.Globalization.CultureInfo.InvariantCulture);
				}
				catch (FormatException) {
					Report.Warning (1692, 1, Location, "Invalid number");
				}
			}
			return values;
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
						return (va & pp_and (ref s));
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
						return va | pp_expr (ref s);
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
				return false;
			}

			return v;
		}
		
		void Error_InvalidDirective ()
		{
			Report.Error (1517, Location, "Invalid preprocessor directive");
		}

		void Error_UnexpectedDirective (string extra)
		{
			Report.Error (
				1028, Location,
				"Unexpected processor directive (" + extra + ")");
		}

		void Error_TokensSeen ()
		{
			Report.Error (1032, Location,
				"Cannot define or undefine preprocessor symbols after first token in file");
		}
		
		//
		// if true, then the code continues processing the code
		// if false, the code stays in a loop until another directive is
		// reached.
		//
		bool handle_preprocessing_directive (bool caller_is_taking)
		{
			string cmd, arg;
			bool region_directive = false;

			current_location = new Location (ref_line, Col);

			get_cmd_arg (out cmd, out arg);

			// Eat any trailing whitespaces and single-line comments
			if (arg.IndexOf ("//") != -1)
				arg = arg.Substring (0, arg.IndexOf ("//"));
			arg = arg.TrimEnd (' ', '\t');

			//
			// The first group of pre-processing instructions is always processed
			//
			switch (cmd){
			case "pragma":
				if (RootContext.Version == LanguageVersion.ISO_1) {
					Report.FeatureIsNotStandardized (Location, "#pragma");
					return caller_is_taking;
				}

				PreProcessPragma (arg);
				return caller_is_taking;
				
			case "line":
				if (!PreProcessLine (arg))
					Report.Error (
						1576, Location,
						"The line number specified for #line directive is missing or invalid");
				return caller_is_taking;

			case "region":
				region_directive = true;
				arg = "true";
				goto case "if";

			case "endregion":
				region_directive = true;
				goto case "endif";
				
			case "if":
				if (arg.Length == 0){
					Error_InvalidDirective ();
					return true;
				}
				bool taking = false;
				if (ifstack == null)
					ifstack = new Stack (2);

				if (ifstack.Count == 0){
					taking = true;
				} else {
					int state = (int) ifstack.Peek ();
					if ((state & TAKING) != 0)
						taking = true;
				}

				if (eval (arg) && taking){
					int push = TAKING | TAKEN_BEFORE | PARENT_TAKING;
					if (region_directive)
						push |= REGION;
					ifstack.Push (push);
					return true;
				} else {
					int push = (taking ? PARENT_TAKING : 0);
					if (region_directive)
						push |= REGION;
					ifstack.Push (push);
					return false;
				}
				
			case "endif":
				if (ifstack == null || ifstack.Count == 0){
					Error_UnexpectedDirective ("no #if for this #endif");
					return true;
				} else {
					int pop = (int) ifstack.Pop ();
					
					if (region_directive && ((pop & REGION) == 0))
						Report.Error (1027, Location, "Expected `#endif' directive");
					else if (!region_directive && ((pop & REGION) != 0))
						Report.Error (1038, Location, "#endregion directive expected");
					
					if (!region_directive && arg.Length != 0) {
						Report.Error (1025, Location, "Single-line comment or end-of-line expected");
					}
					
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

					if ((state & REGION) != 0) {
						Report.Error (1038, Location, "#endregion directive expected");
						return true;
					}

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
					Error_UnexpectedDirective ("no #if for this #else");
					return true;
				} else {
					int state = (int) ifstack.Peek ();

					if ((state & REGION) != 0) {
						Report.Error (1038, Location, "#endregion directive expected");
						return true;
					}

					if ((state & ELSE_SEEN) != 0){
						Error_UnexpectedDirective ("#else within #else");
						return true;
					}

					ifstack.Pop ();

					bool ret;
					if ((state & TAKEN_BEFORE) == 0){
						ret = ((state & PARENT_TAKING) != 0);
					} else
						ret = false;
					
					if (ret)
						state |= TAKING;
					else
						state &= ~TAKING;
					
					ifstack.Push (state | ELSE_SEEN);
					
					return ret;
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
				Report.Warning (1030, 1, Location, "#warning: `{0}'", arg);
				return true;
			}

			Report.Error (1024, Location, "Wrong preprocessor directive");
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
				}

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

		private int consume_identifier (int s)
		{
			int res = consume_identifier (s, false);

			if (doc_state == XmlCommentState.Allowed)
				doc_state = XmlCommentState.NotAllowed;
			switch (res) {
			case Token.USING:
			case Token.NAMESPACE:
				check_incorrect_doc_comment ();
				break;
			}

			if (res == Token.PARTIAL) {
				// Save current position and parse next token.
				PushPosition ();

				int next_token = token ();
				bool ok = (next_token == Token.CLASS) ||
					(next_token == Token.STRUCT) ||
					(next_token == Token.INTERFACE) ||
					(next_token == Token.ENUM); // "partial" is a keyword in 'partial enum', even though it's not valid

				PopPosition ();

				if (ok)
					return res;
				else {
					val = new LocatedToken (Location, "partial");
					return Token.IDENTIFIER;
				}
			}

			return res;
		}

		private int consume_identifier (int s, bool quoted) 
		{
			int pos = 1;
			int c = -1;
			
			id_builder [0] = (char) s;
					
			current_location = new Location (ref_line, Col);

			while ((c = getChar ()) != -1) {
			loop:
				if (is_identifier_part_character ((char) c)){
					if (pos == max_id_size){
						Report.Error (645, Location, "Identifier too long (limit is 512 chars)");
						return Token.ERROR;
					}
					
					id_builder [pos++] = (char) c;
//					putback_char = -1;
				} else if (c == '\\') {
					c = escape (c);
					goto loop;
				} else {
//					putback_char = c;
					putback (c);
					break;
				}
			}

			//
			// Optimization: avoids doing the keyword lookup
			// on uppercase letters and _
			//
			if (!quoted && (s >= 'a' || s == '_')){
				int keyword = GetKeyword (id_builder, pos);
				if (keyword != -1) {
					val = Location;
				return keyword;
				}
			}

			//
			// Keep identifiers in an array of hashtables to avoid needless
			// allocations
			//

			if (identifiers [pos] != null) {
				val = identifiers [pos][id_builder];
				if (val != null) {
					val = new LocatedToken (Location, (string) val);
					if (quoted)
						escapedIdentifiers.Add (val);
					return Token.IDENTIFIER;
				}
			}
			else
				identifiers [pos] = new CharArrayHashtable (pos);

			val = new String (id_builder, 0, pos);
			if (RootContext.Version == LanguageVersion.ISO_1) {
				for (int i = 1; i < id_builder.Length; i += 3) {
					if (id_builder [i] == '_' && (id_builder [i - 1] == '_' || id_builder [i + 1] == '_')) {
						Report.Error (1638, Location, 
							"`{0}': Any identifier with double underscores cannot be used when ISO language version mode is specified", val.ToString ());
						break;
					}
				}
			}

			char [] chars = new char [pos];
			Array.Copy (id_builder, chars, pos);

			identifiers [pos] [chars] = val;

			val = new LocatedToken (Location, (string) val);
			if (quoted)
				escapedIdentifiers.Add (val);
			return Token.IDENTIFIER;
		}

		int consume_whitespace ()
		{
			int c;

			// Whether we have seen comments on the current line
			bool comments_seen = false;
			
			val = null;
			// optimization: eliminate col and implement #directive semantic correctly.
			for (;(c = getChar ()) != -1;) {
				if (c == ' ')
					continue;
				
				if (c == '\t') {
					continue;
				}
				
				if (c == ' ' || c == '\f' || c == '\v' || c == 0xa0)
					continue;

				if (c == '\r') {
					if (peekChar () == '\n')
						getChar ();

					any_token_seen |= tokens_seen;
					tokens_seen = false;
					comments_seen = false;
					continue;
				}

				// Handle double-slash comments.
				if (c == '/'){
					int d = peekChar ();
				
					if (d == '/'){
						getChar ();
						if (RootContext.Documentation != null && peekChar () == '/') {
							getChar ();
							// Don't allow ////.
							if ((d = peekChar ()) != '/') {
								update_comment_location ();
								if (doc_state == XmlCommentState.Allowed)
									handle_one_line_xml_comment ();
								else if (doc_state == XmlCommentState.NotAllowed)
									warn_incorrect_doc_comment ();
							}
						}
						while ((d = getChar ()) != -1 && (d != '\n') && d != '\r')
						if (d == '\n'){
						}
						any_token_seen |= tokens_seen;
						tokens_seen = false;
						comments_seen = false;
						continue;
					} else if (d == '*'){
						getChar ();
						bool docAppend = false;
						if (RootContext.Documentation != null && peekChar () == '*') {
							getChar ();
							update_comment_location ();
							// But when it is /**/, just do nothing.
							if (peekChar () == '/') {
								getChar ();
								continue;
							}
							if (doc_state == XmlCommentState.Allowed)
								docAppend = true;
							else if (doc_state == XmlCommentState.NotAllowed)
								warn_incorrect_doc_comment ();
						}

						int current_comment_start = 0;
						if (docAppend) {
							current_comment_start = xml_comment_buffer.Length;
							xml_comment_buffer.Append (Environment.NewLine);
						}

						Location start_location = Location;

						while ((d = getChar ()) != -1){
							if (d == '*' && peekChar () == '/'){
								getChar ();
								comments_seen = true;
								break;
							}
							if (docAppend)
								xml_comment_buffer.Append ((char) d);
							
							if (d == '\n'){
								any_token_seen |= tokens_seen;
								tokens_seen = false;
								// 
								// Reset 'comments_seen' just to be consistent.
								// It doesn't matter either way, here.
								//
								comments_seen = false;
							}
						}
						if (!comments_seen)
							Report.Error (1035, start_location, "End-of-file found, '*/' expected");

						if (docAppend)
							update_formatted_doc_comment (current_comment_start);
						continue;
					}
					goto is_punct_label;
				}

			is_punct_label:
				// white space
				if (c == '\n'){
					any_token_seen |= tokens_seen;
					tokens_seen = false;
					comments_seen = false;
					continue;
				}

				/* For now, ignore pre-processor commands */
				// FIXME: In C# the '#' is not limited to appear
				// on the first column.
				if (c == '#') {
					bool cont = true;
					
					if (tokens_seen || comments_seen) {
                                               error_details = "Preprocessor directives must appear as the first non-whitespace " +
                                                       "character on a line.";

                                               Report.Error (1040, Location, error_details);

                                               return Token.ERROR;
                                       }
					
				start_again:
					
					cont = handle_preprocessing_directive (cont);

					if (cont){
						continue;
					}

					bool skipping = false;
					for (;(c = getChar ()) != -1;){
						if (c == '\n'){
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
						Report.Error (1027, Location, "Expected `#endif' directive");
					continue;
				}

				return c;
			}

			return -1;
		}
		
		public int xtoken ()
		{
			int t;
			bool doread = false;
			int c;

			val = null;
			// optimization: eliminate col and implement #directive semantic correctly.

			c = consume_whitespace ();
			if (c == -1)
				return Token.EOF;

			if (c == '\\' || is_identifier_start_character ((char)c)){
				tokens_seen = true;
				return consume_identifier (c);
			}

			current_location = new Location (ref_line, Col);
			if ((t = is_punct ((char)c, ref doread)) != Token.ERROR){
				tokens_seen = true;
				if (doread){
					getChar ();
					col++;
				}
				return t;
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
				if (c == '\r' || c == '\n') {
					Report.Error (1010, Location, "Newline in constant");
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
						if (c == '\n'){
							break;
						}
						else if (c == '\'')
							break;
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
					Report.Error (1646, Location, "Keyword, identifier, or string expected after verbatim specifier: @");
				}
			}

			if (c == '#') {
				error_details = "Preprocessor directives must appear as the first non-whitespace " +
					"character on a line.";

				Report.Error (1040, Location, error_details);

				return Token.ERROR;
			}

			error_details = ((char)c).ToString ();

			return Token.ERROR;
		}

		//
		// Handles one line xml comment
		//
		private void handle_one_line_xml_comment ()
		{
			int c;
			while ((c = peekChar ()) == ' ')
				getChar (); // skip heading whitespaces.
			while ((c = peekChar ()) != -1 && c != '\n' && c != '\r') {
				xml_comment_buffer.Append ((char) getChar ());
			}
			if (c == '\r' || c == '\n')
				xml_comment_buffer.Append (Environment.NewLine);
		}

		//
		// Remove heading "*" in Javadoc-like xml documentation.
		//
		private void update_formatted_doc_comment (int current_comment_start)
		{
			int length = xml_comment_buffer.Length - current_comment_start;
			string [] lines = xml_comment_buffer.ToString (
				current_comment_start,
				length).Replace ("\r", "").Split ('\n');
			
			// The first line starts with /**, thus it is not target
			// for the format check.
			for (int i = 1; i < lines.Length; i++) {
				string s = lines [i];
				int idx = s.IndexOf ('*');
				string head = null;
				if (idx < 0) {
					if (i < lines.Length - 1)
						return;
					head = s;
				} else
					head = s.Substring (0, idx);
				foreach (char c in head)
					if (c != ' ')
						return;
				lines [i] = s.Substring (idx + 1);
			}
			xml_comment_buffer.Remove (current_comment_start, length);
			xml_comment_buffer.Insert (current_comment_start, String.Join (Environment.NewLine, lines));
		}

		//
		// Updates current comment location.
		//
		private void update_comment_location ()
		{
			if (current_comment_location.IsNull) {
				// "-2" is for heading "//" or "/*"
				current_comment_location =
					new Location (ref_line, col - 2);
			}
		}

		//
		// Checks if there was incorrect doc comments and raise
		// warnings.
		//
		public void check_incorrect_doc_comment ()
		{
			if (xml_comment_buffer.Length > 0)
				warn_incorrect_doc_comment ();
		}

		//
		// Raises a warning when tokenizer found incorrect doccomment
		// markup.
		//
		private void warn_incorrect_doc_comment ()
		{
			if (doc_state != XmlCommentState.Error) {
				doc_state = XmlCommentState.Error;
				// in csc, it is 'XML comment is not placed on 
				// a valid language element'. But that does not
				// make sense.
				Report.Warning (1587, 2, Location, "XML comment is not placed on a valid language element");
			}
		}

		//
		// Consumes the saved xml comment lines (if any)
		// as for current target member or type.
		//
		public string consume_doc_comment ()
		{
			if (xml_comment_buffer.Length > 0) {
				string ret = xml_comment_buffer.ToString ();
				reset_doc_comment ();
				return ret;
			}
			return null;
		}

		void reset_doc_comment ()
		{
			xml_comment_buffer.Length = 0;
			current_comment_location = Location.Null;
		}

		public void cleanup ()
		{
			if (ifstack != null && ifstack.Count >= 1) {
				int state = (int) ifstack.Pop ();
				if ((state & REGION) != 0)
					Report.Error (1038, Location, "#endregion directive expected");
				else 
					Report.Error (1027, Location, "Expected `#endif' directive");
			}
				
		}
	}

	//
	// Indicates whether it accepts XML documentation or not.
	//
	public enum XmlCommentState {
		// comment is allowed in this state.
		Allowed,
		// comment is not allowed in this state.
		NotAllowed,
		// once comments appeared when it is NotAllowed, then the
		// state is changed to it, until the state is changed to
		// .Allowed.
		Error
	}
}

