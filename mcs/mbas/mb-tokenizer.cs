//
// Mono.MonoBASIC.Tokenizer.cs: The Tokenizer for the MonoBASIC compiler
//
// Author: A Rafael D Teixeira (rafaelteixeirabr@hotmail.com)
//	   
// Based on cs-tokenizer.cs by Miguel de Icaza (miguel@gnu.org)
//
// Licensed under the terms of the GNU GPL
//
// Copyright (C) 2001 A Rafael D Teixeira
//

namespace Mono.MonoBASIC
{
	using System;
	using System.Text;
	using System.Collections;
	using System.IO;
	using System.Globalization;
	using Mono.Languages;
	using Mono.CSharp;
	
	/// <summary>
	///    Tokenizer for MonoBASIC source code. 
	/// </summary>
	
	public class Tokenizer : yyParser.yyInput
	{
		TextReader reader;
		public string ref_name;
		public int ref_line = 1;
		public int line = 1;
		public int col = 1;
		public int current_token;
		bool handle_get_set = false;

		public int ExpandedTabsSize = 4; 

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

			keywords.Add ("addhandler", Token.ADDHANDLER);
			keywords.Add ("addressof", Token.ADDRESSOF);
			keywords.Add ("alias", Token.ALIAS);
			keywords.Add ("and", Token.AND);
			keywords.Add ("andalso", Token.ANDALSO);
			keywords.Add ("ansi", Token.ANSI);
			keywords.Add ("as", Token.AS);
			keywords.Add ("assembly", Token.ASSEMBLY);
			keywords.Add ("auto", Token.AUTO);
			keywords.Add ("boolean", Token.BOOLEAN);
			keywords.Add ("byref", Token.BYREF);
			keywords.Add ("byte", Token.BYTE);
			keywords.Add ("byval", Token.BYVAL);
			keywords.Add ("call", Token.CALL);
			keywords.Add ("case", Token.CASE);
			keywords.Add ("catch", Token.CATCH);
			keywords.Add ("cbool", Token.CBOOL);
			keywords.Add ("cbyte", Token.CBYTE);
			keywords.Add ("cchar", Token.CCHAR);
			keywords.Add ("cdate", Token.CDATE);
			keywords.Add ("cdec", Token.CDEC);
			keywords.Add ("cdbl", Token.CDBL);
			keywords.Add ("char", Token.CHAR);
			keywords.Add ("cint", Token.CINT);
			keywords.Add ("class", Token.CLASS);
			keywords.Add ("clng", Token.CLNG);
			keywords.Add ("cobj", Token.COBJ);
			//keywords.Add ("compare", Token.COMPARE);
			keywords.Add ("const", Token.CONST);
			keywords.Add ("cshort", Token.CSHORT);
			keywords.Add ("csng", Token.CSNG);
			keywords.Add ("cstr", Token.CSTR);
			keywords.Add ("ctype", Token.CTYPE);
			keywords.Add ("date", Token.DATE);
			keywords.Add ("decimal", Token.DECIMAL);
			keywords.Add ("declare", Token.DECLARE);
			keywords.Add ("default", Token.DEFAULT);
			keywords.Add ("delegate", Token.DELEGATE);
			keywords.Add ("dim", Token.DIM);
			keywords.Add ("do", Token.DO);
			keywords.Add ("double", Token.DOUBLE);
			keywords.Add ("each", Token.EACH);
			keywords.Add ("else", Token.ELSE);
			keywords.Add ("elseif", Token.ELSEIF);
			keywords.Add ("end", Token.END);
			keywords.Add ("enum", Token.ENUM);
			keywords.Add ("erase", Token.ERASE);
			keywords.Add ("error", Token.ERROR);
			keywords.Add ("event", Token.EVENT);
			keywords.Add ("exit", Token.EXIT);
			//keywords.Add ("explicit", Token.EXPLICIT);
			keywords.Add ("false", Token.FALSE);
			keywords.Add ("finally", Token.FINALLY);
			keywords.Add ("for", Token.FOR);
			keywords.Add ("friend", Token.FRIEND);
			keywords.Add ("function", Token.FUNCTION);
			keywords.Add ("get", Token.GET);
			keywords.Add ("gettype", Token.GETTYPE);
			keywords.Add ("goto", Token.GOTO);
			keywords.Add ("handles", Token.HANDLES);
			keywords.Add ("if", Token.IF);
			keywords.Add ("implements", Token.IMPLEMENTS);
			keywords.Add ("imports", Token.IMPORTS);
			keywords.Add ("in", Token.IN);
			keywords.Add ("inherits", Token.INHERITS);
			keywords.Add ("integer", Token.INTEGER);
			keywords.Add ("interface", Token.INTERFACE);
			keywords.Add ("is", Token.IS);
			keywords.Add ("let ", Token.LET );
			keywords.Add ("lib ", Token.LIB );
			keywords.Add ("like ", Token.LIKE );
			keywords.Add ("long", Token.LONG);
			keywords.Add ("loop", Token.LOOP);
			keywords.Add ("me", Token.ME);
			keywords.Add ("mod", Token.MOD);
			keywords.Add ("module", Token.MODULE);
			keywords.Add ("mustinherit", Token.MUSTINHERIT);
			keywords.Add ("mustoverride", Token.MUSTOVERRIDE);
			keywords.Add ("mybase", Token.MYBASE);
			keywords.Add ("myclass", Token.MYCLASS);
			keywords.Add ("namespace", Token.NAMESPACE);
			keywords.Add ("new", Token.NEW);
			keywords.Add ("next", Token.NEXT);
			keywords.Add ("not", Token.NOT);
			keywords.Add ("nothing", Token.NOTHING);
			keywords.Add ("notinheritable", Token.NOTINHERITABLE);
			keywords.Add ("notoverridable", Token.NOTOVERRIDABLE);
			keywords.Add ("object", Token.OBJECT);
			keywords.Add ("on", Token.ON);
			keywords.Add ("option", Token.OPTION);
			keywords.Add ("optional", Token.OPTIONAL);
			keywords.Add ("or", Token.OR);
			keywords.Add ("orelse", Token.ORELSE);
			keywords.Add ("overloads", Token.OVERLOADS);
			keywords.Add ("overridable", Token.OVERRIDABLE);
			keywords.Add ("overrides", Token.OVERRIDES);
			keywords.Add ("paramarray", Token.PARAM_ARRAY);
			keywords.Add ("preserve", Token.PRESERVE);
			keywords.Add ("private", Token.PRIVATE);
			keywords.Add ("property", Token.PROPERTY);
			keywords.Add ("protected", Token.PROTECTED);
			keywords.Add ("public", Token.PUBLIC);
			keywords.Add ("raiseevent", Token.RAISEEVENT);
			keywords.Add ("readonly", Token.READONLY);
			keywords.Add ("redim", Token.REDIM);
			keywords.Add ("rem", Token.REM);
			keywords.Add ("removehandler", Token.REMOVEHANDLER);
			keywords.Add ("resume", Token.RESUME);
			keywords.Add ("return", Token.RETURN);
			keywords.Add ("select", Token.SELECT);
			keywords.Add ("set", Token.SET);
			keywords.Add ("shadows", Token.SHADOWS);
			keywords.Add ("shared", Token.SHARED);
			keywords.Add ("short", Token.SHORT);
			keywords.Add ("single", Token.SINGLE);
			keywords.Add ("sizeof", Token.SIZEOF);
			keywords.Add ("static", Token.STATIC);
			keywords.Add ("step", Token.STEP);
			keywords.Add ("stop", Token.STOP);
			keywords.Add ("string", Token.STRING);
			keywords.Add ("structure", Token.STRUCTURE);
			keywords.Add ("sub", Token.SUB);
			keywords.Add ("synclock", Token.SYNCLOCK);
			keywords.Add ("then", Token.THEN);
			keywords.Add ("throw", Token.THROW);
			keywords.Add ("to", Token.TO);
			keywords.Add ("true", Token.TRUE);
			keywords.Add ("try", Token.TRY);
			keywords.Add ("typeof", Token.TYPEOF);
			keywords.Add ("unicode", Token.UNICODE);
			keywords.Add ("until", Token.UNTIL);
			keywords.Add ("variant", Token.VARIANT);
			keywords.Add ("when", Token.WHEN);
			keywords.Add ("while", Token.WHILE);
			keywords.Add ("with", Token.WITH);
			keywords.Add ("withevents", Token.WITHEVENTS);
			keywords.Add ("writeonly", Token.WRITEONLY);
			keywords.Add ("xor", Token.XOR);
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

