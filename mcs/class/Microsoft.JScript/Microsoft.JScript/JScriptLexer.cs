// $ANTLR 2.7.2: "jscript-lexer-parser.g" -> "JScriptLexer.cs"$

namespace Microsoft.JScript
{
	// Generate header specific to lexer CSharp file
	using System;
	using Stream                          = System.IO.Stream;
	using TextReader                      = System.IO.TextReader;
	using Hashtable                       = System.Collections.Hashtable;
	
	using TokenStreamException            = antlr.TokenStreamException;
	using TokenStreamIOException          = antlr.TokenStreamIOException;
	using TokenStreamRecognitionException = antlr.TokenStreamRecognitionException;
	using CharStreamException             = antlr.CharStreamException;
	using CharStreamIOException           = antlr.CharStreamIOException;
	using ANTLRException                  = antlr.ANTLRException;
	using CharScanner                     = antlr.CharScanner;
	using InputBuffer                     = antlr.InputBuffer;
	using ByteBuffer                      = antlr.ByteBuffer;
	using CharBuffer                      = antlr.CharBuffer;
	using Token                           = antlr.Token;
	using CommonToken                     = antlr.CommonToken;
	using RecognitionException            = antlr.RecognitionException;
	using NoViableAltForCharException     = antlr.NoViableAltForCharException;
	using MismatchedCharException         = antlr.MismatchedCharException;
	using TokenStream                     = antlr.TokenStream;
	using LexerSharedInputState           = antlr.LexerSharedInputState;
	using BitSet                          = antlr.collections.impl.BitSet;
	
	public 	class JScriptLexer : antlr.CharScanner	, TokenStream
	 {
		public const int EOF = 1;
		public const int NULL_TREE_LOOKAHEAD = 3;
		public const int LITERAL_function = 4;
		public const int IDENTIFIER = 5;
		public const int LPAREN = 6;
		public const int RPAREN = 7;
		public const int COLON = 8;
		public const int LBRACE = 9;
		public const int RBRACE = 10;
		public const int LITERAL_const = 11;
		public const int ASSIGNMENT = 12;
		public const int SEMI_COLON = 13;
		public const int LITERAL_class = 14;
		public const int LITERAL_extends = 15;
		public const int LITERAL_implements = 16;
		public const int COMMA = 17;
		public const int LITERAL_get = 18;
		public const int LITERAL_set = 19;
		public const int LITERAL_debugger = 20;
		public const int LITERAL_if = 21;
		public const int LITERAL_else = 22;
		public const int LITERAL_import = 23;
		public const int LITERAL_interface = 24;
		public const int LITERAL_do = 25;
		public const int LITERAL_while = 26;
		public const int LITERAL_for = 27;
		public const int LITERAL_in = 28;
		public const int LITERAL_continue = 29;
		public const int LITERAL_break = 30;
		public const int LITERAL_package = 31;
		public const int LITERAL_return = 32;
		public const int LITERAL_with = 33;
		public const int LITERAL_super = 34;
		public const int LITERAL_switch = 35;
		public const int LITERAL_case = 36;
		public const int LITERAL_default = 37;
		public const int LITERAL_enum = 38;
		public const int LITERAL_static = 39;
		public const int LITERAL_throw = 40;
		public const int LITERAL_try = 41;
		public const int LITERAL_catch = 42;
		public const int LITERAL_finally = 43;
		public const int LITERAL_var = 44;
		public const int MULTIPLICATION_ASSIGN = 45;
		public const int DIVISION_ASSIGN = 46;
		public const int REMAINDER_ASSIGN = 47;
		public const int ADDITION_ASSIGN = 48;
		public const int SUBSTRACTION_ASSIGN = 49;
		public const int SIGNED_LEFT_SHIFT_ASSIGN = 50;
		public const int SIGNED_RIGHT_SHIFT_ASSIGN = 51;
		public const int UNSIGNED_RIGHT_SHIFT_ASSIGN = 52;
		public const int BITWISE_AND_ASSIGN = 53;
		public const int BITWISE_XOR_ASSIGN = 54;
		public const int BITWISE_OR_ASSIGN = 55;
		public const int CONDITIONAL = 56;
		public const int LOGICAL_OR = 57;
		public const int LOGICAL_AND = 58;
		public const int BITWISE_OR = 59;
		public const int BITWISE_XOR = 60;
		public const int BITWISE_AND = 61;
		public const int EQUALS = 62;
		public const int DOES_NOT_EQUALS = 63;
		public const int STRICT_EQUALS = 64;
		public const int STRICT_DOES_NOT_EQUALS = 65;
		public const int L_THAN = 66;
		public const int G_THAN = 67;
		public const int LE_THAN = 68;
		public const int GE_THAN = 69;
		public const int LITERAL_instanceof = 70;
		public const int SIGNED_RIGHT_SHIFT = 71;
		public const int SIGNED_LEFT_SHIFT = 72;
		public const int PLUS = 73;
		public const int MINUS = 74;
		public const int TIMES = 75;
		public const int DIVISION = 76;
		public const int REMAINDER = 77;
		public const int LITERAL_delete = 78;
		public const int LITERAL_void = 79;
		public const int LITERAL_typeof = 80;
		public const int INCREMENT = 81;
		public const int DECREMENT = 82;
		public const int BITWISE_NOT = 83;
		public const int LOGICAL_NOT = 84;
		public const int LITERAL_new = 85;
		public const int LSQUARE = 86;
		public const int RSQUARE = 87;
		public const int DOT = 88;
		public const int THIS = 89;
		public const int STRING_LITERAL = 90;
		public const int LITERAL_true = 91;
		public const int LITERAL_false = 92;
		public const int LITERAL_null = 93;
		public const int DECIMAL_LITERAL = 94;
		public const int HEX_INTEGER_LITERAL = 95;
		public const int LITERAL_public = 96;
		public const int LITERAL_private = 97;
		public const int LITERAL_protected = 98;
		public const int LITERAL_internal = 99;
		public const int LITERAL_expando = 100;
		public const int LITERAL_abstract = 101;
		public const int LITERAL_final = 102;
		public const int LITERAL_hide = 103;
		public const int LITERAL_override = 104;
		public const int TAB = 105;
		public const int VERTICAL_TAB = 106;
		public const int FORM_FEED = 107;
		public const int SPACE = 108;
		public const int NO_BREAK_SPACE = 109;
		public const int LINE_FEED = 110;
		public const int CARRIGE_RETURN = 111;
		public const int LINE_SEPARATOR = 112;
		public const int PARAGRAPH_SEPARATOR = 113;
		public const int UNSIGNED_RIGHT_SHIFT = 114;
		public const int SL_COMMENT = 115;
		
