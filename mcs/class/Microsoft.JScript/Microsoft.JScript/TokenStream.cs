//
// TokenStream.cs: Port of Mozilla's Rhino TokenStream
//		   This class implements the JScript scanner
//

/*
 * The contents of this file are subject to the Netscape Public
 * License Version 1.1 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.mozilla.org/NPL/
 *
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
 *
 * The Original Code is Rhino code, released
 * May 6, 1999.
 *
 * The Initial Developer of the Original Code is Netscape
 * Communications Corporation.  Portions created by Netscape are
 * Copyright (C) 1997-1999 Netscape Communications Corporation. All
 * Rights Reserved.
 *
 * Contributor(s):
 * Roger Lawrence
 * Mike McCabe
 * Igor Bukanov
 * Ethan Hugg
 * Terry Lucas
 * Milen Nankov
 *
 * Alternatively, the contents of this file may be used under the
 * terms of the GNU Public License (the "GPL"), in which case the
 * provisions of the GPL are applicable instead of those above.
 * If you wish to allow use of your version of this file only
 * under the terms of the GPL and not to allow others to use your
 * version of this file under the NPL, indicate your decision by
 * deleting the provisions above and replace them with the notice
 * and other provisions required by the GPL.  If you do not delete
 * the provisions above, a recipient may use your version of this
 * file under either the NPL or the GPL.
 */

// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2004, Cesar Lopez Nataren
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
using System.IO;
using System.Collections;
using System.Globalization;

namespace Microsoft.JScript {
internal class TokenStream { 

	//
	// fields
	//
	string source_name;
	internal string SourceName {
		get { return source_name; }
		set { source_name = value; }
	}

	int line_number;
	internal int LineNumber {
		get { return line_number; }
		set { line_number = value; }
	}

	bool hit_eof;
	internal bool EOF {
		get { return hit_eof; }
	}

	int token_number;
	internal int TokenNumber {
		get { return token_number; }
		set { token_number = value; }
	}

	int pushback_token;
	
	// tokenize newlines
	bool significant_eol;

	int string_buffer_top;
	char [] string_buffer = new char [128];

	// Room backtrace from to < on failed match of the last - in <!--
	int [] unget_buffer = new int [3];
	int unget_cursor;

	int line_start;
	int line_end_char;

	string source_string;
	char [] source_buffer;
	int source_end;
	int source_cursor;

	static int EOF_CHAR = -1;
	static int EOL_HINT_MASK = 0xdfd0;

	StreamReader source_reader;

	bool dirty_line;

	string _string;
	internal string GetString {
		get { return _string; }
	}

	static bool reserved_keyword_as_identifier;

	double number;
	internal double GetNumber {
		get { return number; }
	}


	int op;
	internal int GetOp ()
	{
		return op;
	}

	internal bool allow_reg_exp;

	internal string reg_exp_flags;

	//
	// methods
	//

	internal TokenStream (StreamReader source_reader, string source_string, string source_name, int line_number)
	{
		pushback_token = Token.EOF;
		SourceName = source_name;
		this.line_number = line_number;
		if (source_reader != null) {
			this.source_reader = source_reader;
			source_buffer = new char [512];
			source_end = 0;
		} else {
			this.source_string = source_string;
			source_end = source_string.Length;
		}
		source_cursor = 0;
	}

#if false
	static bool IsKeyword (string s)
	{
		return Token.EOF != StringToKeyword (s);
	}
#endif

