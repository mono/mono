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
		public const int COLON = 8;
		public const int OPEN_BRACE = 9;
		public const int CLOSE_BRACE = 10;
		public const int COMMA = 11;
		public const int SEMI_COLON = 12;
		public const int LITERAL_try = 13;
		public const int LITERAL_catch = 14;
		public const int LITERAL_finally = 15;
		public const int LITERAL_throw = 16;
		public const int LITERAL_switch = 17;
		public const int LITERAL_default = 18;
		public const int LITERAL_case = 19;
		public const int LITERAL_with = 20;
		public const int LITERAL_return = 21;
		public const int LITERAL_break = 22;
		public const int LITERAL_continue = 23;
		public const int LITERAL_do = 24;
		public const int LITERAL_while = 25;
		public const int LITERAL_for = 26;
		public const int LITERAL_var = 27;
		public const int IN = 28;
		public const int LITERAL_if = 29;
		public const int LITERAL_else = 30;
		public const int ASSIGN = 31;
		public const int LITERAL_new = 32;
		public const int DOT = 33;
		public const int OPEN_BRACKET = 34;
		public const int CLOSE_BRACKET = 35;
		public const int INCREMENT = 36;
		public const int DECREMENT = 37;
		public const int LITERAL_delete = 38;
		public const int LITERAL_void = 39;
		public const int LITERAL_typeof = 40;
		public const int PLUS = 41;
		public const int MINUS = 42;
		public const int BITWISE_NOT = 43;
		public const int LOGICAL_NOT = 44;
		public const int MULT = 45;
		public const int DIVISION = 46;
		public const int MODULE = 47;
		public const int SHIFT_LEFT = 48;
		public const int SHIFT_RIGHT = 49;
		public const int UNSIGNED_SHIFT_RIGHT = 50;
		public const int LESS_THAN = 51;
		public const int GREATER_THAN = 52;
		public const int LESS_EQ = 53;
		public const int GREATER_EQ = 54;
		public const int INSTANCE_OF = 55;
		public const int EQ = 56;
		public const int NEQ = 57;
		public const int STRICT_EQ = 58;
		public const int STRICT_NEQ = 59;
		public const int BITWISE_AND = 60;
		public const int BITWISE_XOR = 61;
		public const int BITWISE_OR = 62;
		public const int LOGICAL_AND = 63;
		public const int LOGICAL_OR = 64;
		public const int INTERR = 65;
		public const int MULT_ASSIGN = 66;
		public const int DIV_ASSIGN = 67;
		public const int MOD_ASSIGN = 68;
		public const int ADD_ASSIGN = 69;
		public const int SUB_ASSIGN = 70;
		public const int SHIFT_LEFT_ASSIGN = 71;
		public const int SHIFT_RIGHT_ASSIGN = 72;
		public const int AND_ASSIGN = 73;
		public const int XOR_ASSIGN = 74;
		public const int OR_ASSIGN = 75;
		public const int LITERAL_this = 76;
		public const int LITERAL_null = 77;
		public const int LITERAL_true = 78;
		public const int LITERAL_false = 79;
		public const int STRING_LITERAL = 80;
		public const int DECIMAL_LITERAL = 81;
		public const int HEX_INTEGER_LITERAL = 82;
		public const int LINE_FEED = 83;
		public const int CARRIAGE_RETURN = 84;
		public const int LINE_SEPARATOR = 85;
		public const int PARAGRAPH_SEPARATOR = 86;
		public const int TAB = 87;
		public const int VERTICAL_TAB = 88;
		public const int FORM_FEED = 89;
		public const int SPACE = 90;
		public const int NO_BREAK_SPACE = 91;
		public const int SL_COMMENT = 92;
		public const int ML_COMMENT = 93;
		
		
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
		
	public ScriptBlock  program() //throws RecognitionException, TokenStreamException
{
		ScriptBlock prog;
		
		prog = new ScriptBlock ();
		
		source_elements(prog.src_elems);
		return prog;
	}
	
	public void source_elements(
		Block elems
	) //throws RecognitionException, TokenStreamException
{
		
		
		{    // ( ... )*
			for (;;)
			{
				if ((tokenSet_0_.member(LA(1))))
				{
					source_element(elems, elems.parent);
				}
				else
				{
					goto _loop4_breakloop;
				}
				
			}
_loop4_breakloop:			;
		}    // ( ... )*
	}
	
	public void source_element(
		Block elems, AST parent
	) //throws RecognitionException, TokenStreamException
{
		
		AST stm = null;
		
		switch ( LA(1) )
		{
		case IDENTIFIER:
		case OPEN_PARENS:
		case OPEN_BRACE:
		case SEMI_COLON:
		case LITERAL_try:
		case LITERAL_throw:
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
		case INCREMENT:
		case DECREMENT:
		case LITERAL_delete:
		case LITERAL_void:
		case LITERAL_typeof:
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
			stm=statement(parent);
			if (0==inputState.guessing)
			{
				
						  if (stm != null)
						  	  elems.Add (stm); 
					
			}
			break;
		}
		case LITERAL_function:
		{
			stm=function_decl_or_expr(parent);
			if (0==inputState.guessing)
			{
				
						  if (stm != null)
							  elems.Add (stm);
					
			}
			break;
		}
		default:
		{
			throw new NoViableAltException(LT(1), getFilename());
		}
		 }
	}
	
	public AST  statement(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		AST stm;
		
		stm = null;
		
		switch ( LA(1) )
		{
		case IDENTIFIER:
		case OPEN_PARENS:
		case OPEN_BRACE:
		case LITERAL_new:
		case OPEN_BRACKET:
		case INCREMENT:
		case DECREMENT:
		case LITERAL_delete:
		case LITERAL_void:
		case LITERAL_typeof:
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
			stm=expr_stm(parent);
			match(SEMI_COLON);
			break;
		}
		case LITERAL_var:
		{
			stm=var_stm(parent);
			break;
		}
		case SEMI_COLON:
		{
			empty_stm();
			break;
		}
		case LITERAL_if:
		{
			stm=if_stm(parent);
			break;
		}
		case LITERAL_do:
		case LITERAL_while:
		case LITERAL_for:
		{
			iteration_stm(parent);
			break;
		}
		case LITERAL_continue:
		{
			stm=continue_stm();
			break;
		}
		case LITERAL_break:
		{
			stm=break_stm();
			break;
		}
		case LITERAL_return:
		{
			stm=return_stm(parent);
			break;
		}
		case LITERAL_with:
		{
			stm=with_stm(parent);
			break;
		}
		case LITERAL_switch:
		{
			switch_stm(parent);
			break;
		}
		case LITERAL_throw:
		{
			stm=throw_stm(parent);
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
		return stm;
	}
	
	public AST  function_decl_or_expr(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		AST func;
		
		Token  id = null;
		Token  type_annot = null;
		
			func = null;
			bool is_func_exp = false;
			FormalParameterList p = new FormalParameterList ();
			Block body = null;
		
		
		match(LITERAL_function);
		{
			switch ( LA(1) )
			{
			case IDENTIFIER:
			{
				id = LT(1);
				match(IDENTIFIER);
				break;
			}
			case OPEN_PARENS:
			{
				if (0==inputState.guessing)
				{
					is_func_exp = true;
				}
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		match(OPEN_PARENS);
		{
			switch ( LA(1) )
			{
			case IDENTIFIER:
			{
				p=formal_param_list(parent);
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
		{
			switch ( LA(1) )
			{
			case COLON:
			{
				match(COLON);
				type_annot = LT(1);
				match(IDENTIFIER);
				break;
			}
			case OPEN_BRACE:
			{
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		if (0==inputState.guessing)
		{
			
					if (is_func_exp)
						if (type_annot == null)
							func = new FunctionExpression (parent, String.Empty, p,
										       null, null);
						else 
							func = new FunctionExpression (parent, String.Empty, p,
										       type_annot.getText (), null);
					else if (type_annot == null)
						func = new FunctionDeclaration (parent, id.getText (), p, null, null);
					     else 
						func = new FunctionDeclaration (parent, id.getText (), p, 
										type_annot.getText (), null);
				
		}
		match(OPEN_BRACE);
		body=function_body(func);
		if (0==inputState.guessing)
		{
			((FunctionDeclaration) func).Function.body = body;
		}
		match(CLOSE_BRACE);
		return func;
	}
	
	public FormalParameterList  formal_param_list(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		FormalParameterList p;
		
		Token  i = null;
		Token  t1 = null;
		Token  g = null;
		Token  t2 = null;
		
			p = new FormalParameterList ();
		p.parent = parent;
		
		
		i = LT(1);
		match(IDENTIFIER);
		{
			switch ( LA(1) )
			{
			case COLON:
			{
				match(COLON);
				t1 = LT(1);
				match(IDENTIFIER);
				if (0==inputState.guessing)
				{
					p.Add (i.getText (), t1.getText ());
				}
				break;
			}
			case CLOSE_PARENS:
			case COMMA:
			{
				if (0==inputState.guessing)
				{
					p.Add (i.getText (), "Object");
				}
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
					g = LT(1);
					match(IDENTIFIER);
					{
						switch ( LA(1) )
						{
						case COLON:
						{
							match(COLON);
							t2 = LT(1);
							match(IDENTIFIER);
							if (0==inputState.guessing)
							{
								p.Add (g.getText (), t2.getText ());
							}
							break;
						}
						case CLOSE_PARENS:
						case COMMA:
						{
							if (0==inputState.guessing)
							{
								p.Add (g.getText (), "Object");
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
				else
				{
					goto _loop15_breakloop;
				}
				
			}
_loop15_breakloop:			;
		}    // ( ... )*
		return p;
	}
	
	public Block  function_body(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		Block elems;
		
		
			elems = new Block (parent);
		
		
		source_elements(elems);
		return elems;
	}
	
	public AST  expr_stm(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		AST e;
		
		e = null;
		
		e=expr(parent);
		return e;
	}
	
	public VariableStatement  var_stm(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		VariableStatement var_stm;
		
		var_stm = new VariableStatement ();
		
		match(LITERAL_var);
		var_decl_list(var_stm, parent);
		match(SEMI_COLON);
		return var_stm;
	}
	
	public void empty_stm() //throws RecognitionException, TokenStreamException
{
		
		
		match(SEMI_COLON);
	}
	
	public AST  if_stm(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		AST if_stm;
		
		
			if_stm = null;
			AST cond, true_stm, false_stm;
			cond = true_stm = false_stm = null;
		
		
		match(LITERAL_if);
		match(OPEN_PARENS);
		cond=expr(if_stm);
		match(CLOSE_PARENS);
		true_stm=statement(if_stm);
		{
			bool synPredMatched53 = false;
			if (((LA(1)==LITERAL_else)))
			{
				int _m53 = mark();
				synPredMatched53 = true;
				inputState.guessing++;
				try {
					{
						match(LITERAL_else);
					}
				}
				catch (RecognitionException)
				{
					synPredMatched53 = false;
				}
				rewind(_m53);
				inputState.guessing--;
			}
			if ( synPredMatched53 )
			{
				match(LITERAL_else);
				false_stm=statement(if_stm);
			}
			else if ((tokenSet_1_.member(LA(1)))) {
			}
			else
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			
		}
		if (0==inputState.guessing)
		{
			
			if_stm = new If (parent, cond, true_stm, false_stm);
				
		}
		return if_stm;
	}
	
	public void iteration_stm(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		
		
		switch ( LA(1) )
		{
		case LITERAL_do:
		{
			match(LITERAL_do);
			statement(null);
			match(LITERAL_while);
			match(OPEN_PARENS);
			expr(parent);
			match(CLOSE_PARENS);
			match(SEMI_COLON);
			break;
		}
		case LITERAL_while:
		{
			match(LITERAL_while);
			match(OPEN_PARENS);
			expr(parent);
			match(CLOSE_PARENS);
			statement(null);
			break;
		}
		case LITERAL_for:
		{
			match(LITERAL_for);
			match(OPEN_PARENS);
			inside_for(parent);
			match(CLOSE_PARENS);
			statement(null);
			break;
		}
		default:
		{
			throw new NoViableAltException(LT(1), getFilename());
		}
		 }
	}
	
	public AST  continue_stm() //throws RecognitionException, TokenStreamException
{
		AST cont;
		
		Token  id = null;
		cont = new Continue ();
		
		match(LITERAL_continue);
		{
			switch ( LA(1) )
			{
			case IDENTIFIER:
			{
				id = LT(1);
				match(IDENTIFIER);
				if (0==inputState.guessing)
				{
					((Continue) cont).identifier = id.getText ();
				}
				break;
			}
			case SEMI_COLON:
			{
				if (0==inputState.guessing)
				{
					((Continue) cont).identifier = String.Empty;
				}
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		match(SEMI_COLON);
		return cont;
	}
	
	public AST  break_stm() //throws RecognitionException, TokenStreamException
{
		AST b;
		
		Token  id = null;
		
			b = new Break ();
		
		
		match(LITERAL_break);
		{
			switch ( LA(1) )
			{
			case IDENTIFIER:
			{
				id = LT(1);
				match(IDENTIFIER);
				if (0==inputState.guessing)
				{
					((Break) b).identifier = id.getText ();
				}
				break;
			}
			case SEMI_COLON:
			{
				if (0==inputState.guessing)
				{
					((Break) b).identifier = String.Empty;
				}
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		match(SEMI_COLON);
		return b;
	}
	
	public AST  return_stm(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		AST r;
		
		
			r = null;
			AST e = null;
		
		
		match(LITERAL_return);
		{
			switch ( LA(1) )
			{
			case IDENTIFIER:
			case OPEN_PARENS:
			case OPEN_BRACE:
			case LITERAL_new:
			case OPEN_BRACKET:
			case INCREMENT:
			case DECREMENT:
			case LITERAL_delete:
			case LITERAL_void:
			case LITERAL_typeof:
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
				e=expr(parent);
				if (0==inputState.guessing)
				{
					r = new Return (e);
				}
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
		return r;
	}
	
	public AST  with_stm(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		AST with;
		
		
			with = null;
			AST exp, stm;
			exp = stm = null;
		
		
		match(LITERAL_with);
		match(OPEN_PARENS);
		exp=expr(parent);
		match(CLOSE_PARENS);
		stm=statement(null);
		if (0==inputState.guessing)
		{
			
					  with = new With (exp, stm);  
				
		}
		return with;
	}
	
	public void switch_stm(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		
		
		match(LITERAL_switch);
		match(OPEN_PARENS);
		expr(parent);
		match(CLOSE_PARENS);
		case_block();
	}
	
	public AST  throw_stm(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		AST t;
		
		
			t = null;
			AST e = null;
		
		
		match(LITERAL_throw);
		e=expr(parent);
		match(SEMI_COLON);
		if (0==inputState.guessing)
		{
			
					  t = new Throw (e);
				
		}
		return t;
	}
	
	public void try_stm() //throws RecognitionException, TokenStreamException
{
		
		
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
			case SEMI_COLON:
			case LITERAL_try:
			case LITERAL_catch:
			case LITERAL_throw:
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
			case INCREMENT:
			case DECREMENT:
			case LITERAL_delete:
			case LITERAL_void:
			case LITERAL_typeof:
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
							case SEMI_COLON:
							case LITERAL_try:
							case LITERAL_throw:
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
							case INCREMENT:
							case DECREMENT:
							case LITERAL_delete:
							case LITERAL_void:
							case LITERAL_typeof:
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
					case SEMI_COLON:
					case LITERAL_try:
					case LITERAL_throw:
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
					case INCREMENT:
					case DECREMENT:
					case LITERAL_delete:
					case LITERAL_void:
					case LITERAL_typeof:
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
	
	public void block() //throws RecognitionException, TokenStreamException
{
		
		
		match(OPEN_BRACE);
		{    // ( ... )*
			for (;;)
			{
				if ((tokenSet_2_.member(LA(1))))
				{
					statement(null);
				}
				else
				{
					goto _loop19_breakloop;
				}
				
			}
_loop19_breakloop:			;
		}    // ( ... )*
		match(CLOSE_BRACE);
	}
	
	public void catch_exp() //throws RecognitionException, TokenStreamException
{
		
		
		match(LITERAL_catch);
		match(OPEN_PARENS);
		match(IDENTIFIER);
		match(CLOSE_PARENS);
		block();
	}
	
	public void finally_exp() //throws RecognitionException, TokenStreamException
{
		
		
		match(LITERAL_finally);
		block();
	}
	
	public Expression  expr(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		Expression e;
		
		
			e = new Expression (parent);
			AST a = null;
		
		
		a=assignment_expr(parent);
		if (0==inputState.guessing)
		{
			e.Add (a);
		}
		{    // ( ... )*
			for (;;)
			{
				if ((LA(1)==COMMA))
				{
					match(COMMA);
					a=assignment_expr(parent);
					if (0==inputState.guessing)
					{
						e.Add (a);
					}
				}
				else
				{
					goto _loop69_breakloop;
				}
				
			}
_loop69_breakloop:			;
		}    // ( ... )*
		return e;
	}
	
	public void case_block() //throws RecognitionException, TokenStreamException
{
		
		
		match(OPEN_BRACE);
		case_clauses();
		default_clause();
		case_clauses();
		match(CLOSE_BRACE);
	}
	
	public void case_clauses() //throws RecognitionException, TokenStreamException
{
		
		
		{    // ( ... )*
			for (;;)
			{
				if ((LA(1)==LITERAL_case))
				{
					case_clause();
				}
				else
				{
					goto _loop32_breakloop;
				}
				
			}
_loop32_breakloop:			;
		}    // ( ... )*
	}
	
	public void default_clause() //throws RecognitionException, TokenStreamException
{
		
		
		match(LITERAL_default);
		match(COLON);
		statement_list();
	}
	
	public void statement_list() //throws RecognitionException, TokenStreamException
{
		
		
		{    // ( ... )*
			for (;;)
			{
				if ((tokenSet_2_.member(LA(1))))
				{
					statement(null);
				}
				else
				{
					goto _loop66_breakloop;
				}
				
			}
_loop66_breakloop:			;
		}    // ( ... )*
	}
	
	public void case_clause() //throws RecognitionException, TokenStreamException
{
		
		
		match(LITERAL_case);
		expr(null);
		match(COLON);
		statement_list();
	}
	
	public void inside_for(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		
		
		switch ( LA(1) )
		{
		case IDENTIFIER:
		case OPEN_PARENS:
		case OPEN_BRACE:
		case SEMI_COLON:
		case LITERAL_new:
		case OPEN_BRACKET:
		case INCREMENT:
		case DECREMENT:
		case LITERAL_delete:
		case LITERAL_void:
		case LITERAL_typeof:
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
				case INCREMENT:
				case DECREMENT:
				case LITERAL_delete:
				case LITERAL_void:
				case LITERAL_typeof:
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
					expr(parent);
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
				case INCREMENT:
				case DECREMENT:
				case LITERAL_delete:
				case LITERAL_void:
				case LITERAL_typeof:
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
					expr(parent);
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
				case INCREMENT:
				case DECREMENT:
				case LITERAL_delete:
				case LITERAL_void:
				case LITERAL_typeof:
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
					expr(parent);
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
				var_decl_list(null, null);
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
							case INCREMENT:
							case DECREMENT:
							case LITERAL_delete:
							case LITERAL_void:
							case LITERAL_typeof:
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
								expr(parent);
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
							case INCREMENT:
							case DECREMENT:
							case LITERAL_delete:
							case LITERAL_void:
							case LITERAL_typeof:
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
								expr(parent);
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
					case IN:
					{
						match(IN);
						expr(parent);
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
	
	public void var_decl_list(
		VariableStatement var_stm, AST parent
	) //throws RecognitionException, TokenStreamException
{
		
		VariableDeclaration var_decln = null;
		
		var_decln=var_decl(parent);
		if (0==inputState.guessing)
		{
			
					if (var_decln != null && var_stm != null)
						var_stm.Add (var_decln);
				
		}
		{    // ( ... )*
			for (;;)
			{
				if ((LA(1)==COMMA))
				{
					match(COMMA);
					var_decln=var_decl(parent);
					if (0==inputState.guessing)
					{
						
								  if (var_decln != null && var_stm != null) 
								  	  var_stm.Add (var_decln);
							
					}
				}
				else
				{
					goto _loop58_breakloop;
				}
				
			}
_loop58_breakloop:			;
		}    // ( ... )*
	}
	
	public VariableDeclaration  var_decl(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		VariableDeclaration var_decl;
		
		Token  id = null;
		Token  type_annot = null;
		
			var_decl = null;
			AST init = null;
		
		
		id = LT(1);
		match(IDENTIFIER);
		{
			switch ( LA(1) )
			{
			case COLON:
			{
				match(COLON);
				type_annot = LT(1);
				match(IDENTIFIER);
				break;
			}
			case COMMA:
			case SEMI_COLON:
			case IN:
			case ASSIGN:
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
			case ASSIGN:
			{
				init=initializer(parent);
				if (0==inputState.guessing)
				{
					
							  if (type_annot == null)
							  	  var_decl = new VariableDeclaration (parent, id.getText (), null , init);
							  else 
								  var_decl = new VariableDeclaration (parent, id.getText (), type_annot.getText () , init); 
						
				}
				break;
			}
			case COMMA:
			case SEMI_COLON:
			case IN:
			{
				if (0==inputState.guessing)
				{
					
							  if (type_annot == null)
								  var_decl = new VariableDeclaration (parent, id.getText (), null, null);
							  else
								  var_decl = new VariableDeclaration (parent, id.getText (), type_annot.getText (), null);
						
				}
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		return var_decl;
	}
	
	public AST  initializer(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		AST init;
		
		init = null;
		
		match(ASSIGN);
		init=assignment_expr(parent);
		return init;
	}
	
	public AST  assignment_expr(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		AST assign_expr;
		
		
			assign_expr = null;
			JSToken op = JSToken.None;
			AST left, right;
			left = right = null;
		
		
		{
			bool synPredMatched73 = false;
			if (((tokenSet_3_.member(LA(1)))))
			{
				int _m73 = mark();
				synPredMatched73 = true;
				inputState.guessing++;
				try {
					{
						left_hand_side_expr(parent);
						assignment_op();
					}
				}
				catch (RecognitionException)
				{
					synPredMatched73 = false;
				}
				rewind(_m73);
				inputState.guessing--;
			}
			if ( synPredMatched73 )
			{
				left=left_hand_side_expr(parent);
				op=assignment_op();
				right=assignment_expr(parent);
				if (0==inputState.guessing)
				{
					
							  Assign a;
							  if (right is Assign)
							  	  a = new Assign (parent, left, right, op, true);
							  else
							  	  a = new Assign (parent, left, right, op, false);
							  assign_expr = a;
						
				}
			}
			else if ((tokenSet_4_.member(LA(1)))) {
				assign_expr=cond_expr(parent);
			}
			else
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			
		}
		return assign_expr;
	}
	
	public AST  left_hand_side_expr(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		AST lhe;
		
		
			lhe = null;
			Call call = null;
		
		
		bool synPredMatched89 = false;
		if (((tokenSet_3_.member(LA(1)))))
		{
			int _m89 = mark();
			synPredMatched89 = true;
			inputState.guessing++;
			try {
				{
					call_expr(parent);
				}
			}
			catch (RecognitionException)
			{
				synPredMatched89 = false;
			}
			rewind(_m89);
			inputState.guessing--;
		}
		if ( synPredMatched89 )
		{
			call=call_expr(parent);
			if (0==inputState.guessing)
			{
				lhe = call;
			}
		}
		else if ((tokenSet_3_.member(LA(1)))) {
			lhe=new_expr(parent);
		}
		else
		{
			throw new NoViableAltException(LT(1), getFilename());
		}
		
		return lhe;
	}
	
	public JSToken  assignment_op() //throws RecognitionException, TokenStreamException
{
		JSToken assign_op;
		
		
		assign_op = JSToken.None;
		
		
		switch ( LA(1) )
		{
		case ASSIGN:
		{
			match(ASSIGN);
			if (0==inputState.guessing)
			{
				assign_op = JSToken.Assign;
			}
			break;
		}
		case MULT_ASSIGN:
		{
			match(MULT_ASSIGN);
			if (0==inputState.guessing)
			{
				assign_op = JSToken.MultiplyAssign;
			}
			break;
		}
		case DIV_ASSIGN:
		{
			match(DIV_ASSIGN);
			if (0==inputState.guessing)
			{
				assign_op = JSToken.DivideAssign;
			}
			break;
		}
		case MOD_ASSIGN:
		{
			match(MOD_ASSIGN);
			if (0==inputState.guessing)
			{
				assign_op = JSToken.ModuloAssign;
			}
			break;
		}
		case ADD_ASSIGN:
		{
			match(ADD_ASSIGN);
			if (0==inputState.guessing)
			{
				assign_op = JSToken.PlusAssign;
			}
			break;
		}
		case SUB_ASSIGN:
		{
			match(SUB_ASSIGN);
			if (0==inputState.guessing)
			{
				assign_op = JSToken.MinusAssign;
			}
			break;
		}
		case SHIFT_LEFT_ASSIGN:
		{
			match(SHIFT_LEFT_ASSIGN);
			if (0==inputState.guessing)
			{
				assign_op = JSToken.LeftShiftAssign;
			}
			break;
		}
		case SHIFT_RIGHT_ASSIGN:
		{
			match(SHIFT_RIGHT_ASSIGN);
			if (0==inputState.guessing)
			{
				assign_op = JSToken.RightShiftAssign;
			}
			break;
		}
		case AND_ASSIGN:
		{
			match(AND_ASSIGN);
			if (0==inputState.guessing)
			{
				assign_op = JSToken.BitwiseAndAssign;
			}
			break;
		}
		case XOR_ASSIGN:
		{
			match(XOR_ASSIGN);
			if (0==inputState.guessing)
			{
				assign_op = JSToken.BitwiseXorAssign;
			}
			break;
		}
		case OR_ASSIGN:
		{
			match(OR_ASSIGN);
			if (0==inputState.guessing)
			{
				assign_op = JSToken.BitwiseOrAssign;
			}
			break;
		}
		default:
		{
			throw new NoViableAltException(LT(1), getFilename());
		}
		 }
		return assign_op;
	}
	
	public AST  cond_expr(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		AST conditional;
		
		
			conditional = null; 
			AST cond;
			AST trueExpr, falseExpr;
			cond = null;
			trueExpr = falseExpr = null;
		
		
		cond=logical_or_expr(parent);
		{
			switch ( LA(1) )
			{
			case INTERR:
			{
				match(INTERR);
				trueExpr=assignment_expr(parent);
				match(COLON);
				falseExpr=assignment_expr(parent);
				if (0==inputState.guessing)
				{
					
						  	  if (trueExpr != null && falseExpr != null) {
							  	  Conditional c = new Conditional (parent, (AST) cond, trueExpr, falseExpr); 
								  conditional =  c;
							  }
						
				}
				break;
			}
			case CLOSE_PARENS:
			case COLON:
			case CLOSE_BRACE:
			case COMMA:
			case SEMI_COLON:
			case IN:
			case CLOSE_BRACKET:
			{
				if (0==inputState.guessing)
				{
					conditional = cond;
				}
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		return conditional;
	}
	
	public AST  member_expr(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		AST mem_exp;
		
		
			mem_exp = null;
		
		
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
				mem_exp=primary_expr(parent);
				break;
			}
			case LITERAL_new:
			{
				match(LITERAL_new);
				member_expr(parent);
				arguments(parent);
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		member_aux(parent);
		return mem_exp;
	}
	
	public AST  primary_expr(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		AST prim_exp;
		
		Token  p = null;
		Token  id = null;
		
			prim_exp = null;
			AST l = null;
			Expression e = null;
		
		
		switch ( LA(1) )
		{
		case LITERAL_this:
		{
			p = LT(1);
			match(LITERAL_this);
			if (0==inputState.guessing)
			{
				prim_exp = new This ();
			}
			break;
		}
		case OPEN_BRACE:
		{
			object_literal();
			break;
		}
		case IDENTIFIER:
		{
			id = LT(1);
			match(IDENTIFIER);
			if (0==inputState.guessing)
			{
				
						Identifier ident = new Identifier (parent, id.getText ());
						prim_exp = (AST) ident;
					
			}
			break;
		}
		case LITERAL_null:
		case LITERAL_true:
		case LITERAL_false:
		case STRING_LITERAL:
		case DECIMAL_LITERAL:
		case HEX_INTEGER_LITERAL:
		{
			l=literal(parent);
			if (0==inputState.guessing)
			{
				prim_exp = l;
			}
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
			e=expr(parent);
			if (0==inputState.guessing)
			{
				prim_exp = e;
			}
			match(CLOSE_PARENS);
			break;
		}
		default:
		{
			throw new NoViableAltException(LT(1), getFilename());
		}
		 }
		return prim_exp;
	}
	
	public Args  arguments(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		Args args;
		
		
			Args tmp = new Args ();
			args = null; 
		
		
		match(OPEN_PARENS);
		{
			switch ( LA(1) )
			{
			case IDENTIFIER:
			case OPEN_PARENS:
			case OPEN_BRACE:
			case LITERAL_new:
			case OPEN_BRACKET:
			case INCREMENT:
			case DECREMENT:
			case LITERAL_delete:
			case LITERAL_void:
			case LITERAL_typeof:
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
				arguments_list(tmp, parent);
				if (0==inputState.guessing)
				{
					args = tmp;
				}
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
		return args;
	}
	
	public void member_aux(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		
		
		switch ( LA(1) )
		{
		case DOT:
		case OPEN_BRACKET:
		{
			{
				switch ( LA(1) )
				{
				case DOT:
				{
					match(DOT);
					match(IDENTIFIER);
					break;
				}
				case OPEN_BRACKET:
				{
					match(OPEN_BRACKET);
					expr(parent);
					match(CLOSE_BRACKET);
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
			member_aux(parent);
			break;
		}
		case OPEN_PARENS:
		case CLOSE_PARENS:
		case COLON:
		case CLOSE_BRACE:
		case COMMA:
		case SEMI_COLON:
		case IN:
		case ASSIGN:
		case CLOSE_BRACKET:
		case INCREMENT:
		case DECREMENT:
		case PLUS:
		case MINUS:
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
		case INSTANCE_OF:
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
		{
			break;
		}
		default:
		{
			throw new NoViableAltException(LT(1), getFilename());
		}
		 }
	}
	
	public AST  new_expr(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		AST new_exp;
		
		
			new_exp = null; 
			AST mem_exp = null;
		
		
		mem_exp=member_expr(parent);
		if (0==inputState.guessing)
		{
			new_exp = mem_exp;
		}
		return new_exp;
	}
	
	public Call  call_expr(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		Call func_call;
		
		
			func_call = null;
			AST member = null;
			AST args1 = null;
			AST args2 = null;
		
		
		member=member_expr(parent);
		args1=arguments(parent);
		args2=call_aux(parent);
		if (0==inputState.guessing)
		{
			
					  func_call = new Call (parent, member, args1, args2);
				
		}
		return func_call;
	}
	
	public AST  call_aux(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		AST args;
		
		
			args = null;
		
		
		switch ( LA(1) )
		{
		case OPEN_PARENS:
		case DOT:
		case OPEN_BRACKET:
		{
			{
				switch ( LA(1) )
				{
				case OPEN_PARENS:
				{
					arguments(parent);
					break;
				}
				case OPEN_BRACKET:
				{
					match(OPEN_BRACKET);
					expr(parent);
					match(CLOSE_BRACKET);
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
			call_aux(parent);
			break;
		}
		case CLOSE_PARENS:
		case COLON:
		case CLOSE_BRACE:
		case COMMA:
		case SEMI_COLON:
		case IN:
		case ASSIGN:
		case CLOSE_BRACKET:
		case INCREMENT:
		case DECREMENT:
		case PLUS:
		case MINUS:
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
		case INSTANCE_OF:
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
		{
			break;
		}
		default:
		{
			throw new NoViableAltException(LT(1), getFilename());
		}
		 }
		return args;
	}
	
	public void arguments_list(
		Args args, AST parent
	) //throws RecognitionException, TokenStreamException
{
		
		
			AST a = null;
		
		
		a=assignment_expr(parent);
		if (0==inputState.guessing)
		{
			args.Add (a);
		}
		{    // ( ... )*
			for (;;)
			{
				if ((LA(1)==COMMA))
				{
					match(COMMA);
					a=assignment_expr(parent);
					if (0==inputState.guessing)
					{
						args.Add (a);
					}
				}
				else
				{
					goto _loop86_breakloop;
				}
				
			}
_loop86_breakloop:			;
		}    // ( ... )*
	}
	
	public Unary  postfix_expr(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		Unary post_expr;
		
		
			post_expr = null;
			JSToken op = JSToken.None;
			AST left = null;
		
		
		left=left_hand_side_expr(parent);
		{
			switch ( LA(1) )
			{
			case INCREMENT:
			{
				match(INCREMENT);
				if (0==inputState.guessing)
				{
					op = JSToken.Increment;
				}
				break;
			}
			case DECREMENT:
			{
				match(DECREMENT);
				if (0==inputState.guessing)
				{
					op = JSToken.Decrement;
				}
				break;
			}
			case CLOSE_PARENS:
			case COLON:
			case CLOSE_BRACE:
			case COMMA:
			case SEMI_COLON:
			case IN:
			case CLOSE_BRACKET:
			case PLUS:
			case MINUS:
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
			case INSTANCE_OF:
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
			{
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		if (0==inputState.guessing)
		{
			
					  post_expr = new Unary (parent, left, op);
				
		}
		return post_expr;
	}
	
	public Unary  unary_expr(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		Unary unary_exprn;
		
		
			unary_exprn = null;
			JSToken op = JSToken.None;
			AST u_expr = null;
		
		
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
			unary_exprn=postfix_expr(parent);
			break;
		}
		case INCREMENT:
		case DECREMENT:
		case LITERAL_delete:
		case LITERAL_void:
		case LITERAL_typeof:
		case PLUS:
		case MINUS:
		case BITWISE_NOT:
		case LOGICAL_NOT:
		{
			op=unary_op();
			u_expr=unary_expr(parent);
			if (0==inputState.guessing)
			{
				
						  unary_exprn = new Unary (parent, u_expr, op); 
					
			}
			break;
		}
		default:
		{
			throw new NoViableAltException(LT(1), getFilename());
		}
		 }
		return unary_exprn;
	}
	
	public JSToken  unary_op() //throws RecognitionException, TokenStreamException
{
		JSToken unary_op;
		
		unary_op = JSToken.None;
		
		switch ( LA(1) )
		{
		case LITERAL_delete:
		{
			match(LITERAL_delete);
			if (0==inputState.guessing)
			{
				unary_op = JSToken.Delete;
			}
			break;
		}
		case LITERAL_void:
		{
			match(LITERAL_void);
			if (0==inputState.guessing)
			{
				unary_op = JSToken.Void;
			}
			break;
		}
		case LITERAL_typeof:
		{
			match(LITERAL_typeof);
			if (0==inputState.guessing)
			{
				unary_op = JSToken.Typeof;
			}
			break;
		}
		case INCREMENT:
		{
			match(INCREMENT);
			if (0==inputState.guessing)
			{
				unary_op = JSToken.Increment;
			}
			break;
		}
		case DECREMENT:
		{
			match(DECREMENT);
			if (0==inputState.guessing)
			{
				unary_op = JSToken.Decrement;
			}
			break;
		}
		case PLUS:
		{
			match(PLUS);
			if (0==inputState.guessing)
			{
				unary_op = JSToken.Plus;
			}
			break;
		}
		case MINUS:
		{
			match(MINUS);
			if (0==inputState.guessing)
			{
				unary_op = JSToken.Minus;
			}
			break;
		}
		case BITWISE_NOT:
		{
			match(BITWISE_NOT);
			if (0==inputState.guessing)
			{
				unary_op = JSToken.BitwiseNot;
			}
			break;
		}
		case LOGICAL_NOT:
		{
			match(LOGICAL_NOT);
			if (0==inputState.guessing)
			{
				unary_op = JSToken.LogicalNot;
			}
			break;
		}
		default:
		{
			throw new NoViableAltException(LT(1), getFilename());
		}
		 }
		return unary_op;
	}
	
	public AST  multiplicative_expr(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		AST mult_expr;
		
		
			mult_expr = null;
			Unary left = null;
			AST right = null;
		
		
		left=unary_expr(parent);
		right=multiplicative_aux(parent);
		if (0==inputState.guessing)
		{
			
					  if (right == null)
						  mult_expr = left;
					  else
					  	  mult_expr = new Binary (parent, left, right, ((Binary) right).old_op);
				
		}
		return mult_expr;
	}
	
	public AST  multiplicative_aux(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		AST mult_aux;
		
		
			mult_aux = null;
			JSToken mult_op = JSToken.None;
			Unary left = null;
			AST right = null;
		
		
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
						if (0==inputState.guessing)
						{
							mult_op = JSToken.Multiply;
						}
						break;
					}
					case DIVISION:
					{
						match(DIVISION);
						if (0==inputState.guessing)
						{
							mult_op = JSToken.Divide;
						}
						break;
					}
					case MODULE:
					{
						match(MODULE);
						if (0==inputState.guessing)
						{
							mult_op = JSToken.Modulo;
						}
						break;
					}
					default:
					{
						throw new NoViableAltException(LT(1), getFilename());
					}
					 }
				}
				left=unary_expr(parent);
				right=multiplicative_aux(parent);
				if (0==inputState.guessing)
				{
					
								  if (right == null)
									  mult_aux = new Binary (parent, left, null, JSToken.None);
								  else
									  mult_aux = new Binary (parent, left, right, ((Binary) right).old_op);
								  ((Binary) mult_aux).old_op = mult_op;
						
				}
				break;
			}
			case CLOSE_PARENS:
			case COLON:
			case CLOSE_BRACE:
			case COMMA:
			case SEMI_COLON:
			case IN:
			case CLOSE_BRACKET:
			case PLUS:
			case MINUS:
			case SHIFT_LEFT:
			case SHIFT_RIGHT:
			case UNSIGNED_SHIFT_RIGHT:
			case LESS_THAN:
			case GREATER_THAN:
			case LESS_EQ:
			case GREATER_EQ:
			case INSTANCE_OF:
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
			{
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		return mult_aux;
	}
	
	public AST  additive_expr(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		AST add_expr;
		
		
			add_expr = null;
			AST left, right;
			left = right = null;
		
		
		left=multiplicative_expr(parent);
		right=additive_aux(parent);
		if (0==inputState.guessing)
		{
			
						  if (right == null)
							  add_expr = left;
						  else
							  add_expr = new Binary (parent, left, right, ((Binary) right).old_op);
				
		}
		return add_expr;
	}
	
	public AST  additive_aux(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		AST add_aux;
		
		
			add_aux = null;
			JSToken op = JSToken.None;
			AST left, right;
			left = right = null;
		
		
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
						if (0==inputState.guessing)
						{
							op = JSToken.Plus;
						}
						break;
					}
					case MINUS:
					{
						match(MINUS);
						if (0==inputState.guessing)
						{
							op = JSToken.Minus;
						}
						break;
					}
					default:
					{
						throw new NoViableAltException(LT(1), getFilename());
					}
					 }
				}
				left=multiplicative_expr(parent);
				right=additive_aux(parent);
				if (0==inputState.guessing)
				{
					
							     if (right == null)
								     add_aux = new Binary (parent, left, null, JSToken.None);
							     else
								     add_aux = new Binary (parent, left, right, ((Binary) right).old_op);
							     ((Binary) add_aux).old_op = op;
						
				}
				break;
			}
			case CLOSE_PARENS:
			case COLON:
			case CLOSE_BRACE:
			case COMMA:
			case SEMI_COLON:
			case IN:
			case CLOSE_BRACKET:
			case SHIFT_LEFT:
			case SHIFT_RIGHT:
			case UNSIGNED_SHIFT_RIGHT:
			case LESS_THAN:
			case GREATER_THAN:
			case LESS_EQ:
			case GREATER_EQ:
			case INSTANCE_OF:
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
			{
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		return add_aux;
	}
	
	public AST  shift_expr(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		AST shift_expr;
		
		
			shift_expr = null;
			AST left, right;
			left = right = null;
		
		
		left=additive_expr(parent);
		right=shift_aux(parent);
		if (0==inputState.guessing)
		{
			
					  if (right == null)
						  shift_expr = left;
					  else
						  shift_expr = new Binary (parent, left, right, ((Binary) right).old_op);
				
		}
		return shift_expr;
	}
	
	public AST  shift_aux(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		AST shift_auxr;
		
		
			shift_auxr = null; 
			JSToken op = JSToken.None;
			AST left, right;
			left = right = null;
		
		
		{
			switch ( LA(1) )
			{
			case SHIFT_LEFT:
			case SHIFT_RIGHT:
			case UNSIGNED_SHIFT_RIGHT:
			{
				op=shift_op();
				left=additive_expr(parent);
				right=shift_aux(parent);
				if (0==inputState.guessing)
				{
					
							   if (right == null)
								   shift_auxr = new Binary (parent, left, null, JSToken.None);
							   else
								   shift_auxr = new Binary (parent, left, right, ((Binary) right).old_op);
					
							   ((Binary) shift_auxr).old_op = op;
						
				}
				break;
			}
			case CLOSE_PARENS:
			case COLON:
			case CLOSE_BRACE:
			case COMMA:
			case SEMI_COLON:
			case IN:
			case CLOSE_BRACKET:
			case LESS_THAN:
			case GREATER_THAN:
			case LESS_EQ:
			case GREATER_EQ:
			case INSTANCE_OF:
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
			{
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		return shift_auxr;
	}
	
	public JSToken  shift_op() //throws RecognitionException, TokenStreamException
{
		JSToken shift_op;
		
		shift_op = JSToken.None;
		
		switch ( LA(1) )
		{
		case SHIFT_LEFT:
		{
			match(SHIFT_LEFT);
			if (0==inputState.guessing)
			{
				shift_op = JSToken.LeftShift;
			}
			break;
		}
		case SHIFT_RIGHT:
		{
			match(SHIFT_RIGHT);
			if (0==inputState.guessing)
			{
				shift_op = JSToken.RightShift;
			}
			break;
		}
		case UNSIGNED_SHIFT_RIGHT:
		{
			match(UNSIGNED_SHIFT_RIGHT);
			if (0==inputState.guessing)
			{
				shift_op = JSToken.UnsignedRightShift;
			}
			break;
		}
		default:
		{
			throw new NoViableAltException(LT(1), getFilename());
		}
		 }
		return shift_op;
	}
	
	public AST  relational_expr(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		AST rel_expr;
		
		
			rel_expr = null;
			AST left = null;
			Relational right = null;
		
		
		left=shift_expr(parent);
		right=relational_aux(parent);
		if (0==inputState.guessing)
		{
			
					  if (right == null)
						  rel_expr = left;
					  else
						  rel_expr = new Relational (parent, left, right, right.old_op);
				
		}
		return rel_expr;
	}
	
	public Relational  relational_aux(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		Relational rel_aux;
		
		
			rel_aux = null;
			JSToken op = JSToken.None;
			AST left = null;
			Relational right = null;
		
		
		{
			if ((tokenSet_5_.member(LA(1))))
			{
				op=relational_op();
				left=shift_expr(parent);
				right=relational_aux(parent);
				if (0==inputState.guessing)
				{
					
							   if (right == null)
								  rel_aux = new Relational (parent, left, null, JSToken.None);
							   else
								   rel_aux = new Relational (parent, left, right, right.old_op);
							   rel_aux.old_op = op;
					
						
				}
			}
			else if ((tokenSet_6_.member(LA(1)))) {
			}
			else
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			
		}
		return rel_aux;
	}
	
	public JSToken  relational_op() //throws RecognitionException, TokenStreamException
{
		JSToken rel_op;
		
		rel_op = JSToken.None;
		
		switch ( LA(1) )
		{
		case LESS_THAN:
		{
			match(LESS_THAN);
			if (0==inputState.guessing)
			{
				rel_op = JSToken.LessThan;
			}
			break;
		}
		case GREATER_THAN:
		{
			match(GREATER_THAN);
			if (0==inputState.guessing)
			{
				rel_op = JSToken.GreaterThan;
			}
			break;
		}
		case LESS_EQ:
		{
			match(LESS_EQ);
			if (0==inputState.guessing)
			{
				rel_op = JSToken.LessThanEqual;
			}
			break;
		}
		case GREATER_EQ:
		{
			match(GREATER_EQ);
			if (0==inputState.guessing)
			{
				rel_op = JSToken.GreaterThanEqual;
			}
			break;
		}
		case INSTANCE_OF:
		{
			match(INSTANCE_OF);
			if (0==inputState.guessing)
			{
				rel_op = JSToken.InstanceOf;
			}
			break;
		}
		case IN:
		{
			match(IN);
			if (0==inputState.guessing)
			{
				rel_op = JSToken.In;
			}
			break;
		}
		default:
		{
			throw new NoViableAltException(LT(1), getFilename());
		}
		 }
		return rel_op;
	}
	
	public AST  equality_expr(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		AST eq_expr;
		
		
			eq_expr = null;
			AST left = null;
			AST right = null;
		
		
		left=relational_expr(parent);
		right=equality_aux(parent);
		if (0==inputState.guessing)
		{
			
					  if (right == null)
						  eq_expr = left;
					  else {
						  eq_expr = new Binary (parent, left, right, ((Binary) right).old_op);
					  }
				
		}
		return eq_expr;
	}
	
	public AST  equality_aux(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		AST eq_aux;
		
		
			eq_aux = null;
			AST left = null;
			AST right = null;
			JSToken op = JSToken.None;
		
		
		{
			switch ( LA(1) )
			{
			case EQ:
			case NEQ:
			case STRICT_EQ:
			case STRICT_NEQ:
			{
				op=equality_op();
				left=relational_expr(parent);
				right=equality_aux(parent);
				if (0==inputState.guessing)
				{
					
							   if (right == null)
								  eq_aux = new Binary (parent, left, null, JSToken.None);
							   else
								  eq_aux = new Binary (parent, left, right, ((Binary) right).old_op);
							  ((Binary) eq_aux).old_op = op;
						
				}
				break;
			}
			case CLOSE_PARENS:
			case COLON:
			case CLOSE_BRACE:
			case COMMA:
			case SEMI_COLON:
			case IN:
			case CLOSE_BRACKET:
			case BITWISE_AND:
			case BITWISE_XOR:
			case BITWISE_OR:
			case LOGICAL_AND:
			case LOGICAL_OR:
			case INTERR:
			{
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		return eq_aux;
	}
	
	public JSToken  equality_op() //throws RecognitionException, TokenStreamException
{
		JSToken eq_op;
		
		eq_op = JSToken.None;
		
		switch ( LA(1) )
		{
		case EQ:
		{
			match(EQ);
			if (0==inputState.guessing)
			{
				eq_op = JSToken.Equal;
			}
			break;
		}
		case NEQ:
		{
			match(NEQ);
			if (0==inputState.guessing)
			{
				eq_op = JSToken.NotEqual;
			}
			break;
		}
		case STRICT_EQ:
		{
			match(STRICT_EQ);
			if (0==inputState.guessing)
			{
				eq_op = JSToken.StrictEqual;
			}
			break;
		}
		case STRICT_NEQ:
		{
			match(STRICT_NEQ);
			if (0==inputState.guessing)
			{
				eq_op = JSToken.StrictNotEqual;
			}
			break;
		}
		default:
		{
			throw new NoViableAltException(LT(1), getFilename());
		}
		 }
		return eq_op;
	}
	
	public AST  bitwise_and_expr(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		AST bit_and_expr;
		
		
			bit_and_expr = null;
		AST left;
			AST right;
			left = null;
			right = null;
		
		
		left=equality_expr(parent);
		right=bitwise_and_aux(parent);
		if (0==inputState.guessing)
		{
			
					  if (right == null)
						  bit_and_expr = left;
					  else
						  bit_and_expr = new Binary (parent, left, right, JSToken.BitwiseAnd);
				
		}
		return bit_and_expr;
	}
	
	public AST  bitwise_and_aux(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		AST bit_and_aux;
		
		
			bit_and_aux = null;
			AST left = null;
			AST right = null;
		
		
		{
			switch ( LA(1) )
			{
			case BITWISE_AND:
			{
				match(BITWISE_AND);
				left=equality_expr(parent);
				right=bitwise_and_aux(parent);
				if (0==inputState.guessing)
				{
					
							   if (right == null)
								   bit_and_aux = left;
							   else
								   bit_and_aux = new Binary (parent, left, right, JSToken.BitwiseAnd);
						
				}
				break;
			}
			case CLOSE_PARENS:
			case COLON:
			case CLOSE_BRACE:
			case COMMA:
			case SEMI_COLON:
			case IN:
			case CLOSE_BRACKET:
			case BITWISE_XOR:
			case BITWISE_OR:
			case LOGICAL_AND:
			case LOGICAL_OR:
			case INTERR:
			{
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		return bit_and_aux;
	}
	
	public AST  bitwise_xor_expr(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		AST bit_xor_expr;
		
		
			bit_xor_expr = null;
			AST left, right;
			left = right = null;
		
		
		left=bitwise_and_expr(parent);
		right=bitwise_xor_aux(parent);
		if (0==inputState.guessing)
		{
			
					  if (right == null)
						  bit_xor_expr = left;
					  else
						  bit_xor_expr = new Binary (parent, left, right, JSToken.BitwiseXor);
				
		}
		return bit_xor_expr;
	}
	
	public AST  bitwise_xor_aux(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		AST bit_xor_aux;
		
		
			bit_xor_aux = null;
			AST left, right;
			left = right = null;
		
		
		{
			switch ( LA(1) )
			{
			case BITWISE_XOR:
			{
				match(BITWISE_XOR);
				left=bitwise_and_expr(parent);
				right=bitwise_xor_aux(parent);
				if (0==inputState.guessing)
				{
					
							  if (right == null)
								  bit_xor_aux = left;
							  else
								  bit_xor_aux = new Binary (parent, left, right, JSToken.BitwiseXor);
						
				}
				break;
			}
			case CLOSE_PARENS:
			case COLON:
			case CLOSE_BRACE:
			case COMMA:
			case SEMI_COLON:
			case IN:
			case CLOSE_BRACKET:
			case BITWISE_OR:
			case LOGICAL_AND:
			case LOGICAL_OR:
			case INTERR:
			{
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		return bit_xor_aux;
	}
	
	public AST  bitwise_or_expr(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		AST bit_or_expr;
		
		
			bit_or_expr = null;
			AST left, right;
			left = right = null;
		
		
		left=bitwise_xor_expr(parent);
		right=bitwise_or_aux(parent);
		if (0==inputState.guessing)
		{
			
				  	  if (right == null)
						  bit_or_expr = left;
					  else
						  bit_or_expr = new Binary (parent, left, right, JSToken.BitwiseOr);
				
		}
		return bit_or_expr;
	}
	
	public AST  bitwise_or_aux(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		AST bit_or_aux;
		
		
			bit_or_aux = null;
			AST left, right;
			left = right = null;
		
		
		{
			switch ( LA(1) )
			{
			case BITWISE_OR:
			{
				match(BITWISE_OR);
				left=bitwise_xor_expr(parent);
				right=bitwise_or_aux(parent);
				if (0==inputState.guessing)
				{
					
							   if (right == null)
								   bit_or_aux = left;
							   else
								   bit_or_aux = new Binary (parent, left, right, JSToken.BitwiseOr);
						
				}
				break;
			}
			case CLOSE_PARENS:
			case COLON:
			case CLOSE_BRACE:
			case COMMA:
			case SEMI_COLON:
			case IN:
			case CLOSE_BRACKET:
			case LOGICAL_AND:
			case LOGICAL_OR:
			case INTERR:
			{
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		return bit_or_aux;
	}
	
	public AST  logical_and_expr(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		AST log_and_expr;
		
		
			log_and_expr = null;
			AST left, right;
			left = right = null;
		
		
		left=bitwise_or_expr(parent);
		right=logical_and_aux(parent);
		if (0==inputState.guessing)
		{
			
					  if (right == null)
						  log_and_expr = left;
				  	  else
						  log_and_expr = new Binary (parent, left, right, JSToken.LogicalAnd);
				
		}
		return log_and_expr;
	}
	
	public AST  logical_and_aux(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		AST log_and_aux;
		
		
			log_and_aux = null;
			AST left, right;
			left = right = null;
		
		
		{
			switch ( LA(1) )
			{
			case LOGICAL_AND:
			{
				match(LOGICAL_AND);
				left=bitwise_or_expr(parent);
				right=logical_and_aux(parent);
				if (0==inputState.guessing)
				{
					
						   	   if (right == null)
								   log_and_aux = left;
							   else
								   log_and_aux = new Binary (parent, left, right, JSToken.LogicalAnd);
						
				}
				break;
			}
			case CLOSE_PARENS:
			case COLON:
			case CLOSE_BRACE:
			case COMMA:
			case SEMI_COLON:
			case IN:
			case CLOSE_BRACKET:
			case LOGICAL_OR:
			case INTERR:
			{
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		return log_and_aux;
	}
	
	public AST  logical_or_expr(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		AST log_or_expr;
		
		
			log_or_expr = null; 
			AST left, right;
			left = right = null;
		
		
		left=logical_and_expr(parent);
		right=logical_or_aux(parent);
		if (0==inputState.guessing)
		{
			
					  if (right == null)
					  	  log_or_expr = left;
					  else
						  log_or_expr = new Binary (parent, left, right, JSToken.LogicalOr);
				
		}
		return log_or_expr;
	}
	
	public AST  logical_or_aux(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		AST log_or_aux;
		
		
			AST left, right;
			log_or_aux = null;
			left = right = null;	
		
		
		{
			switch ( LA(1) )
			{
			case LOGICAL_OR:
			{
				match(LOGICAL_OR);
				left=logical_and_expr(parent);
				right=logical_or_aux(parent);
				if (0==inputState.guessing)
				{
					
							  if (right == null)
							  	  log_or_aux = left; 
							  else
								  log_or_aux = new Binary (parent, left, right, JSToken.LogicalOr);
						
				}
				break;
			}
			case CLOSE_PARENS:
			case COLON:
			case CLOSE_BRACE:
			case COMMA:
			case SEMI_COLON:
			case IN:
			case CLOSE_BRACKET:
			case INTERR:
			{
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		return log_or_aux;
	}
	
	public void object_literal() //throws RecognitionException, TokenStreamException
{
		
		
		match(OPEN_BRACE);
		{
			bool synPredMatched136 = false;
			if (((tokenSet_7_.member(LA(1)))))
			{
				int _m136 = mark();
				synPredMatched136 = true;
				inputState.guessing++;
				try {
					{
						property_name();
						match(COLON);
					}
				}
				catch (RecognitionException)
				{
					synPredMatched136 = false;
				}
				rewind(_m136);
				inputState.guessing--;
			}
			if ( synPredMatched136 )
			{
				property_name();
				match(COLON);
				assignment_expr(null);
				{    // ( ... )*
					for (;;)
					{
						if ((LA(1)==COMMA))
						{
							match(COMMA);
							property_name();
							match(COLON);
							assignment_expr(null);
						}
						else
						{
							goto _loop138_breakloop;
						}
						
					}
_loop138_breakloop:					;
				}    // ( ... )*
			}
			else if ((tokenSet_8_.member(LA(1)))) {
				{    // ( ... )*
					for (;;)
					{
						if ((tokenSet_2_.member(LA(1))))
						{
							statement(null);
						}
						else
						{
							goto _loop140_breakloop;
						}
						
					}
_loop140_breakloop:					;
				}    // ( ... )*
			}
			else
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			
		}
		match(CLOSE_BRACE);
	}
	
	public AST  literal(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		AST l;
		
		Token  s = null;
		
			l = null; 
		
		
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
			if (0==inputState.guessing)
			{
				
						  BooleanLiteral bl = new BooleanLiteral (parent, true);
						  l = bl;
					
			}
			break;
		}
		case LITERAL_false:
		{
			match(LITERAL_false);
			if (0==inputState.guessing)
			{
				
						  BooleanLiteral bl = new BooleanLiteral (parent, false);
						  l = bl;
					
			}
			break;
		}
		case STRING_LITERAL:
		{
			s = LT(1);
			match(STRING_LITERAL);
			if (0==inputState.guessing)
			{
				
						  StringLiteral str = new StringLiteral (parent, s.getText ());
						  l = str;
					
			}
			break;
		}
		case DECIMAL_LITERAL:
		case HEX_INTEGER_LITERAL:
		{
			l=numeric_literal(parent);
			break;
		}
		default:
		{
			throw new NoViableAltException(LT(1), getFilename());
		}
		 }
		return l;
	}
	
	public void array_literal() //throws RecognitionException, TokenStreamException
{
		
		
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
				primary_expr(null);
				{    // ( ... )*
					for (;;)
					{
						if ((LA(1)==COMMA))
						{
							match(COMMA);
							primary_expr(null);
						}
						else
						{
							goto _loop150_breakloop;
						}
						
					}
_loop150_breakloop:					;
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
	
	public void property_name() //throws RecognitionException, TokenStreamException
{
		
		
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
				numeric_literal(null);
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
	}
	
	public NumericLiteral  numeric_literal(
		AST parent
	) //throws RecognitionException, TokenStreamException
{
		NumericLiteral num_lit;
		
		Token  d = null;
		
			num_lit = null;
		
		
		switch ( LA(1) )
		{
		case DECIMAL_LITERAL:
		{
			d = LT(1);
			match(DECIMAL_LITERAL);
			if (0==inputState.guessing)
			{
				num_lit = new NumericLiteral (parent, System.Convert.ToSingle (d.getText ()));
			}
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
		return num_lit;
	}
	
	public void property_name_and_value_list() //throws RecognitionException, TokenStreamException
{
		
		
		{ // ( ... )+
		int _cnt144=0;
		for (;;)
		{
			if ((tokenSet_7_.member(LA(1))))
			{
				property_name();
				match(COLON);
				primary_expr(null);
			}
			else
			{
				if (_cnt144 >= 1) { goto _loop144_breakloop; } else { throw new NoViableAltException(LT(1), getFilename());; }
			}
			
			_cnt144++;
		}
_loop144_breakloop:		;
		}    // ( ... )+
	}
	
	public void line_terminator() //throws RecognitionException, TokenStreamException
{
		
		
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
	
	public void white_space() //throws RecognitionException, TokenStreamException
{
		
		
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
		@"""COLON""",
		@"""OPEN_BRACE""",
		@"""CLOSE_BRACE""",
		@"""COMMA""",
		@"""SEMI_COLON""",
		@"""try""",
		@"""catch""",
		@"""finally""",
		@"""throw""",
		@"""switch""",
		@"""default""",
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
		@"""DOT""",
		@"""OPEN_BRACKET""",
		@"""CLOSE_BRACKET""",
		@"""INCREMENT""",
		@"""DECREMENT""",
		@"""delete""",
		@"""void""",
		@"""typeof""",
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
		@"""SL_COMMENT""",
		@"""ML_COMMENT"""
	};
	
	private static long[] mk_tokenSet_0_()
	{
		long[] data = { 35137931915888L, 520192L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_0_ = new BitSet(mk_tokenSet_0_());
	private static long[] mk_tokenSet_1_()
	{
		long[] data = { 35139006445170L, 520192L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_1_ = new BitSet(mk_tokenSet_1_());
	private static long[] mk_tokenSet_2_()
	{
		long[] data = { 35137931915872L, 520192L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_2_ = new BitSet(mk_tokenSet_2_());
	private static long[] mk_tokenSet_3_()
	{
		long[] data = { 21474837088L, 520192L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_3_ = new BitSet(mk_tokenSet_3_());
	private static long[] mk_tokenSet_4_()
	{
		long[] data = { 35137127449184L, 520192L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_4_ = new BitSet(mk_tokenSet_4_());
	private static long[] mk_tokenSet_5_()
	{
		long[] data = { 69805794492678144L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_5_ = new BitSet(mk_tokenSet_5_());
	private static long[] mk_tokenSet_6_()
	{
		long[] data = { -72057559409746560L, 3L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_6_ = new BitSet(mk_tokenSet_6_());
	private static long[] mk_tokenSet_7_()
	{
		long[] data = { 32L, 458752L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_7_ = new BitSet(mk_tokenSet_7_());
	private static long[] mk_tokenSet_8_()
	{
		long[] data = { 35137931916896L, 520192L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_8_ = new BitSet(mk_tokenSet_8_());
	
}
}
