//
// Parser.cs: Port of Mozilla's Rhino parser.
//	      This class implements the JScript parser.
//
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

using System.Collections;
using System.IO;
using System;

namespace Microsoft.JScript {

	/**
	 * There are three types of functions that can be defined. The first
	 * is a function statement. This is a function appearing as a top-level
	 * statement (i.e., not nested inside some other statement) in either a
	 * script or a function.
	 *
	 * The second is a function expression, which is a function appearing in
	 * an expression except for the third type, which is...
	 *
	 * The third type is a function expression where the expression is the
	 * top-level expression in an expression statement.
	 *
	 * The three types of functions have different treatment and must be
	 * distinquished.
	 */
	enum FunctionType {
		STATEMENT		= 1,
		EXPRESSION		= 2,
		EXPRESSION_STATEMENT	= 3
	}

	public class Parser {

		TokenStream ts;
		bool ok; // did the parse encounter an error?
		ScriptBlock current_script_or_fn;
		int nesting_of_function;
		int nesting_of_with;
		bool allow_member_expr_as_function_name;

		public Parser ()
		{
		}

		/// <summary>
		///   Build a parse tree from a given source_string
		/// </summary>
		///
		/// <remarks>
		///   return an AST representing the parsed program.
		///    If the parse fails, null will be returned.
		/// </remarks>
		public AST Parse (string source_string, string source_location, int line_number)
		{
			ts = new TokenStream (null, source_string, source_location, line_number);
			try {
				return Parse ();
			} catch (IOException ex) {
				throw new Exception ("Illegal state exception");
			}
		}

		/// <summary>
		///   Build a parse tree from a given source_reader
		/// </summary>
		///
		/// <remarks>
		///   return an AST representing the parsed program.
		///    If the parse fails, null will be returned.
		/// </remarks>
		public AST Parse (StreamReader source_reader, string source_location, int line_number)
		{
			ts = new TokenStream (source_reader, null, source_location, line_number);
			return Parse ();
		}

		void MustMatchToken (int to_match, string message_id)
		{
			int tt;
			if ((tt = ts.GetToken ()) != to_match) {
				ReportError (message_id);
				ts.UnGetToken (tt); // in case the parser decides to continue
			}			
		}

		void ReportError (string message_id)
		{
			ok = false;
			ts.ReportCurrentLineError (message_id);
			throw new ParserException ();
		}
	
		AST Parse ()
		{
			current_script_or_fn = new ScriptBlock ();
			ok = true;
			int base_line_number = ts.LineNumber;

			Block pn = new Block (current_script_or_fn);

			try {
				for (;;) {
					ts.allow_reg_exp = true;
					int tt = ts.GetToken ();
					ts.allow_reg_exp = false;
					
					if (tt <= Token.EOF)
						break;

					AST n;
					if (tt == Token.FUNCTION) {
						try {
							n = Function (FunctionType.STATEMENT); 
						} catch (ParserException e) {
							ok = false;
							break;
						}
					} else {
						ts.UnGetToken (tt);
						n = Statement ();
					}
					// FIXME: check if the semantics of addChildToBack (parent, child) 
					// is the same.
					pn.Add (n);
				}
			} catch (StackOverflowException ex) {
				throw new Exception ("Error: too deep parser recursion.");
			}

			if (!ok)
				return null;
		
			// FIXME: they do nf.initScript(currentScriptOrFn, pn); 
			// check if their semantics maps to ours
			current_script_or_fn.Add (pn);

			return current_script_or_fn;
		}

		public bool Eof {
			get { return ts.EOF; }
		}

		bool InsideFunction {
			get { return nesting_of_function != 0; }
		}

		AST ParseFunctionBody ()
		{
			++nesting_of_function;
			Block pn = new Block (ts.LineNumber);
			try {
				int tt;
				while ((tt = ts.PeekToken ()) > Token.EOF && tt != Token.RC) {
					AST n;
					if (tt == Token.FUNCTION) {
						ts.GetToken ();
						n = Function (FunctionType.STATEMENT);
					} else
						n = Statement ();				
					pn.Add (n);
				}
			} catch (ParserException e) {
				ok = false;
			} finally {
				--nesting_of_function;
			}
			return pn;
		}

