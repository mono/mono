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
		Statement,
		Expression,
		ExpressionStatement
	}

	enum ClauseType {
		Case,
		Default,
		CaseAfterDefault
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
			int base_line_number = ts.LineNumber; // line number where source starts

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
							n = Function (null, FunctionType.Statement);
						} catch (ParserException e) {
							ok = false;
							break;
						}
					} else {
						ts.UnGetToken (tt);
						n = Statement (current_script_or_fn);
					}
					current_script_or_fn.Add (n);
				}
			} catch (StackOverflowException ex) {
				throw new Exception ("Error: too deep parser recursion.");
			}

			if (!ok)
				return null;

			return current_script_or_fn;
		}

		public bool Eof {
			get { return ts.EOF; }
		}

		bool InsideFunction {
			get { return nesting_of_function != 0; }
		}

		Block ParseFunctionBody (AST parent)
		{
			++nesting_of_function;
			Block pn = new Block (ts.LineNumber);
			try {
				int tt;
				while ((tt = ts.PeekToken ()) > Token.EOF && tt != Token.RC) {
					AST n;
					if (tt == Token.FUNCTION) {
						ts.GetToken ();
						n = Function (parent, FunctionType.Statement);
					} else
						n = Statement (parent);
					pn.Add (n);
				}
			} catch (ParserException e) {
				ok = false;
			} finally {
				--nesting_of_function;
			}
			return pn;
		}

		AST Function (AST parent, FunctionType ft)
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
						member_expr = MemberExprTail (parent, false, member_expr_head);
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
					member_expr = MemberExpr (parent, false);
				}
				MustMatchToken (Token.LP, "msg.no.paren.parms");
			}

			if (member_expr != null)
				synthetic_type = FunctionType.Expression;
			
			bool nested = InsideFunction;
			Function fn = CreateFunction (parent, synthetic_type, name);

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
			
			ScriptBlock saved_script_or_fn = current_script_or_fn;
			int saved_nesting_of_with = nesting_of_with;
			nesting_of_with = 0;

			FormalParameterList _params = new FormalParameterList ();
			Block body;
			string source;

			try {
				if (!ts.MatchToken (Token.RP)) {
					do {
						MustMatchToken (Token.NAME, "msg.no.parm");
						string s = ts.GetString;
						_params.Add (s, String.Empty);
					} while (ts.MatchToken (Token.COMMA));
					MustMatchToken (Token.RP, "msg.no.paren.after.parms");
				}
				MustMatchToken (Token.LC, "msg.no.brace.body");
				body = ParseFunctionBody (fn);
				MustMatchToken (Token.RC, "msg.no.brace.after.body");

				if (ft != FunctionType.Expression)
					CheckWellTerminatedFunction ();
			} finally {
				current_script_or_fn = saved_script_or_fn;
				nesting_of_with = saved_nesting_of_with;
			}

			fn.Init (body, _params);
			AST pn;

			if (member_expr == null) {
				// FIXME
				pn = fn;

				// FIXME, research about createExprStatementNoReturn
				if (ft == FunctionType.ExpressionStatement)
					pn = null;
			} else {
				// FIXME
				pn = fn;
				pn = new Assign (null, member_expr, pn, JSToken.Assign, false);
				// FIXME, research about createExprStatement
				if (ft != FunctionType.Expression)
					;
			}
			return pn;
		}

		Function CreateFunction (AST parent, FunctionType func_type, string name)
		{
			Function func;
			if (func_type == FunctionType.Statement)
				func = new FunctionDeclaration (parent, name);
			else if (func_type == FunctionType.Expression)
				func = new FunctionExpression (parent, name);
			else if (func_type == FunctionType.ExpressionStatement) {
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
				pn.Add (Statement (null));
			return pn;
		}

		AST Condition (AST parent)
		{
			AST pn;
			MustMatchToken (Token.LP, "msg.no.paren.cond");
			pn = Expr (parent, false);
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

		AST Statement (AST parent)
		{
			try {
				return StatementHelper (parent);
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

		AST StatementHelper (AST parent)
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
				AST cond = Condition (parent);
				AST if_true = Statement (parent);
				AST if_false = null;
				if (ts.MatchToken (Token.ELSE))
					if_false = Statement (parent);
				pn = new If (parent, cond, if_true, if_false, line_number);
			} else if (tt == Token.SWITCH) {
				skip_semi = true;
				pn = new Switch (parent, ts.LineNumber);
				Clause cur_case;
				MustMatchToken (Token.LP, "msg.no.paren.switch");
				((Switch) pn).exp = Expr (parent, false);
				MustMatchToken (Token.RP, "msg.no.paren.after.switch");
				MustMatchToken (Token.LC, "msg.no.brace.switch");
				ClauseType clause_type = ClauseType.Case;

				while ((tt = ts.GetToken ()) != Token.RC && tt != Token.EOF) {
					if (tt == Token.CASE) {
						cur_case = new Clause (pn);
						cur_case.exp = Expr (pn, false);
						if (clause_type == ClauseType.Default)
							clause_type = ClauseType.CaseAfterDefault;
					} else if (tt == Token.DEFAULT) {
						cur_case = null;
						clause_type = ClauseType.Default;
					} else {
						cur_case = null;
						ReportError ("msg.bad.switch");
					}
					MustMatchToken (Token.COLON, "msg.no.colon.case");
					
					while ((tt = ts.PeekToken ()) != Token.RC && tt != Token.CASE && tt != Token.DEFAULT && tt != Token.EOF) {
						if (clause_type == ClauseType.Case || clause_type == ClauseType.CaseAfterDefault)
							cur_case.AddStm (Statement (pn));
						else if (clause_type == ClauseType.Default)
							((Switch) pn).default_clauses.Add (Statement (pn));
					}
					((Switch) pn).AddClause (cur_case, clause_type);
				}
			} else if (tt == Token.WHILE) {
				skip_semi = true;
				int line_number = ts.LineNumber;
				AST cond = Condition (parent);
				AST body = Statement (parent);
				pn = new While (parent, cond, body, line_number);
			} else if (tt == Token.DO) {
				int line_number = ts.LineNumber;
				AST body = Statement (parent);
				MustMatchToken (Token.WHILE, "msg.no.while.do");
				AST cond = Condition (parent);
				pn = new DoWhile (parent, body, cond, line_number);
			} else if (tt == Token.FOR) {
				skip_semi = true;
				int line_number = ts.LineNumber;
				AST init;
				AST cond;
				AST incr = null;
				AST body;

				MustMatchToken (Token.LP, "msg.no.paren.for");
				tt = ts.PeekToken ();

				if (tt == Token.SEMI)
					init = null;
				else {
					if (tt == Token.VAR) {
						// set init to a var list or initial
						ts.GetToken (); // throw away the 'var' token
						init = Variables (parent, true);
					} else
						init = Expr (parent, true);
				}
				
				if (ts.MatchToken (Token.IN))					
					cond = Expr (parent, false); // 'cond' is the object over which we're iterating
				else { 
					// ordinary for loop
					MustMatchToken (Token.SEMI, "msg.no.semi.for");
					
					if (ts.PeekToken () == Token.SEMI)
						cond = null; // no loop condition
					else
						cond = Expr (parent, false);

					MustMatchToken (Token.SEMI, "msg.no.semi.for.cond");
					
					if (ts.PeekToken () == Token.RP)
						incr = null;
					else
						incr = Expr (parent, false);
				}

				MustMatchToken (Token.RP, "msg.no.paren.for.ctrl");
				body = Statement (parent);

				if (incr == null) // cond could be null if 'in obj' got eaten by the init node.
					pn = new ForIn (parent, line_number, init, cond, body);
				else
					pn = new For (parent, line_number, init, cond, incr, body);
			} else if (tt == Token.TRY) {
				int line_number = ts.LineNumber;
				AST try_block;
				ArrayList catch_blocks = null;
				AST finally_block = null;

				skip_semi = true;

				try_block = Statement (parent);
				catch_blocks = new ArrayList ();

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
							catch_cond = Expr (parent, false);
						else
							saw_default_catch = true;
						
						MustMatchToken (Token.RP, "msg.bad.catchcond");
						MustMatchToken (Token.LC, "msg.no.brace.catchblock");

						catch_blocks.Add (new Catch (var_name, catch_cond, 
									     Statements (), parent, ts.LineNumber));
						MustMatchToken (Token.RC, "msg.no.brace.after.body");
					}
				} else if (peek != Token.FINALLY)
					MustMatchToken (Token.FINALLY, "msg.try.no.catchfinally");
				
				if (ts.MatchToken (Token.FINALLY))
					finally_block = Statement (parent);
				pn = new Try (try_block, catch_blocks, finally_block, parent, line_number);
			} else if (tt == Token.THROW) {
				int line_number = ts.LineNumber;
				pn = new Throw (Expr (parent, false), line_number);

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
				AST obj = Expr (parent, false);
				MustMatchToken (Token.RP, "msg.no.paren.after.with");
				++nesting_of_with;
				AST body;
				try {
					body = Statement (parent);
				} finally {
					--nesting_of_with;
				}
				pn = new With (parent, obj, body, line_number);
			} else if (tt == Token.VAR) {
				int line_number = ts.LineNumber;
				pn = Variables (parent, false);
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
					ret_expr = Expr (parent, false);
					if (ts.LineNumber == line_number)
						CheckWellTerminated ();
				}
				pn = new Return (parent, ret_expr, line_number);
			} else if (tt == Token.LC) { 
				skip_semi = true;
				pn = Statements ();
				MustMatchToken (Token.RC, "msg.no.brace.block");
			} else if (tt == Token.ERROR || tt == Token.EOL || tt == Token.SEMI) {
				// FIXME:
				pn = null;
				skip_semi = true;
			} else if (tt == Token.FUNCTION) {
				pn = Function (parent, FunctionType.ExpressionStatement);
			} else {
				int last_expr_type = tt;
				int token_number = ts.TokenNumber;
				ts.UnGetToken (tt);
				int line_number = ts.LineNumber;

				pn = Expr (parent, false);

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
		
		AST Variables (AST parent, bool in_for_init)
		{
			VariableStatement pn = new VariableStatement (parent, ts.LineNumber);
			for (;;) {
				VariableDeclaration name;
				AST init = null;
				MustMatchToken (Token.NAME, "msg.bad.var");
				string s = ts.GetString;				
				name = new VariableDeclaration (parent, s, null, null);

				// ommited check for argument hiding				
				if (ts.MatchToken (Token.ASSIGN)) {
					init = AssignExpr (parent, in_for_init);
					name.val = init;
				}
				pn.Add (name);

				if (!ts.MatchToken (Token.COMMA))
					break;
			}
			return pn;
		}

		AST Expr (AST parent, bool in_for_init)
		{
			Expression pn = new Expression (null);
			AST init = AssignExpr (parent, in_for_init);
			pn.Add (init);

			if (init == null)
				throw new Exception ("Expr, L680, AST is null");

			//pn.Add (AssignExpr (in_for_init));
			while (ts.MatchToken (Token.COMMA))
				pn.Add (AssignExpr (parent, in_for_init));
			return pn;
		}				   

		AST AssignExpr (AST parent, bool in_for_init)
		{
			AST pn = CondExpr (parent, in_for_init);
			int tt = ts.PeekToken ();

			// omitted: "invalid assignment left-hand side" check.
			if (tt == Token.ASSIGN) {
				ts.GetToken ();
				pn = new Assign (parent, pn, AssignExpr (parent, in_for_init), JSToken.Assign, false);
			} else if (tt == Token.ASSIGNOP) {
				ts.GetToken ();
				int op = ts.GetOp ();
				pn = new Assign (parent, pn, AssignExpr (parent, in_for_init), JSToken.Assign, false);
			}
			return pn;
		}

		AST CondExpr (AST parent, bool in_for_init)
		{
			AST if_true;
			AST if_false;
			AST pn = OrExpr (parent, in_for_init);

			if (ts.MatchToken (Token.HOOK)) {
				if_true = AssignExpr (parent, false);
				MustMatchToken (Token.COLON, "msg.no.colon.cond");
				if_false = AssignExpr (parent, in_for_init);
				return new Conditional (parent, pn, if_true, if_false);
			}
			return pn;
		}

		AST OrExpr (AST parent, bool in_for_init)
		{
			AST pn = AndExpr (parent, in_for_init);
			if (ts.MatchToken (Token.OR))
				return new Binary (parent, pn, OrExpr (parent, in_for_init), JSToken.LogicalOr);
			return pn;
		}

		AST AndExpr (AST parent, bool in_for_init)
		{
			AST pn = BitOrExpr (parent, in_for_init);
			if (ts.MatchToken (Token.AND)) {
				return new Binary (parent, pn, AndExpr (parent, in_for_init), JSToken.LogicalAnd);
			}
			return pn;
		}

		AST BitOrExpr (AST parent, bool in_for_init)
		{
			AST pn = BitXorExpr (parent, in_for_init);
			while (ts.MatchToken (Token.BITOR)) {
				pn = new Binary (parent, pn, BitXorExpr (parent, in_for_init), JSToken.BitwiseOr);
			}
			return pn;
		}

		AST BitXorExpr (AST parent, bool in_for_init)
		{
			AST pn = BitAndExpr (parent, in_for_init);
			while (ts.MatchToken (Token.BITXOR))
				pn = new Binary (parent, pn, BitAndExpr (parent, in_for_init), JSToken.BitwiseXor);
			return pn;
		}

		AST BitAndExpr (AST parent, bool in_for_init)
		{
			AST pn = EqExpr (parent, in_for_init);
			while (ts.MatchToken (Token.BITAND))
				pn = new Binary (parent, pn, EqExpr (parent, in_for_init), JSToken.BitwiseAnd);
			return pn;
		}

		AST EqExpr (AST parent, bool in_for_init)
		{
			AST pn = RelExpr (parent, in_for_init);
			for (;;) {
				int tt = ts.PeekToken ();
				if (tt == Token.EQ || tt == Token.NE || tt == Token.SHEQ || tt == Token.SHNE) {
					ts.GetToken ();
					pn = new Equality (parent, pn, RelExpr (parent, in_for_init), JSToken.Equal);
					continue;
				}
				break;
			}
			return pn;
		}

		AST RelExpr (AST parent, bool in_for_init)
		{	
			AST pn = ShiftExpr (parent);
			for (;;) {
				int tt = ts.PeekToken ();
				if (tt == Token.IN) {
					if (in_for_init)
						break;
				} else if (tt == Token.INSTANCEOF || tt == Token.LE || tt == Token.LT || tt == Token.GE || tt == Token.GT) {
					ts.GetToken ();
					pn = new Relational (parent, pn, ShiftExpr (parent), JSToken.LessThan);
					continue;
				}
				break;
			}
			return pn;
		}

		AST ShiftExpr (AST parent)
		{
			AST pn = AddExpr (parent);
			for (;;) {
				int tt = ts.PeekToken ();
				if (tt == Token.LSH || tt == Token.URSH || tt == Token.RSH) {
					ts.GetToken ();
					pn = new Binary (parent, pn, AddExpr (parent), JSToken.LeftShift);
					continue;
				}
				break;
			}
			return pn;
		}

		AST AddExpr (AST parent)
		{
			AST pn = MulExpr (parent);
			for (;;) {
				int tt = ts.PeekToken ();
				if (tt == Token.ADD || tt == Token.SUB) {
					ts.GetToken ();
					pn = new Binary (parent, pn, MulExpr (parent), JSToken.Plus);
					continue;
				}
				break;
			}
			return pn;
		}

		AST MulExpr (AST parent)
		{
			AST pn = UnaryExpr (parent);
			for (;;) {
				int tt = ts.PeekToken ();
				if (tt == Token.MUL || tt == Token.DIV || tt == Token.MOD) {
					ts.GetToken ();					
					pn = new Binary (parent, pn, UnaryExpr (parent), JSToken.Multiply);
					continue;
				}
				break;
			}
			return pn;
		}
		
		AST UnaryExpr (AST parent)
		{
			int tt;

			ts.allow_reg_exp = true;
			tt = ts.GetToken ();
			ts.allow_reg_exp = false;

			if (tt == Token.VOID || tt == Token.NOT || tt == Token.BITNOT || tt == Token.TYPEOF) {
				// FIXME, set to proper operator
				return new Unary (parent, UnaryExpr (parent), JSToken.Void);
			} else if (tt == Token.ADD) {
				return new Unary (parent, UnaryExpr (parent), JSToken.Plus);
			} else if (tt == Token.SUB) {
				return new Unary (parent, UnaryExpr (parent), JSToken.Minus);
			} else if (tt == Token.INC || tt == Token.DEC) {
				// FIXME, set to proper incr/decr
				return new PostOrPrefixOperator (null, MemberExpr (parent, true), JSToken.Increment);
			} else if (tt == Token.DELPROP) {
				return new Unary (parent, UnaryExpr (parent), JSToken.Delete);
			} if (tt == Token.ERROR) {
				;
			} else {
				ts.UnGetToken (tt);
				int line_number = ts.LineNumber;
				
				AST pn = MemberExpr (parent, true);
				
				/* don't look across a newline boundary for a postfix incop.

				* the rhino scanner seems to work differently than the js
				* scanner here; in js, it works to have the line number check
				* precede the peekToken calls.  It'd be better if they had
				* similar behavior...
				*/
				int peeked;				
				if (((peeked = ts.PeekToken ()) == Token.INC || peeked == Token.DEC) && ts.LineNumber == line_number) {
					int pf = ts.GetToken ();
					// FIXME, set to proper JSToken.
					return new PostOrPrefixOperator (null, pn, JSToken.Increment);
				}
				return pn;
			}
			return new StringLiteral (null, "Error");  // Only reached on error.  Try to continue.
		}

		void ArgumentList (AST parent, ICallable list) 
		{
			bool matched;
			ts.allow_reg_exp = true;
			matched = ts.MatchToken (Token.RP);
			ts.allow_reg_exp = false;

			if (!matched) {
				do {
					list.AddArg (AssignExpr (parent, false));
				} while (ts.MatchToken (Token.COMMA));				
				MustMatchToken (Token.RP, "msg.no.paren.arg");
			}
		}

		AST MemberExpr (AST parent, bool allow_call_syntax)
		{
			int tt;
			AST pn;

			/* Check for new expressions. */
			ts.allow_reg_exp = true;
			tt = ts.PeekToken ();
			ts.allow_reg_exp = false;
			
			if (tt == Token.NEW) {
				/* Eat the NEW token. */
				ts.GetToken ();

				/* Make a NEW node to append to. */
				pn = new New (parent, MemberExpr (parent, false));

				if (ts.MatchToken (Token.LP))
					ArgumentList (parent, (ICallable) pn);
				
				/* XXX there's a check in the C source against
				 * "too many constructor arguments" - how many
				 * do we claim to support?
				 */

				/* Experimental syntax:  allow an object literal to follow a new expression,
				 * which will mean a kind of anonymous class built with the JavaAdapter.
				 * the object literal will be passed as an additional argument to the constructor.
				 */

				tt = ts.PeekToken ();
				if (tt == Token.LC)
					pn = PrimaryExpr (parent);
			} else
				pn = PrimaryExpr (parent);
			return MemberExprTail (parent, allow_call_syntax, pn);
		}

		AST MemberExprTail (AST parent, bool allow_call_syntax, AST pn)
		{
			int tt;

			while ((tt = ts.GetToken ()) > Token.EOF) {
				if (tt == Token.DOT) {
					MustMatchToken (Token.NAME, "msg.no.name.after.dot");
					string s = ts.GetString;					
					// FIXME: is 'new Identifier' appropriate here?
					pn = new Binary (null, pn, new Identifier (null, ts.GetString), JSToken.AccessField);
				} else if (tt == Token.LB) {
					// FIXME, 
					pn = new Binary (parent, pn, Expr (parent, false), JSToken.LeftBracket);
					MustMatchToken (Token.RB, "msg.no.bracket.index");
				} else if (allow_call_syntax && tt == Token.LP) {
					/* make a call node */
					pn = new Call (parent, pn);
					
					/* Add the arguments to pn, if any are supplied. */
					ArgumentList (parent, (ICallable) pn);
				} else {
					ts.UnGetToken (tt);
					break;
				}
			}
			return pn;
		}

		AST PrimaryExpr (AST parent)
		{
			int tt;
			AST pn;

			ts.allow_reg_exp = true;
			tt = ts.GetToken ();
			ts.allow_reg_exp = false;

			if (tt == Token.FUNCTION) {
				return Function (parent, FunctionType.Expression);
			} else if (tt == Token.LB) {
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
						elems.Add (AssignExpr (parent, false));
						after_lb_or_comma = false;
					}
				}
				// FIXME: pass a real Context
				return new ArrayLiteral (null, elems);
			} else if (tt == Token.LC) {
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
							property = new ObjectLiteralItem (s);
						} else if (tt == Token.NUMBER) {
							double n = ts.GetNumber;
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
						property.exp = AssignExpr (parent, false);
						elems.Add (property);
					} while (ts.MatchToken (Token.COMMA));
					MustMatchToken (Token.RC, "msg.no.brace.prop");
					}
					leave_commaloop:
					;
				}
				return new ObjectLiteral (elems);
			} else if (tt == Token.LP) {
				pn = Expr (parent, false);
				MustMatchToken (Token.RP, "msg.no.paren");
				return pn;
			} else if (tt == Token.NAME) {
				string name = ts.GetString;
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
				return new RegExpLiteral (re, flags);
			} else if (tt == Token.NULL) {
				// FIXME, build the null object;
				return null;
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
			return null; // should never reach here
		}		
	}
}