		public JScriptLexer(Stream ins) : this(new ByteBuffer(ins))
		{
		}
		
		public JScriptLexer(TextReader r) : this(new CharBuffer(r))
		{
		}
		
		public JScriptLexer(InputBuffer ib)		 : this(new LexerSharedInputState(ib))
		{
		}
		
		public JScriptLexer(LexerSharedInputState state) : base(state)
		{
			initialize();
		}
		private void initialize()
		{
			caseSensitiveLiterals = true;
			setCaseSensitive(true);
			literals = new Hashtable();
			literals.Add("switch", 35);
			literals.Add("case", 36);
			literals.Add("interface", 24);
			literals.Add("for", 27);
			literals.Add("class", 14);
			literals.Add("get", 18);
			literals.Add("final", 102);
			literals.Add("internal", 99);
			literals.Add("false", 92);
			literals.Add("debugger", 20);
			literals.Add("true", 91);
			literals.Add("try", 41);
			literals.Add("finally", 43);
			literals.Add("void", 79);
			literals.Add("typeof", 80);
			literals.Add("import", 23);
			literals.Add("instanceof", 70);
			literals.Add("private", 97);
			literals.Add("override", 104);
			literals.Add("expando", 100);
			literals.Add("set", 19);
			literals.Add("throw", 40);
			literals.Add("implements", 16);
			literals.Add("continue", 29);
			literals.Add("do", 25);
			literals.Add("in", 28);
			literals.Add("null", 93);
			literals.Add("extends", 15);
			literals.Add("function", 4);
			literals.Add("enum", 38);
			literals.Add("static", 39);
			literals.Add("while", 26);
			literals.Add("const", 11);
			literals.Add("abstract", 101);
			literals.Add("hide", 103);
			literals.Add("protected", 98);
			literals.Add("break", 30);
			literals.Add("new", 85);
			literals.Add("return", 32);
			literals.Add("delete", 78);
			literals.Add("if", 21);
			literals.Add("default", 37);
			literals.Add("super", 34);
			literals.Add("public", 96);
			literals.Add("package", 31);
			literals.Add("else", 22);
			literals.Add("var", 44);
			literals.Add("catch", 42);
			literals.Add("with", 33);
		}
		