	static int StringToKeyword (string name)
	{
		// The following assumes that Token.EOF == 0
		int
		Id_break         = Token.BREAK,
		Id_case          = Token.CASE,
		Id_continue      = Token.CONTINUE,
		Id_default       = Token.DEFAULT,
		Id_delete        = Token.DELPROP,
		Id_do            = Token.DO,
		Id_else          = Token.ELSE,
		Id_export        = Token.EXPORT,
		Id_false         = Token.FALSE,
		Id_for           = Token.FOR,
		Id_function      = Token.FUNCTION,
		Id_if            = Token.IF,
		Id_in            = Token.IN,
		Id_new           = Token.NEW,
		Id_null          = Token.NULL,
		Id_return        = Token.RETURN,
		Id_switch        = Token.SWITCH,
		Id_this          = Token.THIS,
		Id_true          = Token.TRUE,
		Id_typeof        = Token.TYPEOF,
		Id_var           = Token.VAR,
		Id_void          = Token.VOID,
		Id_while         = Token.WHILE,
		Id_with          = Token.WITH,

		// the following are #ifdef RESERVE_JAVA_KEYWORDS in jsscan.c
		Id_abstract      = Token.RESERVED,
		Id_boolean       = Token.RESERVED,
		Id_byte          = Token.RESERVED,
		Id_catch         = Token.CATCH,
		Id_char          = Token.RESERVED,
		Id_class         = Token.RESERVED,
		Id_const         = Token.RESERVED,
		Id_debugger      = Token.RESERVED,
		Id_double        = Token.RESERVED,
		Id_enum          = Token.RESERVED,
		Id_extends       = Token.RESERVED,
		Id_final         = Token.RESERVED,
		Id_finally       = Token.FINALLY,
		Id_float         = Token.RESERVED,
		Id_goto          = Token.RESERVED,
		Id_implements    = Token.RESERVED,
		Id_import        = Token.IMPORT,
		Id_instanceof    = Token.INSTANCEOF,
		Id_int           = Token.RESERVED,
		Id_interface     = Token.RESERVED,
		Id_long          = Token.RESERVED,
		Id_native        = Token.RESERVED,
		Id_package       = Token.RESERVED,
		Id_private       = Token.RESERVED,
		Id_protected     = Token.RESERVED,
		Id_public        = Token.RESERVED,
		Id_short         = Token.RESERVED,
		Id_static        = Token.RESERVED,
		Id_super         = Token.RESERVED,
		Id_synchronized  = Token.RESERVED,
		Id_throw         = Token.THROW,
		Id_throws        = Token.RESERVED,
		Id_transient     = Token.RESERVED,
		Id_try           = Token.TRY,
		Id_volatile      = Token.RESERVED;

		int id;
		string s = name;

		L0: { 
			id = 0; 
			string X = String.Empty; 
			int c; 

			L: {
				switch (s.Length) {
				case 2: c = s [1];
					if (c == 'f') {
						if (s [0] == 'i') {
							id = Id_if;
							goto LEAVE_L0;
						}
					} else if (c == 'n') {
						if (s [0] == 'i') {
							id = Id_in;
							goto LEAVE_L0;
						}
					} else if (c == 'o') {
				    		if (s [0] == 'd') {
					    		id = Id_do;
							goto LEAVE_L0;
				    		}
			    		}
			    		goto LEAVE_L;
		    		case 3: 
			    		switch (s [0]) {
			    		case 'f':
						if (s [2] == 'r' && s [1] == 'o') {
					    		id = Id_for;
							goto LEAVE_L0;
				    		}
				    	goto LEAVE_L;
			    		case 'i':
				    		if (s [2] == 't' && s [1] == 'n') {
					    		id = Id_int;
							goto LEAVE_L0;
				    		}
				    		goto LEAVE_L;
			    		case 'n':
				    		if (s [2] == 'w' && s [1] == 'e') {
					    		id = Id_new;
							goto LEAVE_L0;
				    		}
				    		goto LEAVE_L;
			    		case 't':
				    		if (s [2] == 'y' && s [1] == 'r') {
					    		id = Id_try;
							goto LEAVE_L0;
				    		}
				    		goto LEAVE_L;
			    		case 'v':
				    		if (s [2] == 'r' && s [1] == 'a') {
					    		id = Id_var;
							goto LEAVE_L0;
				    		}
				    		goto LEAVE_L;
			    		}
			    		goto LEAVE_L;
				case 4:
					switch (s [0]) {
					case 'b':
						X = "byte";
						id = Id_byte;
						goto LEAVE_L;
					case 'c':
						c = s [3];
						if (c == 'e') {
							if (s [2] == 's' && s [1] == 'a') {
								id = Id_case;
								goto LEAVE_L0;
							}
						} else if (c == 'r') {
							if (s [2] == 'a' && s [1] == 'h') {
								id = Id_char;
								goto LEAVE_L0;
							}
						}
						goto LEAVE_L;
					case 'e':
						c = s [3];
						if (c == 'e') {
							if (s [2] == 's' && s [1] == 'l') {
								id = Id_else;
								goto LEAVE_L0;
							}
						} else if (c == 'm') {
							if (s [2] == 'u' && s [1] == 'n') {
								id = Id_enum;
								goto LEAVE_L0;
							}
						}
						goto LEAVE_L;
					case 'g':
						X = "goto";
						id = Id_goto;
						goto LEAVE_L;
					case 'l':
						X = "long";
						id = Id_long;
						goto LEAVE_L;
					case 't':
						c = s [3];
						if (c == 'e') {
							if (s [2] == 'u' && s [1] == 'r') {
								id = Id_true;
								goto LEAVE_L0;
							}
						} else if (c == 's') {
							if (s [2] == 'i' && s [1] == 'h') {
								id = Id_this;
								goto LEAVE_L0;
							}
						}
						goto LEAVE_L;
					case 'v':
						X = "void";
						id = Id_void;
						goto LEAVE_L;
					case 'w':
						X = "with";
						id = Id_with;
						goto LEAVE_L;
					}
					goto LEAVE_L;
				case 5:
					switch (s [2]) {
					case 'a':
						X = "class";
						id = Id_class;
						goto LEAVE_L;
					case 'e':
						X = "break";
						id = Id_break;
						goto LEAVE_L;
					case 'i':
						X = "while";
						id = Id_while;
						goto LEAVE_L;
					case 'l':
						X = "false";
						id = Id_false;
						goto LEAVE_L;
					case 'n':
						c = s [0];
						if (c == 'c') {
							X = "const";
							id = Id_const;
						} else if (c == 'f') {
							X = "final";
							id = Id_final;
						}
						goto LEAVE_L;
					case 'o':
						c = s [0];
						if (c == 'f') {
							X = "float";
							id = Id_float;
						} else if (c == 's') {
							X = "short";
							id = Id_short;
						}
						goto LEAVE_L;
					case 'p':
						X = "super";
						id = Id_super;
						goto LEAVE_L;
					case 'r':
						X = "throw";
						id = Id_throw;
						goto LEAVE_L;
					case 't':
						X = "catch";
						id = Id_catch;
						goto LEAVE_L;				    
					}
					goto LEAVE_L;
				case 6:
					switch (s [1]) {
					case 'a':
						X = "native";
						id = Id_native;
						goto LEAVE_L;
					case 'e':
						c = s [0];
						if (c == 'd') {
							X = "delete";
							id = Id_delete;
						} else if (c == 'r') {
							X = "return";
							id = Id_return;
						}
						goto LEAVE_L;
					case 'h':
						X = "throws";
						id = Id_throws;
						goto LEAVE_L;
					case 'm':
						X = "import";
						id = Id_import;
						goto LEAVE_L;
					case 'o':
						X = "double";
						id = Id_double;
						goto LEAVE_L;
					case 't':
						X = "static";
						id = Id_static;
						goto LEAVE_L;
					case 'u':
						X = "public";
						id = Id_public;
						goto LEAVE_L;
					case 'w':
						X = "switch";
						id = Id_switch;
						goto LEAVE_L;
					case 'x':
						X = "export";
						id = Id_export;
						goto LEAVE_L;
					case 'y':
						X = "typeof";
						id = Id_typeof;
						goto LEAVE_L;
					}
					goto LEAVE_L;
				case 7:
					switch (s [1]) {
					case 'a':
						X = "package";
						id = Id_package;
						goto LEAVE_L;
					case 'e':
						X = "default";
						id = Id_default;
						goto LEAVE_L;
					case 'i':
						X = "finally";
						id = Id_finally;
						goto LEAVE_L;
					case 'o':
						X = "boolean";
						id = Id_boolean;
						goto LEAVE_L;
					case 'r':
						X = "private";
						id = Id_private;
						goto LEAVE_L;
					case 'x':
						X = "extends";
						id = Id_extends;
						goto LEAVE_L;
					}
					goto LEAVE_L;
				case 8:
					switch (s [0]) {
					case 'a':
						X = "abstract";
						id = Id_abstract;
						goto LEAVE_L;
					case 'c':
						X = "continue";
						id = Id_continue;
						goto LEAVE_L;
					case 'd':
						X = "debbuger";
						id = Id_debugger;
						goto LEAVE_L;
					case 'f':
						X = "function";
						id = Id_function;
						goto LEAVE_L;
					case 'v':
						X = "volatile";
						id = Id_volatile;
						goto LEAVE_L;
					}
					goto LEAVE_L;
				case 9:
					c = s [0];
					if (c == 'i') {
						X = "interface";
						id = Id_interface;
					} else if (c == 'p') {
						X = "protected";
						id = Id_protected;
					} else if (c == 't') {
						X = "transient";
						id = Id_transient;
					}
					goto LEAVE_L;
				case 10:
					c = s [1];
					if (c == 'm') {
						X = "implements";
						id = Id_implements;
					} else if (c == 'n') {
						X = "instanceof";
						id = Id_instanceof;
					}
					goto LEAVE_L;
				case 12:
					X = "synchronized";
					id = Id_synchronized;
					goto LEAVE_L;
				}
			}
			LEAVE_L:
				if (X != null && X != s && !X.Equals (s))
					id = 0;
		}
		LEAVE_L0: 
		if (id == 0)
			return Token.EOF;
		return id & 0xff;
	}

	
	//
	// return and pop the token from the stream if it matches otherwise return null
	//
	internal bool MatchToken (int to_match) 
	{
		int token = GetToken ();
		if (token == to_match)
			return true;
		// did not match, push back the token
		TokenNumber--;
		pushback_token = token;
		return false;
	}

