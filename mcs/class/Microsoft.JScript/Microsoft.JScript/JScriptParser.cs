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
		public const int LBRACE = 4;
		public const int RBRACE = 5;
		public const int SEMI_COLON = 6;
		public const int LITERAL_do = 7;
		public const int LITERAL_while = 8;
		public const int LPAREN = 9;
		public const int RPAREN = 10;
		public const int LITERAL_with = 11;
		public const int LITERAL_switch = 12;
		public const int LITERAL_case = 13;
		public const int COLON = 14;
		public const int LITERAL_default = 15;
		public const int IDENTIFIER = 16;
		public const int LITERAL_try = 17;
		public const int LITERAL_catch = 18;
		public const int LITERAL_finally = 19;
		public const int LITERAL_var = 20;
		public const int COMMA = 21;
		public const int ASSIGNMENT = 22;
		public const int COMPOUND_ASSIGNMENT = 23;
		public const int INTERROGATION = 24;
		public const int LOGICAL_OR = 25;
		public const int LOGICAL_AND = 26;
		public const int BITWISE_OR = 27;
		public const int TRIANGLE = 28;
		public const int BITWISE_AND = 29;
		public const int L_THAN = 30;
		public const int G_THAN = 31;
		public const int LE_THAN = 32;
		public const int GE_THAN = 33;
		public const int LITERAL_instanceof = 34;
		public const int LITERAL_in = 35;
		public const int PLUS = 36;
		public const int MINUS = 37;
		public const int TIMES = 38;
		public const int SLASH = 39;
		public const int PERCENT = 40;
		public const int LITERAL_delete = 41;
		public const int LITERAL_void = 42;
		public const int LITERAL_typeof = 43;
		public const int INCREMENT = 44;
		public const int DECREMENT = 45;
		public const int ADMIRATION = 46;
		public const int LITERAL_new = 47;
		public const int THIS = 48;
		public const int STRING_LITERAL = 49;
		public const int LITERAL_print = 50;
		public const int LITERAL_function = 51;
		public const int LITERAL_true = 52;
		public const int LITERAL_false = 53;
		public const int LITERAL_null = 54;
		public const int TAB = 55;
		public const int VERTICAL_TAB = 56;
		public const int FORM_FEED = 57;
		public const int SPACE = 58;
		public const int NO_BREAK_SPACE = 59;
		public const int LINE_FEED = 60;
		public const int CARRIGE_RETURN = 61;
		public const int LINE_SEPARATOR = 62;
		public const int PARAGRAPH_SEPARATOR = 63;
		public const int LSQUARE = 64;
		public const int RSQUARE = 65;
		public const int DOT = 66;
		
		
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
		Program p
	) //throws RecognitionException, TokenStreamException
{
		
		
		SourceElements elems;
		
		
		try {      // for error handling
			source_elements(p.SourceElements);
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			consume();
			consumeUntil(tokenSet_0_);
		}
	}
	
	public void source_elements(
		SourceElements elems
	) //throws RecognitionException, TokenStreamException
{
		
		
		SourceElement se;
		
		
		try {      // for error handling
			se=source_element();
			elems.Add (se);
			{
				switch ( LA(1) )
				{
				case LBRACE:
				case SEMI_COLON:
				case LITERAL_do:
				case LITERAL_with:
				case LITERAL_switch:
				case IDENTIFIER:
				case LITERAL_try:
				case LITERAL_var:
				case LITERAL_print:
				case LITERAL_function:
				{
					source_elements(elems);
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
			reportError(ex);
			consume();
			consumeUntil(tokenSet_1_);
		}
	}
	
	public SourceElement  source_element() //throws RecognitionException, TokenStreamException
{
		SourceElement se;
		
		
		se = new SourceElement ();
		Statement stm = null;
		FunctionDeclaration fd = null;
		
		
		try {      // for error handling
			switch ( LA(1) )
			{
			case LBRACE:
			case SEMI_COLON:
			case LITERAL_do:
			case LITERAL_with:
			case LITERAL_switch:
			case IDENTIFIER:
			case LITERAL_try:
			case LITERAL_var:
			case LITERAL_print:
			{
				stm=statement();
				se = stm;
				break;
			}
			case LITERAL_function:
			{
				fd=function_declaration();
				se = fd;
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
			reportError(ex);
			consume();
			consumeUntil(tokenSet_2_);
		}
		return se;
	}
	
	public Statement  statement() //throws RecognitionException, TokenStreamException
{
		Statement stm;
		
		
		stm = null;
		
		
		try {      // for error handling
			switch ( LA(1) )
			{
			case LBRACE:
			{
				block();
				break;
			}
			case LITERAL_var:
			{
				variable_statement();
				break;
			}
			case SEMI_COLON:
			{
				empty_statement();
				break;
			}
			case LITERAL_do:
			{
				iteration_statement();
				break;
			}
			case LITERAL_with:
			{
				with_statement();
				break;
			}
			case LITERAL_switch:
			{
				switch_statement();
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
			case LITERAL_print:
			{
				stm=print_statement();
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
			reportError(ex);
			consume();
			consumeUntil(tokenSet_3_);
		}
		return stm;
	}
	
	public FunctionDeclaration  function_declaration() //throws RecognitionException, TokenStreamException
{
		FunctionDeclaration fd;
		
		
		fd = new FunctionDeclaration ();
		
		
		try {      // for error handling
			match(LITERAL_function);
			match(IDENTIFIER);
			match(LPAREN);
			{
				switch ( LA(1) )
				{
				case IDENTIFIER:
				{
					formal_parameter_list();
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
			function_body(fd.elems);
			match(RBRACE);
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			consume();
			consumeUntil(tokenSet_2_);
		}
		return fd;
	}
	
	public void block() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			match(LBRACE);
			{
				switch ( LA(1) )
				{
				case LBRACE:
				case SEMI_COLON:
				case LITERAL_do:
				case LITERAL_with:
				case LITERAL_switch:
				case IDENTIFIER:
				case LITERAL_try:
				case LITERAL_var:
				case LITERAL_print:
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
			reportError(ex);
			consume();
			consumeUntil(tokenSet_4_);
		}
	}
	
	public void variable_statement() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			match(LITERAL_var);
			variable_declaration_list();
			match(SEMI_COLON);
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			consume();
			consumeUntil(tokenSet_3_);
		}
	}
	
	public void empty_statement() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			match(SEMI_COLON);
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			consume();
			consumeUntil(tokenSet_3_);
		}
	}
	
	public void iteration_statement() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			match(LITERAL_do);
			statement();
			match(LITERAL_while);
			match(LPAREN);
			expression();
			match(RPAREN);
			match(SEMI_COLON);
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			consume();
			consumeUntil(tokenSet_3_);
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
			reportError(ex);
			consume();
			consumeUntil(tokenSet_3_);
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
			reportError(ex);
			consume();
			consumeUntil(tokenSet_3_);
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
			reportError(ex);
			consume();
			consumeUntil(tokenSet_3_);
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
							case LBRACE:
							case RBRACE:
							case SEMI_COLON:
							case LITERAL_do:
							case LITERAL_while:
							case LITERAL_with:
							case LITERAL_switch:
							case LITERAL_case:
							case LITERAL_default:
							case IDENTIFIER:
							case LITERAL_try:
							case LITERAL_var:
							case LITERAL_print:
							case LITERAL_function:
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
			reportError(ex);
			consume();
			consumeUntil(tokenSet_3_);
		}
	}
	
	public PrintStatement  print_statement() //throws RecognitionException, TokenStreamException
{
		PrintStatement pn;
		
		Token  str = null;
		pn = new PrintStatement ();
		
		try {      // for error handling
			match(LITERAL_print);
			match(LPAREN);
			str = LT(1);
			match(STRING_LITERAL);
			match(RPAREN);
			match(SEMI_COLON);
			
			pn.Message =  str.getText (); 
			
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			consume();
			consumeUntil(tokenSet_3_);
		}
		return pn;
	}
	
	public void statement_list() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			statement();
			{
				switch ( LA(1) )
				{
				case LBRACE:
				case SEMI_COLON:
				case LITERAL_do:
				case LITERAL_with:
				case LITERAL_switch:
				case IDENTIFIER:
				case LITERAL_try:
				case LITERAL_var:
				case LITERAL_print:
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
			reportError(ex);
			consume();
			consumeUntil(tokenSet_5_);
		}
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
			reportError(ex);
			consume();
			consumeUntil(tokenSet_6_);
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
			reportError(ex);
			consume();
			consumeUntil(tokenSet_3_);
		}
	}
	
	public void case_clauses() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			{ // ( ... )+
			int _cnt18=0;
			for (;;)
			{
				if ((LA(1)==LITERAL_case))
				{
					case_clause();
				}
				else
				{
					if (_cnt18 >= 1) { goto _loop18_breakloop; } else { throw new NoViableAltException(LT(1), getFilename());; }
				}
				
				_cnt18++;
			}
_loop18_breakloop:			;
			}    // ( ... )+
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			consume();
			consumeUntil(tokenSet_7_);
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
				case LBRACE:
				case SEMI_COLON:
				case LITERAL_do:
				case LITERAL_with:
				case LITERAL_switch:
				case IDENTIFIER:
				case LITERAL_try:
				case LITERAL_var:
				case LITERAL_print:
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
			reportError(ex);
			consume();
			consumeUntil(tokenSet_8_);
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
				case LBRACE:
				case SEMI_COLON:
				case LITERAL_do:
				case LITERAL_with:
				case LITERAL_switch:
				case IDENTIFIER:
				case LITERAL_try:
				case LITERAL_var:
				case LITERAL_print:
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
			reportError(ex);
			consume();
			consumeUntil(tokenSet_5_);
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
			reportError(ex);
			consume();
			consumeUntil(tokenSet_9_);
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
			reportError(ex);
			consume();
			consumeUntil(tokenSet_3_);
		}
	}
	
	public void variable_declaration_list() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			variable_declaration();
			{
				switch ( LA(1) )
				{
				case COMMA:
				{
					match(COMMA);
					variable_declaration_list();
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
			reportError(ex);
			consume();
			consumeUntil(tokenSet_10_);
		}
	}
	
	public void variable_declaration() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			match(IDENTIFIER);
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
			reportError(ex);
			consume();
			consumeUntil(tokenSet_11_);
		}
	}
	
	public void initialiser() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			match(ASSIGNMENT);
			assignment_expression();
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			consume();
			consumeUntil(tokenSet_11_);
		}
	}
	
	public void assignment_expression() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			conditional_expression();
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			consume();
			consumeUntil(tokenSet_12_);
		}
	}
	
	public void conditional_expression() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			logical_or_expression();
			{
				switch ( LA(1) )
				{
				case INTERROGATION:
				{
					match(INTERROGATION);
					assignment_expression();
					match(COLON);
					assignment_expression();
					break;
				}
				case RBRACE:
				case SEMI_COLON:
				case RPAREN:
				case COLON:
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
			reportError(ex);
			consume();
			consumeUntil(tokenSet_12_);
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
			case COMPOUND_ASSIGNMENT:
			{
				match(COMPOUND_ASSIGNMENT);
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
			reportError(ex);
			consume();
			consumeUntil(tokenSet_0_);
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
				case RBRACE:
				case SEMI_COLON:
				case RPAREN:
				case COLON:
				case COMMA:
				case INTERROGATION:
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
			reportError(ex);
			consume();
			consumeUntil(tokenSet_13_);
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
				case RBRACE:
				case SEMI_COLON:
				case RPAREN:
				case COLON:
				case COMMA:
				case INTERROGATION:
				case LOGICAL_OR:
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
			reportError(ex);
			consume();
			consumeUntil(tokenSet_14_);
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
				case RBRACE:
				case SEMI_COLON:
				case RPAREN:
				case COLON:
				case COMMA:
				case INTERROGATION:
				case LOGICAL_OR:
				case LOGICAL_AND:
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
			reportError(ex);
			consume();
			consumeUntil(tokenSet_15_);
		}
	}
	
	public void bitwise_xor_expression() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			bitwise_and_expression();
			{
				switch ( LA(1) )
				{
				case TRIANGLE:
				{
					match(TRIANGLE);
					bitwise_xor_expression();
					break;
				}
				case RBRACE:
				case SEMI_COLON:
				case RPAREN:
				case COLON:
				case COMMA:
				case INTERROGATION:
				case LOGICAL_OR:
				case LOGICAL_AND:
				case BITWISE_OR:
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
			reportError(ex);
			consume();
			consumeUntil(tokenSet_16_);
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
				case RBRACE:
				case SEMI_COLON:
				case RPAREN:
				case COLON:
				case COMMA:
				case INTERROGATION:
				case LOGICAL_OR:
				case LOGICAL_AND:
				case BITWISE_OR:
				case TRIANGLE:
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
			reportError(ex);
			consume();
			consumeUntil(tokenSet_17_);
		}
	}
	
	public void equality_expression() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			relational_expression();
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			consume();
			consumeUntil(tokenSet_18_);
		}
	}
	
	public void relational_expression() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			shift_expression();
			{
				switch ( LA(1) )
				{
				case L_THAN:
				case G_THAN:
				case LE_THAN:
				case GE_THAN:
				case LITERAL_instanceof:
				case LITERAL_in:
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
				case RBRACE:
				case SEMI_COLON:
				case RPAREN:
				case COLON:
				case COMMA:
				case INTERROGATION:
				case LOGICAL_OR:
				case LOGICAL_AND:
				case BITWISE_OR:
				case TRIANGLE:
				case BITWISE_AND:
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
			reportError(ex);
			consume();
			consumeUntil(tokenSet_18_);
		}
	}
	
	public void shift_expression() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			additive_expression();
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			consume();
			consumeUntil(tokenSet_19_);
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
				case RBRACE:
				case SEMI_COLON:
				case RPAREN:
				case COLON:
				case COMMA:
				case INTERROGATION:
				case LOGICAL_OR:
				case LOGICAL_AND:
				case BITWISE_OR:
				case TRIANGLE:
				case BITWISE_AND:
				case L_THAN:
				case G_THAN:
				case LE_THAN:
				case GE_THAN:
				case LITERAL_instanceof:
				case LITERAL_in:
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
			reportError(ex);
			consume();
			consumeUntil(tokenSet_19_);
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
				case SLASH:
				case PERCENT:
				{
					{
						switch ( LA(1) )
						{
						case TIMES:
						{
							match(TIMES);
							break;
						}
						case SLASH:
						{
							match(SLASH);
							break;
						}
						case PERCENT:
						{
							match(PERCENT);
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
				case RBRACE:
				case SEMI_COLON:
				case RPAREN:
				case COLON:
				case COMMA:
				case INTERROGATION:
				case LOGICAL_OR:
				case LOGICAL_AND:
				case BITWISE_OR:
				case TRIANGLE:
				case BITWISE_AND:
				case L_THAN:
				case G_THAN:
				case LE_THAN:
				case GE_THAN:
				case LITERAL_instanceof:
				case LITERAL_in:
				case PLUS:
				case MINUS:
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
			reportError(ex);
			consume();
			consumeUntil(tokenSet_20_);
		}
	}
	
	public void unary_expression() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			switch ( LA(1) )
			{
			case LBRACE:
			case LPAREN:
			case IDENTIFIER:
			case LITERAL_new:
			case THIS:
			case STRING_LITERAL:
			case LITERAL_function:
			case LITERAL_true:
			case LITERAL_false:
			case LITERAL_null:
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
			case ADMIRATION:
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
					case ADMIRATION:
					{
						match(ADMIRATION);
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
			reportError(ex);
			consume();
			consumeUntil(tokenSet_21_);
		}
	}
	
	public void postfix_expression() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			left_hand_side_expression();
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			consume();
			consumeUntil(tokenSet_21_);
		}
	}
	
	public void left_hand_side_expression() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			new_expression();
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			consume();
			consumeUntil(tokenSet_21_);
		}
	}
	
	public void new_expression() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			switch ( LA(1) )
			{
			case LBRACE:
			case LPAREN:
			case IDENTIFIER:
			case THIS:
			case STRING_LITERAL:
			case LITERAL_function:
			case LITERAL_true:
			case LITERAL_false:
			case LITERAL_null:
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
			reportError(ex);
			consume();
			consumeUntil(tokenSet_21_);
		}
	}
	
	public void member_expression() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			switch ( LA(1) )
			{
			case LBRACE:
			case LPAREN:
			case IDENTIFIER:
			case THIS:
			case STRING_LITERAL:
			case LITERAL_true:
			case LITERAL_false:
			case LITERAL_null:
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
		catch (RecognitionException ex)
		{
			reportError(ex);
			consume();
			consumeUntil(tokenSet_22_);
		}
	}
	
	public void call_expression() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			member_expression();
			arguments();
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			consume();
			consumeUntil(tokenSet_0_);
		}
	}
	
	public void arguments() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			match(LPAREN);
			{
				switch ( LA(1) )
				{
				case LBRACE:
				case LPAREN:
				case IDENTIFIER:
				case PLUS:
				case MINUS:
				case LITERAL_delete:
				case LITERAL_void:
				case LITERAL_typeof:
				case INCREMENT:
				case DECREMENT:
				case ADMIRATION:
				case LITERAL_new:
				case THIS:
				case STRING_LITERAL:
				case LITERAL_function:
				case LITERAL_true:
				case LITERAL_false:
				case LITERAL_null:
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
			reportError(ex);
			consume();
			consumeUntil(tokenSet_0_);
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
			{
				literal();
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
			reportError(ex);
			consume();
			consumeUntil(tokenSet_22_);
		}
	}
	
	public void function_expression() //throws RecognitionException, TokenStreamException
{
		
		SourceElements elems = new SourceElements ();
		
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
					formal_parameter_list();
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
			function_body(elems);
			match(RBRACE);
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			consume();
			consumeUntil(tokenSet_22_);
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
			reportError(ex);
			consume();
			consumeUntil(tokenSet_23_);
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
			reportError(ex);
			consume();
			consumeUntil(tokenSet_22_);
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
			reportError(ex);
			consume();
			consumeUntil(tokenSet_22_);
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
			reportError(ex);
			consume();
			consumeUntil(tokenSet_22_);
		}
	}
	
	public void null_literal() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			match(LITERAL_null);
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			consume();
			consumeUntil(tokenSet_22_);
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
			reportError(ex);
			consume();
			consumeUntil(tokenSet_24_);
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
			reportError(ex);
			consume();
			consumeUntil(tokenSet_25_);
		}
	}
	
	public void formal_parameter_list() //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			match(IDENTIFIER);
			{
				switch ( LA(1) )
				{
				case COMMA:
				{
					match(COMMA);
					formal_parameter_list();
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
			reportError(ex);
			consume();
			consumeUntil(tokenSet_23_);
		}
	}
	
	public void function_body(
		SourceElements elems
	) //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			source_elements(elems);
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			consume();
			consumeUntil(tokenSet_24_);
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
		@"""LBRACE""",
		@"""RBRACE""",
		@"""SEMI_COLON""",
		@"""do""",
		@"""while""",
		@"""LPAREN""",
		@"""RPAREN""",
		@"""with""",
		@"""switch""",
		@"""case""",
		@"""COLON""",
		@"""default""",
		@"""IDENTIFIER""",
		@"""try""",
		@"""catch""",
		@"""finally""",
		@"""var""",
		@"""COMMA""",
		@"""ASSIGNMENT""",
		@"""COMPOUND_ASSIGNMENT""",
		@"""INTERROGATION""",
		@"""LOGICAL_OR""",
		@"""LOGICAL_AND""",
		@"""BITWISE_OR""",
		@"""TRIANGLE""",
		@"""BITWISE_AND""",
		@"""L_THAN""",
		@"""G_THAN""",
		@"""LE_THAN""",
		@"""GE_THAN""",
		@"""instanceof""",
		@"""in""",
		@"""PLUS""",
		@"""MINUS""",
		@"""TIMES""",
		@"""SLASH""",
		@"""PERCENT""",
		@"""delete""",
		@"""void""",
		@"""typeof""",
		@"""INCREMENT""",
		@"""DECREMENT""",
		@"""ADMIRATION""",
		@"""new""",
		@"""THIS""",
		@"""STRING_LITERAL""",
		@"""print""",
		@"""function""",
		@"""true""",
		@"""false""",
		@"""null""",
		@"""TAB""",
		@"""VERTICAL_TAB""",
		@"""FORM_FEED""",
		@"""SPACE""",
		@"""NO_BREAK_SPACE""",
		@"""LINE_FEED""",
		@"""CARRIGE_RETURN""",
		@"""LINE_SEPARATOR""",
		@"""PARAGRAPH_SEPARATOR""",
		@"""LSQUARE""",
		@"""RSQUARE""",
		@"""DOT"""
	};
	
	private static long[] mk_tokenSet_0_()
	{
		long[] data = { 2L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_0_ = new BitSet(mk_tokenSet_0_());
	private static long[] mk_tokenSet_1_()
	{
		long[] data = { 34L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_1_ = new BitSet(mk_tokenSet_1_());
	private static long[] mk_tokenSet_2_()
	{
		long[] data = { 3377699721779442L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_2_ = new BitSet(mk_tokenSet_2_());
	private static long[] mk_tokenSet_3_()
	{
		long[] data = { 3377699721820658L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_3_ = new BitSet(mk_tokenSet_3_());
	private static long[] mk_tokenSet_4_()
	{
		long[] data = { 3377699722607090L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_4_ = new BitSet(mk_tokenSet_4_());
	private static long[] mk_tokenSet_5_()
	{
		long[] data = { 40992L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_5_ = new BitSet(mk_tokenSet_5_());
	private static long[] mk_tokenSet_6_()
	{
		long[] data = { 17408L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_6_ = new BitSet(mk_tokenSet_6_());
	private static long[] mk_tokenSet_7_()
	{
		long[] data = { 32800L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_7_ = new BitSet(mk_tokenSet_7_());
	private static long[] mk_tokenSet_8_()
	{
		long[] data = { 8224L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_8_ = new BitSet(mk_tokenSet_8_());
	private static long[] mk_tokenSet_9_()
	{
		long[] data = { 3377699722344946L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_9_ = new BitSet(mk_tokenSet_9_());
	private static long[] mk_tokenSet_10_()
	{
		long[] data = { 64L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_10_ = new BitSet(mk_tokenSet_10_());
	private static long[] mk_tokenSet_11_()
	{
		long[] data = { 2097216L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_11_ = new BitSet(mk_tokenSet_11_());
	private static long[] mk_tokenSet_12_()
	{
		long[] data = { 2114656L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_12_ = new BitSet(mk_tokenSet_12_());
	private static long[] mk_tokenSet_13_()
	{
		long[] data = { 18891872L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_13_ = new BitSet(mk_tokenSet_13_());
	private static long[] mk_tokenSet_14_()
	{
		long[] data = { 52446304L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_14_ = new BitSet(mk_tokenSet_14_());
	private static long[] mk_tokenSet_15_()
	{
		long[] data = { 119555168L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_15_ = new BitSet(mk_tokenSet_15_());
	private static long[] mk_tokenSet_16_()
	{
		long[] data = { 253772896L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_16_ = new BitSet(mk_tokenSet_16_());
	private static long[] mk_tokenSet_17_()
	{
		long[] data = { 522208352L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_17_ = new BitSet(mk_tokenSet_17_());
	private static long[] mk_tokenSet_18_()
	{
		long[] data = { 1059079264L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_18_ = new BitSet(mk_tokenSet_18_());
	private static long[] mk_tokenSet_19_()
	{
		long[] data = { 68704814176L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_19_ = new BitSet(mk_tokenSet_19_());
	private static long[] mk_tokenSet_20_()
	{
		long[] data = { 274863244384L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_20_ = new BitSet(mk_tokenSet_20_());
	private static long[] mk_tokenSet_21_()
	{
		long[] data = { 2199008592992L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_21_ = new BitSet(mk_tokenSet_21_());
	private static long[] mk_tokenSet_22_()
	{
		long[] data = { 2199008593504L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_22_ = new BitSet(mk_tokenSet_22_());
	private static long[] mk_tokenSet_23_()
	{
		long[] data = { 1024L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_23_ = new BitSet(mk_tokenSet_23_());
	private static long[] mk_tokenSet_24_()
	{
		long[] data = { 32L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_24_ = new BitSet(mk_tokenSet_24_());
	private static long[] mk_tokenSet_25_()
	{
		long[] data = { 16384L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_25_ = new BitSet(mk_tokenSet_25_());
	
}
}