		public new Token nextToken()			//throws TokenStreamException
		{
			Token theRetToken = null;
tryAgain:
			for (;;)
			{
				Token _token = null;
				int _ttype = Token.INVALID_TYPE;
				resetText();
				try     // for char stream error handling
				{
					try     // for lexical error handling
					{
						switch ( LA(1) )
						{
						case '\t':
						{
							mTAB(true);
							theRetToken = returnToken_;
							break;
						}
						case '\u000b':
						{
							mVERTICAL_TAB(true);
							theRetToken = returnToken_;
							break;
						}
						case '\u000c':
						{
							mFORM_FEED(true);
							theRetToken = returnToken_;
							break;
						}
						case ' ':
						{
							mSPACE(true);
							theRetToken = returnToken_;
							break;
						}
						case '\u00a0':
						{
							mNO_BREAK_SPACE(true);
							theRetToken = returnToken_;
							break;
						}
						case '\n':
						{
							mLINE_FEED(true);
							theRetToken = returnToken_;
							break;
						}
						case '\r':
						{
							mCARRIGE_RETURN(true);
							theRetToken = returnToken_;
							break;
						}
						case '\u2028':
						{
							mLINE_SEPARATOR(true);
							theRetToken = returnToken_;
							break;
						}
						case '\u2029':
						{
							mPARAGRAPH_SEPARATOR(true);
							theRetToken = returnToken_;
							break;
						}
						case '{':
						{
							mLBRACE(true);
							theRetToken = returnToken_;
							break;
						}
						case '}':
						{
							mRBRACE(true);
							theRetToken = returnToken_;
							break;
						}
						case '(':
						{
							mLPAREN(true);
							theRetToken = returnToken_;
							break;
						}
						case ')':
						{
							mRPAREN(true);
							theRetToken = returnToken_;
							break;
						}
						case '[':
						{
							mLSQUARE(true);
							theRetToken = returnToken_;
							break;
						}
						case ']':
						{
							mRSQUARE(true);
							theRetToken = returnToken_;
							break;
						}
						case '.':
						{
							mDOT(true);
							theRetToken = returnToken_;
							break;
						}
						case ';':
						{
							mSEMI_COLON(true);
							theRetToken = returnToken_;
							break;
						}
						case ',':
						{
							mCOMMA(true);
							theRetToken = returnToken_;
							break;
						}
						case '~':
						{
							mBITWISE_NOT(true);
							theRetToken = returnToken_;
							break;
						}
						case '?':
						{
							mCONDITIONAL(true);
							theRetToken = returnToken_;
							break;
						}
						case ':':
						{
							mCOLON(true);
							theRetToken = returnToken_;
							break;
						}
						case '"':
						{
							mSTRING_LITERAL(true);
							theRetToken = returnToken_;
							break;
						}
						case 'A':  case 'B':  case 'C':  case 'D':
						case 'E':  case 'F':  case 'G':  case 'H':
						case 'I':  case 'J':  case 'K':  case 'L':
						case 'M':  case 'N':  case 'O':  case 'P':
						case 'Q':  case 'R':  case 'S':  case 'T':
						case 'U':  case 'V':  case 'W':  case 'X':
						case 'Y':  case 'Z':  case 'a':  case 'b':
						case 'c':  case 'd':  case 'e':  case 'f':
						case 'g':  case 'h':  case 'i':  case 'j':
						case 'k':  case 'l':  case 'm':  case 'n':
						case 'o':  case 'p':  case 'q':  case 'r':
						case 's':  case 't':  case 'u':  case 'v':
						case 'w':  case 'x':  case 'y':  case 'z':
						{
							mIDENTIFIER(true);
							theRetToken = returnToken_;
							break;
						}
						default:
							if ((LA(1)=='>') && (LA(2)=='>') && (LA(3)=='>') && (LA(4)=='='))
							{
								mUNSIGNED_RIGHT_SHIFT_ASSIGN(true);
								theRetToken = returnToken_;
							}
							else if ((LA(1)=='=') && (LA(2)=='=') && (LA(3)=='=')) {
								mSTRICT_EQUALS(true);
								theRetToken = returnToken_;
							}
							else if ((LA(1)=='!') && (LA(2)=='=') && (LA(3)=='=')) {
								mSTRICT_DOES_NOT_EQUALS(true);
								theRetToken = returnToken_;
							}
							else if ((LA(1)=='>') && (LA(2)=='>') && (LA(3)=='>') && (true)) {
								mUNSIGNED_RIGHT_SHIFT(true);
								theRetToken = returnToken_;
							}
							else if ((LA(1)=='<') && (LA(2)=='<') && (LA(3)=='=')) {
								mSIGNED_LEFT_SHIFT_ASSIGN(true);
								theRetToken = returnToken_;
							}
							else if ((LA(1)=='>') && (LA(2)=='>') && (LA(3)=='=')) {
								mSIGNED_RIGHT_SHIFT_ASSIGN(true);
								theRetToken = returnToken_;
							}
							else if ((LA(1)=='<') && (LA(2)=='=')) {
								mLE_THAN(true);
								theRetToken = returnToken_;
							}
							else if ((LA(1)=='>') && (LA(2)=='=')) {
								mGE_THAN(true);
								theRetToken = returnToken_;
							}
							else if ((LA(1)=='=') && (LA(2)=='=') && (true)) {
								mEQUALS(true);
								theRetToken = returnToken_;
							}
							else if ((LA(1)=='!') && (LA(2)=='=') && (true)) {
								mDOES_NOT_EQUALS(true);
								theRetToken = returnToken_;
							}
							else if ((LA(1)=='+') && (LA(2)=='+')) {
								mINCREMENT(true);
								theRetToken = returnToken_;
							}
							else if ((LA(1)=='-') && (LA(2)=='-')) {
								mDECREMENT(true);
								theRetToken = returnToken_;
							}
							else if ((LA(1)=='<') && (LA(2)=='<') && (true)) {
								mSIGNED_LEFT_SHIFT(true);
								theRetToken = returnToken_;
							}
							else if ((LA(1)=='>') && (LA(2)=='>') && (true)) {
								mSIGNED_RIGHT_SHIFT(true);
								theRetToken = returnToken_;
							}
							else if ((LA(1)=='&') && (LA(2)=='&')) {
								mLOGICAL_AND(true);
								theRetToken = returnToken_;
							}
							else if ((LA(1)=='|') && (LA(2)=='|')) {
								mLOGICAL_OR(true);
								theRetToken = returnToken_;
							}
							else if ((LA(1)=='+') && (LA(2)=='=')) {
								mADDITION_ASSIGN(true);
								theRetToken = returnToken_;
							}
							else if ((LA(1)=='-') && (LA(2)=='=')) {
								mSUBSTRACTION_ASSIGN(true);
								theRetToken = returnToken_;
							}
							else if ((LA(1)=='*') && (LA(2)=='=')) {
								mMULTIPLICATION_ASSIGN(true);
								theRetToken = returnToken_;
							}
							else if ((LA(1)=='%') && (LA(2)=='=')) {
								mREMAINDER_ASSIGN(true);
								theRetToken = returnToken_;
							}
							else if ((LA(1)=='&') && (LA(2)=='=')) {
								mBITWISE_AND_ASSIGN(true);
								theRetToken = returnToken_;
							}
							else if ((LA(1)=='|') && (LA(2)=='=')) {
								mBITWISE_OR_ASSIGN(true);
								theRetToken = returnToken_;
							}
							else if ((LA(1)=='^') && (LA(2)=='=')) {
								mBITWISE_XOR_ASSIGN(true);
								theRetToken = returnToken_;
							}
							else if ((LA(1)=='/') && (LA(2)=='=')) {
								mDIVISION_ASSIGN(true);
								theRetToken = returnToken_;
							}
							else if ((LA(1)=='0') && (LA(2)=='x')) {
								mHEX_INTEGER_LITERAL(true);
								theRetToken = returnToken_;
							}
							else if ((LA(1)=='/') && (LA(2)=='/')) {
								mSL_COMMENT(true);
								theRetToken = returnToken_;
							}
							else if ((LA(1)=='<') && (true)) {
								mL_THAN(true);
								theRetToken = returnToken_;
							}
							else if ((LA(1)=='>') && (true)) {
								mG_THAN(true);
								theRetToken = returnToken_;
							}
							else if ((LA(1)=='+') && (true)) {
								mPLUS(true);
								theRetToken = returnToken_;
							}
							else if ((LA(1)=='-') && (true)) {
								mMINUS(true);
								theRetToken = returnToken_;
							}
							else if ((LA(1)=='*') && (true)) {
								mTIMES(true);
								theRetToken = returnToken_;
							}
							else if ((LA(1)=='%') && (true)) {
								mREMAINDER(true);
								theRetToken = returnToken_;
							}
							else if ((LA(1)=='&') && (true)) {
								mBITWISE_AND(true);
								theRetToken = returnToken_;
							}
							else if ((LA(1)=='|') && (true)) {
								mBITWISE_OR(true);
								theRetToken = returnToken_;
							}
							else if ((LA(1)=='^') && (true)) {
								mBITWISE_XOR(true);
								theRetToken = returnToken_;
							}
							else if ((LA(1)=='!') && (true)) {
								mLOGICAL_NOT(true);
								theRetToken = returnToken_;
							}
							else if ((LA(1)=='=') && (true)) {
								mASSIGNMENT(true);
								theRetToken = returnToken_;
							}
							else if ((LA(1)=='/') && (true)) {
								mDIVISION(true);
								theRetToken = returnToken_;
							}
							else if (((LA(1) >= '0' && LA(1) <= '9')) && (true)) {
								mDECIMAL_LITERAL(true);
								theRetToken = returnToken_;
							}
						else
						{
							if (LA(1)==EOF_CHAR) { uponEOF(); returnToken_ = makeToken(Token.EOF_TYPE); }
				else {throw new NoViableAltForCharException((char)LA(1), getFilename(), getLine(), getColumn());}
						}
						break; }
						if ( null==returnToken_ ) goto tryAgain; // found SKIP token
						_ttype = returnToken_.Type;
						returnToken_.Type = _ttype;
						return returnToken_;
					}
					catch (RecognitionException e) {
							throw new TokenStreamRecognitionException(e);
					}
				}
				catch (CharStreamException cse) {
					if ( cse is CharStreamIOException ) {
						throw new TokenStreamIOException(((CharStreamIOException)cse).io);
					}
					else {
						throw new TokenStreamException(cse.Message);
					}
				}
			}
		}
		
