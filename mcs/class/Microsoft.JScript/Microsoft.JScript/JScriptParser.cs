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
		public const int LITERAL_in = 28;
		public const int LITERAL_if = 29;
		public const int LITERAL_else = 30;
		public const int ASSIGN = 31;
		public const int LITERAL_new = 32;
		// "." = 33
		public const int OPEN_BRACKET = 34;
		public const int CLOSE_BRACKET = 35;
		public const int DOT = 36;
		public const int INCREMENT = 37;
		public const int DECREMENT = 38;
		public const int LITERAL_delete = 39;
		public const int LITERAL_void = 40;
		public const int LITERAL_typeof = 41;
		public const int PLUS = 42;
		public const int MINUS = 43;
		public const int BITWISE_NOT = 44;
		public const int LOGICAL_NOT = 45;
		public const int MULT = 46;
		public const int DIVISION = 47;
		public const int MODULE = 48;
		public const int SHIFT_LEFT = 49;
		public const int SHIFT_RIGHT = 50;
		public const int UNSIGNED_SHIFT_RIGHT = 51;
		public const int LESS_THAN = 52;
		public const int GREATER_THAN = 53;
		public const int LESS_EQ = 54;
		public const int GREATER_EQ = 55;
		public const int LITERAL_instanceof = 56;
		public const int EQ = 57;
		public const int NEQ = 58;
		public const int STRICT_EQ = 59;
		public const int STRICT_NEQ = 60;
		public const int BITWISE_AND = 61;
		public const int BITWISE_XOR = 62;
		public const int BITWISE_OR = 63;
		public const int LOGICAL_AND = 64;
		public const int LOGICAL_OR = 65;
		public const int INTERR = 66;
		public const int MULT_ASSIGN = 67;
		public const int DIV_ASSIGN = 68;
		public const int MOD_ASSIGN = 69;
		public const int ADD_ASSIGN = 70;
		public const int SUB_ASSIGN = 71;
		public const int SHIFT_LEFT_ASSIGN = 72;
		public const int SHIFT_RIGHT_ASSIGN = 73;
		public const int AND_ASSIGN = 74;
		public const int XOR_ASSIGN = 75;
		public const int OR_ASSIGN = 76;
		public const int LITERAL_this = 77;
		public const int LITERAL_null = 78;
		public const int LITERAL_true = 79;
		public const int LITERAL_false = 80;
		public const int STRING_LITERAL = 81;
		public const int DECIMAL_LITERAL = 82;
		public const int HEX_INTEGER_LITERAL = 83;
		public const int LINE_FEED = 84;
		public const int CARRIAGE_RETURN = 85;
		public const int LINE_SEPARATOR = 86;
		public const int PARAGRAPH_SEPARATOR = 87;
		public const int TAB = 88;
		public const int VERTICAL_TAB = 89;
		public const int FORM_FEED = 90;
		public const int SPACE = 91;
		public const int NO_BREAK_SPACE = 92;
		public const int SL_COMMENT = 93;
		
		
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
				
						  if (stm != null) {
						  	  elems.Add (stm); 
						  	  Console.WriteLine ("DEBUG::src_elem::Add::{0}", 
									     stm.ToString ());
						  }
					
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
							  Console.WriteLine ("DEBUG:src_elem::Add (function)");
					
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
			stm=expr_stm();
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
			stm=if_stm();
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
			stm=return_stm();
			break;
		}
		case LITERAL_with:
		{
			stm=with_stm();
			break;
		}
		case LITERAL_switch:
		{
			switch_stm();
			break;
		}
		case LITERAL_throw:
		{
			stm=throw_stm();
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
			FormalParameterList p = null;
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
				p=formal_param_list();
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
	
	public FormalParameterList  formal_param_list() //throws RecognitionException, TokenStreamException
{
		FormalParameterList p;
		
		Token  i = null;
		Token  t1 = null;
		Token  g = null;
		Token  t2 = null;
		
			p = new FormalParameterList ();
		
		
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
	
	public AST  expr_stm() //throws RecognitionException, TokenStreamException
{
		AST e;
		
		e = null;
		
		e=expr();
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
	
	public AST  if_stm() //throws RecognitionException, TokenStreamException
{
		AST ifStm;
		
		
			ifStm = null;
			AST cond, true_stm, false_stm;
			cond = true_stm = false_stm = null;
		
		
		match(LITERAL_if);
		match(OPEN_PARENS);
		cond=expr();
		match(CLOSE_PARENS);
		true_stm=statement(null);
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
				false_stm=statement(null);
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
			
					  ifStm = new If (cond, true_stm, false_stm);
				
		}
		return ifStm;
	}
	
	public void iteration_stm() //throws RecognitionException, TokenStreamException
{
		
		
		switch ( LA(1) )
		{
		case LITERAL_do:
		{
			match(LITERAL_do);
			statement(null);
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
			statement(null);
			break;
		}
		case LITERAL_for:
		{
			match(LITERAL_for);
			match(OPEN_PARENS);
			inside_for();
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
	
	public AST  return_stm() //throws RecognitionException, TokenStreamException
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
				e=expr();
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
	
	public AST  with_stm() //throws RecognitionException, TokenStreamException
{
		AST with;
		
		
			with = null;
			AST exp, stm;
			exp = stm = null;
		
		
		match(LITERAL_with);
		match(OPEN_PARENS);
		exp=expr();
		match(CLOSE_PARENS);
		stm=statement(null);
		if (0==inputState.guessing)
		{
			
					  with = new With (exp, stm);  
				
		}
		return with;
	}
	
	public void switch_stm() //throws RecognitionException, TokenStreamException
{
		
		
		match(LITERAL_switch);
		match(OPEN_PARENS);
		expr();
		match(CLOSE_PARENS);
		case_block();
	}
	
	public AST  throw_stm() //throws RecognitionException, TokenStreamException
{
		AST t;
		
		
			t = null;
			AST e = null;
		
		
		match(LITERAL_throw);
		e=expr();
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
	
	public Expression  expr() //throws RecognitionException, TokenStreamException
{
		Expression e;
		
		
			e = new Expression ();
			AST a = null;
		
		
		a=assignment_expr();
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
					a=assignment_expr();
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
		expr();
		match(COLON);
		statement_list();
	}
	
	public void inside_for() //throws RecognitionException, TokenStreamException
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
	
	public void var_decl_list(
		VariableStatement var_stm, AST parent
	) //throws RecognitionException, TokenStreamException
{
		
		VariableDeclaration var_decln = null;
		
		var_decln=var_decl(parent);
		if (0==inputState.guessing)
		{
			
					if (var_decln != null)
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
						
								  if (var_decln != null) 
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
			case LITERAL_in:
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
				init=initializer();
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
			case LITERAL_in:
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
	
	public AST  initializer() //throws RecognitionException, TokenStreamException
{
		AST init;
		
		init = null;
		
		match(ASSIGN);
		init=assignment_expr();
		return init;
	}
	
	public AST  assignment_expr() //throws RecognitionException, TokenStreamException
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
						left_hand_side_expr();
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
				left=left_hand_side_expr();
				op=assignment_op();
				right=assignment_expr();
				if (0==inputState.guessing)
				{
					
							  Binary a = new Binary (left, right, op);
							  Console.WriteLine ("\nDEBUG::jscript.g::assign_expr::ToString::" + a.ToString () + "\n");
							  assign_expr = a;
						
				}
			}
			else if ((tokenSet_4_.member(LA(1)))) {
				assign_expr=cond_expr();
			}
			else
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			
		}
		return assign_expr;
	}
	
	public Call  left_hand_side_expr() //throws RecognitionException, TokenStreamException
{
		Call call;
		
		
			call = null;
		
		
		call=call_expr();
		return call;
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
	
	public AST  cond_expr() //throws RecognitionException, TokenStreamException
{
		AST conditional;
		
		
			conditional = null; 
			AST cond;
			AST trueExpr, falseExpr;
			cond = null;
			trueExpr = falseExpr = null;
		
		
		cond=logical_or_expr();
		{
			switch ( LA(1) )
			{
			case INTERR:
			{
				match(INTERR);
				trueExpr=assignment_expr();
				match(COLON);
				falseExpr=assignment_expr();
				if (0==inputState.guessing)
				{
					
						  	  if (trueExpr != null && falseExpr != null) {
							  	  Conditional c = new Conditional ((AST) cond, trueExpr, falseExpr); 
								  conditional =  c;
							  }
						
				}
				break;
			}
			case IDENTIFIER:
			case CLOSE_PARENS:
			case COLON:
			case CLOSE_BRACE:
			case COMMA:
			case SEMI_COLON:
			case LITERAL_in:
			case CLOSE_BRACKET:
			case STRING_LITERAL:
			case DECIMAL_LITERAL:
			case HEX_INTEGER_LITERAL:
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
	
	public AST  member_expr() //throws RecognitionException, TokenStreamException
{
		AST mem_exp;
		
		
			mem_exp = null;
		
		
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
			mem_exp=primary_expr();
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
					arguments_list(null);
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
		return mem_exp;
	}
	
	public AST  primary_expr() //throws RecognitionException, TokenStreamException
{
		AST prim_exp;
		
		Token  p = null;
		Token  id = null;
		
			prim_exp = null;
			Literal l = null;
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
				
						Identifier ident = new Identifier (id.getText ());
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
			l=literal();
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
			e=expr();
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
	
	public void member_aux() //throws RecognitionException, TokenStreamException
{
		
		
		{
			if ((LA(1)==33))
			{
				match(33);
				match(IDENTIFIER);
				member_aux();
			}
			else {
				bool synPredMatched79 = false;
				if (((LA(1)==OPEN_BRACKET)))
				{
					int _m79 = mark();
					synPredMatched79 = true;
					inputState.guessing++;
					try {
						{
							match(OPEN_BRACKET);
						}
					}
					catch (RecognitionException)
					{
						synPredMatched79 = false;
					}
					rewind(_m79);
					inputState.guessing--;
				}
				if ( synPredMatched79 )
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
		
	public void arguments_list(
		Args args
	) //throws RecognitionException, TokenStreamException
{
		
		
			AST a = null;
		
		
		a=assignment_expr();
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
					a=assignment_expr();
					if (0==inputState.guessing)
					{
						args.Add (a);
					}
				}
				else
				{
					goto _loop87_breakloop;
				}
				
			}
_loop87_breakloop:			;
		}    // ( ... )*
	}
	
	public Call  call_expr() //throws RecognitionException, TokenStreamException
{
		Call func_call;
		
		
			func_call = null;
			AST member = null;
			AST args;
		
		
		member=member_expr();
		args=call_aux();
		if (0==inputState.guessing)
		{
			
					  func_call = new Call (member, args);
				
		}
		return func_call;
	}
	
	public AST  call_aux() //throws RecognitionException, TokenStreamException
{
		AST args;
		
		
			Args tmp_args = new Args ();
			args = null;
		
		
		{
			switch ( LA(1) )
			{
			case OPEN_PARENS:
			case OPEN_BRACKET:
			case DOT:
			{
				{
					switch ( LA(1) )
					{
					case OPEN_PARENS:
					{
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
								arguments_list(tmp_args);
								if (0==inputState.guessing)
								{
									args = tmp_args;
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
						break;
					}
					case OPEN_BRACKET:
					{
						match(OPEN_BRACKET);
						expr();
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
				call_aux();
				break;
			}
			case IDENTIFIER:
			case CLOSE_PARENS:
			case COLON:
			case CLOSE_BRACE:
			case COMMA:
			case SEMI_COLON:
			case LITERAL_in:
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
		return args;
	}
	
	public Unary  postfix_expr() //throws RecognitionException, TokenStreamException
{
		Unary post_expr;
		
		
			post_expr = null;
			JSToken op = JSToken.None;
			AST left = null;
		
		
		left=left_hand_side_expr();
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
			case IDENTIFIER:
			case CLOSE_PARENS:
			case COLON:
			case CLOSE_BRACE:
			case COMMA:
			case SEMI_COLON:
			case LITERAL_in:
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
		if (0==inputState.guessing)
		{
			
					  post_expr = new Unary (left, op);
				
		}
		return post_expr;
	}
	
	public Unary  unary_expr() //throws RecognitionException, TokenStreamException
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
			unary_exprn=postfix_expr();
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
			u_expr=unary_expr();
			if (0==inputState.guessing)
			{
				
						  unary_exprn = new Unary (u_expr, op); 
					
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
	
	public AST  multiplicative_expr() //throws RecognitionException, TokenStreamException
{
		AST mult_expr;
		
		
			mult_expr = null;
			Unary left = null;
			AST right = null;
		
		
		left=unary_expr();
		right=multiplicative_aux();
		if (0==inputState.guessing)
		{
			
					  if (right == null)
						  mult_expr = left;
					  else
					  	  mult_expr = new Binary (left, right, ((Binary) right).old_op);
				
		}
		return mult_expr;
	}
	
	public AST  multiplicative_aux() //throws RecognitionException, TokenStreamException
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
				left=unary_expr();
				right=multiplicative_aux();
				if (0==inputState.guessing)
				{
					
								  if (right == null)
									  mult_aux = new Binary (left, null, JSToken.None);
								  else
									  mult_aux = new Binary (left, right, ((Binary) right).old_op);
								  ((Binary) mult_aux).old_op = mult_op;
						
				}
				break;
			}
			case IDENTIFIER:
			case CLOSE_PARENS:
			case COLON:
			case CLOSE_BRACE:
			case COMMA:
			case SEMI_COLON:
			case LITERAL_in:
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
		return mult_aux;
	}
	
	public AST  additive_expr() //throws RecognitionException, TokenStreamException
{
		AST add_expr;
		
		
			add_expr = null;
			AST left, right;
			left = right = null;
		
		
		left=multiplicative_expr();
		right=additive_aux();
		if (0==inputState.guessing)
		{
			
						  if (right == null)
							  add_expr = left;
						  else
							  add_expr = new Binary (left, right, ((Binary) right).old_op);
				
		}
		return add_expr;
	}
	
	public AST  additive_aux() //throws RecognitionException, TokenStreamException
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
				left=multiplicative_expr();
				right=additive_aux();
				if (0==inputState.guessing)
				{
					
							     if (right == null)
								     add_aux = new Binary (left, null, JSToken.None);
							     else
								     add_aux = new Binary (left, right, ((Binary) right).old_op);
							     ((Binary) add_aux).old_op = op;
						
				}
				break;
			}
			case IDENTIFIER:
			case CLOSE_PARENS:
			case COLON:
			case CLOSE_BRACE:
			case COMMA:
			case SEMI_COLON:
			case LITERAL_in:
			case CLOSE_BRACKET:
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
		return add_aux;
	}
	
	public AST  shift_expr() //throws RecognitionException, TokenStreamException
{
		AST shift_expr;
		
		
			shift_expr = null;
			AST left, right;
			left = right = null;
		
		
		left=additive_expr();
		right=shift_aux();
		if (0==inputState.guessing)
		{
			
					  if (right == null)
						  shift_expr = left;
					  else
						  shift_expr = new Binary (left, right, ((Binary) right).old_op);
				
		}
		return shift_expr;
	}
	
	public AST  shift_aux() //throws RecognitionException, TokenStreamException
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
				left=additive_expr();
				right=shift_aux();
				if (0==inputState.guessing)
				{
					
							   if (right == null)
								   shift_auxr = new Binary (left, null, JSToken.None);
							   else
								   shift_auxr = new Binary (left, right, ((Binary) right).old_op);
					
							   ((Binary) shift_auxr).old_op = op;
						
				}
				break;
			}
			case IDENTIFIER:
			case CLOSE_PARENS:
			case COLON:
			case CLOSE_BRACE:
			case COMMA:
			case SEMI_COLON:
			case LITERAL_in:
			case CLOSE_BRACKET:
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
	
	public AST  relational_expr() //throws RecognitionException, TokenStreamException
{
		AST rel_expr;
		
		
			rel_expr = null;
			AST left = null;
			Relational right = null;
		
		
		left=shift_expr();
		right=relational_aux();
		if (0==inputState.guessing)
		{
			
					  if (right == null)
						  rel_expr = left;
					  else
						  rel_expr = new Relational (left, right, right.old_op);
				
		}
		return rel_expr;
	}
	
	public Relational  relational_aux() //throws RecognitionException, TokenStreamException
{
		Relational rel_aux;
		
		
			rel_aux = null;
			JSToken op = JSToken.None;
			AST left = null;
			Relational right = null;
		
		
		{
			switch ( LA(1) )
			{
			case LESS_THAN:
			case GREATER_THAN:
			case LESS_EQ:
			case GREATER_EQ:
			case LITERAL_instanceof:
			{
				op=relational_op();
				left=shift_expr();
				right=relational_aux();
				if (0==inputState.guessing)
				{
					
							   if (right == null)
								  rel_aux = new Relational (left, null, JSToken.None);
							   else
								   rel_aux = new Relational (left, right, right.old_op);
							   rel_aux.old_op = op;
					
						
				}
				break;
			}
			case IDENTIFIER:
			case CLOSE_PARENS:
			case COLON:
			case CLOSE_BRACE:
			case COMMA:
			case SEMI_COLON:
			case LITERAL_in:
			case CLOSE_BRACKET:
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
		case LITERAL_instanceof:
		{
			match(LITERAL_instanceof);
			if (0==inputState.guessing)
			{
				rel_op = JSToken.InstanceOf;
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
	
	public AST  equality_expr() //throws RecognitionException, TokenStreamException
{
		AST eq_expr;
		
		
			eq_expr = null;
			AST left = null;
			Equality right = null;
		
		
		left=relational_expr();
		right=equality_aux();
		if (0==inputState.guessing)
		{
			
					  if (right == null)
						  eq_expr = left;
					  else {
						  eq_expr = new Equality (left, right, right.old_op);
					  }
				
		}
		return eq_expr;
	}
	
	public Equality  equality_aux() //throws RecognitionException, TokenStreamException
{
		Equality eq_aux;
		
		
			eq_aux = null;
			AST left = null;
			Equality right = null;
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
				left=relational_expr();
				right=equality_aux();
				if (0==inputState.guessing)
				{
					
							   if (right == null)
								  eq_aux = new Equality (left, null, JSToken.None);
							   else
								  eq_aux = new Equality (left, right, right.old_op);
					
							  eq_aux.old_op = op;
						
				}
				break;
			}
			case IDENTIFIER:
			case CLOSE_PARENS:
			case COLON:
			case CLOSE_BRACE:
			case COMMA:
			case SEMI_COLON:
			case LITERAL_in:
			case CLOSE_BRACKET:
			case BITWISE_AND:
			case BITWISE_XOR:
			case BITWISE_OR:
			case LOGICAL_AND:
			case LOGICAL_OR:
			case INTERR:
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
	
	public AST  bitwise_and_expr() //throws RecognitionException, TokenStreamException
{
		AST bit_and_expr;
		
		
			bit_and_expr = null;
		AST left;
			AST right;
			left = null;
			right = null;
		
		
		left=equality_expr();
		right=bitwise_and_aux();
		if (0==inputState.guessing)
		{
			
					  if (right == null)
						  bit_and_expr = left;
					  else
						  bit_and_expr = new Binary (left, right, JSToken.BitwiseAnd);
				
		}
		return bit_and_expr;
	}
	
	public AST  bitwise_and_aux() //throws RecognitionException, TokenStreamException
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
				left=equality_expr();
				right=bitwise_and_aux();
				if (0==inputState.guessing)
				{
					
							   if (right == null)
								   bit_and_aux = left;
							   else
								   bit_and_aux = new Binary (left, right, JSToken.BitwiseAnd);
						
				}
				break;
			}
			case IDENTIFIER:
			case CLOSE_PARENS:
			case COLON:
			case CLOSE_BRACE:
			case COMMA:
			case SEMI_COLON:
			case LITERAL_in:
			case CLOSE_BRACKET:
			case BITWISE_XOR:
			case BITWISE_OR:
			case LOGICAL_AND:
			case LOGICAL_OR:
			case INTERR:
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
		return bit_and_aux;
	}
	
	public AST  bitwise_xor_expr() //throws RecognitionException, TokenStreamException
{
		AST bit_xor_expr;
		
		
			bit_xor_expr = null;
			AST left, right;
			left = right = null;
		
		
		left=bitwise_and_expr();
		right=bitwise_xor_aux();
		if (0==inputState.guessing)
		{
			
					  if (right == null)
						  bit_xor_expr = left;
					  else
						  bit_xor_expr = new Binary (left, right, JSToken.BitwiseXor);
				
		}
		return bit_xor_expr;
	}
	
	public AST  bitwise_xor_aux() //throws RecognitionException, TokenStreamException
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
				left=bitwise_and_expr();
				right=bitwise_xor_aux();
				if (0==inputState.guessing)
				{
					
							  if (right == null)
								  bit_xor_aux = left;
							  else
								  bit_xor_aux = new Binary (left, right, JSToken.BitwiseXor);
						
				}
				break;
			}
			case IDENTIFIER:
			case CLOSE_PARENS:
			case COLON:
			case CLOSE_BRACE:
			case COMMA:
			case SEMI_COLON:
			case LITERAL_in:
			case CLOSE_BRACKET:
			case BITWISE_OR:
			case LOGICAL_AND:
			case LOGICAL_OR:
			case INTERR:
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
		return bit_xor_aux;
	}
	
	public AST  bitwise_or_expr() //throws RecognitionException, TokenStreamException
{
		AST bit_or_expr;
		
		
			bit_or_expr = null;
			AST left, right;
			left = right = null;
		
		
		left=bitwise_xor_expr();
		right=bitwise_or_aux();
		if (0==inputState.guessing)
		{
			
				  	  if (right == null)
						  bit_or_expr = left;
					  else
						  bit_or_expr = new Binary (left, right, JSToken.BitwiseOr);
				
		}
		return bit_or_expr;
	}
	
	public AST  bitwise_or_aux() //throws RecognitionException, TokenStreamException
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
				left=bitwise_xor_expr();
				right=bitwise_or_aux();
				if (0==inputState.guessing)
				{
					
							   if (right == null)
								   bit_or_aux = left;
							   else
								   bit_or_aux = new Binary (left, right, JSToken.BitwiseOr);
						
				}
				break;
			}
			case IDENTIFIER:
			case CLOSE_PARENS:
			case COLON:
			case CLOSE_BRACE:
			case COMMA:
			case SEMI_COLON:
			case LITERAL_in:
			case CLOSE_BRACKET:
			case LOGICAL_AND:
			case LOGICAL_OR:
			case INTERR:
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
		return bit_or_aux;
	}
	
	public AST  logical_and_expr() //throws RecognitionException, TokenStreamException
{
		AST log_and_expr;
		
		
			log_and_expr = null;
			AST left, right;
			left = right = null;
		
		
		left=bitwise_or_expr();
		right=logical_and_aux();
		if (0==inputState.guessing)
		{
			
					  if (right == null)
						  log_and_expr = left;
				  	  else
						  log_and_expr = new Binary (left, right, JSToken.LogicalAnd);
				
		}
		return log_and_expr;
	}
	
	public AST  logical_and_aux() //throws RecognitionException, TokenStreamException
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
				left=bitwise_or_expr();
				right=logical_and_aux();
				if (0==inputState.guessing)
				{
					
						   	   if (right == null)
								   log_and_aux = left;
							   else
								   log_and_aux = new Binary (left, right, JSToken.LogicalAnd);
						
				}
				break;
			}
			case IDENTIFIER:
			case CLOSE_PARENS:
			case COLON:
			case CLOSE_BRACE:
			case COMMA:
			case SEMI_COLON:
			case LITERAL_in:
			case CLOSE_BRACKET:
			case LOGICAL_OR:
			case INTERR:
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
		return log_and_aux;
	}
	
	public AST  logical_or_expr() //throws RecognitionException, TokenStreamException
{
		AST log_or_expr;
		
		
			log_or_expr = null; 
			AST left, right;
			left = right = null;
		
		
		left=logical_and_expr();
		right=logical_or_aux();
		if (0==inputState.guessing)
		{
			
					  if (right == null)
					  	  log_or_expr = left;
					  else
						  log_or_expr = new Binary (left, right, JSToken.LogicalOr);
				
		}
		return log_or_expr;
	}
	
	public AST  logical_or_aux() //throws RecognitionException, TokenStreamException
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
				left=logical_and_expr();
				right=logical_or_aux();
				if (0==inputState.guessing)
				{
					
							  if (right == null)
							  	  log_or_aux = left; 
							  else
								  log_or_aux = new Binary (left, right, JSToken.LogicalOr);
						
				}
				break;
			}
			case IDENTIFIER:
			case CLOSE_PARENS:
			case COLON:
			case CLOSE_BRACE:
			case COMMA:
			case SEMI_COLON:
			case LITERAL_in:
			case CLOSE_BRACKET:
			case INTERR:
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
		return log_or_aux;
	}
	
	public void object_literal() //throws RecognitionException, TokenStreamException
{
		
		
		match(OPEN_BRACE);
		{
			bool synPredMatched135 = false;
			if (((LA(1)==OPEN_BRACE)))
			{
				int _m135 = mark();
				synPredMatched135 = true;
				inputState.guessing++;
				try {
					{
						property_name();
						match(COLON);
					}
				}
				catch (RecognitionException)
				{
					synPredMatched135 = false;
				}
				rewind(_m135);
				inputState.guessing--;
			}
			if ( synPredMatched135 )
			{
				match(OPEN_BRACE);
				{ // ( ... )+
				int _cnt137=0;
				for (;;)
				{
					if ((tokenSet_6_.member(LA(1))))
					{
						property_name();
						match(COLON);
						assignment_expr();
					}
					else
					{
						if (_cnt137 >= 1) { goto _loop137_breakloop; } else { throw new NoViableAltException(LT(1), getFilename());; }
					}
					
					_cnt137++;
				}
_loop137_breakloop:				;
				}    // ( ... )+
			}
			else if ((tokenSet_7_.member(LA(1)))) {
				{    // ( ... )*
					for (;;)
					{
						if ((tokenSet_2_.member(LA(1))))
						{
							statement(null);
						}
						else
						{
							goto _loop139_breakloop;
						}
						
					}
_loop139_breakloop:					;
				}    // ( ... )*
			}
			else
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			
		}
		match(CLOSE_BRACE);
	}
	
	public Literal  literal() //throws RecognitionException, TokenStreamException
{
		Literal l;
		
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
				
						  BooleanLiteral bl = new BooleanLiteral (true);
						  l = bl;
					
			}
			break;
		}
		case LITERAL_false:
		{
			match(LITERAL_false);
			if (0==inputState.guessing)
			{
				
						  BooleanLiteral bl = new BooleanLiteral (false);
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
				
						  StringLiteral str = new StringLiteral (s.getText ());
						  l = str;
					
			}
			break;
		}
		case DECIMAL_LITERAL:
		case HEX_INTEGER_LITERAL:
		{
			l=numeric_literal();
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
							goto _loop149_breakloop;
						}
						
					}
_loop149_breakloop:					;
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
	
	public NumericLiteral  numeric_literal() //throws RecognitionException, TokenStreamException
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
				num_lit = new NumericLiteral (Convert.ToSingle (d.getText ()));
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
		int _cnt143=0;
		for (;;)
		{
			if ((tokenSet_6_.member(LA(1))))
			{
				property_name();
				match(COLON);
				primary_expr();
			}
			else
			{
				if (_cnt143 >= 1) { goto _loop143_breakloop; } else { throw new NoViableAltException(LT(1), getFilename());; }
			}
			
			_cnt143++;
		}
_loop143_breakloop:		;
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
		@""".""",
		@"""OPEN_BRACKET""",
		@"""CLOSE_BRACKET""",
		@"""DOT""",
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
		@"""SL_COMMENT"""
	};
	
	private static long[] mk_tokenSet_0_()
	{
		long[] data = { 70253584527984L, 1040384L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_0_ = new BitSet(mk_tokenSet_0_());
	private static long[] mk_tokenSet_1_()
	{
		long[] data = { 70254659057266L, 1040384L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_1_ = new BitSet(mk_tokenSet_1_());
	private static long[] mk_tokenSet_2_()
	{
		long[] data = { 70253584527968L, 1040384L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_2_ = new BitSet(mk_tokenSet_2_());
	private static long[] mk_tokenSet_3_()
	{
		long[] data = { 21474837088L, 1040384L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_3_ = new BitSet(mk_tokenSet_3_());
	private static long[] mk_tokenSet_4_()
	{
		long[] data = { 70252780061280L, 1040384L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_4_ = new BitSet(mk_tokenSet_4_());
	private static long[] mk_tokenSet_5_()
	{
		long[] data = { -56639612772896L, 925695L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_5_ = new BitSet(mk_tokenSet_5_());
	private static long[] mk_tokenSet_6_()
	{
		long[] data = { 32L, 917504L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_6_ = new BitSet(mk_tokenSet_6_());
	private static long[] mk_tokenSet_7_()
	{
		long[] data = { 70253584528992L, 1040384L, 0L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_7_ = new BitSet(mk_tokenSet_7_());
	
}
}