		AST Function (FunctionType ft)
		{
			FunctionType synthetic_type = ft;
			int base_line_number = ts.LineNumber; // line number where source starts
			string name;
			AST member_expr = null;

			if (ts.MatchToken (Token.NAME)) {
				name = ts.GetString;
				if (!ts.MatchToken (Token.LP)) {
					if (allow_member_expr_as_function_name) {
						// Extension to ECMA: if 'function <name>' does not follow
						// by '(', assume <name> starts memberExpr
						// FIXME: is StringLiteral the correct AST to build?
						AST member_expr_head = new StringLiteral (null, name);
						name = "";
						member_expr = MemberExprTail (false, member_expr_head);
					}
					MustMatchToken (Token.LP, "msg.no.paren.parms");
				}
			} else if (ts.MatchToken (Token.LP)) {
				// Anonymous function
				name = "";
			} else {
				name = "";
				if (allow_member_expr_as_function_name) {
					// Note that memberExpr can not start with '(' like
					// in function (1+2).toString(), because 'function (' already
					// processed as anonymous function
					member_expr = MemberExpr (false);
				}
				MustMatchToken (Token.LP, "msg.no.paren.parms");
			}

			if (member_expr != null)
				synthetic_type = FunctionType.EXPRESSION;
			
			bool nested = InsideFunction;			
			Function fn = CreateFunction (synthetic_type, name);

			if (nested)
				fn.CheckThis = true;
			
			if (nested || nesting_of_with > 0) {
				// 1. Nested functions are not affected by the dynamic scope flag
				// as dynamic scope is already a parent of their scope.
				// 2. Functions defined under the with statement also immune to
				// this setup, in which case dynamic scope is ignored in favor
				// of with object.
				fn.IgnoreDynamicScope = true;
			}
			
			current_script_or_fn.Add (fn);
			ScriptBlock saved_script_or_fn = current_script_or_fn;
			// FIXME:
			// current_script_or_fn = fn;
			int saved_nesting_of_with = nesting_of_with;
			nesting_of_with = 0;

			AST body;
			string source;

			try {
				if (!ts.MatchToken (Token.RP)) {
					bool first = true;
					do {
						first = false;
						MustMatchToken (Token.NAME, "msg.no.parm");
						string s = ts.GetString;
						// FIXME: check if func has repeated param or local vars
						// if (fnNode.hasParamOrVar(s)) {
						// 	ts.reportCurrentLineWarning(Context.getMessage1(
						// 				"msg.dup.parms", s));
						// }
						// FIXME: add the parameter 
						//fnNode.addParam(s);
					} while (ts.MatchToken (Token.COMMA));

					MustMatchToken (Token.RP, "msg.no.paren.after.parms");
				}

				MustMatchToken (Token.LC, "msg.no.brace.body");
				body = ParseFunctionBody ();
				MustMatchToken (Token.RC, "msg.no.brace.after.body");

				if (ft != FunctionType.EXPRESSION)
					CheckWellTerminatedFunction ();
			} finally {
				current_script_or_fn = saved_script_or_fn;
				nesting_of_with = saved_nesting_of_with;
			}

			// FIXME, set it to something meaningful in the if-cases that come below
			AST pn = null; 
			if (member_expr == null) {
				// FIXME
				// pn = nf.initFunction (...);
				// if (functionType == FunctionNode.FUNCTION_EXPRESSION_STATEMENT) {
					// The following can be removed but then code generators should
					// be modified not to push on the stack function expression
					// statements
					// pn = nf.createExprStatementNoReturn(pn, baseLineno);
				// }
				;
			} else {
				;
			}
			return pn;
		}

		Function CreateFunction (FunctionType func_type, string name)
		{
			Function func;
			if (func_type == FunctionType.STATEMENT)
				func = new FunctionDeclaration (name);
			else if (func_type == FunctionType.EXPRESSION)
				func = new FunctionExpression (name);
			else if (func_type == FunctionType.EXPRESSION_STATEMENT) {
				// FIXME: set it to something meaningful
				func = null;
			} else func = null;
			return func;
		}