	internal void UnGetToken (int tt)
	{
		// Can not unreadmore than one token
#if false
		if (pushback_token != Token.EOF && tt != Token.ERROR)
			;
#endif
		pushback_token = tt;
		TokenNumber--;
	}

	internal int PeekToken ()
	{
		int result = GetToken ();
		pushback_token = result;
		TokenNumber--;
		return result;
	}

	internal int PeekTokenSameLine ()
	{
		significant_eol = true;
		int result = GetToken ();
		pushback_token = result;
		TokenNumber--;
		significant_eol = false;
		return result;
	}

	internal int GetToken ()
	{
		int c;
		TokenNumber++;

		// Check for pushed-back token
		if (pushback_token != Token.EOF) {
			int result = pushback_token;
			pushback_token = Token.EOF;
			if (result != Token.EOL || significant_eol)
				return result;
		}

		retry:
			for (;;) {
				// Eat whitespace, possibly sensitive to newlines
				for (;;) {
					c = GetChar ();
					if (c == EOF_CHAR)
						return Token.EOF;
					else if (c == '\n') {
						dirty_line = false;
						if (significant_eol) 
							return Token.EOL;
					} else if (!IsJSSpace (c)) {
						if (c != '-') 
							dirty_line = true;
						break;
					}
				}
				
				// identifier/keyword/instanceof?
				// watch out for starting with a <backslash>
				bool identifier_start;
				bool is_unicode_escape_start = false;

				if (c == '\\') {
					c = GetChar ();
					if (c == 'u') {
						identifier_start = true;
						is_unicode_escape_start = true;
						string_buffer_top = 0;
					} else {
						identifier_start = false;
						UnGetChar (c);
						c = '\\';
					}
				} else {
					identifier_start = IsJavaIdentifierStart ((char) c);
					if (identifier_start) {
						string_buffer_top = 0;
						AddToString (c);
					}
				}
				
				if (identifier_start) {
					bool contains_escape = is_unicode_escape_start;
					for (;;) {
						if (is_unicode_escape_start) {
							// strictly speaking we should probably push-back
							// all the bad characters if the <backslash>uXXXX
							// sequence is malformed. But since there isn't a
							// correct context(is there?) for a bad Unicode
							// escape sequence in an identifier, we can report
							// an error here.
							int escape_val = 0;
							for (int i = 0; i != 4; ++i) {
								c = GetChar ();
								escape_val = (escape_val << 4) | xDigitToInt (c);
								// Next check takes care about c < 0 and bad escape
								if (escape_val < 0)
									break;
							}
							if (escape_val < 0) {
								ReportCurrentLineError ("msg.invalid.escape");
								return Token.ERROR;
							}
							AddToString (escape_val);
							is_unicode_escape_start = false;
						} else {
							c = GetChar ();
							if (c == '\\') {
								c = GetChar ();
								if (c == 'u') {
									is_unicode_escape_start = true;
									contains_escape = true;
								} else {
									ReportCurrentLineError ("msg.illegal.character");
									return Token.ERROR;
								}
							} else {
								if (c == EOF_CHAR || !IsJavaIdentifierPart ((char) c))
									break;
								AddToString (c);
							}
						}
					}
					UnGetChar (c);

					string str = GetStringFromBuffer ();
					if (!contains_escape) {
						// OPT we shouldn't have to make a string (object!) to
						// check if it's a keyword.
						
						// Return the corresponding token if it's a keyword
						int result = StringToKeyword (str);
						if (result != Token.EOF) {
							if (result != Token.RESERVED) 
								return result;
							else if (!reserved_keyword_as_identifier)
								return result;
							else {
								// If implementation permits to use future reserved
								// keywords in violation with the EcmaScript,
								// treat it as name but issue warning
								ReportCurrentLineWarning ("msg.reserved.keyword", str);
								Console.WriteLine ("Warning: using future reserved keyword as name");
							}
						}
					}					
					_string = String.Intern (str);
					return Token.NAME;
				}

				// is it a number?
				if (IsDigit (c) || (c == '.' && IsDigit (PeekChar ()))) {
					string_buffer_top = 0;
					int _base = 10;

					if (c == '0') {
						c = GetChar ();
						if (c == 'x' || c == 'X') {
							_base = 16;
							c = GetChar ();
						} else if (IsDigit (c))
							_base = 8;
						else
							AddToString ('0');
					}

					if (_base == 16) {
						while (0 <= xDigitToInt (c)) {
							AddToString (c);
							c = GetChar ();
						}
					} else {
						while ('0' <= c && c <= '9') {
							/*
							 * We permit 08 and 09 as decimal numbers, which
							 * makes our behavior a superset of the ECMA
							 * numeric grammar.  We might not always be so
							 * permissive, so we warn about it.
							 */
							if (_base == 8 && c >= '8') {
								ReportCurrentLineWarning ("msg.bad.octal.literal", c == '8' ? "8" : "9");
								_base = 10;
							}
							AddToString (c);
							c = GetChar ();
						}
					}
					
					bool is_integer = true;
					
					if (_base == 10 && (c == '.' || c == 'e' || c == 'E')) {
						is_integer = false;
						if (c == '.') {
							do {
								AddToString (c);
								c = GetChar ();
							} while (IsDigit (c));
						}
						if (c == 'e' || c == 'E') {
							AddToString (c);
							c = GetChar ();
							if (c == '+' || c == '-') {
								AddToString (c);
								c = GetChar ();
							}
							if (!IsDigit (c)) {
								ReportCurrentLineError ("msg.missing.exponent");
								return Token.ERROR;
							}
							do {
								AddToString (c);
								c = GetChar ();
							} while (IsDigit (c));
						}
					}
					UnGetChar (c);
					string num_string = GetStringFromBuffer ();

					double dval;
					if (_base == 10 && !is_integer) {
						try {
							// Use C# conversion to number from string
							dval = Double.Parse (num_string, CultureInfo.InvariantCulture);
						} catch (FormatException) {
							ReportCurrentLineError ("msg.caught.nfe");
							return Token.ERROR;
						} catch (OverflowException) {
							dval = Double.NaN;
						}
					} else
						dval = StringToNumber (num_string, 0, _base);
					
					number = dval;
					return Token.NUMBER;
				}

				// is it a string?
				if (c == '"' || c == '\'') {
					// We attempt to accumulate a string the fast way, by
					// building it directly out of the reader.  But if there
					// are any escaped characters in the string, we revert to
					// building it out of a StringBuffer.
					
					int quote_char = c;
					string_buffer_top = 0;
					c = GetChar ();
					
				strLoop: while (c != quote_char) {
					if (c == '\n' || c == EOF_CHAR) {
						UnGetChar (c);
						ReportCurrentLineError ("msg.unterminated.string.lit");
						return Token.ERROR;
					}

					if (c == '\\') {
						// We've hit an escaped character
						int escape_val;
						
						c = GetChar ();
						switch (c) {
						case 'b': c = '\b'; break;
						case 'f': c = '\f'; break;
						case 'n': c = '\n'; break;
						case 'r': c = '\r'; break;
						case 't': c = '\t'; break;
							
						// \v a late addition to the ECMA spec,
						// it is not in Java, so use 0xb
						case 'v': c = 0xb; break;

						case 'u':
							// Get 4 hex digits; if the u escape is not
							// followed by 4 hex digits, use 'u' + the
							// literal character sequence that follows.
							int escape_start = string_buffer_top;
							AddToString ('u');
							escape_val = 0;
							for (int i = 0; i != 4; ++i) {
								c = GetChar ();
								escape_val = (escape_val << 4) | xDigitToInt (c);
								if (escape_val < 0)
									goto strLoop;
								AddToString (c);
							}
							// prepare for replace of stored 'u' sequence
							// by escape value
							string_buffer_top = escape_start;
							c = escape_val;
							break;
						case 'x':
							// Get 2 hex digits, defaulting to 'x'+literal
							// sequence, as above.
							c = GetChar ();
							escape_val = xDigitToInt (c);
							if (escape_val < 0) {
								AddToString ('x');
								goto strLoop;
							} else {
								int c1 = c;
								c = GetChar ();
								escape_val = (escape_val << 4) | xDigitToInt (c);
								if (escape_val < 0) {
									AddToString ('x');
									AddToString (c1);
									goto strLoop;
								} else // got 2 hex digits
									c = escape_val;
							}
							break;
						default:
							if ('0' <= c && c < '8') {
								int val = c - '0';
								c = GetChar ();
								if ('0' <= c && c < '8') {
									val = 8 * val + c - '0';
									c = GetChar ();
									if ('0' <= c && c < '8' && val <= 037) {
										// c is 3rd char of octal sequence only
										// if the resulting val <= 0377
										val = 8 * val + c - '0';
										c = GetChar ();
									}
								}
								UnGetChar (c);
								c = val;
							}
							break;
						}
					}
					AddToString (c);
					c = GetChar ();
				}
					string str = GetStringFromBuffer ();
					_string = String.Intern (str);
					return Token.STRING;
				}
				
				switch (c) {
				case ';': return Token.SEMI;
				case '[': return Token.LB;
				case ']': return Token.RB;
				case '{': return Token.LC;
				case '}': return Token.RC;
				case '(': return Token.LP;
				case ')': return Token.RP;
				case ',': return Token.COMMA;
				case '?': return Token.HOOK;
				case ':': return Token.COLON;
				case '.': return Token.DOT;
					
				case '|':
					if (MatchChar ('|'))
						return Token.OR;
					else if (MatchChar ('=')) {
						op = Token.BITOR;
						return Token.ASSIGNOP;
					} else
						return Token.BITOR;
					
				case '^':
					if (MatchChar ('=')) {
						op = Token.BITXOR;
						return Token.ASSIGNOP;
					} else
						return Token.BITXOR;
					
				case '&':
					if (MatchChar ('&'))
						return Token.AND;
					else if (MatchChar ('=')) {
						op = Token.BITAND;
						return Token.ASSIGNOP;
					} else
						return Token.BITAND;
					
				case '=':
					if (MatchChar ('=')) {
						if (MatchChar ('='))
							return Token.SHEQ;
						else
							return Token.EQ;
					} else 
						return Token.ASSIGN;
					
				case '!':
					if (MatchChar ('=')) {
						if (MatchChar ('='))
							return Token.SHNE;
						else
							return Token.NE;
					} else 
						return Token.NOT;
					
				case '<':
					/* NB:treat HTML begin-comment as comment-till-eol */
					if (MatchChar ('!')) {
						if (MatchChar ('-')) {
							if (MatchChar ('-')) {
								SkipLine ();
								goto retry;
							}
							UnGetChar ('-');
						}
						UnGetChar ('!');
					}
					if (MatchChar ('<')) {
						if (MatchChar ('=')) {
							op = Token.LSH;
							return Token.ASSIGNOP;
						} else 
							return Token.LSH;
					} else {
						if (MatchChar ('=')) 
							return Token.LE;
						else
							return Token.LT;
					}

				case '>':
					if (MatchChar ('>')) {
						if (MatchChar ('>')) {
							if (MatchChar ('=')) {
								op = Token.URSH;
								return Token.ASSIGNOP;
							} else
								return Token.URSH;
						} else {
							if (MatchChar ('=')) {
								op = Token.RSH;
								return Token.ASSIGNOP;
							} else 
								return Token.RSH;
						}
					} else {
						if (MatchChar ('='))
							return Token.GE;
						else
							return Token.GT;
					}
					
				case '*':
					if (MatchChar ('=')) {
						op = Token.MUL;
						return Token.ASSIGNOP;
					} else
						return Token.MUL;
					
				case '/':
					// is it a // comment?
					if (MatchChar ('/')) {
						SkipLine ();
						goto retry;
					}
					if (MatchChar ('*')) {
						bool look_for_slash = false;
						for (;;) {
							c = GetChar ();
							if (c == EOF_CHAR) {
								ReportCurrentLineError ("msg.unterminated.comment");
								return Token.ERROR;
							} else if (c == '*')
								look_for_slash = true;
							else if (c == '/') {
								if (look_for_slash)
									goto retry;
							} else
								look_for_slash = false;
						}
					}
					
					// is it a RegExp?
					if (allow_reg_exp) {
						string_buffer_top = 0;
						while ((c = GetChar ()) != '/') {
							if (c == '\n' || c == EOF_CHAR) {
								UnGetChar (c);
								ReportCurrentLineError ("msg.unterminated.re.lit");
								return Token.ERROR;
							}
							if (c == '\\') {
								AddToString (c);
								c = GetChar ();
							}							
							AddToString (c);
						}
						int re_end = string_buffer_top;
						
						while (true) {
							if (MatchChar ('g'))
								AddToString ('g');
							else if (MatchChar ('i'))
								AddToString ('i');
							else if (MatchChar ('m'))
								AddToString ('m');
							else 
								break;
						}
						
						if (IsAlpha (PeekChar ())) {
							ReportCurrentLineError ("msg.invalid.re.flag");
							return Token.ERROR;
						}

						_string = new String (string_buffer, 0, re_end);
						reg_exp_flags = new String (string_buffer, re_end, string_buffer_top - re_end);
						return Token.REGEXP;
					}
					
					if (MatchChar ('=')) {
						op = Token.DIV;
						return Token.ASSIGNOP;
					} else
						return Token.DIV;
							
				case '%':
					if (MatchChar ('=')) {
						op = Token.MOD;
						return Token.ASSIGNOP;
					} else 
						return Token.MOD;
					
				case '~':
					return Token.BITNOT;
					
				case '+':
					if (MatchChar ('=')) {
						op = Token.ADD;
						return Token.ASSIGNOP;
					} else if (MatchChar ('+'))
						return Token.INC;
					else
						return Token.ADD;
					
				case '-':
					if (MatchChar ('=')) {
						op = Token.SUB;
						c = Token.ASSIGNOP;
					} else if (MatchChar ('-')) {
						if (!dirty_line) {
							// treat HTML end-comment after possible whitespace
							// after line start as comment-utill-eol
							if (MatchChar ('>')) {
								SkipLine ();
								goto retry;
							} 
						}
						c = Token.DEC;
					} else
						c = Token.SUB;
					dirty_line = true;
					return c;
					
				default:
					ReportCurrentLineError ("msg.illegal.character");
					return Token.ERROR;
				}
			}
	}


