// $ANTLR 2.7.2: "jscript-lexer-parser.g" -> "JScriptParser.cs"$

namespace Microsoft.JScript
{
	// Generate the header common to all output files.
	using System;
	
	using TokenBuffer              = antlr.TokenBuffer;
	using TokenStreamException     = antlr.TokenStreamException;
	using TokenStreamIOException   = antlr.TokenStreamIOException;
	using ANTLRException           = antlr.ANTLRException;
	using LLkParser = antlr.LLkParser;
	using Token                    = antlr.Token;
	using TokenStream              = antlr.TokenStream;
	using RecognitionException     = antlr.RecognitionException;
	using NoViableAltException     = antlr.NoViableAltException;
	using MismatchedTokenException = antlr.MismatchedTokenException;
	using SemanticException        = antlr.SemanticException;
	using ParserSharedInputState   = antlr.ParserSharedInputState;
	using BitSet                   = antlr.collections.impl.BitSet;
	
	public 	class JScriptParser : antlr.LLkParser
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
		public const int CC_ON = 42;
		public const int COND_SET = 43;
		public const int COND_DEBUG = 44;
		public const int LITERAL_on = 45;
		public const int LITERAL_off = 46;
		public const int COND_POSITION = 47;
		public const int LITERAL_end = 48;
		public const int LITERAL_file = 49;
		public const int STRING_LITERAL = 50;
		public const int LITERAL_catch = 51;
		public const int LITERAL_finally = 52;
		public const int LITERAL_var = 53;
		public const int MULTIPLICATION_ASSIGN = 54;
		public const int DIVISION_ASSIGN = 55;
		public const int REMAINDER_ASSIGN = 56;
		public const int ADDITION_ASSIGN = 57;
		public const int SUBSTRACTION_ASSIGN = 58;
		public const int SIGNED_LEFT_SHIFT_ASSIGN = 59;
		public const int SIGNED_RIGHT_SHIFT_ASSIGN = 60;
		public const int UNSIGNED_RIGHT_SHIFT_ASSIGN = 61;
		public const int BITWISE_AND_ASSIGN = 62;
		public const int BITWISE_XOR_ASSIGN = 63;
		public const int BITWISE_OR_ASSIGN = 64;
		public const int CONDITIONAL = 65;
		public const int LOGICAL_OR = 66;
		public const int LOGICAL_AND = 67;
		public const int BITWISE_OR = 68;
		public const int BITWISE_XOR = 69;
		public const int BITWISE_AND = 70;
		public const int EQUALS = 71;
		public const int DOES_NOT_EQUALS = 72;
		public const int STRICT_EQUALS = 73;
		public const int STRICT_DOES_NOT_EQUALS = 74;
		public const int L_THAN = 75;
		public const int G_THAN = 76;
		public const int LE_THAN = 77;
		public const int GE_THAN = 78;
		public const int LITERAL_instanceof = 79;
		public const int SIGNED_RIGHT_SHIFT = 80;
		public const int SIGNED_LEFT_SHIFT = 81;
		public const int PLUS = 82;
		public const int MINUS = 83;
		public const int TIMES = 84;
		public const int DIVISION = 85;
		public const int REMAINDER = 86;
		public const int LITERAL_delete = 87;
		public const int LITERAL_void = 88;
		public const int LITERAL_typeof = 89;
		public const int INCREMENT = 90;
		public const int DECREMENT = 91;
		public const int BITWISE_NOT = 92;
		public const int LOGICAL_NOT = 93;
		public const int LITERAL_new = 94;
		public const int LSQUARE = 95;
		public const int RSQUARE = 96;
		public const int DOT = 97;
		public const int THIS = 98;
		public const int LITERAL_true = 99;
		public const int LITERAL_false = 100;
		public const int LITERAL_null = 101;
		public const int DECIMAL_LITERAL = 102;
		public const int HEX_INTEGER_LITERAL = 103;
		public const int LITERAL_public = 104;
		public const int LITERAL_private = 105;
		public const int LITERAL_protected = 106;
		public const int LITERAL_internal = 107;
		public const int LITERAL_expando = 108;
		public const int LITERAL_abstract = 109;
		public const int LITERAL_final = 110;
		public const int LITERAL_hide = 111;
		public const int LITERAL_override = 112;
		public const int TAB = 113;
		public const int VERTICAL_TAB = 114;
		public const int FORM_FEED = 115;
		public const int SPACE = 116;
		public const int NO_BREAK_SPACE = 117;
		public const int LINE_FEED = 118;
		public const int CARRIGE_RETURN = 119;
		public const int LINE_SEPARATOR = 120;
		public const int PARAGRAPH_SEPARATOR = 121;
		public const int UNSIGNED_RIGHT_SHIFT = 122;
		public const int COND_IF = 123;
		public const int COND_ELIF = 124;
		public const int COND_ELSE = 125;
		public const int COND_END = 126;
		public const int SL_COMMENT = 127;
		
		
		protected void initialize()
		{
			tokenNames = tokenNames_;
		}
		
		
		protected JScriptParser(TokenBuffer tokenBuf, int k) : base(tokenBuf, k)
		{
			initialize();
		}
		
		public JScriptParser(TokenBuffer tokenBuf) : this(tokenBuf,1)
		{
		}
		
		protected JScriptParser(TokenStream lexer, int k) : base(lexer,k)
		{
			initialize();
		}
		
		public JScriptParser(TokenStream lexer) : this(lexer,1)
		{
		}
		
		public JScriptParser(ParserSharedInputState state) : base(state,1)
		{
			initialize();
		}
		