		AST Statements ()
		{
			int tt;
			// FIXME: parent == null
			Block pn = new Block (null);
			
			while ((tt = ts.PeekToken ()) > Token.EOF && tt != Token.RC)
				pn.Add (Statement ());			
			return pn;
		}

		AST Condition ()
		{
			AST pn;
			MustMatchToken (Token.LP, "msg.no.paren.cond");
			pn = Expr (false);
			MustMatchToken (Token.RP, "msg.no.paren.after.cond");
			return pn;
		}

		void CheckWellTerminated ()
		{
			int tt = ts.PeekTokenSameLine ();
			if (tt == Token.ERROR || tt == Token.EOF || tt == Token.EOL ||
			    tt == Token.SEMI || tt == Token.RC || tt == Token.FUNCTION)
				return;			
			ReportError ("msg.no.semi.stmt");
		}

		void CheckWellTerminatedFunction ()
		{
			// FIXME:
			CheckWellTerminated ();
		}

		string MatchLabel ()
		{
			int line_number = ts.LineNumber;
			string label = null;
			int tt;
			tt = ts.PeekTokenSameLine ();
			if (tt == Token.NAME) {
				ts.GetToken ();
				label = ts.GetString;
			}
			
			if (line_number == ts.LineNumber)
				CheckWellTerminated ();
			
			return label;
		}

		AST Statement ()
		{
			try {
				return StatementHelper ();
			} catch (ParserException e) {
				// skip to end of statement
				int line_number = ts.LineNumber;
				int t;
				do {
					t = ts.GetToken ();
				} while (t != Token.SEMI && t != Token.EOL &&
					 t != Token.EOF && t != Token.ERROR);
				// FIXME:
				throw new Exception ("must create expr stm with ");
			}
		}

		/**
		 * Whether the "catch (e: e instanceof Exception) { ... }" syntax
		 * is implemented.
		 */