	static bool IsAlpha (int c)
	{
		// Use 'Z' < 'a'
		if (c <= 'Z')
			return 'A' <= c;
		else
			return 'a' <= c && c <= 'z';
	}						
	
	double StringToNumber (string s, int start, int radix)
	{
		char digit_max = '9';
		char lower_case_bound = 'a';
		char upper_case_bound = 'A';
		int len = s.Length;

		if (radix > 10) {
			lower_case_bound = (char) ('a' + radix - 10);
			upper_case_bound = (char) ('A' + radix - 10);
		}

		int end;
		double sum = 0.0;

		for (end = start; end < len; end++) {
			char c = s [end];
			int new_digit;
			if ('0' <= c && c <= digit_max)
				new_digit = c - '0';
			else if ('a' <= c && c < lower_case_bound)
				new_digit = c - 'a' + 10;
			else if ('A' <= c && c < upper_case_bound)
				new_digit = c - 'A' + 10;
			else
				break;
			sum = sum * radix + new_digit;
		}

		if (start == end)
			return Double.NaN;

		if (sum >= 9007199254740992.0) {
			if (radix == 10) {
				/* If we're accumulating a decimal number and the number
				 * is >= 2^53, then the result from the repeated multiply-add
				 * above may be inaccurate.  Call Java to get the correct
				 * answer.
				 */
				try {
					return Double.Parse (s, CultureInfo.InvariantCulture);
				} catch (FormatException fe) {
					return Double.NaN;
				}
			} else if (radix == 2 || radix == 4 || radix == 8 ||
				   radix == 16 || radix == 32) {
				/* The number may also be inaccurate for one of these bases.
				 * This happens if the addition in value*radix + digit causes
				 * a round-down to an even least significant mantissa bit
				 * when the first dropped bit is a one.  If any of the
				 * following digits in the number (which haven't been added
				 * in yet) are nonzero then the correct action would have
				 * been to round up instead of down.  An example of this
				 * occurs when reading the number 0x1000000000000081, which
				 * rounds to 0x1000000000000000 instead of 0x1000000000000100.
				 */
				int bit_shift_in_char = 1;
				int digit = 0;

				const int SKIP_LEADING_ZEROS = 0;
				const int FIRST_EXACT_53_BITS = 1;
				const int AFTER_BIT_53         = 2;
				const int ZEROS_AFTER_54 = 3;
				const int MIXED_AFTER_54 = 4;

				int state = SKIP_LEADING_ZEROS;
				int exact_bits_limit = 53;
				double factor = 0.0;
				bool bit53 = false;
				// bit54 is the 54th bit (the first dropped from the mantissa)
				bool bit54 = false;

				for (;;) {
					if (bit_shift_in_char == 1) {
						if (start == end)
							break;
						digit = s [start++];
						if ('0' <= digit && digit <= '9')
							digit -= '0';
						else if ('a' <= digit && digit <= 'z')
							digit -= 'a' - 10;
						else
							digit -= 'A' - 10;
						bit_shift_in_char = radix;
					}
					bit_shift_in_char >>= 1;
					bool bit = (digit & bit_shift_in_char) != 0;

					switch (state) {
					case SKIP_LEADING_ZEROS:
						if (bit) {
							--exact_bits_limit;
							sum = 1.0;
							state = FIRST_EXACT_53_BITS;
						}
						break;
					case FIRST_EXACT_53_BITS:
						sum *= 2.0;
						if (bit)
							sum += 1.0;
						--exact_bits_limit;
						if (exact_bits_limit == 0) {
							bit53 = bit;
							state = AFTER_BIT_53;
						}
						break;
					case AFTER_BIT_53:
						bit54 = bit;
						factor = 2.0;
						state = ZEROS_AFTER_54;
						break;
					// FIXME: check if this work
					case ZEROS_AFTER_54:
					case MIXED_AFTER_54:
						if (state == ZEROS_AFTER_54 && bit) {
							state = MIXED_AFTER_54;
						}
						// fallthrough					
						factor *= 2;
						break;
					}
				}
				switch (state) {
				case SKIP_LEADING_ZEROS:
					sum = 0.0;
					break;
				case FIRST_EXACT_53_BITS:
				case AFTER_BIT_53:
					// do nothing
					break;
				case ZEROS_AFTER_54:
					// x1.1 -> x1 + 1 (round up)
					// x0.1 -> x0 (round down)
					if (bit54 & bit53)
						sum += 1.0;
					sum *= factor;
					break;
				case MIXED_AFTER_54:
					// x.100...1.. -> x + 1 (round up)
					// x.0anything -> x (round down)
					if (bit54)
						sum += 1.0;
					sum *= factor;
					break;
				}
			}
			/* We don't worry about inaccurate numbers for any other base. */
		}
		return sum;
	}
	