	public void mTAB(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = TAB;
		
		match('\u0009');
		_ttype = Token.SKIP;
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mVERTICAL_TAB(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = VERTICAL_TAB;
		
		match('\u000B');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mFORM_FEED(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = FORM_FEED;
		
		match('\u000C');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mSPACE(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = SPACE;
		
		match('\u0020');
		_ttype =Token.SKIP;
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mNO_BREAK_SPACE(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = NO_BREAK_SPACE;
		
		match('\u00A0');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mLINE_FEED(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = LINE_FEED;
		
		match('\u000A');
		newline (); { _ttype =Token.SKIP; }
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mCARRIGE_RETURN(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = CARRIGE_RETURN;
		
		match('\u000D');
		newline (); { _ttype =Token.SKIP; }
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mLINE_SEPARATOR(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = LINE_SEPARATOR;
		
		match('\u2028');
		newline (); { _ttype =Token.SKIP; }
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mPARAGRAPH_SEPARATOR(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = PARAGRAPH_SEPARATOR;
		
		match('\u2029');
		newline (); { _ttype =Token.SKIP; }
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mLBRACE(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = LBRACE;
		
		match('{');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mRBRACE(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = RBRACE;
		
		match('}');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mLPAREN(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = LPAREN;
		
		match('(');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mRPAREN(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = RPAREN;
		
		match(')');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mLSQUARE(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = LSQUARE;
		
		match('[');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mRSQUARE(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = RSQUARE;
		
		match(']');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mDOT(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = DOT;
		
		match('.');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mSEMI_COLON(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = SEMI_COLON;
		
		match(';');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mCOMMA(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = COMMA;
		
		match(',');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mL_THAN(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = L_THAN;
		
		match('<');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mG_THAN(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = G_THAN;
		
		match('>');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mLE_THAN(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = LE_THAN;
		
		match("<=");
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mGE_THAN(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = GE_THAN;
		
		match(">=");
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mEQUALS(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = EQUALS;
		
		match("==");
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mDOES_NOT_EQUALS(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = DOES_NOT_EQUALS;
		
		match("!=");
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mSTRICT_EQUALS(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = STRICT_EQUALS;
		
		match("===");
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mSTRICT_DOES_NOT_EQUALS(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = STRICT_DOES_NOT_EQUALS;
		
		match("!==");
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mPLUS(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = PLUS;
		
		match('+');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mMINUS(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = MINUS;
		
		match('-');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mTIMES(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = TIMES;
		
		match('*');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mREMAINDER(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = REMAINDER;
		
		match('%');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mINCREMENT(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = INCREMENT;
		
		match("++");
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mDECREMENT(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = DECREMENT;
		
		match("--");
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mSIGNED_LEFT_SHIFT(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = SIGNED_LEFT_SHIFT;
		
		match("<<");
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mSIGNED_RIGHT_SHIFT(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = SIGNED_RIGHT_SHIFT;
		
		match(">>");
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mUNSIGNED_RIGHT_SHIFT(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = UNSIGNED_RIGHT_SHIFT;
		
		match(">>>");
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mBITWISE_AND(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = BITWISE_AND;
		
		match('&');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mBITWISE_OR(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = BITWISE_OR;
		
		match('|');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mBITWISE_XOR(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = BITWISE_XOR;
		
		match('^');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mLOGICAL_NOT(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = LOGICAL_NOT;
		
		match('!');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mBITWISE_NOT(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = BITWISE_NOT;
		
		match('~');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mLOGICAL_AND(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = LOGICAL_AND;
		
		match("&&");
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mLOGICAL_OR(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = LOGICAL_OR;
		
		match("||");
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mCONDITIONAL(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = CONDITIONAL;
		
		match('?');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mCOLON(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = COLON;
		
		match(':');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mASSIGNMENT(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = ASSIGNMENT;
		
		match('=');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mADDITION_ASSIGN(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = ADDITION_ASSIGN;
		
		match("+=");
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mSUBSTRACTION_ASSIGN(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = SUBSTRACTION_ASSIGN;
		
		match("-=");
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mMULTIPLICATION_ASSIGN(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = MULTIPLICATION_ASSIGN;
		
		match("*=");
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mREMAINDER_ASSIGN(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = REMAINDER_ASSIGN;
		
		match("%=");
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mSIGNED_LEFT_SHIFT_ASSIGN(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = SIGNED_LEFT_SHIFT_ASSIGN;
		
		match("<<=");
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mSIGNED_RIGHT_SHIFT_ASSIGN(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = SIGNED_RIGHT_SHIFT_ASSIGN;
		
		match(">>=");
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mUNSIGNED_RIGHT_SHIFT_ASSIGN(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = UNSIGNED_RIGHT_SHIFT_ASSIGN;
		
		match(">>>=");
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mBITWISE_AND_ASSIGN(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = BITWISE_AND_ASSIGN;
		
		match("&=");
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mBITWISE_OR_ASSIGN(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = BITWISE_OR_ASSIGN;
		
		match("|=");
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mBITWISE_XOR_ASSIGN(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = BITWISE_XOR_ASSIGN;
		
		match("^=");
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mDIVISION(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = DIVISION;
		
		match('/');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mDIVISION_ASSIGN(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = DIVISION_ASSIGN;
		
		match("/=");
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mDECIMAL_LITERAL(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = DECIMAL_LITERAL;
		
		{
			switch ( LA(1) )
			{
			case '0':
			{
				match('0');
				break;
			}
			case '1':  case '2':  case '3':  case '4':
			case '5':  case '6':  case '7':  case '8':
			case '9':
			{
				{
					matchRange('1','9');
				}
				{    // ( ... )*
					for (;;)
					{
						if (((LA(1) >= '0' && LA(1) <= '9')))
						{
							matchRange('0','9');
						}
						else
						{
							goto _loop235_breakloop;
						}
						
					}
_loop235_breakloop:					;
				}    // ( ... )*
				break;
			}
			default:
			{
				throw new NoViableAltForCharException((char)LA(1), getFilename(), getLine(), getColumn());
			}
			 }
		}
		{
			if ((LA(1)=='.'))
			{
				mDOT(false);
				{    // ( ... )*
					for (;;)
					{
						if (((LA(1) >= '0' && LA(1) <= '9')))
						{
							matchRange('0','9');
						}
						else
						{
							goto _loop238_breakloop;
						}
						
					}
_loop238_breakloop:					;
				}    // ( ... )*
			}
			else {
			}
			
		}
		{
			if ((LA(1)=='E'||LA(1)=='e'))
			{
				{
					switch ( LA(1) )
					{
					case 'e':
					{
						match('e');
						break;
					}
					case 'E':
					{
						match('E');
						break;
					}
					default:
					{
						throw new NoViableAltForCharException((char)LA(1), getFilename(), getLine(), getColumn());
					}
					 }
				}
				{
					{
						switch ( LA(1) )
						{
						case '+':
						{
							match('+');
							break;
						}
						case '-':
						{
							match('-');
							break;
						}
						case '0':  case '1':  case '2':  case '3':
						case '4':  case '5':  case '6':  case '7':
						case '8':  case '9':
						{
							break;
						}
						default:
						{
							throw new NoViableAltForCharException((char)LA(1), getFilename(), getLine(), getColumn());
						}
						 }
					}
					{ // ( ... )+
					int _cnt244=0;
					for (;;)
					{
						if (((LA(1) >= '0' && LA(1) <= '9')))
						{
							matchRange('0','9');
						}
						else
						{
							if (_cnt244 >= 1) { goto _loop244_breakloop; } else { throw new NoViableAltForCharException((char)LA(1), getFilename(), getLine(), getColumn());; }
						}
						
						_cnt244++;
					}
_loop244_breakloop:					;
					}    // ( ... )+
				}
			}
			else {
			}
			
		}
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mHEX_INTEGER_LITERAL(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = HEX_INTEGER_LITERAL;
		
		match('0');
		match('x');
		{ // ( ... )+
		int _cnt247=0;
		for (;;)
		{
			switch ( LA(1) )
			{
			case '0':  case '1':  case '2':  case '3':
			case '4':  case '5':  case '6':  case '7':
			case '8':  case '9':
			{
				matchRange('0','9');
				break;
			}
			case 'a':  case 'b':  case 'c':  case 'd':
			case 'e':  case 'f':
			{
				matchRange('a','f');
				break;
			}
			case 'A':  case 'B':  case 'C':  case 'D':
			case 'E':  case 'F':
			{
				matchRange('A','F');
				break;
			}
			default:
			{
				if (_cnt247 >= 1) { goto _loop247_breakloop; } else { throw new NoViableAltForCharException((char)LA(1), getFilename(), getLine(), getColumn());; }
			}
			break; }
			_cnt247++;
		}
_loop247_breakloop:		;
		}    // ( ... )+
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mSTRING_LITERAL(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = STRING_LITERAL;
		
		int _saveIndex = 0;
		_saveIndex = text.Length;
		match('"');
		text.Length = _saveIndex;
		{ // ( ... )+
		int _cnt250=0;
		for (;;)
		{
			switch ( LA(1) )
			{
			case 'a':  case 'b':  case 'c':  case 'd':
			case 'e':  case 'f':  case 'g':  case 'h':
			case 'i':  case 'j':  case 'k':  case 'l':
			case 'm':  case 'n':  case 'o':  case 'p':
			case 'q':  case 'r':  case 's':  case 't':
			case 'u':  case 'v':  case 'w':  case 'x':
			case 'y':  case 'z':
			{
				matchRange('a','z');
				break;
			}
			case 'A':  case 'B':  case 'C':  case 'D':
			case 'E':  case 'F':  case 'G':  case 'H':
			case 'I':  case 'J':  case 'K':  case 'L':
			case 'M':  case 'N':  case 'O':  case 'P':
			case 'Q':  case 'R':  case 'S':  case 'T':
			case 'U':  case 'V':  case 'W':  case 'X':
			case 'Y':  case 'Z':
			{
				matchRange('A','Z');
				break;
			}
			case ' ':
			{
				match('\u0020');
				break;
			}
			default:
			{
				if (_cnt250 >= 1) { goto _loop250_breakloop; } else { throw new NoViableAltForCharException((char)LA(1), getFilename(), getLine(), getColumn());; }
			}
			break; }
			_cnt250++;
		}
_loop250_breakloop:		;
		}    // ( ... )+
		_saveIndex = text.Length;
		match('"');
		text.Length = _saveIndex;
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mIDENTIFIER(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = IDENTIFIER;
		
		{
			switch ( LA(1) )
			{
			case 'a':  case 'b':  case 'c':  case 'd':
			case 'e':  case 'f':  case 'g':  case 'h':
			case 'i':  case 'j':  case 'k':  case 'l':
			case 'm':  case 'n':  case 'o':  case 'p':
			case 'q':  case 'r':  case 's':  case 't':
			case 'u':  case 'v':  case 'w':  case 'x':
			case 'y':  case 'z':
			{
				matchRange('a','z');
				break;
			}
			case 'A':  case 'B':  case 'C':  case 'D':
			case 'E':  case 'F':  case 'G':  case 'H':
			case 'I':  case 'J':  case 'K':  case 'L':
			case 'M':  case 'N':  case 'O':  case 'P':
			case 'Q':  case 'R':  case 'S':  case 'T':
			case 'U':  case 'V':  case 'W':  case 'X':
			case 'Y':  case 'Z':
			{
				matchRange('A','Z');
				break;
			}
			default:
			{
				throw new NoViableAltForCharException((char)LA(1), getFilename(), getLine(), getColumn());
			}
			 }
		}
		{    // ( ... )*
			for (;;)
			{
				switch ( LA(1) )
				{
				case 'a':  case 'b':  case 'c':  case 'd':
				case 'e':  case 'f':  case 'g':  case 'h':
				case 'i':  case 'j':  case 'k':  case 'l':
				case 'm':  case 'n':  case 'o':  case 'p':
				case 'q':  case 'r':  case 's':  case 't':
				case 'u':  case 'v':  case 'w':  case 'x':
				case 'y':  case 'z':
				{
					matchRange('a','z');
					break;
				}
				case 'A':  case 'B':  case 'C':  case 'D':
				case 'E':  case 'F':  case 'G':  case 'H':
				case 'I':  case 'J':  case 'K':  case 'L':
				case 'M':  case 'N':  case 'O':  case 'P':
				case 'Q':  case 'R':  case 'S':  case 'T':
				case 'U':  case 'V':  case 'W':  case 'X':
				case 'Y':  case 'Z':
				{
					matchRange('A','Z');
					break;
				}
				case '0':  case '1':  case '2':  case '3':
				case '4':  case '5':  case '6':  case '7':
				case '8':  case '9':
				{
					matchRange('0','9');
					break;
				}
				default:
				{
					goto _loop254_breakloop;
				}
				 }
			}
_loop254_breakloop:			;
		}    // ( ... )*
		_ttype = testLiteralsTable(_ttype);
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mSL_COMMENT(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; Token _token=null; int _begin=text.Length;
		_ttype = SL_COMMENT;
		
		match("//");
		{    // ( ... )*
			for (;;)
			{
				if ((tokenSet_0_.member(LA(1))))
				{
					{
						match(tokenSet_0_);
					}
				}
				else
				{
					goto _loop258_breakloop;
				}
				
			}
_loop258_breakloop:			;
		}    // ( ... )*
		_ttype = Token.SKIP; newline ();
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	
	private static long[] mk_tokenSet_0_()
	{
		long[] data = new long[2048];
		data[0]=-9217L;
		for (int i = 1; i<=127; i++) { data[i]=-1L; }
		data[128]=-3298534883329L;
		for (int i = 129; i<=1022; i++) { data[i]=-1L; }
		data[1023]=9223372036854775807L;
		for (int i = 1024; i<=2047; i++) { data[i]=0L; }
		return data;
	}
	public static readonly BitSet tokenSet_0_ = new BitSet(mk_tokenSet_0_());
	
}
}