		AST StatementHelper ()
		{
			AST pn = null;

			// If skipsemi == true, don't add SEMI + EOL to source at the
			// end of this statment.  For compound statements, IF/FOR etc.
			bool skip_semi = false;

			int tt;
			tt = ts.GetToken ();

			if (tt == Token.IF) {
				skip_semi = true;
				int line_number = ts.LineNumber;
				AST cond = Condition ();
				AST if_true = Statement ();
				AST if_false = null;
				if (ts.MatchToken (Token.ELSE))
					if_false = Statement ();
				pn = new If (null, cond, if_true, if_false, line_number);
			} else if (tt == Token.SWITCH) {
				skip_semi = true;
				pn = new Switch (ts.LineNumber);
				AST cur_case = null;
				ArrayList case_statements;

				MustMatchToken (Token.LP, "msg.no.paren.switch");
				((Switch) pn).exp = Expr (false);
				MustMatchToken (Token.RP, "msg.no.paren.after.switch");
				MustMatchToken (Token.LC, "msg.no.brace.switch");
				
				while ((tt = ts.GetToken ()) != Token.RC && tt != Token.EOF) {
					if (tt == Token.CASE) {
						// FIXME:
						cur_case = Expr (false);
					} else if (tt == Token.DEFAULT) {
						// FIXME:
						// cur_case = nf.createLeaf (Token.DEFAULT);
						;
					} else
						ReportError ("msg.bad.switch");
					
					MustMatchToken (Token.COLON, "msg.no.colon.case");

					// FIXME:
					case_statements = new ArrayList ();

					while ((tt = ts.PeekToken ()) != Token.RC && tt != Token.CASE &&
					       tt != Token.DEFAULT && tt != Token.EOF) {
						case_statements.Add (Statement ());
					}
					// assert cur_case
					// FIXME: 
					// nf.addChildToBack(cur_case, case_statements);
				}
			} else if (tt == Token.WHILE) {
				skip_semi = true;
				int line_number = ts.LineNumber;
				AST cond = Condition ();
				AST body = Statement ();
				// FIXME
				pn = new While (null, cond, body, line_number);
			} else if (tt == Token.DO) {
				int line_number = ts.LineNumber;
				AST body = Statement ();
				MustMatchToken (Token.WHILE, "msg.no.while.do");
				AST cond = Condition ();
				// FIXME:
				pn = new DoWhile (null, body, cond, line_number);
			} else if (tt == Token.FOR) {
				skip_semi = true;
				int line_number = ts.LineNumber;
				AST init;
				AST cond;
				AST incr = null;
				AST body;

				MustMatchToken (Token.LP, "msg.no.paren.for");
				tt = ts.PeekToken ();

				if (tt == Token.SEMI) {
					// FIXME
					init = null;
				} else {
					if (tt == Token.VAR) {
						// set init to a var list or initial
						ts.GetToken (); // throw away the 'var' token
						init = Variables (true);
					} else
						init = Expr (true);
				}

				if (ts.MatchToken (Token.IN)) {
					// 'cond' is the object over which we're iterating
					cond = Expr (false);
				} else { // ordinary for loop
					MustMatchToken (Token.SEMI, "msg.no.semi.for");
					if (ts.PeekToken () == Token.SEMI) {
						// no loop condition
						// FIXME:
						cond = null;
					} else
						cond = Expr (false);

					MustMatchToken (Token.SEMI, "msg.no.semi.for.cond");
					if (ts.PeekToken () == Token.RP) {
						// FIXME:
						incr = null;
					} else
						incr = Expr (false);
				}

				MustMatchToken (Token.RP, "msg.no.paren.for.ctrl");
				body = Statement ();

				if (incr == null) {
					// cond could be null if 'in obj' got eaten by the init node.
					// FIXME:
					// pn = nf.createForIn(init, cond, body, lineno);
					;
				} else {
					// FIXME:
					// pn = nf.createFor(init, cond, incr, body, lineno);
					;
				}
			} else if (tt == Token.TRY) {
				int line_number = ts.LineNumber;
				AST try_block;
				Block catch_blocks = null;
				AST finally_block = null;

				skip_semi = true;

				try_block = Statement ();
				catch_blocks = new Block ();

				bool saw_default_catch = false;
				int peek = ts.PeekToken ();

				if (peek == Token.CATCH) {
					while (ts.MatchToken (Token.CATCH)) {
						if (saw_default_catch)
							ReportError ("msg.catch.unreachable");
						MustMatchToken (Token.LP, "msg.no.paren.catch");
						MustMatchToken (Token.NAME, "msg.bad.catchcond");
						string var_name = ts.GetString;
						AST catch_cond = null;
						
						if (ts.MatchToken (Token.IF))
							catch_cond = Expr (false);
						else
							saw_default_catch = true;
						
						MustMatchToken (Token.RP, "msg.bad.catchcond");
						MustMatchToken (Token.LC, "msg.no.brace.catchblock");

						// FIXME
						catch_blocks.Add (new Catch (var_name, catch_cond, 
									     Statements (), ts.LineNumber));
						MustMatchToken (Token.RC, "msg.no.brace.after.body");
					}
				} else if (peek != Token.FINALLY)
					MustMatchToken (Token.FINALLY, "msg.try.no.catchfinally");
				
				if (ts.MatchToken (Token.FINALLY))
					finally_block = Statement ();

				// FIXME:
				// pn=nf.createTryCatchFinally(tryblock, catchblocks,finallyblock, lineno);

			} else if (tt == Token.THROW) {
				int line_number = ts.LineNumber;
				pn = new Throw (Expr (false), line_number);

				if (line_number == ts.LineNumber)
					CheckWellTerminated ();
			} else if (tt == Token.BREAK) {
				int line_number = ts.LineNumber;
				// matchLabel only matches if there is one
				string label = MatchLabel ();
				pn = new Break (label, line_number);
			} else if (tt == Token.CONTINUE) {
				int line_number = ts.LineNumber;
				string label = MatchLabel ();
				pn = new Continue (null, label, line_number);
			} else if (tt == Token.WITH) {
				skip_semi = true;
				int line_number = ts.LineNumber;
				MustMatchToken (Token.LP, "msg.no.paren.with");
				AST obj = Expr (false);
				MustMatchToken (Token.RP, "msg.no.paren.after.with");
				++nesting_of_with;
				AST body;
				try {
					body = Statement ();
				} finally {
					--nesting_of_with;
				}
				pn = new With (null, obj, body, line_number);
			} else if (tt == Token.VAR) {
				int line_number = ts.LineNumber;
				pn = Variables (false);
				if (ts.LineNumber == line_number)
					CheckWellTerminated ();
			} else if (tt == Token.RETURN) {
				AST ret_expr = null;

				if (!InsideFunction)
					ReportError ("msg.bad.return");
				
				/* This is ugly, but we don't want to require a semicolon. */
				ts.allow_reg_exp = true;
				tt = ts.PeekTokenSameLine ();
				ts.allow_reg_exp = false;
				
				int line_number = ts.LineNumber;
				if (tt != Token.EOF && tt != Token.EOL && tt != Token.SEMI && tt != Token.RC) {
					ret_expr = Expr (false);
					if (ts.LineNumber == line_number)
						CheckWellTerminated ();
				}
				pn = new Return (null, ret_expr, line_number);
			} else if (tt == Token.LC) { 
				skip_semi = true;
				pn = Statements ();
				MustMatchToken (Token.RC, "msg.no.brace.block");
			} else if (tt == Token.ERROR || tt == Token.EOL || tt == Token.SEMI) {
				// FIXME:
				pn = null;
				skip_semi = true;
			} else if (tt == Token.FUNCTION) {
				// FIXME:
				pn = Function (FunctionType.EXPRESSION_STATEMENT);
			} else {
				int last_expr_type = tt;
				int token_number = ts.TokenNumber;
				ts.UnGetToken (tt);
				int line_number = ts.LineNumber;

				pn = Expr (false);

				if (ts.PeekToken () == Token.COLON) {
					/* check that the last thing the tokenizer returned was a
					 * NAME and that only one token was consumed.
					 */
					if (last_expr_type != Token.NAME || (ts.TokenNumber != token_number))
						ReportError ("msg.bad.label");
					
					ts.GetToken (); // eat the colon
					
					string name = ts.GetString;
					// FIXME:
					pn = new Labelled (name, line_number);
					return pn;
				}
				// FIXME
				// pn = nf.createExprStatement(pn, lineno);
				if (ts.LineNumber == line_number)
					CheckWellTerminated ();
			}
			ts.MatchToken (Token.SEMI);
			return pn;
		}
		