	bool IsDigit (int c)
	{
		return '0' <= c && c <= '9';
	}

	static int xDigitToInt (int c)
	{
		// use 0..9 < A..Z < a..z
		if (c <= '9') {
			c -= '0';
			if (0 <= c)
				return c;
		} else if (c <= 'F') {
			if ('A' <= c)
				return c - ('A' - 10);
		} else if (c <= 'f') {
			if ('a' <= c)
				return c - ('a' - 10);
		}
		return -1;
	}


	internal static bool IsJSSpace (int c)
	{
		if (c < 127)
			return c == 0x20 || c == 0x9 || c == 0xC || c == 0xB;
		else
			return c == 0xA0 || Char.GetUnicodeCategory ((char) c) == UnicodeCategory.SpaceSeparator;
	}

	internal static bool IsJSLineTerminator (int c)
	{
		return c == '\n' || c == '\r' || c == 0x2028 || c == 0x2029;
	}
	
	static bool IsJSFormatChar (int c)
	{
		return (c > 127) && (Char.GetUnicodeCategory ((char) c) == UnicodeCategory.Format);
	}

	string GetStringFromBuffer ()
	{
		return new string (string_buffer, 0, string_buffer_top);
	}

	void AddToString (int c)
	{
		int N = string_buffer_top;
		if (N == string_buffer.Length) {
			char [] tmp = new char [string_buffer.Length * 2];
			Array.Copy (string_buffer, 0, tmp, 0, N);
			string_buffer = tmp;
		}
		string_buffer [N] = (char) c;
		string_buffer_top = N + 1;
	}

