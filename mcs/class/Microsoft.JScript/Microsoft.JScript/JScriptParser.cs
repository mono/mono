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
		public const int OPEN_PARENS = 6;
		public const int CLOSE_PARENS = 7;
		public const int OPEN_BRACE = 8;
		public const int CLOSE_BRACE = 9;
		public const int COMMA = 10;
		public const int LITERAL_try = 11;
		public const int LITERAL_catch = 12;
		public const int LITERAL_finally = 13;
		public const int LITERAL_throw = 14;
		public const int SEMI_COLON = 15;
		public const int LITERAL_switch = 16;
		public const int LITERAL_default = 17;
		public const int COLON = 18;
		public const int LITERAL_case = 19;
		public const int LITERAL_with = 20;
		public const int LITERAL_return = 21;
		public const int LITERAL_break = 22;
		public const int LITERAL_continue = 23;
		public const int LITERAL_do = 24;
		public const int LITERAL_while = 25;
		public const int LITERAL_for = 26;
		public const int LITERAL_var = 27;
		public const int LITERAL_in = 28;
		public const int LITERAL_if = 29;
		public const int LITERAL_else = 30;
		public const int ASSIGN = 31;
		public const int LITERAL_new = 32;
		// "." = 33
		public const int OPEN_BRACKET = 34;
		public const int CLOSE_BRACKET = 35;
		// "(" = 36
		// ")" = 37
		// "[" = 38
		// "]" = 39
		public const int DOT = 40;
		// "++" = 41
		// "--" = 42
		public const int LITERAL_delete = 43;
		public const int LITERAL_void = 44;
		public const int LITERAL_typeof = 45;
		public const int INCREMENT = 46;
		public const int DECREMENT = 47;
		public const int PLUS = 48;
		public const int MINUS = 49;
		public const int BITWISE_NOT = 50;
		public const int LOGICAL_NOT = 51;
		public const int MULT = 52;
		public const int DIVISION = 53;
		public const int MODULE = 54;
		public const int SHIFT_LEFT = 55;
		public const int SHIFT_RIGHT = 56;
		public const int UNSIGNED_SHIFT_RIGHT = 57;
		public const int LESS_THAN = 58;
		public const int GREATER_THAN = 59;
		public const int LESS_EQ = 60;
		public const int GREATER_EQ = 61;
		public const int LITERAL_instanceof = 62;
		public const int EQ = 63;
		public const int NEQ = 64;
		public const int STRICT_EQ = 65;
		public const int STRICT_NEQ = 66;
		public const int BITWISE_AND = 67;
		public const int BITWISE_XOR = 68;
		public const int BITWISE_OR = 69;
		public const int LOGICAL_AND = 70;
		public const int LOGICAL_OR = 71;
		public const int INTERR = 72;
		public const int MULT_ASSIGN = 73;
		public const int DIV_ASSIGN = 74;
		public const int MOD_ASSIGN = 75;
		public const int ADD_ASSIGN = 76;
		public const int SUB_ASSIGN = 77;
		public const int SHIFT_LEFT_ASSIGN = 78;
		public const int SHIFT_RIGHT_ASSIGN = 79;
		public const int AND_ASSIGN = 80;
		public const int XOR_ASSIGN = 81;
		public const int OR_ASSIGN = 82;
		public const int LITERAL_this = 83;
		public const int LITERAL_null = 84;
		public const int LITERAL_true = 85;
		public const int LITERAL_false = 86;
		public const int STRING_LITERAL = 87;
		public const int DECIMAL_LITERAL = 88;
		public const int HEX_INTEGER_LITERAL = 89;
		public const int LINE_FEED = 90;
		public const int CARRIAGE_RETURN = 91;
		public const int LINE_SEPARATOR = 92;
		public const int PARAGRAPH_SEPARATOR = 93;
		public const int TAB = 94;
		public const int VERTICAL_TAB = 95;
		public const int FORM_FEED = 96;
		public const int SPACE = 97;
		public const int NO_BREAK_SPACE = 98;
		public const int STRICT_NEW = 99;
		public const int SL_COMMENT = 100;
		
		
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
		
	public void program() //throws RecognitionException, TokenStreamException
{
		
		traceIn("program");
		try { // debugging
			
			source_elements();
		}
		finally
		{ // debugging
			traceOut("program");
		}
	}
	
	public void source_elements() //throws RecognitionException, TokenStreamException
{
		
		traceIn("source_elements");
		try { // debugging
			
			{ // ( ... )+
			int _cnt4=0;
			for (;;)
			{
				if ((tokenSet_0_.member(LA(1))))
				{
					source_element();
				}
				else
				{
					if (_cnt4 >= 1) { goto _loop4_breakloop; } else { throw new NoViableAltException(LT(1), getFilename());; }
				}
				
				_cnt4++;
			}
_loop4_breakloop:			;
			}    // ( ... )+
		}
		finally
		{ // debugging
			traceOut("source_elements");
		}
	}
	
	public void source_element() //throws RecognitionException, TokenStreamException
{
		
		traceIn("source_element");
		try { // debugging
			
			switch ( LA(1) )
			{
			case IDENTIFIER:
			case OPEN_PARENS:
			case OPEN_BRACE:
			case LITERAL_try:
			case LITERAL_throw:
			case SEMI_COLON:
			case LITERAL_switch:
			case LITERAL_with:
			case LITERAL_return:
			case LITERAL_break:
			case LITERAL_continue:
			case LITERAL_do:
			case LITERAL_while:
			case LITERAL_for:
			case LITERAL_var:
			case LITERAL_if:
			case LITERAL_new:
			case OPEN_BRACKET:
			case LITERAL_delete:
			case LITERAL_void:
			case LITERAL_typeof:
			case INCREMENT:
			case DECREMENT:
			case PLUS:
			case MINUS:
			case BITWISE_NOT:
			case LOGICAL_NOT:
			case LITERAL_this:
			case LITERAL_null:
			case LITERAL_true:
			case LITERAL_false:
			case STRING_LITERAL:
			case DECIMAL_LITERAL:
			case HEX_INTEGER_LITERAL:
			{
				statement();
				break;
			}
			case LITERAL_function:
			{
				function_decl_or_expr();
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		finally
		{ // debugging
			traceOut("source_element");
		}
	}
	
	public void statement() //throws RecognitionException, TokenStreamException
{
		
		traceIn("statement");
		try { // debugging
			
			switch ( LA(1) )
			{
			case IDENTIFIER:
			case OPEN_PARENS:
			case OPEN_BRACE:
			case LITERAL_new:
			case OPEN_BRACKET:
			case LITERAL_delete:
			case LITERAL_void:
			case LITERAL_typeof:
			case INCREMENT:
			case DECREMENT:
			case PLUS:
			case MINUS:
			case BITWISE_NOT:
			case LOGICAL_NOT:
			case LITERAL_this:
			case LITERAL_null:
			case LITERAL_true:
			case LITERAL_false:
			case STRING_LITERAL:
			case DECIMAL_LITERAL:
			case HEX_INTEGER_LITERAL:
			{
				expr_stm();
				break;
			}
			case LITERAL_var:
			{
				var_stm();
				break;
			}
			case SEMI_COLON:
			{
				empty_stm();
				break;
			}
			case LITERAL_if:
			{
				if_stm();
				break;
			}
			case LITERAL_do:
			case LITERAL_while:
			case LITERAL_for:
			{
				iteration_stm();
				break;
			}
			case LITERAL_continue:
			{
				continue_stm();
				break;
			}
			case LITERAL_break:
			{
				break_stm();
				break;
			}
			case LITERAL_return:
			{
				return_stm();
				break;
			}
			case LITERAL_with:
			{
				with_stm();
				break;
			}
			case LITERAL_switch:
			{
				switch_stm();
				break;
			}
			case LITERAL_throw:
			{
				throw_stm();
				break;
			}
			case LITERAL_try:
			{
				try_stm();
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		finally
		{ // debugging
			traceOut("statement");
		}
	}
	
	public void function_decl_or_expr() //throws RecognitionException, TokenStreamException
{
		
		traceIn("function_decl_or_expr");
		try { // debugging
			
			match(LITERAL_function);
			{
				switch ( LA(1) )
				{
				case IDENTIFIER:
				{
					match(IDENTIFIER);
					break;
				}
				case OPEN_PARENS:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
			match(OPEN_PARENS);
			formal_param_list();
			match(CLOSE_PARENS);
			match(OPEN_BRACE);
			function_body();
			match(CLOSE_BRACE);
		}
		finally
		{ // debugging
			traceOut("function_decl_or_expr");
		}
	}
	
	public void formal_param_list() //throws RecognitionException, TokenStreamException
{
		
		traceIn("formal_param_list");
		try { // debugging
			
			{
				switch ( LA(1) )
				{
				case IDENTIFIER:
				{
					match(IDENTIFIER);
					break;
				}
				case CLOSE_PARENS:
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
						goto _loop12_breakloop;
					}
					
				}
_loop12_breakloop:				;
			}    // ( ... )*
		}
		finally
		{ // debugging
			traceOut("formal_param_list");
		}
	}
	
	public void function_body() //throws RecognitionException, TokenStreamException
{
		
		traceIn("function_body");
		try { // debugging
			
			source_elements();
		}
		finally
		{ // debugging
			traceOut("function_body");
		}
	}
	
	public void expr_stm() //throws RecognitionException, TokenStreamException
{
		
		traceIn("expr_stm");
		try { // debugging
			
			expr();
		}
		finally
		{ // debugging
			traceOut("expr_stm");
		}
	}
	
	public void var_stm() //throws RecognitionException, TokenStreamException
{
		
		traceIn("var_stm");
		try { // debugging
			
			match(LITERAL_var);
			var_decl_list();
			match(SEMI_COLON);
		}
		finally
		{ // debugging
			traceOut("var_stm");
		}
	}
	
	public void empty_stm() //throws RecognitionException, TokenStreamException
{
		
		traceIn("empty_stm");
		try { // debugging
			
			match(SEMI_COLON);
		}
		finally
		{ // debugging
			traceOut("empty_stm");
		}
	}
	
	public void if_stm() //throws RecognitionException, TokenStreamException
{
		
		traceIn("if_stm");
		try { // debugging
			
			match(LITERAL_if);
			match(OPEN_PARENS);
			expr();
			match(CLOSE_PARENS);
			statement();
			{
				bool synPredMatched50 = false;
				if (((LA(1)==LITERAL_else)))
				{
					int _m50 = mark();
					synPredMatched50 = true;
					inputState.guessing++;
					try {
						{
							match(LITERAL_else);
						}
					}
					catch (RecognitionException)
					{
						synPredMatched50 = false;
					}
					rewind(_m50);
					inputState.guessing--;
				}
				if ( synPredMatched50 )
				{
					match(LITERAL_else);
					statement();
				}
				else if ((tokenSet_1_.member(LA(1)))) {
				}
				else
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				
			}
		}
		finally
		{ // debugging
			traceOut("if_stm");
		}
	}
	
	public void iteration_stm() //throws RecognitionException, TokenStreamException
{
		
		traceIn("iteration_stm");
		try { // debugging
			
			switch ( LA(1) )
			{
			case LITERAL_do:
			{
				match(LITERAL_do);
				statement();
				match(LITERAL_while);
				match(OPEN_PARENS);
				expr();
				match(CLOSE_PARENS);
				match(SEMI_COLON);
				break;
			}
			case LITERAL_while:
			{
				match(LITERAL_while);
				match(OPEN_PARENS);
				expr();
				match(CLOSE_PARENS);
				statement();
				break;
			}
			case LITERAL_for:
			{
				match(LITERAL_for);
				match(OPEN_PARENS);
				inside_for();
				match(CLOSE_PARENS);
				statement();
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		finally
		{ // debugging
			traceOut("iteration_stm");
		}
	}
	
	public void continue_stm() //throws RecognitionException, TokenStreamException
{
		
		traceIn("continue_stm");
		try { // debugging
			
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
		finally
		{ // debugging
			traceOut("continue_stm");
		}
	}
	
	public void break_stm() //throws RecognitionException, TokenStreamException
{
		
		traceIn("break_stm");
		try { // debugging
			
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
		finally
		{ // debugging
			traceOut("break_stm");
		}
	}
	
	public void return_stm() //throws RecognitionException, TokenStreamException
{
		
		traceIn("return_stm");
		try { // debugging
			
			match(LITERAL_return);
			{
				switch ( LA(1) )
				{
				case IDENTIFIER:
				case OPEN_PARENS:
				case OPEN_BRACE:
				case LITERAL_new:
				case OPEN_BRACKET:
				case LITERAL_delete:
				case LITERAL_void:
				case LITERAL_typeof:
				case INCREMENT:
				case DECREMENT:
				case PLUS:
				case MINUS:
				case BITWISE_NOT:
				case LOGICAL_NOT:
				case LITERAL_this:
				case LITERAL_null:
				case LITERAL_true:
				case LITERAL_false:
				case STRING_LITERAL:
				case DECIMAL_LITERAL:
				case HEX_INTEGER_LITERAL:
				{
					expr();
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
		finally
		{ // debugging
			traceOut("return_stm");
		}
	}
	
	public void with_stm() //throws RecognitionException, TokenStreamException
{
		
		traceIn("with_stm");
		try { // debugging
			
			match(LITERAL_with);
			match(OPEN_PARENS);
			expr();
			match(CLOSE_PARENS);
			statement();
		}
		finally
		{ // debugging
			traceOut("with_stm");
		}
	}
	
	public void switch_stm() //throws RecognitionException, TokenStreamException
{
		
		traceIn("switch_stm");
		try { // debugging
			
			match(LITERAL_switch);
			match(OPEN_PARENS);
			expr();
			match(CLOSE_PARENS);
			case_block();
		}
		finally
		{ // debugging
			traceOut("switch_stm");
		}
	}
	
	public void throw_stm() //throws RecognitionException, TokenStreamException
{
		
		traceIn("throw_stm");
		try { // debugging
			
			match(LITERAL_throw);
			expr();
			match(SEMI_COLON);
		}
		finally
		{ // debugging
			traceOut("throw_stm");
		}
	}
	
	public void try_stm() //throws RecognitionException, TokenStreamException
{
		
		traceIn("try_stm");
		try { // debugging
			
			match(LITERAL_try);
			block();
			{
				switch ( LA(1) )
				{
				case EOF:
				case LITERAL_function:
				case IDENTIFIER:
				case OPEN_PARENS:
				case OPEN_BRACE:
				case CLOSE_BRACE:
				case LITERAL_try:
				case LITERAL_catch:
				case LITERAL_throw:
				case SEMI_COLON:
				case LITERAL_switch:
				case LITERAL_default:
				case LITERAL_case:
				case LITERAL_with:
				case LITERAL_return:
				case LITERAL_break:
				case LITERAL_continue:
				case LITERAL_do:
				case LITERAL_while:
				case LITERAL_for:
				case LITERAL_var:
				case LITERAL_if:
				case LITERAL_else:
				case LITERAL_new:
				case OPEN_BRACKET:
				case LITERAL_delete:
				case LITERAL_void:
				case LITERAL_typeof:
				case INCREMENT:
				case DECREMENT:
				case PLUS:
				case MINUS:
				case BITWISE_NOT:
				case LOGICAL_NOT:
				case LITERAL_this:
				case LITERAL_null:
				case LITERAL_true:
				case LITERAL_false:
				case STRING_LITERAL:
				case DECIMAL_LITERAL:
				case HEX_INTEGER_LITERAL:
				{
					{
						switch ( LA(1) )
						{
						case LITERAL_catch:
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
								case OPEN_PARENS:
								case OPEN_BRACE:
								case CLOSE_BRACE:
								case LITERAL_try:
								case LITERAL_throw:
								case SEMI_COLON:
								case LITERAL_switch:
								case LITERAL_default:
								case LITERAL_case:
								case LITERAL_with:
								case LITERAL_return:
								case LITERAL_break:
								case LITERAL_continue:
								case LITERAL_do:
								case LITERAL_while:
								case LITERAL_for:
								case LITERAL_var:
								case LITERAL_if:
								case LITERAL_else:
								case LITERAL_new:
								case OPEN_BRACKET:
								case LITERAL_delete:
								case LITERAL_void:
								case LITERAL_typeof:
								case INCREMENT:
								case DECREMENT:
								case PLUS:
								case MINUS:
								case BITWISE_NOT:
								case LOGICAL_NOT:
								case LITERAL_this:
								case LITERAL_null:
								case LITERAL_true:
								case LITERAL_false:
								case STRING_LITERAL:
								case DECIMAL_LITERAL:
								case HEX_INTEGER_LITERAL:
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
						case EOF:
						case LITERAL_function:
						case IDENTIFIER:
						case OPEN_PARENS:
						case OPEN_BRACE:
						case CLOSE_BRACE:
						case LITERAL_try:
						case LITERAL_throw:
						case SEMI_COLON:
						case LITERAL_switch:
						case LITERAL_default:
						case LITERAL_case:
						case LITERAL_with:
						case LITERAL_return:
						case LITERAL_break:
						case LITERAL_continue:
						case LITERAL_do:
						case LITERAL_while:
						case LITERAL_for:
						case LITERAL_var:
						case LITERAL_if:
						case LITERAL_else:
						case LITERAL_new:
						case OPEN_BRACKET:
						case LITERAL_delete:
						case LITERAL_void:
						case LITERAL_typeof:
						case INCREMENT:
						case DECREMENT:
						case PLUS:
						case MINUS:
						case BITWISE_NOT:
						case LOGICAL_NOT:
						case LITERAL_this:
						case LITERAL_null:
						case LITERAL_true:
						case LITERAL_false:
						case STRING_LITERAL:
						case DECIMAL_LITERAL:
						case HEX_INTEGER_LITERAL:
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
		finally
		{ // debugging
			traceOut("try_stm");
		}
	}
	
	public void block() //throws RecognitionException, TokenStreamException
{
		
		traceIn("block");
		try { // debugging
			
			match(OPEN_BRACE);
			{    // ( ... )*
				for (;;)
				{
					if ((tokenSet_2_.member(LA(1))))
					{
						statement();
					}
					else
					{
						goto _loop16_breakloop;
					}
					
				}
_loop16_breakloop:				;
			}    // ( ... )*
			match(CLOSE_BRACE);
		}
		finally
		{ // debugging
			traceOut("block");
		}
	}
	
	public void catch_exp() //throws RecognitionException, TokenStreamException
{
		
		traceIn("catch_exp");
		try { // debugging
			
			match(LITERAL_catch);
			match(OPEN_PARENS);
			match(IDENTIFIER);
			match(CLOSE_PARENS);
			block();
		}
		finally
		{ // debugging
			traceOut("catch_exp");
		}
	}
	
	public void finally_exp() //throws RecognitionException, TokenStreamException
{
		
		traceIn("finally_exp");
		try { // debugging
			
			match(LITERAL_finally);
			block();
		}
		finally
		{ // debugging
			traceOut("finally_exp");
		}
	}
	
	public void expr() //throws RecognitionException, TokenStreamException
{
		
		traceIn("expr");
		try { // debugging
			
			assignment_expr();
			{    // ( ... )*
				for (;;)
				{
					if ((LA(1)==COMMA))
					{
						match(COMMA);
						assignment_expr();
					}
					else
					{
						goto _loop65_breakloop;
					}
					
				}
_loop65_breakloop:				;
			}    // ( ... )*
		}
		finally
		{ // debugging
			traceOut("expr");
		}
	}
	
	public void case_block() //throws RecognitionException, TokenStreamException
{
		
		traceIn("case_block");
		try { // debugging
			
			match(OPEN_BRACE);
			case_clauses();
			default_clause();
			case_clauses();
			match(CLOSE_BRACE);
		}
		finally
		{ // debugging
			traceOut("case_block");
		}
	}
	
	public void case_clauses() //throws RecognitionException, TokenStreamException
{
		
		traceIn("case_clauses");
		try { // debugging
			
			{    // ( ... )*
				for (;;)
				{
					if ((LA(1)==LITERAL_case))
					{
						case_clause();
					}
					else
					{
						goto _loop29_breakloop;
					}
					
				}
_loop29_breakloop:				;
			}    // ( ... )*
		}
		finally
		{ // debugging
			traceOut("case_clauses");
		}
	}
	
	public void default_clause() //throws RecognitionException, TokenStreamException
{
		
		traceIn("default_clause");
		try { // debugging
			
			match(LITERAL_default);
			match(COLON);
			statement_list();
		}
		finally
		{ // debugging
			traceOut("default_clause");
		}
	}
	
	public void statement_list() //throws RecognitionException, TokenStreamException
{
		
		traceIn("statement_list");
		try { // debugging
			
			{    // ( ... )*
				for (;;)
				{
					if ((tokenSet_2_.member(LA(1))))
					{
						statement();
					}
					else
					{
						goto _loop62_breakloop;
					}
					
				}
_loop62_breakloop:				;
			}    // ( ... )*
		}
		finally
		{ // debugging
			traceOut("statement_list");
		}
	}
	
	public void case_clause() //throws RecognitionException, TokenStreamException
{
		
		traceIn("case_clause");
		try { // debugging
			
			match(LITERAL_case);
			expr();
			match(COLON);
			statement_list();
		}
		finally
		{ // debugging
			traceOut("case_clause");
		}
	}
	
	public void inside_for() //throws RecognitionException, TokenStreamException
{
		
		traceIn("inside_for");
		try { // debugging
			
			switch ( LA(1) )
			{
			case IDENTIFIER:
			case OPEN_PARENS:
			case OPEN_BRACE:
			case SEMI_COLON:
			case LITERAL_new:
			case OPEN_BRACKET:
			case LITERAL_delete:
			case LITERAL_void:
			case LITERAL_typeof:
			case INCREMENT:
			case DECREMENT:
			case PLUS:
			case MINUS:
			case BITWISE_NOT:
			case LOGICAL_NOT:
			case LITERAL_this:
			case LITERAL_null:
			case LITERAL_true:
			case LITERAL_false:
			case STRING_LITERAL:
			case DECIMAL_LITERAL:
			case HEX_INTEGER_LITERAL:
			{
				{
					switch ( LA(1) )
					{
					case IDENTIFIER:
					case OPEN_PARENS:
					case OPEN_BRACE:
					case LITERAL_new:
					case OPEN_BRACKET:
					case LITERAL_delete:
					case LITERAL_void:
					case LITERAL_typeof:
					case INCREMENT:
					case DECREMENT:
					case PLUS:
					case MINUS:
					case BITWISE_NOT:
					case LOGICAL_NOT:
					case LITERAL_this:
					case LITERAL_null:
					case LITERAL_true:
					case LITERAL_false:
					case STRING_LITERAL:
					case DECIMAL_LITERAL:
					case HEX_INTEGER_LITERAL:
					{
						expr();
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
				{
					switch ( LA(1) )
					{
					case IDENTIFIER:
					case OPEN_PARENS:
					case OPEN_BRACE:
					case LITERAL_new:
					case OPEN_BRACKET:
					case LITERAL_delete:
					case LITERAL_void:
					case LITERAL_typeof:
					case INCREMENT:
					case DECREMENT:
					case PLUS:
					case MINUS:
					case BITWISE_NOT:
					case LOGICAL_NOT:
					case LITERAL_this:
					case LITERAL_null:
					case LITERAL_true:
					case LITERAL_false:
					case STRING_LITERAL:
					case DECIMAL_LITERAL:
					case HEX_INTEGER_LITERAL:
					{
						expr();
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
				{
					switch ( LA(1) )
					{
					case IDENTIFIER:
					case OPEN_PARENS:
					case OPEN_BRACE:
					case LITERAL_new:
					case OPEN_BRACKET:
					case LITERAL_delete:
					case LITERAL_void:
					case LITERAL_typeof:
					case INCREMENT:
					case DECREMENT:
					case PLUS:
					case MINUS:
					case BITWISE_NOT:
					case LOGICAL_NOT:
					case LITERAL_this:
					case LITERAL_null:
					case LITERAL_true:
					case LITERAL_false:
					case STRING_LITERAL:
					case DECIMAL_LITERAL:
					case HEX_INTEGER_LITERAL:
					{
						expr();
						break;
					}
					case CLOSE_PARENS:
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
			case LITERAL_var:
			{
				match(LITERAL_var);
				{
					var_decl_list();
					{
						switch ( LA(1) )
						{
						case SEMI_COLON:
						{
							match(SEMI_COLON);
							{
								switch ( LA(1) )
								{
								case IDENTIFIER:
								case OPEN_PARENS:
								case OPEN_BRACE:
								case LITERAL_new:
								case OPEN_BRACKET:
								case LITERAL_delete:
								case LITERAL_void:
								case LITERAL_typeof:
								case INCREMENT:
								case DECREMENT:
								case PLUS:
								case MINUS:
								case BITWISE_NOT:
								case LOGICAL_NOT:
								case LITERAL_this:
								case LITERAL_null:
								case LITERAL_true:
								case LITERAL_false:
								case STRING_LITERAL:
								case DECIMAL_LITERAL:
								case HEX_INTEGER_LITERAL:
								{
									expr();
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
							{
								switch ( LA(1) )
								{
								case IDENTIFIER:
								case OPEN_PARENS:
								case OPEN_BRACE:
								case LITERAL_new:
								case OPEN_BRACKET:
								case LITERAL_delete:
								case LITERAL_void:
								case LITERAL_typeof:
								case INCREMENT:
								case DECREMENT:
								case PLUS:
								case MINUS:
								case BITWISE_NOT:
								case LOGICAL_NOT:
								case LITERAL_this:
								case LITERAL_null:
								case LITERAL_true:
								case LITERAL_false:
								case STRING_LITERAL:
								case DECIMAL_LITERAL:
								case HEX_INTEGER_LITERAL:
								{
									expr();
									break;
								}
								case CLOSE_PARENS:
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
						case LITERAL_in:
						{
							match(LITERAL_in);
							expr();
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
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		finally
		{ // debugging
			traceOut("inside_for");
		}
	}
	
	public void var_decl_list() //throws RecognitionException, TokenStreamException
{
		
		traceIn("var_decl_list");
		try { // debugging
			
			var_decl();
			{    // ( ... )*
				for (;;)
				{
					if ((LA(1)==COMMA))
					{
						match(COMMA);
						var_decl();
					}
					else
					{
						goto _loop55_breakloop;
					}
					
				}
_loop55_breakloop:				;
			}    // ( ... )*
		}
		finally
		{ // debugging
			traceOut("var_decl_list");
		}
	}
	
	public void var_decl() //throws RecognitionException, TokenStreamException
{
		
		traceIn("var_decl");
		try { // debugging
			
			match(IDENTIFIER);
			{
				switch ( LA(1) )
				{
				case ASSIGN:
				{
					initializer();
					break;
				}
				case COMMA:
				case SEMI_COLON:
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
		finally
		{ // debugging
			traceOut("var_decl");
		}
	}
	
	public void initializer() //throws RecognitionException, TokenStreamException
{
		
		traceIn("initializer");
		try { // debugging
			
			match(ASSIGN);
			assignment_expr();
		}
		finally
		{ // debugging
			traceOut("initializer");
		}
	}
	
	public void assignment_expr() //throws RecognitionException, TokenStreamException
{
		
		traceIn("assignment_expr");
		try { // debugging
			
			{
				bool synPredMatched69 = false;
				if (((tokenSet_3_.member(LA(1)))))
				{
					int _m69 = mark();
					synPredMatched69 = true;
					inputState.guessing++;
					try {
						{
							left_hand_side_expr();
							assignment_op();
						}
					}
					catch (RecognitionException)
					{
						synPredMatched69 = false;
					}
					rewind(_m69);
					inputState.guessing--;
				}
				if ( synPredMatched69 )
				{
					left_hand_side_expr();
					assignment_op();
					assignment_expr();
				}
				else if ((tokenSet_4_.member(LA(1)))) {
					cond_expr();
				}
				else
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				
			}
		}
		finally
		{ // debugging
			traceOut("assignment_expr");
		}
	}
	
	public void left_hand_side_expr() //throws RecognitionException, TokenStreamException
{
		
		traceIn("left_hand_side_expr");
		try { // debugging
			
			call_expr();
		}
		finally
		{ // debugging
			traceOut("left_hand_side_expr");
		}
	}
	
	public void assignment_op() //throws RecognitionException, TokenStreamException
{
		
		traceIn("assignment_op");
		try { // debugging
			
			switch ( LA(1) )
			{
			case ASSIGN:
			{
				match(ASSIGN);
				break;
			}
			case MULT_ASSIGN:
			{
				match(MULT_ASSIGN);
				break;
			}
			case DIV_ASSIGN:
			{
				match(DIV_ASSIGN);
				break;
			}
			case MOD_ASSIGN:
			{
				match(MOD_ASSIGN);
				break;
			}
			case ADD_ASSIGN:
			{
				match(ADD_ASSIGN);
				break;
			}
			case SUB_ASSIGN:
			{
				match(SUB_ASSIGN);
				break;
			}
			case SHIFT_LEFT_ASSIGN:
			{
				match(SHIFT_LEFT_ASSIGN);
				break;
			}
			case SHIFT_RIGHT_ASSIGN:
			{
				match(SHIFT_RIGHT_ASSIGN);
				break;
			}
			case AND_ASSIGN:
			{
				match(AND_ASSIGN);
				break;
			}
			case XOR_ASSIGN:
			{
				match(XOR_ASSIGN);
				break;
			}
			case OR_ASSIGN:
			{
				match(OR_ASSIGN);
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		finally
		{ // debugging
			traceOut("assignment_op");
		}
	}
	
	public void cond_expr() //throws RecognitionException, TokenStreamException
{
		
		traceIn("cond_expr");
		try { // debugging
			
			logical_or_expr();
			{
				switch ( LA(1) )
				{
				case INTERR:
				{
					match(INTERR);
					assignment_expr();
					match(COLON);
					assignment_expr();
					break;
				}
				case EOF:
				case LITERAL_function:
				case IDENTIFIER:
				case OPEN_PARENS:
				case CLOSE_PARENS:
				case OPEN_BRACE:
				case CLOSE_BRACE:
				case COMMA:
				case LITERAL_try:
				case LITERAL_throw:
				case SEMI_COLON:
				case LITERAL_switch:
				case LITERAL_default:
				case COLON:
				case LITERAL_case:
				case LITERAL_with:
				case LITERAL_return:
				case LITERAL_break:
				case LITERAL_continue:
				case LITERAL_do:
				case LITERAL_while:
				case LITERAL_for:
				case LITERAL_var:
				case LITERAL_in:
				case LITERAL_if:
				case LITERAL_else:
				case LITERAL_new:
				case OPEN_BRACKET:
				case CLOSE_BRACKET:
				case 37:
				case 39:
				case LITERAL_delete:
				case LITERAL_void:
				case LITERAL_typeof:
				case INCREMENT:
				case DECREMENT:
				case PLUS:
				case MINUS:
				case BITWISE_NOT:
				case LOGICAL_NOT:
				case LITERAL_this:
				case LITERAL_null:
				case LITERAL_true:
				case LITERAL_false:
				case STRING_LITERAL:
				case DECIMAL_LITERAL:
				case HEX_INTEGER_LITERAL:
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
		finally
		{ // debugging
			traceOut("cond_expr");
		}
	}
	
	public void member_expr() //throws RecognitionException, TokenStreamException
{
		
		traceIn("member_expr");
		try { // debugging
			
			switch ( LA(1) )
			{
			case IDENTIFIER:
			case OPEN_PARENS:
			case OPEN_BRACE:
			case OPEN_BRACKET:
			case LITERAL_this:
			case LITERAL_null:
			case LITERAL_true:
			case LITERAL_false:
			case STRING_LITERAL:
			case DECIMAL_LITERAL:
			case HEX_INTEGER_LITERAL:
			{
				primary_expr();
				member_aux();
				break;
			}
			case LITERAL_new:
			{
				match(LITERAL_new);
				member_expr();
				match(OPEN_PARENS);
				{
					switch ( LA(1) )
					{
					case IDENTIFIER:
					case OPEN_PARENS:
					case OPEN_BRACE:
					case LITERAL_new:
					case OPEN_BRACKET:
					case LITERAL_delete:
					case LITERAL_void:
					case LITERAL_typeof:
					case INCREMENT:
					case DECREMENT:
					case PLUS:
					case MINUS:
					case BITWISE_NOT:
					case LOGICAL_NOT:
					case LITERAL_this:
					case LITERAL_null:
					case LITERAL_true:
					case LITERAL_false:
					case STRING_LITERAL:
					case DECIMAL_LITERAL:
					case HEX_INTEGER_LITERAL:
					{
						arguments_list();
						break;
					}
					case CLOSE_PARENS:
					{
						break;
					}
					default:
					{
						throw new NoViableAltException(LT(1), getFilename());
					}
					 }
				}
				match(CLOSE_PARENS);
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		finally
		{ // debugging
			traceOut("member_expr");
		}
	}
	
	public void primary_expr() //throws RecognitionException, TokenStreamException
{
		
		traceIn("primary_expr");
		try { // debugging
			
			switch ( LA(1) )
			{
			case LITERAL_this:
			{
				match(LITERAL_this);
				break;
			}
			case OPEN_BRACE:
			{
				object_literal();
				break;
			}
			case IDENTIFIER:
			{
				match(IDENTIFIER);
				break;
			}
			case LITERAL_null:
			case LITERAL_true:
			case LITERAL_false:
			case STRING_LITERAL:
			case DECIMAL_LITERAL:
			case HEX_INTEGER_LITERAL:
			{
				literal();
				break;
			}
			case OPEN_BRACKET:
			{
				array_literal();
				break;
			}
			case OPEN_PARENS:
			{
				match(OPEN_PARENS);
				expr();
				match(CLOSE_PARENS);
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		finally
		{ // debugging
			traceOut("primary_expr");
		}
	}
	
	public void member_aux() //throws RecognitionException, TokenStreamException
{
		
		traceIn("member_aux");
		try { // debugging
			
			{
				if ((LA(1)==33))
				{
					match(33);
					match(IDENTIFIER);
					member_aux();
				}
				else {
					bool synPredMatched75 = false;
					if (((LA(1)==OPEN_BRACKET)))
					{
						int _m75 = mark();
						synPredMatched75 = true;
						inputState.guessing++;
						try {
							{
								match(OPEN_BRACKET);
							}
						}
						catch (RecognitionException)
						{
							synPredMatched75 = false;
						}
						rewind(_m75);
						inputState.guessing--;
					}
					if ( synPredMatched75 )
					{
						match(OPEN_BRACKET);
						expr();
						match(CLOSE_BRACKET);
					}
					else if ((tokenSet_5_.member(LA(1)))) {
					}
					else
					{
						throw new NoViableAltException(LT(1), getFilename());
					}
					}
				}
			}
			finally
			{ // debugging
				traceOut("member_aux");
			}
		}
		
	public void arguments_list() //throws RecognitionException, TokenStreamException
{
		
		traceIn("arguments_list");
		try { // debugging
			
			assignment_expr();
			{    // ( ... )*
				for (;;)
				{
					if ((LA(1)==COMMA))
					{
						match(COMMA);
						assignment_expr();
					}
					else
					{
						goto _loop83_breakloop;
					}
					
				}
_loop83_breakloop:				;
			}    // ( ... )*
		}
		finally
		{ // debugging
			traceOut("arguments_list");
		}
	}
	
	public void call_expr() //throws RecognitionException, TokenStreamException
{
		
		traceIn("call_expr");
		try { // debugging
			
			member_expr();
			call_aux();
		}
		finally
		{ // debugging
			traceOut("call_expr");
		}
	}
	
	public void call_aux() //throws RecognitionException, TokenStreamException
{
		
		traceIn("call_aux");
		try { // debugging
			
			{
				switch ( LA(1) )
				{
				case 36:
				case 38:
				case DOT:
				{
					{
						switch ( LA(1) )
						{
						case 36:
						{
							match(36);
							{
								switch ( LA(1) )
								{
								case IDENTIFIER:
								case OPEN_PARENS:
								case OPEN_BRACE:
								case LITERAL_new:
								case OPEN_BRACKET:
								case LITERAL_delete:
								case LITERAL_void:
								case LITERAL_typeof:
								case INCREMENT:
								case DECREMENT:
								case PLUS:
								case MINUS:
								case BITWISE_NOT:
								case LOGICAL_NOT:
								case LITERAL_this:
								case LITERAL_null:
								case LITERAL_true:
								case LITERAL_false:
								case STRING_LITERAL:
								case DECIMAL_LITERAL:
								case HEX_INTEGER_LITERAL:
								{
									arguments_list();
									break;
								}
								case 37:
								{
									break;
								}
								default:
								{
									throw new NoViableAltException(LT(1), getFilename());
								}
								 }
							}
							match(37);
							break;
						}
						case 38:
						{
							match(38);
							expr();
							match(39);
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
							throw new NoViableAltException(LT(1), getFilename());
						}
						 }
					}
					call_aux();
					break;
				}
				case EOF:
				case LITERAL_function:
				case IDENTIFIER:
				case OPEN_PARENS:
				case CLOSE_PARENS:
				case OPEN_BRACE:
				case CLOSE_BRACE:
				case COMMA:
				case LITERAL_try:
				case LITERAL_throw:
				case SEMI_COLON:
				case LITERAL_switch:
				case LITERAL_default:
				case COLON:
				case LITERAL_case:
				case LITERAL_with:
				case LITERAL_return:
				case LITERAL_break:
				case LITERAL_continue:
				case LITERAL_do:
				case LITERAL_while:
				case LITERAL_for:
				case LITERAL_var:
				case LITERAL_in:
				case LITERAL_if:
				case LITERAL_else:
				case ASSIGN:
				case LITERAL_new:
				case OPEN_BRACKET:
				case CLOSE_BRACKET:
				case 37:
				case 39:
				case 41:
				case 42:
				case LITERAL_delete:
				case LITERAL_void:
				case LITERAL_typeof:
				case INCREMENT:
				case DECREMENT:
				case PLUS:
				case MINUS:
				case BITWISE_NOT:
				case LOGICAL_NOT:
				case MULT:
				case DIVISION:
				case MODULE:
				case SHIFT_LEFT:
				case SHIFT_RIGHT:
				case UNSIGNED_SHIFT_RIGHT:
				case LESS_THAN:
				case GREATER_THAN:
				case LESS_EQ:
				case GREATER_EQ:
				case LITERAL_instanceof:
				case EQ:
				case NEQ:
				case STRICT_EQ:
				case STRICT_NEQ:
				case BITWISE_AND:
				case BITWISE_XOR:
				case BITWISE_OR:
				case LOGICAL_AND:
				case LOGICAL_OR:
				case INTERR:
				case MULT_ASSIGN:
				case DIV_ASSIGN:
				case MOD_ASSIGN:
				case ADD_ASSIGN:
				case SUB_ASSIGN:
				case SHIFT_LEFT_ASSIGN:
				case SHIFT_RIGHT_ASSIGN:
				case AND_ASSIGN:
				case XOR_ASSIGN:
				case OR_ASSIGN:
				case LITERAL_this:
				case LITERAL_null:
				case LITERAL_true:
				case LITERAL_false:
				case STRING_LITERAL:
				case DECIMAL_LITERAL:
				case HEX_INTEGER_LITERAL:
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
		finally
		{ // debugging
			traceOut("call_aux");
		}
	}
	
	public void postfix_expr() //throws RecognitionException, TokenStreamException
{
		
		traceIn("postfix_expr");
		try { // debugging
			
			left_hand_side_expr();
			{
				switch ( LA(1) )
				{
				case 41:
				{
					match(41);
					break;
				}
				case 42:
				{
					match(42);
					break;
				}
				case EOF:
				case LITERAL_function:
				case IDENTIFIER:
				case OPEN_PARENS:
				case CLOSE_PARENS:
				case OPEN_BRACE:
				case CLOSE_BRACE:
				case COMMA:
				case LITERAL_try:
				case LITERAL_throw:
				case SEMI_COLON:
				case LITERAL_switch:
				case LITERAL_default:
				case COLON:
				case LITERAL_case:
				case LITERAL_with:
				case LITERAL_return:
				case LITERAL_break:
				case LITERAL_continue:
				case LITERAL_do:
				case LITERAL_while:
				case LITERAL_for:
				case LITERAL_var:
				case LITERAL_in:
				case LITERAL_if:
				case LITERAL_else:
				case LITERAL_new:
				case OPEN_BRACKET:
				case CLOSE_BRACKET:
				case 37:
				case 39:
				case LITERAL_delete:
				case LITERAL_void:
				case LITERAL_typeof:
				case INCREMENT:
				case DECREMENT:
				case PLUS:
				case MINUS:
				case BITWISE_NOT:
				case LOGICAL_NOT:
				case MULT:
				case DIVISION:
				case MODULE:
				case SHIFT_LEFT:
				case SHIFT_RIGHT:
				case UNSIGNED_SHIFT_RIGHT:
				case LESS_THAN:
				case GREATER_THAN:
				case LESS_EQ:
				case GREATER_EQ:
				case LITERAL_instanceof:
				case EQ:
				case NEQ:
				case STRICT_EQ:
				case STRICT_NEQ:
				case BITWISE_AND:
				case BITWISE_XOR:
				case BITWISE_OR:
				case LOGICAL_AND:
				case LOGICAL_OR:
				case INTERR:
				case LITERAL_this:
				case LITERAL_null:
				case LITERAL_true:
				case LITERAL_false:
				case STRING_LITERAL:
				case DECIMAL_LITERAL:
				case HEX_INTEGER_LITERAL:
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
		finally
		{ // debugging
			traceOut("postfix_expr");
		}
	}
	
	public void unary_expr() //throws RecognitionException, TokenStreamException
{
		
		traceIn("unary_expr");
		try { // debugging
			
			switch ( LA(1) )
			{
			case IDENTIFIER:
			case OPEN_PARENS:
			case OPEN_BRACE:
			case LITERAL_new:
			case OPEN_BRACKET:
			case LITERAL_this:
			case LITERAL_null:
			case LITERAL_true:
			case LITERAL_false:
			case STRING_LITERAL:
			case DECIMAL_LITERAL:
			case HEX_INTEGER_LITERAL:
			{
				postfix_expr();
				break;
			}
			case LITERAL_delete:
			case LITERAL_void:
			case LITERAL_typeof:
			case INCREMENT:
			case DECREMENT:
			case PLUS:
			case MINUS:
			case BITWISE_NOT:
			case LOGICAL_NOT:
			{
				unary_op();
				unary_expr();
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		finally
		{ // debugging
			traceOut("unary_expr");
		}
	}
	
	public void unary_op() //throws RecognitionException, TokenStreamException
{
		
		traceIn("unary_op");
		try { // debugging
			
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
		finally
		{ // debugging
			traceOut("unary_op");
		}
	}
	
	public void multiplicative_expr() //throws RecognitionException, TokenStreamException
{
		
		traceIn("multiplicative_expr");
		try { // debugging
			
			unary_expr();
			multiplicative_aux();
		}
		finally
		{ // debugging
			traceOut("multiplicative_expr");
		}
	}
	
	public void multiplicative_aux() //throws RecognitionException, TokenStreamException
{
		
		traceIn("multiplicative_aux");
		try { // debugging
			
			{
				switch ( LA(1) )
				{
				case MULT:
				case DIVISION:
				case MODULE:
				{
					{
						switch ( LA(1) )
						{
						case MULT:
						{
							match(MULT);
							break;
						}
						case DIVISION:
						{
							match(DIVISION);
							break;
						}
						case MODULE:
						{
							match(MODULE);
							break;
						}
						default:
						{
							throw new NoViableAltException(LT(1), getFilename());
						}
						 }
					}
					unary_expr();
					multiplicative_aux();
					break;
				}
				case EOF:
				case LITERAL_function:
				case IDENTIFIER:
				case OPEN_PARENS:
				case CLOSE_PARENS:
				case OPEN_BRACE:
				case CLOSE_BRACE:
				case COMMA:
				case LITERAL_try:
				case LITERAL_throw:
				case SEMI_COLON:
				case LITERAL_switch:
				case LITERAL_default:
				case COLON:
				case LITERAL_case:
				case LITERAL_with:
				case LITERAL_return:
				case LITERAL_break:
				case LITERAL_continue:
				case LITERAL_do:
				case LITERAL_while:
				case LITERAL_for:
				case LITERAL_var:
				case LITERAL_in:
				case LITERAL_if:
				case LITERAL_else:
				case LITERAL_new:
				case OPEN_BRACKET:
				case CLOSE_BRACKET:
				case 37:
				case 39:
				case LITERAL_delete:
				case LITERAL_void:
				case LITERAL_typeof:
				case INCREMENT:
				case DECREMENT:
				case PLUS:
				case MINUS:
				case BITWISE_NOT:
				case LOGICAL_NOT:
				case SHIFT_LEFT:
				case SHIFT_RIGHT:
				case UNSIGNED_SHIFT_RIGHT:
				case LESS_THAN:
				case GREATER_THAN:
				case LESS_EQ:
				case GREATER_EQ:
				case LITERAL_instanceof:
				case EQ:
				case NEQ:
				case STRICT_EQ:
				case STRICT_NEQ:
				case BITWISE_AND:
				case BITWISE_XOR:
				case BITWISE_OR:
				case LOGICAL_AND:
				case LOGICAL_OR:
				case INTERR:
				case LITERAL_this:
				case LITERAL_null:
				case LITERAL_true:
				case LITERAL_false:
				case STRING_LITERAL:
				case DECIMAL_LITERAL:
				case HEX_INTEGER_LITERAL:
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
		finally
		{ // debugging
			traceOut("multiplicative_aux");
		}
	}
	
	public void additive_expr() //throws RecognitionException, TokenStreamException
{
		
		traceIn("additive_expr");
		try { // debugging
			
			multiplicative_expr();
			additive_aux();
		}
		finally
		{ // debugging
			traceOut("additive_expr");
		}
	}
	
	public void additive_aux() //throws RecognitionException, TokenStreamException
{
		
		traceIn("additive_aux");
		try { // debugging
			
			{
				if ((LA(1)==PLUS||LA(1)==MINUS))
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
					multiplicative_expr();
					additive_aux();
				}
				else if ((tokenSet_6_.member(LA(1)))) {
				}
				else
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				
			}
		}
		finally
		{ // debugging
			traceOut("additive_aux");
		}
	}
	
	public void shift_expr() //throws RecognitionException, TokenStreamException
{
		
		traceIn("shift_expr");
		try { // debugging
			
			additive_expr();
			shift_aux();
		}
		finally
		{ // debugging
			traceOut("shift_expr");
		}
	}
	
	public void shift_aux() //throws RecognitionException, TokenStreamException
{
		
		traceIn("shift_aux");
		try { // debugging
			
			{
				switch ( LA(1) )
				{
				case SHIFT_LEFT:
				case SHIFT_RIGHT:
				case UNSIGNED_SHIFT_RIGHT:
				{
					shift_op();
					additive_expr();
					shift_aux();
					break;
				}
				case EOF:
				case LITERAL_function:
				case IDENTIFIER:
				case OPEN_PARENS:
				case CLOSE_PARENS:
				case OPEN_BRACE:
				case CLOSE_BRACE:
				case COMMA:
				case LITERAL_try:
				case LITERAL_throw:
				case SEMI_COLON:
				case LITERAL_switch:
				case LITERAL_default:
				case COLON:
				case LITERAL_case:
				case LITERAL_with:
				case LITERAL_return:
				case LITERAL_break:
				case LITERAL_continue:
				case LITERAL_do:
				case LITERAL_while:
				case LITERAL_for:
				case LITERAL_var:
				case LITERAL_in:
				case LITERAL_if:
				case LITERAL_else:
				case LITERAL_new:
				case OPEN_BRACKET:
				case CLOSE_BRACKET:
				case 37:
				case 39:
				case LITERAL_delete:
				case LITERAL_void:
				case LITERAL_typeof:
				case INCREMENT:
				case DECREMENT:
				case PLUS:
				case MINUS:
				case BITWISE_NOT:
				case LOGICAL_NOT:
				case LESS_THAN:
				case GREATER_THAN:
				case LESS_EQ:
				case GREATER_EQ:
				case LITERAL_instanceof:
				case EQ:
				case NEQ:
				case STRICT_EQ:
				case STRICT_NEQ:
				case BITWISE_AND:
				case BITWISE_XOR:
				case BITWISE_OR:
				case LOGICAL_AND:
				case LOGICAL_OR:
				case INTERR:
				case LITERAL_this:
				case LITERAL_null:
				case LITERAL_true:
				case LITERAL_false:
				case STRING_LITERAL:
				case DECIMAL_LITERAL:
				case HEX_INTEGER_LITERAL:
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
		finally
		{ // debugging
			traceOut("shift_aux");
		}
	}
	
	public void shift_op() //throws RecognitionException, TokenStreamException
{
		
		traceIn("shift_op");
		try { // debugging
			
			switch ( LA(1) )
			{
			case SHIFT_LEFT:
			{
				match(SHIFT_LEFT);
				break;
			}
			case SHIFT_RIGHT:
			{
				match(SHIFT_RIGHT);
				break;
			}
			case UNSIGNED_SHIFT_RIGHT:
			{
				match(UNSIGNED_SHIFT_RIGHT);
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		finally
		{ // debugging
			traceOut("shift_op");
		}
	}
	
	public void relational_expr() //throws RecognitionException, TokenStreamException
{
		
		traceIn("relational_expr");
		try { // debugging
			
			shift_expr();
			relational_aux();
		}
		finally
		{ // debugging
			traceOut("relational_expr");
		}
	}
	
	public void relational_aux() //throws RecognitionException, TokenStreamException
{
		
		traceIn("relational_aux");
		try { // debugging
			
			{
				switch ( LA(1) )
				{
				case LESS_THAN:
				case GREATER_THAN:
				case LESS_EQ:
				case GREATER_EQ:
				case LITERAL_instanceof:
				{
					relational_op();
					shift_expr();
					relational_aux();
					break;
				}
				case EOF:
				case LITERAL_function:
				case IDENTIFIER:
				case OPEN_PARENS:
				case CLOSE_PARENS:
				case OPEN_BRACE:
				case CLOSE_BRACE:
				case COMMA:
				case LITERAL_try:
				case LITERAL_throw:
				case SEMI_COLON:
				case LITERAL_switch:
				case LITERAL_default:
				case COLON:
				case LITERAL_case:
				case LITERAL_with:
				case LITERAL_return:
				case LITERAL_break:
				case LITERAL_continue:
				case LITERAL_do:
				case LITERAL_while:
				case LITERAL_for:
				case LITERAL_var:
				case LITERAL_in:
				case LITERAL_if:
				case LITERAL_else:
				case LITERAL_new:
				case OPEN_BRACKET:
				case CLOSE_BRACKET:
				case 37:
				case 39:
				case LITERAL_delete:
				case LITERAL_void:
				case LITERAL_typeof:
				case INCREMENT:
				case DECREMENT:
				case PLUS:
				case MINUS:
				case BITWISE_NOT:
				case LOGICAL_NOT:
				case EQ:
				case NEQ:
				case STRICT_EQ:
				case STRICT_NEQ:
				case BITWISE_AND:
				case BITWISE_XOR:
				case BITWISE_OR:
				case LOGICAL_AND:
				case LOGICAL_OR:
				case INTERR:
				case LITERAL_this:
				case LITERAL_null:
				case LITERAL_true:
				case LITERAL_false:
				case STRING_LITERAL:
				case DECIMAL_LITERAL:
				case HEX_INTEGER_LITERAL:
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
		finally
		{ // debugging
			traceOut("relational_aux");
		}
	}
	
	public void relational_op() //throws RecognitionException, TokenStreamException
{
		
		traceIn("relational_op");
		try { // debugging
			
			switch ( LA(1) )
			{
			case LESS_THAN:
			{
				match(LESS_THAN);
				break;
			}
			case GREATER_THAN:
			{
				match(GREATER_THAN);
				break;
			}
			case LESS_EQ:
			{
				match(LESS_EQ);
				break;
			}
			case GREATER_EQ:
			{
				match(GREATER_EQ);
				break;
			}
			case LITERAL_instanceof:
			{
				match(LITERAL_instanceof);
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		finally
		{ // debugging
			traceOut("relational_op");
		}
	}
	
	public void equality_expr() //throws RecognitionException, TokenStreamException
{
		
		traceIn("equality_expr");
		try { // debugging
			
			relational_expr();
			equality_aux();
		}
		finally
		{ // debugging
			traceOut("equality_expr");
		}
	}
	
	public void equality_aux() //throws RecognitionException, TokenStreamException
{
		
		traceIn("equality_aux");
		try { // debugging
			
			{
				switch ( LA(1) )
				{
				case EQ:
				case NEQ:
				case STRICT_EQ:
				case STRICT_NEQ:
				{
					equality_op();
					relational_expr();
					equality_aux();
					break;
				}
				case EOF:
				case LITERAL_function:
				case IDENTIFIER:
				case OPEN_PARENS:
				case CLOSE_PARENS:
				case OPEN_BRACE:
				case CLOSE_BRACE:
				case COMMA:
				case LITERAL_try:
				case LITERAL_throw:
				case SEMI_COLON:
				case LITERAL_switch:
				case LITERAL_default:
				case COLON:
				case LITERAL_case:
				case LITERAL_with:
				case LITERAL_return:
				case LITERAL_break:
				case LITERAL_continue:
				case LITERAL_do:
				case LITERAL_while:
				case LITERAL_for:
				case LITERAL_var:
				case LITERAL_in:
				case LITERAL_if:
				case LITERAL_else:
				case LITERAL_new:
				case OPEN_BRACKET:
				case CLOSE_BRACKET:
				case 37:
				case 39:
				case LITERAL_delete:
				case LITERAL_void:
				case LITERAL_typeof:
				case INCREMENT:
				case DECREMENT:
				case PLUS:
				case MINUS:
				case BITWISE_NOT:
				case LOGICAL_NOT:
				case BITWISE_AND:
				case BITWISE_XOR:
				case BITWISE_OR:
				case LOGICAL_AND:
				case LOGICAL_OR:
				case INTERR:
				case LITERAL_this:
				case LITERAL_null:
				case LITERAL_true:
				case LITERAL_false:
				case STRING_LITERAL:
				case DECIMAL_LITERAL:
				case HEX_INTEGER_LITERAL:
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
		finally
		{ // debugging
			traceOut("equality_aux");
		}
	}
	
	public void equality_op() //throws RecognitionException, TokenStreamException
{
		
		traceIn("equality_op");
		try { // debugging
			
			switch ( LA(1) )
			{
			case EQ:
			{
				match(EQ);
				break;
			}
			case NEQ:
			{
				match(NEQ);
				break;
			}
			case STRICT_EQ:
			{
				match(STRICT_EQ);
				break;
			}
			case STRICT_NEQ:
			{
				match(STRICT_NEQ);
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		finally
		{ // debugging
			traceOut("equality_op");
		}
	}
	
	public void bitwise_and_expr() //throws RecognitionException, TokenStreamException
{
		
		traceIn("bitwise_and_expr");
		try { // debugging
			
			equality_expr();
			bitwise_and_aux();
		}
		finally
		{ // debugging
			traceOut("bitwise_and_expr");
		}
	}
	
	public void bitwise_and_aux() //throws RecognitionException, TokenStreamException
{
		
		traceIn("bitwise_and_aux");
		try { // debugging
			
			{
				switch ( LA(1) )
				{
				case BITWISE_AND:
				{
					match(BITWISE_AND);
					equality_expr();
					bitwise_and_aux();
					break;
				}
				case EOF:
				case LITERAL_function:
				case IDENTIFIER:
				case OPEN_PARENS:
				case CLOSE_PARENS:
				case OPEN_BRACE:
				case CLOSE_BRACE:
				case COMMA:
				case LITERAL_try:
				case LITERAL_throw:
				case SEMI_COLON:
				case LITERAL_switch:
				case LITERAL_default:
				case COLON:
				case LITERAL_case:
				case LITERAL_with:
				case LITERAL_return:
				case LITERAL_break:
				case LITERAL_continue:
				case LITERAL_do:
				case LITERAL_while:
				case LITERAL_for:
				case LITERAL_var:
				case LITERAL_in:
				case LITERAL_if:
				case LITERAL_else:
				case LITERAL_new:
				case OPEN_BRACKET:
				case CLOSE_BRACKET:
				case 37:
				case 39:
				case LITERAL_delete:
				case LITERAL_void:
				case LITERAL_typeof:
				case INCREMENT:
				case DECREMENT:
				case PLUS:
				case MINUS:
				case BITWISE_NOT:
				case LOGICAL_NOT:
				case BITWISE_XOR:
				case BITWISE_OR:
				case LOGICAL_AND:
				case LOGICAL_OR:
				case INTERR:
				case LITERAL_this:
				case LITERAL_null:
				case LITERAL_true:
				case LITERAL_false:
				case STRING_LITERAL:
				case DECIMAL_LITERAL:
				case HEX_INTEGER_LITERAL:
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
		finally
		{ // debugging
			traceOut("bitwise_and_aux");
		}
	}
	
	public void bitwise_xor_expr() //throws RecognitionException, TokenStreamException
{
		
		traceIn("bitwise_xor_expr");
		try { // debugging
			
			bitwise_and_expr();
			bitwise_xor_aux();
		}
		finally
		{ // debugging
			traceOut("bitwise_xor_expr");
		}
	}
	
	public void bitwise_xor_aux() //throws RecognitionException, TokenStreamException
{
		
		traceIn("bitwise_xor_aux");
		try { // debugging
			
			{
				switch ( LA(1) )
				{
				case BITWISE_XOR:
				{
					match(BITWISE_XOR);
					bitwise_and_expr();
					bitwise_xor_aux();
					break;
				}
				case EOF:
				case LITERAL_function:
				case IDENTIFIER:
				case OPEN_PARENS:
				case CLOSE_PARENS:
				case OPEN_BRACE:
				case CLOSE_BRACE:
				case COMMA:
				case LITERAL_try:
				case LITERAL_throw:
				case SEMI_COLON:
				case LITERAL_switch:
				case LITERAL_default:
				case COLON:
				case LITERAL_case:
				case LITERAL_with:
				case LITERAL_return:
				case LITERAL_break:
				case LITERAL_continue:
				case LITERAL_do:
				case LITERAL_while:
				case LITERAL_for:
				case LITERAL_var:
				case LITERAL_in:
				case LITERAL_if:
				case LITERAL_else:
				case LITERAL_new:
				case OPEN_BRACKET:
				case CLOSE_BRACKET:
				case 37:
				case 39:
				case LITERAL_delete:
				case LITERAL_void:
				case LITERAL_typeof:
				case INCREMENT:
				case DECREMENT:
				case PLUS:
				case MINUS:
				case BITWISE_NOT:
				case LOGICAL_NOT:
				case BITWISE_OR:
				case LOGICAL_AND:
				case LOGICAL_OR:
				case INTERR:
				case LITERAL_this:
				case LITERAL_null:
				case LITERAL_true:
				case LITERAL_false:
				case STRING_LITERAL:
				case DECIMAL_LITERAL:
				case HEX_INTEGER_LITERAL:
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
		finally
		{ // debugging
			traceOut("bitwise_xor_aux");
		}
	}
	
	public void bitwise_or_expr() //throws RecognitionException, TokenStreamException
{
		
		traceIn("bitwise_or_expr");
		try { // debugging
			
			bitwise_xor_expr();
			bitwise_or_aux();
		}
		finally
		{ // debugging
			traceOut("bitwise_or_expr");
		}
	}
	
	public void bitwise_or_aux() //throws RecognitionException, TokenStreamException
{
		
		traceIn("bitwise_or_aux");
		try { // debugging
			
			{
				switch ( LA(1) )
				{
				case BITWISE_OR:
				{
					match(BITWISE_OR);
					bitwise_xor_expr();
					bitwise_or_aux();
					break;
				}
				case EOF:
				case LITERAL_function:
				case IDENTIFIER:
				case OPEN_PARENS:
				case CLOSE_PARENS:
				case OPEN_BRACE:
				case CLOSE_BRACE:
				case COMMA:
				case LITERAL_try:
				case LITERAL_throw:
				case SEMI_COLON:
				case LITERAL_switch:
				case LITERAL_default:
				case COLON:
				case LITERAL_case:
				case LITERAL_with:
				case LITERAL_return:
				case LITERAL_break:
				case LITERAL_continue:
				case LITERAL_do:
				case LITERAL_while:
				case LITERAL_for:
				case LITERAL_var:
				case LITERAL_in:
				case LITERAL_if:
				case LITERAL_else:
				case LITERAL_new:
				case OPEN_BRACKET:
				case CLOSE_BRACKET:
				case 37:
				case 39:
				case LITERAL_delete:
				case LITERAL_void:
				case LITERAL_typeof:
				case INCREMENT:
				case DECREMENT:
				case PLUS:
				case MINUS:
				case BITWISE_NOT:
				case LOGICAL_NOT:
				case LOGICAL_AND:
				case LOGICAL_OR:
				case INTERR:
				case LITERAL_this:
				case LITERAL_null:
				case LITERAL_true:
				case LITERAL_false:
				case STRING_LITERAL:
				case DECIMAL_LITERAL:
				case HEX_INTEGER_LITERAL:
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
		finally
		{ // debugging
			traceOut("bitwise_or_aux");
		}
	}
	
	public void logical_and_expr() //throws RecognitionException, TokenStreamException
{
		
		traceIn("logical_and_expr");
		try { // debugging
			
			bitwise_or_expr();
			logical_and_aux();
		}
		finally
		{ // debugging
			traceOut("logical_and_expr");
		}
	}
	
	public void logical_and_aux() //throws RecognitionException, TokenStreamException
{
		
		traceIn("logical_and_aux");
		try { // debugging
			
			{
				switch ( LA(1) )
				{
				case LOGICAL_AND:
				{
					match(LOGICAL_AND);
					bitwise_or_expr();
					logical_and_aux();
					break;
				}
				case EOF:
				case LITERAL_function:
				case IDENTIFIER:
				case OPEN_PARENS:
				case CLOSE_PARENS:
				case OPEN_BRACE:
				case CLOSE_BRACE:
				case COMMA:
				case LITERAL_try:
				case LITERAL_throw:
				case SEMI_COLON:
				case LITERAL_switch:
				case LITERAL_default:
				case COLON:
				case LITERAL_case:
				case LITERAL_with:
				case LITERAL_return:
				case LITERAL_break:
				case LITERAL_continue:
				case LITERAL_do:
				case LITERAL_while:
				case LITERAL_for:
				case LITERAL_var:
				case LITERAL_in:
				case LITERAL_if:
				case LITERAL_else:
				case LITERAL_new:
				case OPEN_BRACKET:
				case CLOSE_BRACKET:
				case 37:
				case 39:
				case LITERAL_delete:
				case LITERAL_void:
				case LITERAL_typeof:
				case INCREMENT:
				case DECREMENT:
				case PLUS:
				case MINUS:
				case BITWISE_NOT:
				case LOGICAL_NOT:
				case LOGICAL_OR:
				case INTERR:
				case LITERAL_this:
				case LITERAL_null:
				case LITERAL_true:
				case LITERAL_false:
				case STRING_LITERAL:
				case DECIMAL_LITERAL:
				case HEX_INTEGER_LITERAL:
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
		finally
		{ // debugging
			traceOut("logical_and_aux");
		}
	}
	
	public void logical_or_expr() //throws RecognitionException, TokenStreamException
{
		
		traceIn("logical_or_expr");
		try { // debugging
			
			logical_and_expr();
			logical_or_aux();
		}
		finally
		{ // debugging
			traceOut("logical_or_expr");
		}
	}
	
	public void logical_or_aux() //throws RecognitionException, TokenStreamException
{
		
		traceIn("logical_or_aux");
		try { // debugging
			
			{
				switch ( LA(1) )
				{
				case LOGICAL_OR:
				{
					match(LOGICAL_OR);
					logical_and_expr();
					logical_or_aux();
					break;
				}
				case EOF:
				case LITERAL_function:
				case IDENTIFIER:
				case OPEN_PARENS:
				case CLOSE_PARENS:
				case OPEN_BRACE:
				case CLOSE_BRACE:
				case COMMA:
				case LITERAL_try:
				case LITERAL_throw:
				case SEMI_COLON:
				case LITERAL_switch:
				case LITERAL_default:
				case COLON:
				case LITERAL_case:
				case LITERAL_with:
				case LITERAL_return:
				case LITERAL_break:
				case LITERAL_continue:
				case LITERAL_do:
				case LITERAL_while:
				case LITERAL_for:
				case LITERAL_var:
				case LITERAL_in:
				case LITERAL_if:
				case LITERAL_else:
				case LITERAL_new:
				case OPEN_BRACKET:
				case CLOSE_BRACKET:
				case 37:
				case 39:
				case LITERAL_delete:
				case LITERAL_void:
				case LITERAL_typeof:
				case INCREMENT:
				case DECREMENT:
				case PLUS:
				case MINUS:
				case BITWISE_NOT:
				case LOGICAL_NOT:
				case INTERR:
				case LITERAL_this:
				case LITERAL_null:
				case LITERAL_true:
				case LITERAL_false:
				case STRING_LITERAL:
				case DECIMAL_LITERAL:
				case HEX_INTEGER_LITERAL:
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
		finally
		{ // debugging
			traceOut("logical_or_aux");
		}
	}
	
	public void object_literal() //throws RecognitionException, TokenStreamException
{
		
		traceIn("object_literal");
		try { // debugging
			
			match(OPEN_BRACE);
			{
				bool synPredMatched131 = false;
				if (((LA(1)==OPEN_BRACE)))
				{
					int _m131 = mark();
					synPredMatched131 = true;
					inputState.guessing++;
					try {
						{
							property_name();
							match(COLON);
						}
					}
					catch (RecognitionException)
					{
						synPredMatched131 = false;
					}
					rewind(_m131);
					inputState.guessing--;
				}
				if ( synPredMatched131 )
				{
					match(OPEN_BRACE);
					{ // ( ... )+
					int _cnt133=0;
					for (;;)
					{
						if ((tokenSet_7_.member(LA(1))))
						{
							property_name();
							match(COLON);
							assignment_expr();
						}
						else
						{
							if (_cnt133 >= 1) { goto _loop133_breakloop; } else { throw new NoViableAltException(LT(1), getFilename());; }
						}
						
						_cnt133++;
					}
_loop133_breakloop:					;
					}    // ( ... )+
				}
				else if ((tokenSet_8_.member(LA(1)))) {
					{    // ( ... )*
						for (;;)
						{
							if ((tokenSet_2_.member(LA(1))))
							{
								statement();
							}
							else
							{
								goto _loop135_breakloop;
							}
							
						}
_loop135_breakloop:						;
					}    // ( ... )*
				}
				else
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				
			}
			match(CLOSE_BRACE);
		}
		finally
		{ // debugging
			traceOut("object_literal");
		}
	}
	
	public void literal() //throws RecognitionException, TokenStreamException
{
		
		traceIn("literal");
		try { // debugging
			
			switch ( LA(1) )
			{
			case LITERAL_null:
			{
				match(LITERAL_null);
				break;
			}
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
			case STRING_LITERAL:
			{
				match(STRING_LITERAL);
				break;
			}
			case DECIMAL_LITERAL:
			case HEX_INTEGER_LITERAL:
			{
				numeric_literal();
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		finally
		{ // debugging
			traceOut("literal");
		}
	}
	
	public void array_literal() //throws RecognitionException, TokenStreamException
{
		
		traceIn("array_literal");
		try { // debugging
			
			match(OPEN_BRACKET);
			{
				switch ( LA(1) )
				{
				case IDENTIFIER:
				case OPEN_PARENS:
				case OPEN_BRACE:
				case OPEN_BRACKET:
				case LITERAL_this:
				case LITERAL_null:
				case LITERAL_true:
				case LITERAL_false:
				case STRING_LITERAL:
				case DECIMAL_LITERAL:
				case HEX_INTEGER_LITERAL:
				{
					primary_expr();
					{    // ( ... )*
						for (;;)
						{
							if ((LA(1)==COMMA))
							{
								match(COMMA);
								primary_expr();
							}
							else
							{
								goto _loop145_breakloop;
							}
							
						}
_loop145_breakloop:						;
					}    // ( ... )*
					break;
				}
				case CLOSE_BRACKET:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
			match(CLOSE_BRACKET);
		}
		finally
		{ // debugging
			traceOut("array_literal");
		}
	}
	
	public void property_name() //throws RecognitionException, TokenStreamException
{
		
		traceIn("property_name");
		try { // debugging
			
			{
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
				case DECIMAL_LITERAL:
				case HEX_INTEGER_LITERAL:
				{
					numeric_literal();
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
		}
		finally
		{ // debugging
			traceOut("property_name");
		}
	}
	
	public void numeric_literal() //throws RecognitionException, TokenStreamException
{
		
		traceIn("numeric_literal");
		try { // debugging
			
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
		finally
		{ // debugging
			traceOut("numeric_literal");
		}
	}
	
	public void property_name_and_value_list() //throws RecognitionException, TokenStreamException
{
		
		traceIn("property_name_and_value_list");
		try { // debugging
			
			{ // ( ... )+
			int _cnt139=0;
			for (;;)
			{
				if ((tokenSet_7_.member(LA(1))))
				{
					property_name();
					match(COLON);
					primary_expr();
				}
				else
				{
					if (_cnt139 >= 1) { goto _loop139_breakloop; } else { throw new NoViableAltException(LT(1), getFilename());; }
				}
				
				_cnt139++;
			}
_loop139_breakloop:			;
			}    // ( ... )+
		}
		finally
		{ // debugging
			traceOut("property_name_and_value_list");
		}
	}
	
	public void line_terminator() //throws RecognitionException, TokenStreamException
{
		
		traceIn("line_terminator");
		try { // debugging
			
			switch ( LA(1) )
			{
			case LINE_FEED:
			{
				match(LINE_FEED);
				break;
			}
			case CARRIAGE_RETURN:
			{
				match(CARRIAGE_RETURN);
				break;
			}
			case LINE_SEPARATOR:
			{
				match(LINE_SEPARATOR);
				break;
			}
			case PARAGRAPH_SEPARATOR:
			{
				match(PARAGRAPH_SEPARATOR);
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		finally
		{ // debugging
			traceOut("line_terminator");
		}
	}
	
	public void white_space() //throws RecognitionException, TokenStreamException
{
		
		traceIn("white_space");
		try { // debugging
			
			switch ( LA(1) )
			{
			case TAB:
			{
				match(TAB);
				break;
			}
			case VERTICAL_TAB:
			{
				match(VERTICAL_TAB);
				break;
			}
			case FORM_FEED:
			{
				match(FORM_FEED);
				break;
			}
			case SPACE:
			{
				match(SPACE);
				break;
			}
			case NO_BREAK_SPACE:
			{
				match(NO_BREAK_SPACE);
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		finally
		{ // debugging
			traceOut("white_space");
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
		@"""OPEN_PARENS""",
		@"""CLOSE_PARENS""",
		@"""OPEN_BRACE""",
		@"""CLOSE_BRACE""",
		@"""COMMA""",
		@"""try""",
		@"""catch""",
		@"""finally""",
		@"""throw""",
		@"""SEMI_COLON""",
		@"""switch""",
		@"""default""",
		@"""COLON""",
		@"""case""",
		@"""with""",
		@"""return""",
		@"""break""",
		@"""continue""",
		@"""do""",
		@"""while""",
		@"""for""",
		@"""var""",
		@"""in""",
		@"""if""",
		@"""else""",
		@"""ASSIGN""",
		@"""new""",
		@""".""",
		@"""OPEN_BRACKET""",
		@"""CLOSE_BRACKET""",
		@"""(""",
		@""")""",
		@"""[""",
		@"""]""",
		@"""DOT""",
		@"""++""",
		@"""--""",
		@"""delete""",
		@"""void""",
		@"""typeof""",
		@"""INCREMENT""",
		@"""DECREMENT""",
		@"""PLUS""",
		@"""MINUS""",
		@"""BITWISE_NOT""",
		@"""LOGICAL_NOT""",
		@"""MULT""",
		@"""DIVISION""",
		@"""MODULE""",
		@"""SHIFT_LEFT""",
		@"""SHIFT_RIGHT""",
		@"""UNSIGNED_SHIFT_RIGHT""",
		@"""LESS_THAN""",
		@"""GREATER_THAN""",
		@"""LESS_EQ""",
		@"""GREATER_EQ""",
		@"""instanceof""",
		@"""EQ""",
		@"""NEQ""",
		@"""STRICT_EQ""",
		@"""STRICT_NEQ""",
		@"""BITWISE_AND""",
		@"""BITWISE_XOR""",
		@"""BITWISE_OR""",
		@"""LOGICAL_AND""",
		@"""LOGICAL_OR""",
		@"""INTERR""",
		@"""MULT_ASSIGN""",
		@"""DIV_ASSIGN""",
		@"""MOD_ASSIGN""",
		@"""ADD_ASSIGN""",
		@"""SUB_ASSIGN""",
		@"""SHIFT_LEFT_ASSIGN""",
		@"""SHIFT_RIGHT_ASSIGN""",
		@"""AND_ASSIGN""",
		@"""XOR_ASSIGN""",
		@"""OR_ASSIGN""",
		@"""this""",
		@"""null""",
		@"""true""",
		@"""false""",
		@"""STRING_LITERAL""",
		@"""DECIMAL_LITERAL""",
		@"""HEX_INTEGER_LITERAL""",
		@"""LINE_FEED""",
		@"""CARRIAGE_RETURN""",
		@"""LINE_SEPARATOR""",
		@"""PARAGRAPH_SEPARATOR""",
		@"""TAB""",
		@"""VERTICAL_TAB""",
		@"""FORM_FEED""",
		@"""SPACE""",
		@"""NO_BREAK_SPACE""",
		@"""STRICT_NEW""",
		@"""SL_COMMENT"""
	};
	
	private static long[] mk_tokenSet_0_()
	{
		long[] data = { 4494825813559664L, 66584576L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_0_ = new BitSet(mk_tokenSet_0_());
	private static long[] mk_tokenSet_1_()
	{
		long[] data = { 4494826887957362L, 66584576L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_1_ = new BitSet(mk_tokenSet_1_());
	private static long[] mk_tokenSet_2_()
	{
		long[] data = { 4494825813559648L, 66584576L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_2_ = new BitSet(mk_tokenSet_2_());
	private static long[] mk_tokenSet_3_()
	{
		long[] data = { 21474836832L, 66584576L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_3_ = new BitSet(mk_tokenSet_3_());
	private static long[] mk_tokenSet_4_()
	{
		long[] data = { 4494825009185120L, 66584576L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_4_ = new BitSet(mk_tokenSet_4_());
	private static long[] mk_tokenSet_5_()
	{
		long[] data = { -8589946894L, 67108863L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_5_ = new BitSet(mk_tokenSet_5_());
	private static long[] mk_tokenSet_6_()
	{
		long[] data = { -31533248307802126L, 66585087L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_6_ = new BitSet(mk_tokenSet_6_());
	private static long[] mk_tokenSet_7_()
	{
		long[] data = { 32L, 58720256L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_7_ = new BitSet(mk_tokenSet_7_());
	private static long[] mk_tokenSet_8_()
	{
		long[] data = { 4494825813560160L, 66584576L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_8_ = new BitSet(mk_tokenSet_8_());
	
}
}