		AST Variables (bool in_for_init)
		{
			// FIXME
			VariableStatement pn = new VariableStatement (ts.LineNumber);
			bool first = true;

			for (;;) {
				VariableDeclaration name;
				AST init;
				MustMatchToken (Token.NAME, "msg.bad.var");
				string s = ts.GetString;
				
				first = false;

				// FIXME
				name = new VariableDeclaration (null, s, null, null);

				// ommited check for argument hiding

				if (ts.MatchToken (Token.ASSIGN)) {
					init = AssignExpr (in_for_init);
					name.val = init;
					
				}

				// FIXME
				pn.Add (name);

				if (!ts.MatchToken (Token.COMMA))
					break;
			}
			return pn;
		}

		AST Expr (bool in_for_init)
		{
			// FIXME: parent hardcoded to be null
			Expression pn = new Expression (null);
			pn.Add (AssignExpr (in_for_init));

			while (ts.MatchToken (Token.COMMA)) {
				// FIXME
				//pn = nf.createBinary(Token.COMMA, pn, assignExpr(inForInit));
				pn.Add (AssignExpr (in_for_init));
			}
			return pn;
		}				   

		AST AssignExpr (bool in_for_init)
		{
			AST pn = null;
			AST cond_expr = CondExpr (in_for_init);
			int tt = ts.PeekToken ();
			// omitted: "invalid assignment left-hand side" check.
			if (tt == Token.ASSIGN) {
				ts.GetToken ();
				// FIXME:
				pn = new Assign (null, cond_expr, AssignExpr (in_for_init), JSToken.Assign, false);
			} else if (tt == Token.ASSIGNOP) {
				ts.GetToken ();
				int op = ts.GetOp ();
				// FIXME: discriminate the operator, write Token->JSToken
				pn = new Assign (null, cond_expr, AssignExpr (in_for_init), JSToken.Assign, false);
			}
			return pn;
		}