	void UnGetChar (int c)
	{
		// can not unread past across line boundary
#if false
		if (unget_cursor != 0 && unget_buffer [unget_cursor - 1] == '\n')
			;
#endif
		unget_buffer [unget_cursor++] = c;
	}

	bool MatchChar (int test)
	{
		int c = GetChar ();
		if (c == test)
			return true;
		else {
			UnGetChar (c);
			return false;
		}
	}

	int PeekChar ()
	{
		int c = GetChar ();
		UnGetChar (c);
		return c;
	}
	

	int GetChar ()
	{
		if (unget_cursor != 0)
			return unget_buffer [--unget_cursor];
		
		for (;;) {
			int c;
			if (source_string != null) {
				if (source_cursor == source_end) {
					hit_eof = true;
					return EOF_CHAR;
				}
				c = source_string [source_cursor++];
			} else {
				if (source_cursor == source_end) {
					if (!FillSourceBuffer ()) {
						hit_eof = true;
						return EOF_CHAR;
					}
				}
				c = source_buffer [source_cursor++];
			}
			
			if (line_end_char >= 0) {
				if (line_end_char == '\r' && c == '\n') {
					line_end_char = '\n';
					continue;
				}
				line_end_char = -1;
				line_start = source_cursor - 1;
				LineNumber++;
			}

			if (c <= 127) {
				if (c == '\n' || c == '\r') {
					line_end_char = c;
					c = '\n';
				}
			} else {
				if (IsJSFormatChar (c)) 
					continue;
				if ((c & EOL_HINT_MASK) == 0 && IsJSLineTerminator (c)) {
					line_end_char = c;
					c = '\n';
				}
			}
			return c;
		}
	}