	public void program(
		ASTList astList
	) //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			source_elements(astList);
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_0_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void source_elements(
		ASTList astList
	) //throws RecognitionException, TokenStreamException
{
		
		AST ast = null;
		
		try {      // for error handling
			ast=source_element();
			if (0==inputState.guessing)
			{
				if (ast != null) astList. Add (ast);
			}
			{
				switch ( LA(1) )
				{
				case LITERAL_function:
				case IDENTIFIER:
				case LBRACE:
				case LITERAL_const:
				case SEMI_COLON:
				case LITERAL_class:
				case LITERAL_debugger:
				case LITERAL_if:
				case LITERAL_import:
				case LITERAL_interface:
				case LITERAL_do:
				case LITERAL_while:
				case LITERAL_for:
				case LITERAL_continue:
				case LITERAL_break:
				case LITERAL_package:
				case LITERAL_return:
				case LITERAL_with:
				case LITERAL_super:
				case LITERAL_switch:
				case LITERAL_enum:
				case LITERAL_static:
				case LITERAL_throw:
				case LITERAL_try:
				case CC_ON:
				case COND_SET:
				case LITERAL_var:
				case LITERAL_public:
				case LITERAL_private:
				case LITERAL_protected:
				case LITERAL_internal:
				case LITERAL_expando:
				case LITERAL_abstract:
				case LITERAL_final:
				{
					source_elements(astList);
					break;
				}
				case EOF:
				case RBRACE:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_1_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public AST  source_element() //throws RecognitionException, TokenStreamException
{
		AST ast;
		
		
			ast = null;
			Statement stm = null;
			FunctionDeclaration fd = null;
		
		
		try {      // for error handling
			switch ( LA(1) )
			{
			case IDENTIFIER:
			case LBRACE:
			case LITERAL_const:
			case SEMI_COLON:
			case LITERAL_class:
			case LITERAL_debugger:
			case LITERAL_if:
			case LITERAL_import:
			case LITERAL_interface:
			case LITERAL_do:
			case LITERAL_while:
			case LITERAL_for:
			case LITERAL_continue:
			case LITERAL_break:
			case LITERAL_package:
			case LITERAL_return:
			case LITERAL_with:
			case LITERAL_super:
			case LITERAL_switch:
			case LITERAL_enum:
			case LITERAL_static:
			case LITERAL_throw:
			case LITERAL_try:
			case CC_ON:
			case LITERAL_var:
			case LITERAL_public:
			case LITERAL_private:
			case LITERAL_protected:
			case LITERAL_internal:
			case LITERAL_expando:
			case LITERAL_abstract:
			case LITERAL_final:
			{
				stm=statement();
				if (0==inputState.guessing)
				{
					ast = stm;
				}
				break;
			}
			case LITERAL_function:
			{
				fd=global_function_declaration();
				if (0==inputState.guessing)
				{
					ast = fd;
				}
				break;
			}
			case COND_SET:
			{
				conditional_compilation_directive();
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_2_);
			}
			else
			{
				throw;
			}
		}
		return ast;
	}
	
	public Statement  statement() //throws RecognitionException, TokenStreamException
{
		Statement stm;
		
		
			stm = null; 
			VariableStatement varStm = null;
		
		
		try {      // for error handling
			switch ( LA(1) )
			{
			case LBRACE:
			{
				block();
				break;
			}
			case LITERAL_const:
			case LITERAL_class:
			case LITERAL_interface:
			case LITERAL_enum:
			case LITERAL_static:
			case LITERAL_var:
			case LITERAL_public:
			case LITERAL_private:
			case LITERAL_protected:
			case LITERAL_internal:
			case LITERAL_expando:
			case LITERAL_abstract:
			case LITERAL_final:
			{
				modifiers();
				{
					switch ( LA(1) )
					{
					case LITERAL_class:
					{
						class_statement();
						break;
					}
					case LITERAL_const:
					{
						const_statement();
						break;
					}
					case LITERAL_enum:
					{
						enum_statement();
						break;
					}
					case LITERAL_interface:
					{
						interface_statement();
						break;
					}
					case LITERAL_var:
					{
						varStm=variable_statement();
						if (0==inputState.guessing)
						{
							stm = (Statement) varStm;
						}
						break;
					}
					default:
					{
						throw new NoViableAltException(LT(1), getFilename());
					}
					 }
				}
				break;
			}
			case SEMI_COLON:
			{
				empty_statement();
				break;
			}
			case LITERAL_debugger:
			{
				debugger_statement();
				break;
			}
			case LITERAL_if:
			{
				if_statement();
				break;
			}
			case LITERAL_import:
			{
				import_statement();
				break;
			}
			case LITERAL_do:
			case LITERAL_while:
			case LITERAL_for:
			{
				iteration_statement();
				break;
			}
			case LITERAL_continue:
			{
				continue_statement();
				break;
			}
			case LITERAL_break:
			{
				break_statement();
				break;
			}
			case LITERAL_package:
			{
				package_statement();
				break;
			}
			case LITERAL_return:
			{
				return_statement();
				break;
			}
			case LITERAL_with:
			{
				with_statement();
				break;
			}
			case LITERAL_super:
			{
				super_statement();
				break;
			}
			case LITERAL_switch:
			{
				switch_statement();
				break;
			}
			case LITERAL_throw:
			{
				throw_statement();
				break;
			}
			case IDENTIFIER:
			{
				labelled_statement();
				break;
			}
			case LITERAL_try:
			{
				try_statement();
				break;
			}
			case CC_ON:
			{
				cc_on_statement();
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_3_);
			}
			else
			{
				throw;
			}
		}
		return stm;
	}
	
	public FunctionDeclaration  global_function_declaration() //throws RecognitionException, TokenStreamException
{
		FunctionDeclaration fd;
		
		Token  id = null;
		fd = new FunctionDeclaration ();
		
		try {      // for error handling
			match(LITERAL_function);
			id = LT(1);
			match(IDENTIFIER);
			match(LPAREN);
			{
				switch ( LA(1) )
				{
				case IDENTIFIER:
				{
					formal_parameter_list(fd.parameters);
					break;
				}
				case RPAREN:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
			match(RPAREN);
			{
				switch ( LA(1) )
				{
				case COLON:
				{
					match(COLON);
					match(IDENTIFIER);
					break;
				}
				case LBRACE:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
			match(LBRACE);
			{
				switch ( LA(1) )
				{
				case LITERAL_function:
				case IDENTIFIER:
				case LBRACE:
				case LITERAL_const:
				case SEMI_COLON:
				case LITERAL_class:
				case LITERAL_debugger:
				case LITERAL_if:
				case LITERAL_import:
				case LITERAL_interface:
				case LITERAL_do:
				case LITERAL_while:
				case LITERAL_for:
				case LITERAL_continue:
				case LITERAL_break:
				case LITERAL_package:
				case LITERAL_return:
				case LITERAL_with:
				case LITERAL_super:
				case LITERAL_switch:
				case LITERAL_enum:
				case LITERAL_static:
				case LITERAL_throw:
				case LITERAL_try:
				case CC_ON:
				case COND_SET:
				case LITERAL_var:
				case LITERAL_public:
				case LITERAL_private:
				case LITERAL_protected:
				case LITERAL_internal:
				case LITERAL_expando:
				case LITERAL_abstract:
				case LITERAL_final:
				{
					function_body(fd.funcBody);
					break;
				}
				case RBRACE:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
			match(RBRACE);
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_2_);
			}
			else
			{
				throw;
			}
		}
		return fd;
	}
	
	public void conditional_compilation_directive() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			match(COND_SET);
			{
				switch ( LA(1) )
				{
				case COND_DEBUG:
				{
					{
						match(COND_DEBUG);
						match(LPAREN);
						{
							switch ( LA(1) )
							{
							case LITERAL_on:
							{
								match(LITERAL_on);
								break;
							}
							case LITERAL_off:
							{
								match(LITERAL_off);
								break;
							}
							default:
							{
								throw new NoViableAltException(LT(1), getFilename());
							}
							 }
						}
						match(RPAREN);
					}
					break;
				}
				case COND_POSITION:
				{
					{
						match(COND_POSITION);
						match(LPAREN);
						{
							switch ( LA(1) )
							{
							case LITERAL_end:
							{
								match(LITERAL_end);
								break;
							}
							case RPAREN:
							case LITERAL_file:
							{
								{
									switch ( LA(1) )
									{
									case LITERAL_file:
									{
										match(LITERAL_file);
										match(ASSIGNMENT);
										match(STRING_LITERAL);
										break;
									}
									case RPAREN:
									{
										break;
									}
									default:
									{
										throw new NoViableAltException(LT(1), getFilename());
									}
									 }
								}
								break;
							}
							default:
							{
								throw new NoViableAltException(LT(1), getFilename());
							}
							 }
						}
						match(RPAREN);
					}
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_2_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void formal_parameter_list(
		FormalParameterList param
	) //throws RecognitionException, TokenStreamException
{
		
		Token  id = null;
		
		try {      // for error handling
			id = LT(1);
			match(IDENTIFIER);
			if (0==inputState.guessing)
			{
				param.Add (id.getText ());
			}
			{
				switch ( LA(1) )
				{
				case COMMA:
				{
					match(COMMA);
					formal_parameter_list(param);
					break;
				}
				case RPAREN:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_4_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void function_body(
		ASTList funcBody
	) //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			source_elements(funcBody);
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_5_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void block() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			match(LBRACE);
			{
				switch ( LA(1) )
				{
				case IDENTIFIER:
				case LBRACE:
				case LITERAL_const:
				case SEMI_COLON:
				case LITERAL_class:
				case LITERAL_debugger:
				case LITERAL_if:
				case LITERAL_import:
				case LITERAL_interface:
				case LITERAL_do:
				case LITERAL_while:
				case LITERAL_for:
				case LITERAL_continue:
				case LITERAL_break:
				case LITERAL_package:
				case LITERAL_return:
				case LITERAL_with:
				case LITERAL_super:
				case LITERAL_switch:
				case LITERAL_enum:
				case LITERAL_static:
				case LITERAL_throw:
				case LITERAL_try:
				case CC_ON:
				case LITERAL_var:
				case LITERAL_public:
				case LITERAL_private:
				case LITERAL_protected:
				case LITERAL_internal:
				case LITERAL_expando:
				case LITERAL_abstract:
				case LITERAL_final:
				{
					statement_list();
					break;
				}
				case RBRACE:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
			match(RBRACE);
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_6_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void modifiers() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			{    // ( ... )*
				for (;;)
				{
					if ((tokenSet_7_.member(LA(1))))
					{
						modifier();
					}
					else
					{
						goto _loop179_breakloop;
					}
					
				}
_loop179_breakloop:				;
			}    // ( ... )*
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_8_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void class_statement() //throws RecognitionException, TokenStreamException
{
		
		Token  classname = null;
		Token  baseclass = null;
		
		try {      // for error handling
			match(LITERAL_class);
			classname = LT(1);
			match(IDENTIFIER);
			{
				switch ( LA(1) )
				{
				case LITERAL_extends:
				{
					match(LITERAL_extends);
					baseclass = LT(1);
					match(IDENTIFIER);
					break;
				}
				case LBRACE:
				case LITERAL_implements:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
			{
				switch ( LA(1) )
				{
				case LITERAL_implements:
				{
					match(LITERAL_implements);
					interfaces_list();
					break;
				}
				case LBRACE:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
			match(LBRACE);
			class_members();
			match(RBRACE);
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_3_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void const_statement() //throws RecognitionException, TokenStreamException
{
		
		Token  name = null;
		Token  type = null;
		
		try {      // for error handling
			match(LITERAL_const);
			name = LT(1);
			match(IDENTIFIER);
			{
				switch ( LA(1) )
				{
				case COLON:
				{
					match(COLON);
					type = LT(1);
					match(IDENTIFIER);
					break;
				}
				case ASSIGNMENT:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
			match(ASSIGNMENT);
			numeric_literal();
			match(SEMI_COLON);
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_3_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void enum_statement() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			match(LITERAL_enum);
			match(IDENTIFIER);
			{
				switch ( LA(1) )
				{
				case COLON:
				{
					match(COLON);
					match(IDENTIFIER);
					break;
				}
				case LBRACE:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
			match(LBRACE);
			{
				match(IDENTIFIER);
				{
					switch ( LA(1) )
					{
					case ASSIGNMENT:
					{
						match(ASSIGNMENT);
						numeric_literal();
						break;
					}
					case RBRACE:
					case COMMA:
					{
						break;
					}
					default:
					{
						throw new NoViableAltException(LT(1), getFilename());
					}
					 }
				}
			}
			{    // ( ... )*
				for (;;)
				{
					if ((LA(1)==COMMA))
					{
						match(COMMA);
						match(IDENTIFIER);
						{
							switch ( LA(1) )
							{
							case ASSIGNMENT:
							{
								match(ASSIGNMENT);
								numeric_literal();
								break;
							}
							case RBRACE:
							case COMMA:
							{
								break;
							}
							default:
							{
								throw new NoViableAltException(LT(1), getFilename());
							}
							 }
						}
					}
					else
					{
						goto _loop81_breakloop;
					}
					
				}
_loop81_breakloop:				;
			}    // ( ... )*
			match(RBRACE);
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_3_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void interface_statement() //throws RecognitionException, TokenStreamException
{
		
		Token  intfName = null;
		
		try {      // for error handling
			match(LITERAL_interface);
			intfName = LT(1);
			match(IDENTIFIER);
			{
				switch ( LA(1) )
				{
				case LITERAL_implements:
				{
					match(LITERAL_implements);
					match(IDENTIFIER);
					break;
				}
				case LBRACE:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
			match(LBRACE);
			interface_members();
			match(RBRACE);
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_3_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public VariableStatement  variable_statement() //throws RecognitionException, TokenStreamException
{
		VariableStatement varStm;
		
		varStm = new VariableStatement ();
		
		try {      // for error handling
			match(LITERAL_var);
			variable_declaration_list(varStm);
			match(SEMI_COLON);
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_3_);
			}
			else
			{
				throw;
			}
		}
		return varStm;
	}
	
	public void empty_statement() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			match(SEMI_COLON);
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_3_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void debugger_statement() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			match(LITERAL_debugger);
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_3_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void if_statement() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			match(LITERAL_if);
			match(LPAREN);
			expression();
			match(RPAREN);
			statement();
			{
				bool synPredMatched39 = false;
				if (((LA(1)==LITERAL_else)))
				{
					int _m39 = mark();
					synPredMatched39 = true;
					inputState.guessing++;
					try {
						{
							match(LITERAL_else);
						}
					}
					catch (RecognitionException)
					{
						synPredMatched39 = false;
					}
					rewind(_m39);
					inputState.guessing--;
				}
				if ( synPredMatched39 )
				{
					match(LITERAL_else);
					statement();
				}
				else if ((tokenSet_3_.member(LA(1)))) {
				}
				else
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_3_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void import_statement() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			match(LITERAL_import);
			match(IDENTIFIER);
			match(SEMI_COLON);
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_3_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void iteration_statement() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			switch ( LA(1) )
			{
			case LITERAL_do:
			{
				match(LITERAL_do);
				statement();
				match(LITERAL_while);
				match(LPAREN);
				expression();
				match(RPAREN);
				match(SEMI_COLON);
				break;
			}
			case LITERAL_while:
			{
				match(LITERAL_while);
				match(LPAREN);
				expression();
				match(RPAREN);
				statement();
				break;
			}
			case LITERAL_for:
			{
				match(LITERAL_for);
				match(LPAREN);
				left_hand_side_expression();
				match(LITERAL_in);
				expression();
				match(RPAREN);
				statement();
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_3_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void continue_statement() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			match(LITERAL_continue);
			{
				switch ( LA(1) )
				{
				case IDENTIFIER:
				{
					match(IDENTIFIER);
					break;
				}
				case SEMI_COLON:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
			match(SEMI_COLON);
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_3_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void break_statement() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			match(LITERAL_break);
			{
				switch ( LA(1) )
				{
				case IDENTIFIER:
				{
					match(IDENTIFIER);
					break;
				}
				case SEMI_COLON:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
			match(SEMI_COLON);
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_3_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void package_statement() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			match(LITERAL_package);
			match(IDENTIFIER);
			match(LBRACE);
			package_members();
			match(RBRACE);
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_3_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void return_statement() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			match(LITERAL_return);
			{
				switch ( LA(1) )
				{
				case LITERAL_function:
				case IDENTIFIER:
				case LPAREN:
				case LBRACE:
				case STRING_LITERAL:
				case PLUS:
				case MINUS:
				case LITERAL_delete:
				case LITERAL_void:
				case LITERAL_typeof:
				case INCREMENT:
				case DECREMENT:
				case BITWISE_NOT:
				case LOGICAL_NOT:
				case LITERAL_new:
				case LSQUARE:
				case THIS:
				case LITERAL_true:
				case LITERAL_false:
				case LITERAL_null:
				case DECIMAL_LITERAL:
				case HEX_INTEGER_LITERAL:
				{
					expression();
					break;
				}
				case SEMI_COLON:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
			match(SEMI_COLON);
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_3_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void with_statement() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			match(LITERAL_with);
			match(LPAREN);
			expression();
			match(RPAREN);
			statement();
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_3_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void super_statement() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			match(LITERAL_super);
			{
				arguments();
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_3_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void switch_statement() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			match(LITERAL_switch);
			match(LPAREN);
			expression();
			match(RPAREN);
			case_block();
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_3_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void throw_statement() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			match(LITERAL_throw);
			expression();
			match(SEMI_COLON);
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_3_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void labelled_statement() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			match(IDENTIFIER);
			match(COLON);
			statement();
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_3_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void try_statement() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			match(LITERAL_try);
			block();
			{
				switch ( LA(1) )
				{
				case LITERAL_catch:
				{
					{
						catch_exp();
						{
							switch ( LA(1) )
							{
							case LITERAL_finally:
							{
								finally_exp();
								break;
							}
							case EOF:
							case LITERAL_function:
							case IDENTIFIER:
							case LBRACE:
							case RBRACE:
							case LITERAL_const:
							case SEMI_COLON:
							case LITERAL_class:
							case LITERAL_debugger:
							case LITERAL_if:
							case LITERAL_else:
							case LITERAL_import:
							case LITERAL_interface:
							case LITERAL_do:
							case LITERAL_while:
							case LITERAL_for:
							case LITERAL_continue:
							case LITERAL_break:
							case LITERAL_package:
							case LITERAL_return:
							case LITERAL_with:
							case LITERAL_super:
							case LITERAL_switch:
							case LITERAL_case:
							case LITERAL_default:
							case LITERAL_enum:
							case LITERAL_static:
							case LITERAL_throw:
							case LITERAL_try:
							case CC_ON:
							case COND_SET:
							case LITERAL_var:
							case LITERAL_public:
							case LITERAL_private:
							case LITERAL_protected:
							case LITERAL_internal:
							case LITERAL_expando:
							case LITERAL_abstract:
							case LITERAL_final:
							{
								break;
							}
							default:
							{
								throw new NoViableAltException(LT(1), getFilename());
							}
							 }
						}
					}
					break;
				}
				case LITERAL_finally:
				{
					finally_exp();
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_3_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void cc_on_statement() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			match(CC_ON);
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_3_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void statement_list() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			statement();
			{
				switch ( LA(1) )
				{
				case IDENTIFIER:
				case LBRACE:
				case LITERAL_const:
				case SEMI_COLON:
				case LITERAL_class:
				case LITERAL_debugger:
				case LITERAL_if:
				case LITERAL_import:
				case LITERAL_interface:
				case LITERAL_do:
				case LITERAL_while:
				case LITERAL_for:
				case LITERAL_continue:
				case LITERAL_break:
				case LITERAL_package:
				case LITERAL_return:
				case LITERAL_with:
				case LITERAL_super:
				case LITERAL_switch:
				case LITERAL_enum:
				case LITERAL_static:
				case LITERAL_throw:
				case LITERAL_try:
				case CC_ON:
				case LITERAL_var:
				case LITERAL_public:
				case LITERAL_private:
				case LITERAL_protected:
				case LITERAL_internal:
				case LITERAL_expando:
				case LITERAL_abstract:
				case LITERAL_final:
				{
					statement_list();
					break;
				}
				case RBRACE:
				case LITERAL_case:
				case LITERAL_default:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_9_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void numeric_literal() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			switch ( LA(1) )
			{
			case DECIMAL_LITERAL:
			{
				match(DECIMAL_LITERAL);
				break;
			}
			case HEX_INTEGER_LITERAL:
			{
				match(HEX_INTEGER_LITERAL);
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_10_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void interfaces_list() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			match(IDENTIFIER);
			{    // ( ... )*
				for (;;)
				{
					if ((LA(1)==COMMA))
					{
						match(COMMA);
						match(IDENTIFIER);
					}
					else
					{
						goto _loop20_breakloop;
					}
					
				}
_loop20_breakloop:				;
			}    // ( ... )*
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_11_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void class_members() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			{    // ( ... )*
				for (;;)
				{
					bool synPredMatched24 = false;
					if (((LA(1)==LITERAL_static)))
					{
						int _m24 = mark();
						synPredMatched24 = true;
						inputState.guessing++;
						try {
							{
								static_statement();
							}
						}
						catch (RecognitionException)
						{
							synPredMatched24 = false;
						}
						rewind(_m24);
						inputState.guessing--;
					}
					if ( synPredMatched24 )
					{
						static_statement();
					}
					else if ((tokenSet_12_.member(LA(1)))) {
						{
							modifiers();
							{
								switch ( LA(1) )
								{
								case LITERAL_function:
								{
									type_function_declaration();
									break;
								}
								case LITERAL_const:
								{
									const_statement();
									break;
								}
								case LITERAL_var:
								{
									variable_statement();
									break;
								}
								case LITERAL_enum:
								{
									enum_statement();
									break;
								}
								case LITERAL_class:
								{
									class_statement();
									break;
								}
								default:
								{
									throw new NoViableAltException(LT(1), getFilename());
								}
								 }
							}
						}
					}
					else
					{
						goto _loop27_breakloop;
					}
					
				}
_loop27_breakloop:				;
			}    // ( ... )*
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_5_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void static_statement() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			match(LITERAL_static);
			match(IDENTIFIER);
			match(LBRACE);
			match(RBRACE);
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_13_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public FunctionDeclaration  type_function_declaration() //throws RecognitionException, TokenStreamException
{
		FunctionDeclaration fd;
		
		Token  functionName = null;
		Token  type = null;
		fd = new FunctionDeclaration ();
		
		try {      // for error handling
			match(LITERAL_function);
			{
				switch ( LA(1) )
				{
				case LITERAL_get:
				{
					match(LITERAL_get);
					break;
				}
				case LITERAL_set:
				{
					match(LITERAL_set);
					break;
				}
				case IDENTIFIER:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
			functionName = LT(1);
			match(IDENTIFIER);
			if (0==inputState.guessing)
			{
				fd.id = functionName.getText ();
			}
			match(LPAREN);
			{
				switch ( LA(1) )
				{
				case IDENTIFIER:
				{
					formal_parameter_list(fd.parameters);
					break;
				}
				case RPAREN:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
			match(RPAREN);
			{
				switch ( LA(1) )
				{
				case COLON:
				{
					match(COLON);
					type = LT(1);
					match(IDENTIFIER);
					break;
				}
				case LBRACE:
				case SEMI_COLON:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
			{
				switch ( LA(1) )
				{
				case LBRACE:
				{
					match(LBRACE);
					{
						switch ( LA(1) )
						{
						case LITERAL_function:
						case IDENTIFIER:
						case LBRACE:
						case LITERAL_const:
						case SEMI_COLON:
						case LITERAL_class:
						case LITERAL_debugger:
						case LITERAL_if:
						case LITERAL_import:
						case LITERAL_interface:
						case LITERAL_do:
						case LITERAL_while:
						case LITERAL_for:
						case LITERAL_continue:
						case LITERAL_break:
						case LITERAL_package:
						case LITERAL_return:
						case LITERAL_with:
						case LITERAL_super:
						case LITERAL_switch:
						case LITERAL_enum:
						case LITERAL_static:
						case LITERAL_throw:
						case LITERAL_try:
						case CC_ON:
						case COND_SET:
						case LITERAL_var:
						case LITERAL_public:
						case LITERAL_private:
						case LITERAL_protected:
						case LITERAL_internal:
						case LITERAL_expando:
						case LITERAL_abstract:
						case LITERAL_final:
						{
							function_body(fd.funcBody);
							break;
						}
						case RBRACE:
						{
							break;
						}
						default:
						{
							throw new NoViableAltException(LT(1), getFilename());
						}
						 }
					}
					match(RBRACE);
					break;
				}
				case SEMI_COLON:
				{
					match(SEMI_COLON);
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_13_);
			}
			else
			{
				throw;
			}
		}
		return fd;
	}
	
	public void expression() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			assignment_expression();
			{
				switch ( LA(1) )
				{
				case COMMA:
				{
					match(COMMA);
					expression();
					break;
				}
				case RPAREN:
				case COLON:
				case SEMI_COLON:
				case RSQUARE:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_14_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void interface_members() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			{    // ( ... )*
				for (;;)
				{
					if ((tokenSet_15_.member(LA(1))))
					{
						modifiers();
						type_function_declaration();
					}
					else
					{
						goto _loop45_breakloop;
					}
					
				}
_loop45_breakloop:				;
			}    // ( ... )*
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_5_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void left_hand_side_expression() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			new_expression();
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_16_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void package_members() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			{    // ( ... )*
				for (;;)
				{
					if ((tokenSet_17_.member(LA(1))))
					{
						package_member();
					}
					else
					{
						goto _loop54_breakloop;
					}
					
				}
_loop54_breakloop:				;
			}    // ( ... )*
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_5_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void package_member() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			{
				modifiers();
				{
					switch ( LA(1) )
					{
					case LITERAL_class:
					{
						class_statement();
						break;
					}
					case LITERAL_interface:
					{
						interface_statement();
						break;
					}
					case LITERAL_enum:
					{
						enum_statement();
						break;
					}
					default:
					{
						throw new NoViableAltException(LT(1), getFilename());
					}
					 }
				}
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_18_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void arguments() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			match(LPAREN);
			{
				switch ( LA(1) )
				{
				case LITERAL_function:
				case IDENTIFIER:
				case LPAREN:
				case LBRACE:
				case STRING_LITERAL:
				case PLUS:
				case MINUS:
				case LITERAL_delete:
				case LITERAL_void:
				case LITERAL_typeof:
				case INCREMENT:
				case DECREMENT:
				case BITWISE_NOT:
				case LOGICAL_NOT:
				case LITERAL_new:
				case LSQUARE:
				case THIS:
				case LITERAL_true:
				case LITERAL_false:
				case LITERAL_null:
				case DECIMAL_LITERAL:
				case HEX_INTEGER_LITERAL:
				{
					argument_list();
					break;
				}
				case RPAREN:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
			match(RPAREN);
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_19_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void case_block() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			match(LBRACE);
			{
				switch ( LA(1) )
				{
				case LITERAL_case:
				{
					case_clauses();
					break;
				}
				case RBRACE:
				case LITERAL_default:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
			{
				switch ( LA(1) )
				{
				case LITERAL_default:
				{
					default_clause();
					{
						switch ( LA(1) )
						{
						case LITERAL_case:
						{
							case_clauses();
							break;
						}
						case RBRACE:
						{
							break;
						}
						default:
						{
							throw new NoViableAltException(LT(1), getFilename());
						}
						 }
					}
					break;
				}
				case RBRACE:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
			match(RBRACE);
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_3_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void case_clauses() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			{ // ( ... )+
			int _cnt70=0;
			for (;;)
			{
				if ((LA(1)==LITERAL_case))
				{
					case_clause();
				}
				else
				{
					if (_cnt70 >= 1) { goto _loop70_breakloop; } else { throw new NoViableAltException(LT(1), getFilename());; }
				}
				
				_cnt70++;
			}
_loop70_breakloop:			;
			}    // ( ... )+
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_20_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void default_clause() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			match(LITERAL_default);
			match(COLON);
			{
				switch ( LA(1) )
				{
				case IDENTIFIER:
				case LBRACE:
				case LITERAL_const:
				case SEMI_COLON:
				case LITERAL_class:
				case LITERAL_debugger:
				case LITERAL_if:
				case LITERAL_import:
				case LITERAL_interface:
				case LITERAL_do:
				case LITERAL_while:
				case LITERAL_for:
				case LITERAL_continue:
				case LITERAL_break:
				case LITERAL_package:
				case LITERAL_return:
				case LITERAL_with:
				case LITERAL_super:
				case LITERAL_switch:
				case LITERAL_enum:
				case LITERAL_static:
				case LITERAL_throw:
				case LITERAL_try:
				case CC_ON:
				case LITERAL_var:
				case LITERAL_public:
				case LITERAL_private:
				case LITERAL_protected:
				case LITERAL_internal:
				case LITERAL_expando:
				case LITERAL_abstract:
				case LITERAL_final:
				{
					statement_list();
					break;
				}
				case RBRACE:
				case LITERAL_case:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_21_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void case_clause() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			match(LITERAL_case);
			expression();
			match(COLON);
			{
				switch ( LA(1) )
				{
				case IDENTIFIER:
				case LBRACE:
				case LITERAL_const:
				case SEMI_COLON:
				case LITERAL_class:
				case LITERAL_debugger:
				case LITERAL_if:
				case LITERAL_import:
				case LITERAL_interface:
				case LITERAL_do:
				case LITERAL_while:
				case LITERAL_for:
				case LITERAL_continue:
				case LITERAL_break:
				case LITERAL_package:
				case LITERAL_return:
				case LITERAL_with:
				case LITERAL_super:
				case LITERAL_switch:
				case LITERAL_enum:
				case LITERAL_static:
				case LITERAL_throw:
				case LITERAL_try:
				case CC_ON:
				case LITERAL_var:
				case LITERAL_public:
				case LITERAL_private:
				case LITERAL_protected:
				case LITERAL_internal:
				case LITERAL_expando:
				case LITERAL_abstract:
				case LITERAL_final:
				{
					statement_list();
					break;
				}
				case RBRACE:
				case LITERAL_case:
				case LITERAL_default:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_9_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void catch_exp() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			match(LITERAL_catch);
			match(LPAREN);
			match(IDENTIFIER);
			match(RPAREN);
			block();
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_22_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void finally_exp() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			match(LITERAL_finally);
			block();
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_3_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void variable_declaration_list(
		VariableStatement varStm
	) //throws RecognitionException, TokenStreamException
{
		
		VariableDeclaration varDecl = null;
		
		try {      // for error handling
			varDecl=variable_declaration();
			if (0==inputState.guessing)
			{
				varStm.Add (varDecl);
			}
			{
				switch ( LA(1) )
				{
				case COMMA:
				{
					match(COMMA);
					variable_declaration_list(varStm);
					break;
				}
				case SEMI_COLON:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_23_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public VariableDeclaration  variable_declaration() //throws RecognitionException, TokenStreamException
{
		VariableDeclaration varDecl;
		
		Token  id = null;
		Token  type = null;
		varDecl = new VariableDeclaration ();
		
		try {      // for error handling
			id = LT(1);
			match(IDENTIFIER);
			if (0==inputState.guessing)
			{
				varDecl.Id = id.getText ();
			}
			{
				switch ( LA(1) )
				{
				case COLON:
				{
					match(COLON);
					type = LT(1);
					match(IDENTIFIER);
					break;
				}
				case ASSIGNMENT:
				case SEMI_COLON:
				case COMMA:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
			{
				switch ( LA(1) )
				{
				case ASSIGNMENT:
				{
					initialiser();
					break;
				}
				case SEMI_COLON:
				case COMMA:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_24_);
			}
			else
			{
				throw;
			}
		}
		return varDecl;
	}
	
	public void initialiser() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			match(ASSIGNMENT);
			assignment_expression();
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_24_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void assignment_expression() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			conditional_expression();
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_25_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void conditional_expression() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			logical_or_expression();
			{
				switch ( LA(1) )
				{
				case CONDITIONAL:
				{
					match(CONDITIONAL);
					assignment_expression();
					match(COLON);
					assignment_expression();
					break;
				}
				case RPAREN:
				case COLON:
				case RBRACE:
				case SEMI_COLON:
				case COMMA:
				case RSQUARE:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_25_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void assignment_operator() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			switch ( LA(1) )
			{
			case ASSIGNMENT:
			{
				match(ASSIGNMENT);
				break;
			}
			case MULTIPLICATION_ASSIGN:
			{
				match(MULTIPLICATION_ASSIGN);
				break;
			}
			case DIVISION_ASSIGN:
			{
				match(DIVISION_ASSIGN);
				break;
			}
			case REMAINDER_ASSIGN:
			{
				match(REMAINDER_ASSIGN);
				break;
			}
			case ADDITION_ASSIGN:
			{
				match(ADDITION_ASSIGN);
				break;
			}
			case SUBSTRACTION_ASSIGN:
			{
				match(SUBSTRACTION_ASSIGN);
				break;
			}
			case SIGNED_LEFT_SHIFT_ASSIGN:
			{
				match(SIGNED_LEFT_SHIFT_ASSIGN);
				break;
			}
			case SIGNED_RIGHT_SHIFT_ASSIGN:
			{
				match(SIGNED_RIGHT_SHIFT_ASSIGN);
				break;
			}
			case UNSIGNED_RIGHT_SHIFT_ASSIGN:
			{
				match(UNSIGNED_RIGHT_SHIFT_ASSIGN);
				break;
			}
			case BITWISE_AND_ASSIGN:
			{
				match(BITWISE_AND_ASSIGN);
				break;
			}
			case BITWISE_XOR_ASSIGN:
			{
				match(BITWISE_XOR_ASSIGN);
				break;
			}
			case BITWISE_OR_ASSIGN:
			{
				match(BITWISE_OR_ASSIGN);
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_0_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void logical_or_expression() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			logical_and_expression();
			{
				switch ( LA(1) )
				{
				case LOGICAL_OR:
				{
					match(LOGICAL_OR);
					logical_or_expression();
					break;
				}
				case RPAREN:
				case COLON:
				case RBRACE:
				case SEMI_COLON:
				case COMMA:
				case CONDITIONAL:
				case RSQUARE:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_26_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void logical_and_expression() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			bitwise_or_expression();
			{
				switch ( LA(1) )
				{
				case LOGICAL_AND:
				{
					match(LOGICAL_AND);
					logical_and_expression();
					break;
				}
				case RPAREN:
				case COLON:
				case RBRACE:
				case SEMI_COLON:
				case COMMA:
				case CONDITIONAL:
				case LOGICAL_OR:
				case RSQUARE:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_27_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void bitwise_or_expression() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			bitwise_xor_expression();
			{
				switch ( LA(1) )
				{
				case BITWISE_OR:
				{
					match(BITWISE_OR);
					bitwise_or_expression();
					break;
				}
				case RPAREN:
				case COLON:
				case RBRACE:
				case SEMI_COLON:
				case COMMA:
				case CONDITIONAL:
				case LOGICAL_OR:
				case LOGICAL_AND:
				case RSQUARE:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_28_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void bitwise_xor_expression() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			bitwise_and_expression();
			{
				switch ( LA(1) )
				{
				case BITWISE_XOR:
				{
					match(BITWISE_XOR);
					bitwise_xor_expression();
					break;
				}
				case RPAREN:
				case COLON:
				case RBRACE:
				case SEMI_COLON:
				case COMMA:
				case CONDITIONAL:
				case LOGICAL_OR:
				case LOGICAL_AND:
				case BITWISE_OR:
				case RSQUARE:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_29_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void bitwise_and_expression() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			equality_expression();
			{
				switch ( LA(1) )
				{
				case BITWISE_AND:
				{
					match(BITWISE_AND);
					bitwise_and_expression();
					break;
				}
				case RPAREN:
				case COLON:
				case RBRACE:
				case SEMI_COLON:
				case COMMA:
				case CONDITIONAL:
				case LOGICAL_OR:
				case LOGICAL_AND:
				case BITWISE_OR:
				case BITWISE_XOR:
				case RSQUARE:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_30_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void equality_expression() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			relational_expression();
			{
				switch ( LA(1) )
				{
				case EQUALS:
				case DOES_NOT_EQUALS:
				case STRICT_EQUALS:
				case STRICT_DOES_NOT_EQUALS:
				{
					{
						switch ( LA(1) )
						{
						case EQUALS:
						{
							match(EQUALS);
							break;
						}
						case DOES_NOT_EQUALS:
						{
							match(DOES_NOT_EQUALS);
							break;
						}
						case STRICT_EQUALS:
						{
							match(STRICT_EQUALS);
							break;
						}
						case STRICT_DOES_NOT_EQUALS:
						{
							match(STRICT_DOES_NOT_EQUALS);
							break;
						}
						default:
						{
							throw new NoViableAltException(LT(1), getFilename());
						}
						 }
					}
					equality_expression();
					break;
				}
				case RPAREN:
				case COLON:
				case RBRACE:
				case SEMI_COLON:
				case COMMA:
				case CONDITIONAL:
				case LOGICAL_OR:
				case LOGICAL_AND:
				case BITWISE_OR:
				case BITWISE_XOR:
				case BITWISE_AND:
				case RSQUARE:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_31_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void relational_expression() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			shift_expression();
			{
				switch ( LA(1) )
				{
				case LITERAL_in:
				case L_THAN:
				case G_THAN:
				case LE_THAN:
				case GE_THAN:
				case LITERAL_instanceof:
				{
					{
						switch ( LA(1) )
						{
						case L_THAN:
						{
							match(L_THAN);
							break;
						}
						case G_THAN:
						{
							match(G_THAN);
							break;
						}
						case LE_THAN:
						{
							match(LE_THAN);
							break;
						}
						case GE_THAN:
						{
							match(GE_THAN);
							break;
						}
						case LITERAL_instanceof:
						{
							match(LITERAL_instanceof);
							break;
						}
						case LITERAL_in:
						{
							match(LITERAL_in);
							break;
						}
						default:
						{
							throw new NoViableAltException(LT(1), getFilename());
						}
						 }
					}
					relational_expression();
					break;
				}
				case RPAREN:
				case COLON:
				case RBRACE:
				case SEMI_COLON:
				case COMMA:
				case CONDITIONAL:
				case LOGICAL_OR:
				case LOGICAL_AND:
				case BITWISE_OR:
				case BITWISE_XOR:
				case BITWISE_AND:
				case EQUALS:
				case DOES_NOT_EQUALS:
				case STRICT_EQUALS:
				case STRICT_DOES_NOT_EQUALS:
				case RSQUARE:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_32_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void shift_expression() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			additive_expression();
			{
				switch ( LA(1) )
				{
				case SIGNED_RIGHT_SHIFT:
				case SIGNED_LEFT_SHIFT:
				{
					{
						switch ( LA(1) )
						{
						case SIGNED_RIGHT_SHIFT:
						{
							match(SIGNED_RIGHT_SHIFT);
							break;
						}
						case SIGNED_LEFT_SHIFT:
						{
							match(SIGNED_LEFT_SHIFT);
							break;
						}
						default:
						{
							throw new NoViableAltException(LT(1), getFilename());
						}
						 }
					}
					shift_expression();
					break;
				}
				case RPAREN:
				case COLON:
				case RBRACE:
				case SEMI_COLON:
				case COMMA:
				case LITERAL_in:
				case CONDITIONAL:
				case LOGICAL_OR:
				case LOGICAL_AND:
				case BITWISE_OR:
				case BITWISE_XOR:
				case BITWISE_AND:
				case EQUALS:
				case DOES_NOT_EQUALS:
				case STRICT_EQUALS:
				case STRICT_DOES_NOT_EQUALS:
				case L_THAN:
				case G_THAN:
				case LE_THAN:
				case GE_THAN:
				case LITERAL_instanceof:
				case RSQUARE:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_33_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void additive_expression() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			multiplicative_expression();
			{
				switch ( LA(1) )
				{
				case PLUS:
				case MINUS:
				{
					{
						switch ( LA(1) )
						{
						case PLUS:
						{
							match(PLUS);
							break;
						}
						case MINUS:
						{
							match(MINUS);
							break;
						}
						default:
						{
							throw new NoViableAltException(LT(1), getFilename());
						}
						 }
					}
					additive_expression();
					break;
				}
				case RPAREN:
				case COLON:
				case RBRACE:
				case SEMI_COLON:
				case COMMA:
				case LITERAL_in:
				case CONDITIONAL:
				case LOGICAL_OR:
				case LOGICAL_AND:
				case BITWISE_OR:
				case BITWISE_XOR:
				case BITWISE_AND:
				case EQUALS:
				case DOES_NOT_EQUALS:
				case STRICT_EQUALS:
				case STRICT_DOES_NOT_EQUALS:
				case L_THAN:
				case G_THAN:
				case LE_THAN:
				case GE_THAN:
				case LITERAL_instanceof:
				case SIGNED_RIGHT_SHIFT:
				case SIGNED_LEFT_SHIFT:
				case RSQUARE:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_34_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void multiplicative_expression() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			unary_expression();
			{
				switch ( LA(1) )
				{
				case TIMES:
				case DIVISION:
				case REMAINDER:
				{
					{
						switch ( LA(1) )
						{
						case TIMES:
						{
							match(TIMES);
							break;
						}
						case DIVISION:
						{
							match(DIVISION);
							break;
						}
						case REMAINDER:
						{
							match(REMAINDER);
							break;
						}
						default:
						{
							throw new NoViableAltException(LT(1), getFilename());
						}
						 }
					}
					multiplicative_expression();
					break;
				}
				case RPAREN:
				case COLON:
				case RBRACE:
				case SEMI_COLON:
				case COMMA:
				case LITERAL_in:
				case CONDITIONAL:
				case LOGICAL_OR:
				case LOGICAL_AND:
				case BITWISE_OR:
				case BITWISE_XOR:
				case BITWISE_AND:
				case EQUALS:
				case DOES_NOT_EQUALS:
				case STRICT_EQUALS:
				case STRICT_DOES_NOT_EQUALS:
				case L_THAN:
				case G_THAN:
				case LE_THAN:
				case GE_THAN:
				case LITERAL_instanceof:
				case SIGNED_RIGHT_SHIFT:
				case SIGNED_LEFT_SHIFT:
				case PLUS:
				case MINUS:
				case RSQUARE:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_35_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void unary_expression() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			switch ( LA(1) )
			{
			case LITERAL_function:
			case IDENTIFIER:
			case LPAREN:
			case LBRACE:
			case STRING_LITERAL:
			case LITERAL_new:
			case LSQUARE:
			case THIS:
			case LITERAL_true:
			case LITERAL_false:
			case LITERAL_null:
			case DECIMAL_LITERAL:
			case HEX_INTEGER_LITERAL:
			{
				postfix_expression();
				break;
			}
			case PLUS:
			case MINUS:
			case LITERAL_delete:
			case LITERAL_void:
			case LITERAL_typeof:
			case INCREMENT:
			case DECREMENT:
			case BITWISE_NOT:
			case LOGICAL_NOT:
			{
				{
					switch ( LA(1) )
					{
					case LITERAL_delete:
					{
						match(LITERAL_delete);
						break;
					}
					case LITERAL_void:
					{
						match(LITERAL_void);
						break;
					}
					case LITERAL_typeof:
					{
						match(LITERAL_typeof);
						break;
					}
					case INCREMENT:
					{
						match(INCREMENT);
						break;
					}
					case DECREMENT:
					{
						match(DECREMENT);
						break;
					}
					case PLUS:
					{
						match(PLUS);
						break;
					}
					case MINUS:
					{
						match(MINUS);
						break;
					}
					case BITWISE_NOT:
					{
						match(BITWISE_NOT);
						break;
					}
					case LOGICAL_NOT:
					{
						match(LOGICAL_NOT);
						break;
					}
					default:
					{
						throw new NoViableAltException(LT(1), getFilename());
					}
					 }
				}
				unary_expression();
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_36_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void postfix_expression() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			left_hand_side_expression();
			{
				switch ( LA(1) )
				{
				case INCREMENT:
				{
					match(INCREMENT);
					break;
				}
				case DECREMENT:
				{
					match(DECREMENT);
					break;
				}
				case RPAREN:
				case COLON:
				case RBRACE:
				case SEMI_COLON:
				case COMMA:
				case LITERAL_in:
				case CONDITIONAL:
				case LOGICAL_OR:
				case LOGICAL_AND:
				case BITWISE_OR:
				case BITWISE_XOR:
				case BITWISE_AND:
				case EQUALS:
				case DOES_NOT_EQUALS:
				case STRICT_EQUALS:
				case STRICT_DOES_NOT_EQUALS:
				case L_THAN:
				case G_THAN:
				case LE_THAN:
				case GE_THAN:
				case LITERAL_instanceof:
				case SIGNED_RIGHT_SHIFT:
				case SIGNED_LEFT_SHIFT:
				case PLUS:
				case MINUS:
				case TIMES:
				case DIVISION:
				case REMAINDER:
				case RSQUARE:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_36_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void new_expression() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			switch ( LA(1) )
			{
			case LITERAL_function:
			case IDENTIFIER:
			case LPAREN:
			case LBRACE:
			case STRING_LITERAL:
			case LSQUARE:
			case THIS:
			case LITERAL_true:
			case LITERAL_false:
			case LITERAL_null:
			case DECIMAL_LITERAL:
			case HEX_INTEGER_LITERAL:
			{
				member_expression();
				break;
			}
			case LITERAL_new:
			{
				match(LITERAL_new);
				new_expression();
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_16_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void member_expression() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			{
				switch ( LA(1) )
				{
				case IDENTIFIER:
				case LPAREN:
				case LBRACE:
				case STRING_LITERAL:
				case LSQUARE:
				case THIS:
				case LITERAL_true:
				case LITERAL_false:
				case LITERAL_null:
				case DECIMAL_LITERAL:
				case HEX_INTEGER_LITERAL:
				{
					primary_expression();
					break;
				}
				case LITERAL_function:
				{
					function_expression();
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
			{    // ( ... )*
				for (;;)
				{
					switch ( LA(1) )
					{
					case LSQUARE:
					{
						match(LSQUARE);
						expression();
						match(RSQUARE);
						break;
					}
					case DOT:
					{
						match(DOT);
						match(IDENTIFIER);
						break;
					}
					default:
					{
						goto _loop149_breakloop;
					}
					 }
				}
_loop149_breakloop:				;
			}    // ( ... )*
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_37_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void call_expression() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			member_expression();
			arguments();
			{    // ( ... )*
				for (;;)
				{
					switch ( LA(1) )
					{
					case LPAREN:
					{
						arguments();
						break;
					}
					case LSQUARE:
					{
						match(LSQUARE);
						expression();
						match(RSQUARE);
						break;
					}
					case DOT:
					{
						match(DOT);
						match(IDENTIFIER);
						break;
					}
					default:
					{
						goto _loop145_breakloop;
					}
					 }
				}
_loop145_breakloop:				;
			}    // ( ... )*
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_0_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void primary_expression() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			switch ( LA(1) )
			{
			case THIS:
			{
				match(THIS);
				break;
			}
			case IDENTIFIER:
			{
				match(IDENTIFIER);
				break;
			}
			case STRING_LITERAL:
			case LITERAL_true:
			case LITERAL_false:
			case LITERAL_null:
			case DECIMAL_LITERAL:
			case HEX_INTEGER_LITERAL:
			{
				literal();
				break;
			}
			case LSQUARE:
			{
				array_literal();
				break;
			}
			case LBRACE:
			{
				object_literal();
				break;
			}
			case LPAREN:
			{
				match(LPAREN);
				expression();
				match(RPAREN);
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_10_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void function_expression() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			match(LITERAL_function);
			{
				switch ( LA(1) )
				{
				case IDENTIFIER:
				{
					match(IDENTIFIER);
					break;
				}
				case LPAREN:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
			match(LPAREN);
			{
				switch ( LA(1) )
				{
				case IDENTIFIER:
				{
					formal_parameter_list(null);
					break;
				}
				case RPAREN:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
			match(RPAREN);
			match(LBRACE);
			function_body(null);
			match(RBRACE);
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_10_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void argument_list() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			assignment_expression();
			{
				switch ( LA(1) )
				{
				case COMMA:
				{
					match(COMMA);
					argument_list();
					break;
				}
				case RPAREN:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_4_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void literal() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			switch ( LA(1) )
			{
			case LITERAL_true:
			case LITERAL_false:
			{
				boolean_literal();
				break;
			}
			case DECIMAL_LITERAL:
			case HEX_INTEGER_LITERAL:
			{
				numeric_literal();
				break;
			}
			case LITERAL_null:
			{
				null_literal();
				break;
			}
			case STRING_LITERAL:
			{
				match(STRING_LITERAL);
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_10_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void array_literal() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			match(LSQUARE);
			{
				switch ( LA(1) )
				{
				case COMMA:
				{
					elision();
					break;
				}
				case RSQUARE:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
			match(RSQUARE);
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_10_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void object_literal() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			match(LBRACE);
			{
				switch ( LA(1) )
				{
				case IDENTIFIER:
				case STRING_LITERAL:
				{
					property_name_and_value_list();
					break;
				}
				case RBRACE:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
			match(RBRACE);
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_10_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void boolean_literal() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			switch ( LA(1) )
			{
			case LITERAL_true:
			{
				match(LITERAL_true);
				break;
			}
			case LITERAL_false:
			{
				match(LITERAL_false);
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_10_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void null_literal() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			match(LITERAL_null);
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_10_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void elision() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			{ // ( ... )+
			int _cnt160=0;
			for (;;)
			{
				if ((LA(1)==COMMA))
				{
					match(COMMA);
				}
				else
				{
					if (_cnt160 >= 1) { goto _loop160_breakloop; } else { throw new NoViableAltException(LT(1), getFilename());; }
				}
				
				_cnt160++;
			}
_loop160_breakloop:			;
			}    // ( ... )+
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_38_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void property_name_and_value_list() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			property_name();
			match(COLON);
			assignment_expression();
			{
				switch ( LA(1) )
				{
				case COMMA:
				{
					match(COMMA);
					property_name_and_value_list();
					break;
				}
				case RBRACE:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_5_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void property_name() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			switch ( LA(1) )
			{
			case IDENTIFIER:
			{
				match(IDENTIFIER);
				break;
			}
			case STRING_LITERAL:
			{
				match(STRING_LITERAL);
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_39_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void modifier() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			switch ( LA(1) )
			{
			case LITERAL_public:
			{
				match(LITERAL_public);
				break;
			}
			case LITERAL_private:
			{
				match(LITERAL_private);
				break;
			}
			case LITERAL_protected:
			{
				match(LITERAL_protected);
				break;
			}
			case LITERAL_internal:
			{
				match(LITERAL_internal);
				break;
			}
			case LITERAL_expando:
			{
				match(LITERAL_expando);
				break;
			}
			case LITERAL_static:
			{
				match(LITERAL_static);
				break;
			}
			case LITERAL_abstract:
			{
				match(LITERAL_abstract);
				break;
			}
			case LITERAL_final:
			{
				match(LITERAL_final);
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_40_);
			}
			else
			{
				throw;
			}
		}
	}
	
	public void version_modifiers() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			switch ( LA(1) )
			{
			case LITERAL_hide:
			{
				match(LITERAL_hide);
				break;
			}
			case LITERAL_override:
			{
				match(LITERAL_override);
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				consume();
				consumeUntil(tokenSet_0_);
			}
			else
			{
				throw;
			}
		}
	}
	
	private void initializeFactory()
	{
	}
	
	public static readonly string[] tokenNames_ = new string[] {
		@"""<0>""",
		@"""EOF""",
		@"""<2>""",
		@"""NULL_TREE_LOOKAHEAD""",
		@"""function""",
		@"""IDENTIFIER""",
		@"""LPAREN""",
		@"""RPAREN""",
		@"""COLON""",
		@"""LBRACE""",
		@"""RBRACE""",
		@"""const""",
		@"""ASSIGNMENT""",
		@"""SEMI_COLON""",
		@"""class""",
		@"""extends""",
		@"""implements""",
		@"""COMMA""",
		@"""get""",
		@"""set""",
		@"""debugger""",
		@"""if""",
		@"""else""",
		@"""import""",
		@"""interface""",
		@"""do""",
		@"""while""",
		@"""for""",
		@"""in""",
		@"""continue""",
		@"""break""",
		@"""package""",
		@"""return""",
		@"""with""",
		@"""super""",
		@"""switch""",
		@"""case""",
		@"""default""",
		@"""enum""",
		@"""static""",
		@"""throw""",
		@"""try""",
		@"""CC_ON""",
		@"""COND_SET""",
		@"""COND_DEBUG""",
		@"""on""",
		@"""off""",
		@"""COND_POSITION""",
		@"""end""",
		@"""file""",
		@"""STRING_LITERAL""",
		@"""catch""",
		@"""finally""",
		@"""var""",
		@"""MULTIPLICATION_ASSIGN""",
		@"""DIVISION_ASSIGN""",
		@"""REMAINDER_ASSIGN""",
		@"""ADDITION_ASSIGN""",
		@"""SUBSTRACTION_ASSIGN""",
		@"""SIGNED_LEFT_SHIFT_ASSIGN""",
		@"""SIGNED_RIGHT_SHIFT_ASSIGN""",
		@"""UNSIGNED_RIGHT_SHIFT_ASSIGN""",
		@"""BITWISE_AND_ASSIGN""",
		@"""BITWISE_XOR_ASSIGN""",
		@"""BITWISE_OR_ASSIGN""",
		@"""CONDITIONAL""",
		@"""LOGICAL_OR""",
		@"""LOGICAL_AND""",
		@"""BITWISE_OR""",
		@"""BITWISE_XOR""",
		@"""BITWISE_AND""",
		@"""EQUALS""",
		@"""DOES_NOT_EQUALS""",
		@"""STRICT_EQUALS""",
		@"""STRICT_DOES_NOT_EQUALS""",
		@"""L_THAN""",
		@"""G_THAN""",
		@"""LE_THAN""",
		@"""GE_THAN""",
		@"""instanceof""",
		@"""SIGNED_RIGHT_SHIFT""",
		@"""SIGNED_LEFT_SHIFT""",
		@"""PLUS""",
		@"""MINUS""",
		@"""TIMES""",
		@"""DIVISION""",
		@"""REMAINDER""",
		@"""delete""",
		@"""void""",
		@"""typeof""",
		@"""INCREMENT""",
		@"""DECREMENT""",
		@"""BITWISE_NOT""",
		@"""LOGICAL_NOT""",
		@"""new""",
		@"""LSQUARE""",
		@"""RSQUARE""",
		@"""DOT""",
		@"""THIS""",
		@"""true""",
		@"""false""",
		@"""null""",
		@"""DECIMAL_LITERAL""",
		@"""HEX_INTEGER_LITERAL""",
		@"""public""",
		@"""private""",
		@"""protected""",
		@"""internal""",
		@"""expando""",
		@"""abstract""",
		@"""final""",
		@"""hide""",
		@"""override""",
		@"""TAB""",
		@"""VERTICAL_TAB""",
		@"""FORM_FEED""",
		@"""SPACE""",
		@"""NO_BREAK_SPACE""",
		@"""LINE_FEED""",
		@"""CARRIGE_RETURN""",
		@"""LINE_SEPARATOR""",
		@"""PARAGRAPH_SEPARATOR""",
		@"""UNSIGNED_RIGHT_SHIFT""",
		@"""COND_IF""",
		@"""COND_ELIF""",
		@"""COND_ELSE""",
		@"""COND_END""",
		@"""SL_COMMENT"""
	};
	
	private static long[] mk_tokenSet_0_()
	{
		long[] data = { 2L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_0_ = new BitSet(mk_tokenSet_0_());
	private static long[] mk_tokenSet_1_()
	{
		long[] data = { 1026L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_1_ = new BitSet(mk_tokenSet_1_());
	private static long[] mk_tokenSet_2_()
	{
		long[] data = { 9024585008705074L, 139637976727552L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_2_ = new BitSet(mk_tokenSet_2_());
	private static long[] mk_tokenSet_3_()
	{
		long[] data = { 9024791171329586L, 139637976727552L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_3_ = new BitSet(mk_tokenSet_3_());
	private static long[] mk_tokenSet_4_()
	{
		long[] data = { 128L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_4_ = new BitSet(mk_tokenSet_4_());
	private static long[] mk_tokenSet_5_()
	{
		long[] data = { 1024L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_5_ = new BitSet(mk_tokenSet_5_());
	private static long[] mk_tokenSet_6_()
	{
		long[] data = { 15780190612385330L, 139637976727552L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_6_ = new BitSet(mk_tokenSet_6_());
	private static long[] mk_tokenSet_7_()
	{
		long[] data = { 549755813888L, 139637976727552L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_7_ = new BitSet(mk_tokenSet_7_());
	private static long[] mk_tokenSet_8_()
	{
		long[] data = { 9007474149443600L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_8_ = new BitSet(mk_tokenSet_8_());
	private static long[] mk_tokenSet_9_()
	{
		long[] data = { 206158431232L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_9_ = new BitSet(mk_tokenSet_9_());
	private static long[] mk_tokenSet_10_()
	{
		long[] data = { 268576192L, 15242100734L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_10_ = new BitSet(mk_tokenSet_10_());
	private static long[] mk_tokenSet_11_()
	{
		long[] data = { 512L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_11_ = new BitSet(mk_tokenSet_11_());
	private static long[] mk_tokenSet_12_()
	{
		long[] data = { 9008023888480272L, 139637976727552L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_12_ = new BitSet(mk_tokenSet_12_());
	private static long[] mk_tokenSet_13_()
	{
		long[] data = { 9008023888481296L, 139637976727552L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_13_ = new BitSet(mk_tokenSet_13_());
	private static long[] mk_tokenSet_14_()
	{
		long[] data = { 8576L, 4294967296L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_14_ = new BitSet(mk_tokenSet_14_());
	private static long[] mk_tokenSet_15_()
	{
		long[] data = { 549755813904L, 139637976727552L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_15_ = new BitSet(mk_tokenSet_15_());
	private static long[] mk_tokenSet_16_()
	{
		long[] data = { 268576128L, 4504682494L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_16_ = new BitSet(mk_tokenSet_16_());
	private static long[] mk_tokenSet_17_()
	{
		long[] data = { 824650514432L, 139637976727552L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_17_ = new BitSet(mk_tokenSet_17_());
	private static long[] mk_tokenSet_18_()
	{
		long[] data = { 824650515456L, 139637976727552L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_18_ = new BitSet(mk_tokenSet_18_());
	private static long[] mk_tokenSet_19_()
	{
		long[] data = { 9024791171329650L, 139648714145792L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_19_ = new BitSet(mk_tokenSet_19_());
	private static long[] mk_tokenSet_20_()
	{
		long[] data = { 137438954496L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_20_ = new BitSet(mk_tokenSet_20_());
	private static long[] mk_tokenSet_21_()
	{
		long[] data = { 68719477760L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_21_ = new BitSet(mk_tokenSet_21_());
	private static long[] mk_tokenSet_22_()
	{
		long[] data = { 13528390798700082L, 139637976727552L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_22_ = new BitSet(mk_tokenSet_22_());
	private static long[] mk_tokenSet_23_()
	{
		long[] data = { 8192L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_23_ = new BitSet(mk_tokenSet_23_());
	private static long[] mk_tokenSet_24_()
	{
		long[] data = { 139264L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_24_ = new BitSet(mk_tokenSet_24_());
	private static long[] mk_tokenSet_25_()
	{
		long[] data = { 140672L, 4294967296L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_25_ = new BitSet(mk_tokenSet_25_());
	private static long[] mk_tokenSet_26_()
	{
		long[] data = { 140672L, 4294967298L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_26_ = new BitSet(mk_tokenSet_26_());
	private static long[] mk_tokenSet_27_()
	{
		long[] data = { 140672L, 4294967302L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_27_ = new BitSet(mk_tokenSet_27_());
	private static long[] mk_tokenSet_28_()
	{
		long[] data = { 140672L, 4294967310L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_28_ = new BitSet(mk_tokenSet_28_());
	private static long[] mk_tokenSet_29_()
	{
		long[] data = { 140672L, 4294967326L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_29_ = new BitSet(mk_tokenSet_29_());
	private static long[] mk_tokenSet_30_()
	{
		long[] data = { 140672L, 4294967358L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_30_ = new BitSet(mk_tokenSet_30_());
	private static long[] mk_tokenSet_31_()
	{
		long[] data = { 140672L, 4294967422L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_31_ = new BitSet(mk_tokenSet_31_());
	private static long[] mk_tokenSet_32_()
	{
		long[] data = { 140672L, 4294969342L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_32_ = new BitSet(mk_tokenSet_32_());
	private static long[] mk_tokenSet_33_()
	{
		long[] data = { 268576128L, 4295032830L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_33_ = new BitSet(mk_tokenSet_33_());
	private static long[] mk_tokenSet_34_()
	{
		long[] data = { 268576128L, 4295229438L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_34_ = new BitSet(mk_tokenSet_34_());
	private static long[] mk_tokenSet_35_()
	{
		long[] data = { 268576128L, 4296015870L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_35_ = new BitSet(mk_tokenSet_35_());
	private static long[] mk_tokenSet_36_()
	{
		long[] data = { 268576128L, 4303355902L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_36_ = new BitSet(mk_tokenSet_36_());
	private static long[] mk_tokenSet_37_()
	{
		long[] data = { 268576192L, 4504682494L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_37_ = new BitSet(mk_tokenSet_37_());
	private static long[] mk_tokenSet_38_()
	{
		long[] data = { 0L, 4294967296L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_38_ = new BitSet(mk_tokenSet_38_());
	private static long[] mk_tokenSet_39_()
	{
		long[] data = { 256L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_39_ = new BitSet(mk_tokenSet_39_());
	private static long[] mk_tokenSet_40_()
	{
		long[] data = { 9008023905257488L, 139637976727552L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_40_ = new BitSet(mk_tokenSet_40_());
	
}
}