		AST CondExpr (bool in_for_init)
		{
			AST if_true;
			AST if_false;
			AST pn = OrExpr (in_for_init);

			if (ts.MatchToken (Token.HOOK)) {
				if_true = AssignExpr (false);
				MustMatchToken (Token.COLON, "msg.no.colon.cond");
				if_false = AssignExpr (in_for_init);
				// FIXME
				return new Conditional (null, pn, if_true, if_false);
			}
			return pn;
		}

		AST OrExpr (bool in_for_init)
		{
			AST pn = AndExpr (in_for_init);
			if (ts.MatchToken (Token.OR)) {
				// FIXME
				return new Binary (null, pn, OrExpr (in_for_init), JSToken.LogicalOr);
			}
			return pn;
		}

		AST AndExpr (bool in_for_init)
		{
			AST pn = BitOrExpr (in_for_init);
			if (ts.MatchToken (Token.AND)) {
				// FIXME
				return new Binary (null, pn, AndExpr (in_for_init), JSToken.LogicalAnd);
			}
			return pn;
		}

		AST BitOrExpr (bool in_for_init)
		{
			AST pn = BitXorExpr (in_for_init);
			while (ts.MatchToken (Token.BITOR)) {
				// FIXME, create binary
				BitXorExpr (in_for_init);
			}
			return pn;
		}

		AST BitXorExpr (bool in_for_init)
		{
			AST pn = BitAndExpr (in_for_init);
			while (ts.MatchToken (Token.BITXOR)) {
				// FIMXE, create binary
				BitAndExpr (in_for_init);
			}
			return pn;
		}

		AST BitAndExpr (bool in_for_init)
		{
			AST pn = EqExpr (in_for_init);
			while (ts.MatchToken (Token.BITAND)) {
				// FIXME, must build binary
				EqExpr (in_for_init);
			}
			return pn;
		}

		AST EqExpr (bool in_for_init)
		{
			AST pn = RelExpr (in_for_init);
			for (;;) {
				int tt = ts.PeekToken ();
				if (tt == Token.EQ || tt == Token.NE || tt == Token.SHEQ || tt == Token.SHNE) {
					ts.GetToken ();
					// FIXME, create binary
					RelExpr (in_for_init);
					continue;
				}
				break;
			}
			return pn;
		}

		AST RelExpr (bool in_for_init)
		{	
			AST pn = ShiftExpr ();
			for (;;) {
				int tt = ts.PeekToken ();
				if (tt == Token.IN) {
					if (in_for_init)
						break;
				} else if (tt == Token.INSTANCEOF || tt == Token.LE || tt == Token.LT || tt == Token.GE || 
					   tt == Token.GT) {
					ts.GetToken ();
					// FIXME, build binary
					ShiftExpr ();
					continue;
				}
				break;
			}
			return pn;
		}

		AST ShiftExpr ()
		{
			AST pn = AddExpr ();
			for (;;) {
				int tt = ts.PeekToken ();
				if (tt == Token.LSH || tt == Token.URSH || tt == Token.RSH) {
					ts.GetToken ();
					// FIXME, build binary
					AddExpr ();
					continue;
				}
				break;
			}
			return pn;
		}

		AST AddExpr ()
		{
			AST pn = MulExpr ();
			for (;;) {
				int tt = ts.PeekToken ();
				if (tt == Token.ADD || tt == Token.SUB) {
					ts.GetToken ();
					// FIXME, build binary
					MulExpr ();
					continue;
				}
				break;
			}
			return pn;
		}

		AST MulExpr ()
		{
			AST pn = UnaryExpr ();
			for (;;) {
				int tt = ts.PeekToken ();
				if (tt == Token.MUL || tt == Token.DIV || tt == Token.MOD) {
					ts.GetToken ();					
					// FIXME, create binary
					UnaryExpr ();
					continue;
				}
				break;
			}
			return pn;
		}
		