	void SkipLine ()
	{
		// skip to end of line
		int c;
		while ((c = GetChar ()) != EOF_CHAR && c != '\n')
			;
		UnGetChar (c);
	}

	bool FillSourceBuffer ()
	{
#if false
		if (source_string == null)
			;
#endif
		if (source_end == source_buffer.Length) {
			if (line_start != 0) {
				Array.Copy (source_buffer, line_start, source_buffer, 0, source_end - line_start);
				source_end -= line_start;
				source_cursor -= line_start;
				line_start = 0;
			} else {
				char [] tmp = new char [source_buffer.Length *  2];
				Array.Copy (source_buffer, 0, tmp, 0, source_end);
				source_buffer = tmp;
			}
		}
		int n = source_reader.Read (source_buffer, source_end, source_buffer.Length - source_end);
		if (n == 0)
			return false;
		source_end += n;
		return true;
	}

	internal void ReportCurrentLineWarning (string message, string str)
	{
		Console.WriteLine ("warning: {0}, {1}, {2}, {3}", message, SourceName, LineNumber, str);
	}

	internal void ReportCurrentLineError (string message)
	{
		Console.WriteLine ("{0} ({1}, 0): error: {2}", SourceName, LineNumber, message);
	}

	// FIXME: we don't check for combining mark yet
	static bool IsJavaIdentifierPart (char c)
	{
		UnicodeCategory unicode_category = Char.GetUnicodeCategory (c);
		return Char.IsLetter (c) || unicode_category == UnicodeCategory.CurrencySymbol ||
			unicode_category == UnicodeCategory.ConnectorPunctuation || Char.IsDigit (c) ||
			unicode_category == UnicodeCategory.LetterNumber || 
			unicode_category == UnicodeCategory.NonSpacingMark || IsIdentifierIgnorable (c);
	}

	static bool IsIdentifierIgnorable (char c)
	{
		return (c >= '\u0000' && c <= '\u0008') || (c >= '\u000E' && c <= '\u001B') || 
			(c >= '\u007F' && c <= '\u009F') || Char.GetUnicodeCategory (c) == UnicodeCategory.Format;
	}
	
	static bool IsJavaIdentifierStart (char c)
	{
		UnicodeCategory unicode_category = Char.GetUnicodeCategory (c);
		return Char.IsLetter (c) || unicode_category == UnicodeCategory.LetterNumber ||
			unicode_category == UnicodeCategory.CurrencySymbol || 
			unicode_category == UnicodeCategory.ConnectorPunctuation;
	}		
}
}