			res = keywords.Contains(name.ToLower());
			if ((name == "get" || name == "set") && handle_get_set == false)
				return false;
			return res;
		}

		int getKeyword (string name)
		{
			return (int) (keywords [name.ToLower()]);
		}
		
		public Location Location {
			get {
				return new Location (ref_line);
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
//			case '[':
//				return Token.OPEN_BRACKET;
//			case ']':
//				return Token.CLOSE_BRACKET;
			case '(':
				return Token.OPEN_PARENS;
			case ')':
				return Token.CLOSE_PARENS;
			case ',':
				return Token.COMMA;
			case ':':
				return Token.COLON;
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
				if (d == '=')
					t = Token.OP_SUB_ASSIGN;
				else
					return Token.MINUS;
				doread = true;
				return t;
			}

			if (c == '='){
				/*if (d == '='){
					doread = true;
					return Token.OP_EQ;
				}*/
				return Token.ASSIGN;
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

			if (c == '\\'){
				if (d == '='){
					doread = true;
					return Token.OP_IDIV_ASSIGN;
				}
				return Token.OP_IDIV;
			}

			if (c == '^'){
				if (d == '='){
					doread = true;
					return Token.OP_EXP_ASSIGN;
				}
				return Token.OP_EXP;
			}

			if (c == '<'){
				if (d == '>')
				{
					doread = true;
					return Token.OP_NE;
				}
				if (d == '='){
					doread = true;
					return Token.OP_LE;
				}
				return Token.OP_LT;
			}

			if (c == '>'){
				if (d == '='){
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
				t =  Token.LITERAL_SINGLE;
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
			case Token.LITERAL_SINGLE:
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
			// "1.1" vs "1.string" (LITERAL_SINGLE vs NUMBER DOT IDENTIFIER)
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
			return current_token != Token.EOF ;
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

		private bool IsEOL(int currentChar)
		{
			if (currentChar ==  0x0D)
			{
				if (peekChar() ==  0x0A) // if it is a CR-LF pair consume LF also
					getChar();

				return true;
			}
			return (currentChar ==  -1 || currentChar ==  0x0A || currentChar ==  0x2028 || currentChar ==  0x2029);
		}

		private int DropComments()		
		{
			int d;
			while (!IsEOL(d = getChar ()))
				col++;
			line++;
			ref_line++;
			col = 0;

			return Token.EOL;
		}	
			
		public int token ()
		{
			int lastToken = current_token;
			do
			{
				current_token = xtoken ();
				if (current_token == 0) 
					return Token.EOF;
				if (current_token == Token.REM)
					current_token = DropComments();
			} while (lastToken == Token.EOL && current_token == Token.EOL);

			return current_token;
		}

		private string GetIdentifier()
		{
			int c = getChar();
			if (is_identifier_start_character ((char) c))
				return GetIdentifier(c);
			else
				return null;
		}

		private string GetIdentifier(int c)
		{
			System.Text.StringBuilder id = new System.Text.StringBuilder ();

			id.Append ((char) c);
				
			while ((c = peekChar ()) != -1) 
			{
				if (is_identifier_part_character ((char) c))
				{
					id.Append ((char)getChar ());
					col++;
				} 
				else 
					break;
			}

			return id.ToString ();
		}

		public int xtoken ()
		{
			int t;
			bool doread = false;
			int c;

			val = null;
			for (;(c = getChar ()) != -1; col++) {
			
				// Handle line comments.
				if (c == '\'')
					return Token.REM;

				// Handle EOL.
				if (IsEOL(c))
				{
					line++;
					ref_line++;
					col = 0;
					if (current_token == Token.EOL) // if last token was also EOL keep skipping
						continue;
					return Token.EOL;
				}
				
				// Handle escaped identifiers
				if (c == '[')
				{
					if ((val = GetIdentifier()) == null)
						break;
					if ((c = getChar()) != ']')
						break;
					return Token.IDENTIFIER;
				}

				// Handle unescaped identifiers
				if (is_identifier_start_character ((char) c))
				{
					string id;
					if ((id = GetIdentifier(c)) == null)
						break;
					if (is_keyword(id))
						return getKeyword(id);
					val = id;
					return Token.IDENTIFIER;
				}

				// handle numeric literals
				if (c == '.'){
					if (Char.IsDigit ((char) peekChar ()))
						return is_number (c);
					return Token.DOT;
				}
				
				if (Char.IsDigit ((char) c))
					return is_number (c);

				/* For now, limited support for pre-processor commands */
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
				
				// Treat string literals
				if (c == '"'){
					System.Text.StringBuilder s = new System.Text.StringBuilder ();

					while ((c = getChar ()) != -1){
						if (c == '"'){ // TODO: treat double-doublequotes
							val = s.ToString ();
							return Token.LITERAL_STRING;
						}

						c = escape (c);
						if (c == -1)
							return Token.ERROR;
						s.Append ((char) c);
					}
				}
			
				// expand tabs for location and ignore it as whitespace
				if (c == '\t')
				{
					col = (((col + ExpandedTabsSize) / ExpandedTabsSize) * ExpandedTabsSize) - 1;
					continue;
				}

				// white space
				if (c == ' ' || c == '\f' || c == '\v')
					continue;

				error_details = ((char)c).ToString ();
				
				return Token.ERROR;
			}

			if (current_token != Token.EOL) // if last token wasn´t EOL send it before EOF
				return Token.EOL;
			
			return Token.EOF;
		}

		public Tokenizer (System.IO.TextReader input, string fname, ArrayList defines)
		{
			this.ref_name = fname;
			reader = input;
			putback_char = -1;
			
			Location.Push (fname);
		}

	}
}