		AST UnaryExpr ()
		{
			int tt;

			ts.allow_reg_exp = true;
			tt = ts.GetToken ();
			ts.allow_reg_exp = false;

			if (tt == Token.VOID || tt == Token.NOT || tt == Token.BITNOT || tt == Token.TYPEOF) {
				// FIXME, create unary
				return UnaryExpr ();
			} else if (tt == Token.ADD) {
				// FIXME, create unary
				return UnaryExpr ();
			} else if (tt == Token.SUB) {
				// FIXME, create unary
				return UnaryExpr ();
			} else if (tt == Token.INC || tt == Token.DEC) {
				// FIXME, create inc decl
				return MemberExpr (true);
			} else if (tt == Token.DELPROP) {
				// FIXME, create unary
				return UnaryExpr ();
			} if (tt == Token.ERROR) {
				;
			} else {
				ts.UnGetToken (tt);
				int line_number = ts.LineNumber;
				
				AST pn = MemberExpr (true);

				/* don't look across a newline boundary for a postfix incop.

				* the rhino scanner seems to work differently than the js
				* scanner here; in js, it works to have the line number check
				* precede the peekToken calls.  It'd be better if they had
				* similar behavior...
				*/
				int peeked;				
				if (((peeked = ts.PeekToken ()) == Token.INC || peeked == Token.DEC) && ts.LineNumber == line_number) {
					int pf = ts.GetToken ();
					// FIXME, create inc decl
					return null;
				}
				return pn;
			}
			// FIXME, create name with error
			return null;
		}

		void ArgumentList (AST list) 
		{
			bool matched;
			ts.allow_reg_exp = true;
			matched = ts.MatchToken (Token.RP);
			ts.allow_reg_exp = false;

			if (!matched) {
				bool first = true;
				do {
					first = false;
					// FIXME, add the AssignExpr to the list
					AssignExpr (false);
				} while (ts.MatchToken (Token.COMMA));
				
				MustMatchToken (Token.RP, "msg.no.paren.arg");
			}
		}

		AST MemberExpr (bool allow_call_syntax)
		{
			int tt;
			// FIXME, set it to something meaningful in the rest of the func
			AST pn = null;

			/* Check for new expressions. */
			ts.allow_reg_exp = true;
			tt = ts.PeekToken ();
			ts.allow_reg_exp = false;
			
			if (tt == Token.NEW) {
				/* Eat the NEW token. */
				ts.GetToken ();

				/* Make a NEW node to append to. */
				// FIXME, create Call or New ast node.
				MemberExpr (false);

				if (ts.MatchToken (Token.LP)) {
					// FIXME, add the arguments list to pn
					ArgumentList (pn);
				}

				/* XXX there's a check in the C source against
				 * "too many constructor arguments" - how many
				 * do we claim to support?
				 */

				/* Experimental syntax:  allow an object literal to follow a new expression,
				 * which will mean a kind of anonymous class built with the JavaAdapter.
				 * the object literal will be passed as an additional argument to the constructor.
				 */

				tt = ts.PeekToken ();
				if (tt == Token.LC) {
					// FIXME, set the result of PrimaryExpr to something ;-)
					pn = PrimaryExpr ();
				}
			} else
				pn = PrimaryExpr ();
			
			return MemberExprTail (allow_call_syntax, pn);
		}

		AST MemberExprTail (bool allow_call_syntax, AST pn)
		{
			int tt;

			while ((tt = ts.GetToken ()) > Token.EOF) {
				if (tt == Token.DOT) {
					MustMatchToken (Token.NAME, "msg.no.name.after.dot");
					string s = ts.GetString;					
					// FIXME, create binary
					// pn = nf.createBinary(Token.DOT, pn,
					// nf.createName(ts.getString());
					string foo = ts.GetString;
				} else if (tt == Token.LB) {
					// FIXME, create binary
					// pn = nf.createBinary(Token.LB, pn, expr(false));
					Expr (false);
					MustMatchToken (Token.RB, "msg.no.bracket.index");
				} else if (allow_call_syntax && tt == Token.LP) {
					/* make a call node */
					// FIXME, create call node
					// pn = nf.createCallOrNew(Token.CALL, pn);

					/* Add the arguments to pn, if any are supplied. */
					ArgumentList (pn);
				} else {
					ts.UnGetToken (tt);
					break;
				}
			}
			return pn;
		}

		AST PrimaryExpr ()
		{
			int tt;
			AST pn;

			ts.allow_reg_exp = true;
			tt = ts.GetToken ();
			ts.allow_reg_exp = false;

			if (tt == Token.FUNCTION) {
				// FIXME
				return Function (FunctionType.EXPRESSION);
			} else if (tt == Token.LB) {
				// FIXME, set elems to something like an ArrayLiteral
				ASTList elems = new ASTList ();
				int skip_count = 0;
				bool after_lb_or_comma = true;
				for (;;) {
					ts.allow_reg_exp = true;
					tt = ts.PeekToken ();
					ts.allow_reg_exp = false;
					
					if (tt == Token.COMMA) {
						ts.GetToken ();
						if (!after_lb_or_comma) 
							after_lb_or_comma = true;
						else {
							elems.Add (null);
							++skip_count;
						}
					} else if (tt == Token.RB) {
						ts.GetToken ();
						break;
					} else {
						if (!after_lb_or_comma) 
							ReportError ("msg.no.bracket.arg");
						elems.Add (AssignExpr (false));
						after_lb_or_comma = false;
					}
				}
				// FIXME: pass a real Context
				return new ArrayLiteral (null, elems);
			} else if (tt == Token.LC) {
				// FIXME, create an elements container
				ArrayList elems = new ArrayList ();

				if (!ts.MatchToken (Token.RC)) {
					bool first = true;
					
					commaloop: {
					do {
						ObjectLiteralItem property;

						if (!first)
							;
						else
							first = false;
						
						tt = ts.GetToken ();
						
						if (tt == Token.NAME || tt == Token.STRING) {
							string s = ts.GetString;
							// FIXME: they do
							// property = ScriptRuntime.getIndexObject(s);
							property = new ObjectLiteralItem (s);
						} else if (tt == Token.NUMBER) {
							double n = ts.GetNumber;
							// FIXME, they do
							// property = ScriptRuntime.getIndexObject(s);
							property = new ObjectLiteralItem (n);
						} else if (tt == Token.RC) {
							// trailing comma is OK
							ts.UnGetToken (tt);
							goto leave_commaloop;
						} else {
							ReportError ("msg.bad.prop");
							goto leave_commaloop;
						}
						MustMatchToken (Token.COLON, "msg.no.colon.prop");

						// FIXME, add properties to elems
						// elems.add(property);
						// elems.add(assignExpr(false));
						property.exp = AssignExpr (false);
						elems.Add (property);
					} while (ts.MatchToken (Token.COMMA));
					MustMatchToken (Token.RC, "msg.no.brace.prop");
					}
					leave_commaloop:
					;
				}
				return new ObjectLiteral (elems);
			} else if (tt == Token.LP) {
				pn = Expr (false);
				MustMatchToken (Token.RP, "msg.no.paren");
				return pn;
			} else if (tt == Token.NAME) {
				string name = ts.GetString;
				// FIXME, sure must create a identifier?
				return new Identifier (null, name);
			} else if (tt == Token.NUMBER) {
				double n = ts.GetNumber;
				return new NumericLiteral (null, n);
			} else if (tt == Token.STRING) {
				string s = ts.GetString;
				return new StringLiteral (null, s);
			} else if (tt == Token.REGEXP) {
				string flags = ts.reg_exp_flags;
				ts.reg_exp_flags = null;
				string re = ts.GetString;
				// FIXME, add reg exp to script
				// int index = currentScriptOrFn.addRegexp(re, flags);
				return new RegExpLiteral (re, flags);
			} else if (tt == Token.NULL) {
				// FIXME, build the null object;
			} else if (tt ==  Token.THIS) {
				return new This ();
			} else if (tt == Token.FALSE || tt == Token.TRUE) {
				bool v;
				if (tt == Token.FALSE)
					v = false;
				else
					v = true;
				return new BooleanLiteral (null, v);
			} else if (tt == Token.RESERVED) {
				ReportError ("msg.reserved.id");
			} else if (tt == Token.ERROR) {
				/* the scanner or one of its subroutines reported the error. */
			} else
				ReportError ("msg.syntax");
			return null;
		}
	}
}
