//
// Parser.cs: Port of Mozilla's Rhino parser.
//	      This class implements the JScript parser.
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
 * Mike Ang
 * Igor Bukanov
 * Ethan Hugg
 * Terry Lucas
 * Mike McCabe
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
// Copyright (C) 2005, Novell Inc (http://novell.com)
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
using Microsoft.Vsa;
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

	internal class Location {
		private string source_name;
		private int line_number;

		internal string SourceName {
			get { return source_name; }
		}

		internal int LineNumber {
			get { return line_number; }
		}

		internal Location (string source_name, int line_number)
		{
			this.source_name = source_name;
			this.line_number = line_number;
		}
	}

	internal class Parser {

		TokenStream ts;
		bool ok; // did the parse encounter an error?
		int nesting_of_function;
		int nesting_of_with;
		bool allow_member_expr_as_function_name;
		Decompiler decompiler; 
		ArrayList code_items;

		internal Parser ()
		{
		}
		
		internal Parser (ArrayList code_items)
		{
			this.code_items = code_items;
		}
		
		internal ScriptBlock [] ParseAll ()
		{
			int i = 0, n = code_items.Count;
			ScriptBlock [] blocks = new ScriptBlock [n];

			foreach (VsaCodeItem item in code_items)
				blocks [i++] = Parse (item.SourceText, item.Name, 0);

			return blocks;
		}

		internal Decompiler CreateDecompiler ()
		{
			return new Decompiler ();
		}

		/// <summary>
		/// Test if n is between the range stablished by min and max
		/// </summary>
		private bool InRangeOf (double n, double min, double max)
		{
			return min <= n && n <= max;
		}

		private bool HasNoDecimals (double v)
		{
			return Math.Round (v) == v;
		}

		/// <summary>
		///   Build a parse tree from a given source_string
		/// </summary>
		///
		/// <remarks>
		///   return an ScriptBlock representing the parsed program
		///   that corresponds to a source file.
		///   If the parse fails, null will be returned.
		/// </remarks>
		internal ScriptBlock Parse (string source_string, string source_location, int line_number)
		{
			ts = new TokenStream (null, source_string, source_location, line_number);
			try {
				return Parse ();
			} catch (IOException) {
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
		internal AST Parse (StreamReader source_reader, string source_location, int line_number)
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
	
		ScriptBlock Parse ()
		{
			decompiler = CreateDecompiler ();
			ScriptBlock current_script_or_fn = new ScriptBlock (new Location (ts.SourceName, ts.LineNumber));
			decompiler.GetCurrentOffset ();
			decompiler.AddToken (Token.SCRIPT);
			ok = true;

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
							n = Function (current_script_or_fn, FunctionType.Statement);
						} catch (ParserException) {
							ok = false;
							break;
						}
					} else {
						ts.UnGetToken (tt);
						n = Statement (current_script_or_fn);
					}
					current_script_or_fn.Add (n);
				}
			} catch (StackOverflowException) {
				throw new Exception ("Error: too deep parser recursion.");
			}

			if (!ok)
				return null;

			this.decompiler = null; // It helps GC
			return current_script_or_fn;
		}

		internal bool Eof {
			get { return ts.EOF; }
		}

		bool InsideFunction {
			get { return nesting_of_function != 0; }
		}

		Block ParseFunctionBody (AST parent)
		{
			++nesting_of_function;
			Block pn = new Block (parent, new Location (ts.SourceName, ts.LineNumber));

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
			} catch (ParserException) {
				ok = false;
			} finally {
				--nesting_of_function;
			}
			return pn;
		}

		AST Function (AST parent, FunctionType ft)
		{
			FunctionType synthetic_type = ft;
			string name;
			AST member_expr = null;

			if (ts.MatchToken (Token.NAME)) {
				name = ts.GetString;
				if (!ts.MatchToken (Token.LP)) {
					if (allow_member_expr_as_function_name) {
						// Extension to ECMA: if 'function <name>' does not follow
						// by '(', assume <name> starts memberExpr
						// FIXME: is StringLiteral the correct AST to build?
						decompiler.AddName (name);
						AST member_expr_head = new StringLiteral (null, name, 
									  new Location (ts.SourceName, ts.LineNumber));
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

			if (member_expr != null) {
				synthetic_type = FunctionType.Expression;
				decompiler.AddToken (Token.ASSIGN);
			}
			
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

			// FIXME: which is old version of Decompiler.MarkFunctionStart
			int functionSourceStart = decompiler.MarkFunctionStart ((int) synthetic_type);

			if (name != "")
				decompiler.AddName (name);

			int saved_nesting_of_with = nesting_of_with;
			nesting_of_with = 0;

			FormalParameterList _params = new FormalParameterList (new Location (ts.SourceName, ts.LineNumber));
			Block body;

			try {
				decompiler.AddToken (Token.LP);
				if (!ts.MatchToken (Token.RP)) {
					bool first = true;
					do {
						if (!first)
							decompiler.AddToken (Token.COMMA);
						first = false;
						MustMatchToken (Token.NAME, "msg.no.parm");
						string s = ts.GetString;
						_params.Add (s, String.Empty, new Location (ts.SourceName, ts.LineNumber));
						decompiler.AddName (s);
					} while (ts.MatchToken (Token.COMMA));
					MustMatchToken (Token.RP, "msg.no.paren.after.parms");
				}
				decompiler.AddToken (Token.RP);

				MustMatchToken (Token.LC, "msg.no.brace.body");
				decompiler.AddEOL (Token.LC);
				body = ParseFunctionBody (fn);
				MustMatchToken (Token.RC, "msg.no.brace.after.body");

				decompiler.AddToken (Token.RC);
				decompiler.MarkFunctionEnd (functionSourceStart);

				fn.func_obj.source = decompiler.SourceToString (functionSourceStart);

				if (ft != FunctionType.Expression) {
					CheckWellTerminatedFunction ();
					if (member_expr == null)
						decompiler.AddToken (Token.EOL);
					else
						decompiler.AddEOL (Token.SEMI);
				}
			} finally {
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
				Assign assign = new Assign (null, JSToken.Assign, new Location (ts.SourceName, ts.LineNumber));
				assign.Init (member_expr, pn, false);
				pn = assign;

#if false
				// FIXME, research about createExprStatement
				if (ft != FunctionType.Expression)
					;
#endif
			}
			return pn;
		}

		Function CreateFunction (AST parent, FunctionType func_type, string name)
		{
			Function func;
			Location location = new Location (ts.SourceName, ts.LineNumber);

			if (func_type == FunctionType.Statement)
				func = new FunctionDeclaration (parent, name, location);
			else if (func_type == FunctionType.Expression)
				func = new FunctionExpression (parent, name, location);
			else if (func_type == FunctionType.ExpressionStatement)
				throw new NotImplementedException ();
			else
				throw new Exception ("Unknown FunctionType");
			return func;
		}

		AST Statements (AST parent)
		{
			int tt;
			Block pn = new Block (parent, new Location (ts.SourceName, ts.LineNumber));
			while ((tt = ts.PeekToken ()) > Token.EOF && tt != Token.RC)
				pn.Add (Statement (pn));
			return pn;
		}

		AST Condition (AST parent)
		{
			AST pn;
			MustMatchToken (Token.LP, "msg.no.paren.cond");
			decompiler.AddToken (Token.LP);
			pn = Expr (parent, false);
			MustMatchToken (Token.RP, "msg.no.paren.after.cond");
			decompiler.AddToken (Token.RP);
			return pn;
		}

		AST Import (AST parent)
		{
			System.Text.StringBuilder @namespace = new System.Text.StringBuilder ();

			while (true) {
				MustMatchToken (Token.NAME, "msg.bad.namespace.name");
				@namespace.Append (ts.GetString);
				if (ts.MatchToken (Token.DOT))
					@namespace.Append (".");
				else
					break;
			}
			return new Import (parent, @namespace.ToString (), new Location (ts.SourceName, ts.LineNumber));
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
			} catch (ParserException) {
				// skip to end of statement
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
				decompiler.AddToken (Token.IF);
				AST cond = Condition (parent);

				decompiler.AddEOL (Token.LC);

				AST if_true = Statement (parent);
				AST if_false = null;

				if (ts.MatchToken (Token.ELSE)) {
					decompiler.AddToken (Token.RC);
					decompiler.AddToken (Token.ELSE);
					decompiler.AddEOL (Token.LC);
					if_false = Statement (parent);
				}
				decompiler.AddEOL (Token.RC);
				pn = new If (parent, cond, if_true, if_false, new Location (ts.SourceName, ts.LineNumber));
			} else if (tt == Token.SWITCH) {
				skip_semi = true;

				decompiler.AddToken (Token.SWITCH);

				pn = new Switch (parent, new Location (ts.SourceName, ts.LineNumber));
				Clause cur_case;
				MustMatchToken (Token.LP, "msg.no.paren.switch");

				decompiler.AddToken (Token.LP);

				((Switch) pn).exp = Expr (parent, false);
				MustMatchToken (Token.RP, "msg.no.paren.after.switch");

				decompiler.AddToken (Token.RP);

				MustMatchToken (Token.LC, "msg.no.brace.switch");

				decompiler.AddEOL (Token.LC);

				ClauseType clause_type = ClauseType.Case;

				while ((tt = ts.GetToken ()) != Token.RC && tt != Token.EOF) {
					if (tt == Token.CASE) {
						decompiler.AddToken (Token.CASE);
						cur_case = new Clause (pn, new Location (ts.SourceName, ts.LineNumber));
						cur_case.exp = Expr (pn, false);
						decompiler.AddEOL (Token.COLON);
						if (clause_type == ClauseType.Default)
							clause_type = ClauseType.CaseAfterDefault;
					} else if (tt == Token.DEFAULT) {
						cur_case = null;
						clause_type = ClauseType.Default;
						decompiler.AddToken (Token.DEFAULT);
						decompiler.AddEOL (Token.COLON);
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
				decompiler.AddEOL (Token.RC);
			} else if (tt == Token.WHILE) {
				skip_semi = true;
				decompiler.AddToken (Token.WHILE);
				While w = new While (new Location (ts.SourceName, ts.LineNumber));
				AST cond = Condition (w);
				decompiler.AddEOL (Token.LC);
				AST body = Statement (w);
				decompiler.AddEOL (Token.RC);
				w.Init (parent, cond, body);
				pn = w;
			} else if (tt == Token.DO) {
				decompiler.AddToken (Token.DO);
				decompiler.AddEOL (Token.LC);
				int line_number = ts.LineNumber;
				DoWhile do_while = new DoWhile (new Location (ts.SourceName, line_number));
				AST body = Statement (do_while);
				decompiler.AddToken (Token.RC);
				MustMatchToken (Token.WHILE, "msg.no.while.do");
				decompiler.AddToken (Token.WHILE);
				AST cond = Condition (do_while);
				do_while.Init (parent, body, cond);
				pn  = do_while;
			} else if (tt == Token.FOR) {
				skip_semi = true;
				decompiler.AddToken (Token.FOR);
				AST init, cond, incr = null, body;

				MustMatchToken (Token.LP, "msg.no.paren.for");
				decompiler.AddToken (Token.LP);
				tt = ts.PeekToken ();

				if (tt == Token.SEMI)
					init = new EmptyAST ();
				else {
					if (tt == Token.VAR) {
						// set init to a var list or initial
						ts.GetToken (); // throw away the 'var' token
						init = Variables (parent, true);
					} else
						init = Expr (parent, true);
				}
				
				if (ts.MatchToken (Token.IN)) {
					decompiler.AddToken (Token.IN);
					cond = Expr (parent, false); // 'cond' is the object over which we're iterating
				} else { 
					// ordinary for loop
					MustMatchToken (Token.SEMI, "msg.no.semi.for");
					decompiler.AddToken (Token.SEMI);
					
					if (ts.PeekToken () == Token.SEMI)
						cond = new EmptyAST (); // no loop condition
					else
						cond = Expr (parent, false);

					MustMatchToken (Token.SEMI, "msg.no.semi.for.cond");
					decompiler.AddToken (Token.SEMI);

					if (ts.PeekToken () == Token.RP)
						incr = new EmptyAST ();
					else
						incr = Expr (parent, false);
				}

				MustMatchToken (Token.RP, "msg.no.paren.for.ctrl");
				decompiler.AddToken (Token.RP);
				decompiler.AddEOL (Token.LC);
				body = Statement (pn);
				decompiler.AddEOL (Token.RC);

				if (incr == null) // cond could be null if 'in obj' got eaten by the init node. 
					pn = new ForIn (parent, init, cond, body, new Location (ts.SourceName, ts.LineNumber));
				else
					pn = new For (parent, init, cond, incr, body, new Location (ts.SourceName, ts.LineNumber));
				body.PropagateParent (pn);
			} else if (tt == Token.TRY) {
				int line_number = ts.LineNumber;
				AST try_block;
				ArrayList catch_blocks = null;
				AST finally_block = null;

				skip_semi = true;
				decompiler.AddToken (Token.TRY);
				decompiler.AddEOL (Token.LC);

				try_block = Statement (parent);
				decompiler.AddEOL (Token.RC);
				catch_blocks = new ArrayList ();

				bool saw_default_catch = false;
				int peek = ts.PeekToken ();

				if (peek == Token.CATCH) {
					while (ts.MatchToken (Token.CATCH)) {
						if (saw_default_catch)
							ReportError ("msg.catch.unreachable");
						decompiler.AddToken (Token.CATCH);
						MustMatchToken (Token.LP, "msg.no.paren.catch");
						decompiler.AddToken (Token.LP);
						MustMatchToken (Token.NAME, "msg.bad.catchcond");
						string var_name = ts.GetString;
						decompiler.AddName (var_name);
						AST catch_cond = null;
						
						if (ts.MatchToken (Token.IF)) {
							decompiler.AddToken (Token.IF);
							catch_cond = Expr (parent, false);
						} else
							saw_default_catch = true;
						
						MustMatchToken (Token.RP, "msg.bad.catchcond");
						decompiler.AddToken (Token.RP);
						MustMatchToken (Token.LC, "msg.no.brace.catchblock");
						decompiler.AddEOL (Token.LC);

						catch_blocks.Add (new Catch (var_name, catch_cond, 
									     Statements (null), parent, new Location (ts.SourceName, line_number)));
						MustMatchToken (Token.RC, "msg.no.brace.after.body");
						decompiler.AddEOL (Token.RC);
					}
				} else if (peek != Token.FINALLY)
					MustMatchToken (Token.FINALLY, "msg.try.no.catchfinally");
				
				if (ts.MatchToken (Token.FINALLY)) {
					decompiler.AddToken (Token.FINALLY);
					decompiler.AddEOL (Token.LC);
					finally_block = Statement (parent);
					decompiler.AddEOL (Token.RC);
				}
				pn = new Try (try_block, catch_blocks, finally_block, parent, new Location (ts.SourceName, ts.LineNumber));
			} else if (tt == Token.THROW) {
				int line_number = ts.LineNumber;
				decompiler.AddToken (Token.THROW);
				pn = new Throw (Expr (parent, false), new Location (ts.SourceName, ts.LineNumber));

				if (line_number == ts.LineNumber)
					CheckWellTerminated ();
			} else if (tt == Token.BREAK) {
				decompiler.AddToken (Token.BREAK);

				// MatchLabel only matches if there is one
				string label = MatchLabel ();

				if (label != null)
					decompiler.AddName (label);

				pn = new Break (parent, label, new Location (ts.SourceName, ts.LineNumber));
			} else if (tt == Token.CONTINUE) {
				decompiler.AddToken (Token.CONTINUE);

				// MatchLabel only matches if there is one
				string label = MatchLabel ();

				if (label != null)
					decompiler.AddName (label);

				pn = new Continue (parent, label, new Location (ts.SourceName, ts.LineNumber));
			} else if (tt == Token.WITH) {
				skip_semi = true;
				decompiler.AddToken (Token.WITH);
				MustMatchToken (Token.LP, "msg.no.paren.with");
				decompiler.AddToken (Token.LP);
				AST obj = Expr (parent, false);
				MustMatchToken (Token.RP, "msg.no.paren.after.with");
				decompiler.AddToken (Token.RP);
				decompiler.AddToken (Token.LC);
				++nesting_of_with;
				AST body;
				try {
					body = Statement (parent);
				} finally {
					--nesting_of_with;
				}
				decompiler.AddEOL (Token.RC);
				pn = new With (parent, obj, body, new Location (ts.SourceName, ts.LineNumber));
			} else if (tt == Token.VAR) {
				int line_number = ts.LineNumber;
				pn = Variables (parent, false);
				if (ts.LineNumber == line_number)
					CheckWellTerminated ();
			} else if (tt == Token.RETURN) {
				AST ret_expr = null;
				decompiler.AddToken (Token.RETURN);
				pn = new Return (new Location (ts.SourceName, ts.LineNumber));

				if (!InsideFunction)
					ReportError ("msg.bad.return");
				
				/* This is ugly, but we don't want to require a semicolon. */
				ts.allow_reg_exp = true;
				tt = ts.PeekTokenSameLine ();
				ts.allow_reg_exp = false;
				
				int line_number = ts.LineNumber;
				if (tt != Token.EOF && tt != Token.EOL && tt != Token.SEMI && tt != Token.RC) {
					ret_expr = Expr (pn, false);
					if (ts.LineNumber == line_number)
						CheckWellTerminated ();
				}
				((Return) pn).Init (parent, ret_expr);
			} else if (tt == Token.LC) { 
				skip_semi = true;
				pn = Statements (parent);
				MustMatchToken (Token.RC, "msg.no.brace.block");
			} else if (tt == Token.ERROR || tt == Token.EOL || tt == Token.SEMI) {
 				pn = new EmptyAST ();
				skip_semi = true;
			} else if (tt == Token.FUNCTION) {
				pn = Function (parent, FunctionType.ExpressionStatement);
			} else if (tt == Token.IMPORT) {
				decompiler.AddToken (Token.IMPORT);
				pn = Import (parent);
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

					// bind 'Statement (pn)' to the label
					Labelled labelled = new Labelled (parent, new Location (ts.SourceName, ts.LineNumber));
					labelled.Init (parent, name, Statement (labelled), new Location (ts.SourceName, ts.LineNumber));
					pn = labelled;
					// depend on decompiling lookahead to guess that that
					// last name was a label.
					decompiler.AddEOL (Token.COLON);
					return pn;
				}
				// FIXME:
				// pn = nf.createExprStatement(pn, lineno);
				if (ts.LineNumber == line_number)
					CheckWellTerminated ();
			}
			ts.MatchToken (Token.SEMI);

			if (!skip_semi)
				decompiler.AddEOL (Token.SEMI);

			return pn;
		}
		
		AST Variables (AST parent, bool in_for_init)
		{
			VariableStatement pn = new VariableStatement (parent, new Location (ts.SourceName, ts.LineNumber));
			bool first = true;
			decompiler.AddToken (Token.VAR);

			for (;;) {
				VariableDeclaration name;
				AST init = null;
				MustMatchToken (Token.NAME, "msg.bad.var");
				string s = ts.GetString;

				if (!first)
					decompiler.AddToken (Token.COMMA);
				first = false;
				
				decompiler.AddName (s);
				name = new VariableDeclaration (parent, s, null, null, new Location (ts.SourceName, ts.LineNumber));

				// ommited check for argument hiding				
				if (ts.MatchToken (Token.ASSIGN)) {
					decompiler.AddToken (Token.ASSIGN);
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
			Expression pn = new Expression (parent, new Location (ts.SourceName, ts.LineNumber));
			AST init = AssignExpr (parent, in_for_init);
			pn.Add (init);

			if (init == null)
				throw new Exception ("Expr, L680, AST is null");
			
			while (ts.MatchToken (Token.COMMA)) {
				decompiler.AddToken (Token.COMMA);
				pn.Add (AssignExpr (parent, in_for_init));
			}
			return pn;
		}

		AST AssignExpr (AST parent, bool in_for_init)
		{
			AST pn = CondExpr (parent, in_for_init);
			int tt = ts.PeekToken ();

			// omitted: "invalid assignment left-hand side" check.
			if (tt == Token.ASSIGN) {
				ts.GetToken ();
				decompiler.AddToken (Token.ASSIGN);
				Assign assign = new Assign (parent, JSToken.Assign, new Location (ts.SourceName, ts.LineNumber));
				assign.Init (pn, AssignExpr (assign, in_for_init), false);
				pn = assign;
				return pn;
			} else if (tt == Token.ASSIGNOP) {
				ts.GetToken ();
				int op = ts.GetOp ();
				decompiler.AddAssignOp (op);
				Assign assign = new Assign (parent, ToJSToken (op, tt), new Location (ts.SourceName, ts.LineNumber));
				assign.Init (pn, AssignExpr (assign, in_for_init), false);
				pn = assign;
			}
			return pn;
		}

		AST CondExpr (AST parent, bool in_for_init)
		{
			AST if_true;
			AST if_false;
			AST pn = OrExpr (parent, in_for_init);

			if (ts.MatchToken (Token.HOOK)) {
				decompiler.AddToken (Token.HOOK);
				if_true = AssignExpr (parent, false);
				MustMatchToken (Token.COLON, "msg.no.colon.cond");
				decompiler.AddToken (Token.COLON);
				if_false = AssignExpr (parent, in_for_init);
				return new Conditional (parent, pn, if_true, if_false, new Location (ts.SourceName, ts.LineNumber));
			}
			return pn;
		}

		AST OrExpr (AST parent, bool in_for_init)
		{
			AST pn = AndExpr (parent, in_for_init);
			if (ts.MatchToken (Token.OR)) {
				decompiler.AddToken (Token.OR);
				return new Binary (parent, pn, OrExpr (parent, in_for_init), JSToken.LogicalOr, 
					   new Location (ts.SourceName, ts.LineNumber));
			}
			return pn;
		}

		AST AndExpr (AST parent, bool in_for_init)
		{
			AST pn = BitOrExpr (parent, in_for_init);
			if (ts.MatchToken (Token.AND)) {
				decompiler.AddToken (Token.AND);
				return new Binary (parent, pn, AndExpr (parent, in_for_init), JSToken.LogicalAnd,
					   new Location (ts.SourceName, ts.LineNumber));
			}
			return pn;
		}

		AST BitOrExpr (AST parent, bool in_for_init)
		{
			AST pn = BitXorExpr (parent, in_for_init);
			while (ts.MatchToken (Token.BITOR)) {
				decompiler.AddToken (Token.BITOR);
				pn = new Binary (parent, pn, BitXorExpr (parent, in_for_init), JSToken.BitwiseOr,
					 new Location (ts.SourceName, ts.LineNumber));
			}
			return pn;
		}

		AST BitXorExpr (AST parent, bool in_for_init)
		{
			AST pn = BitAndExpr (parent, in_for_init);
			while (ts.MatchToken (Token.BITXOR)) {
				decompiler.AddToken (Token.BITXOR);
				pn = new Binary (parent, pn, BitAndExpr (parent, in_for_init), JSToken.BitwiseXor,
					 new Location (ts.SourceName, ts.LineNumber));
			}
			return pn;
		}

		AST BitAndExpr (AST parent, bool in_for_init)
		{
			AST pn = EqExpr (parent, in_for_init);
			while (ts.MatchToken (Token.BITAND)) {
				decompiler.AddToken (Token.BITAND);
				pn = new Binary (parent, pn, EqExpr (parent, in_for_init), JSToken.BitwiseAnd,
					 new Location (ts.SourceName, ts.LineNumber));
			}
			return pn;
		}

		AST EqExpr (AST parent, bool in_for_init)
		{
			AST pn = RelExpr (parent, in_for_init);
			ArrayList tokens = new ArrayList ();
			for (;;) {
				int tt = ts.PeekToken ();
				tokens.Add (tt);
				if (tt == Token.EQ || tt == Token.NE) {
					foreach (int token in tokens)
						decompiler.AddToken (token);
					tokens.Clear ();

					ts.GetToken ();
					pn = new Equality (parent, pn, RelExpr (parent, in_for_init), ToJSToken (tt), 
						   new Location (ts.SourceName, ts.LineNumber));
					continue;
				} else if (tt == Token.SHEQ || tt == Token.SHNE) {
					foreach (int token in tokens)
						decompiler.AddToken (token);
					tokens.Clear ();
					
					ts.GetToken ();
					pn = new StrictEquality (parent, pn, RelExpr (parent, in_for_init), ToJSToken (tt), new Location (ts.SourceName, ts.LineNumber));
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
					else {
						ts.GetToken ();
						decompiler.AddToken (tt);
						pn = new Relational (parent, pn, ShiftExpr (parent), ToJSToken (tt), new Location (ts.SourceName, ts.LineNumber));
						continue;
					}
				} else if (tt == Token.INSTANCEOF || tt == Token.LE || tt == Token.LT || tt == Token.GE || tt == Token.GT) {
					ts.GetToken ();
					decompiler.AddToken (tt);
					pn = new Relational (parent, pn, ShiftExpr (parent), ToJSToken (tt), new Location (ts.SourceName, ts.LineNumber));
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
					decompiler.AddToken (tt);
					
					JSToken op = JSToken.LeftShift;
					if (tt == Token.RSH)
						op = JSToken.RightShift;
					else if (tt == Token.URSH)
						op = JSToken.UnsignedRightShift;

					pn = new Binary (parent, pn, AddExpr (parent), op,
						 new Location (ts.SourceName, ts.LineNumber));
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
					decompiler.AddToken (tt);
					pn = new Binary (parent, pn, MulExpr (parent), ToJSToken (tt),
						 new Location (ts.SourceName, ts.LineNumber));
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
					decompiler.AddToken (tt);
					pn = new Binary (parent, pn, UnaryExpr (parent), ToJSToken (tt),
						 new Location (ts.SourceName, ts.LineNumber));
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

			if (tt == Token.VOID || tt == Token.NOT || tt == Token.BITNOT || tt == Token.TYPEOF ||
			    tt == Token.ADD || tt == Token.SUB || tt == Token.DELPROP) {
				if (tt == Token.VOID || tt == Token.NOT || tt == Token.BITNOT || tt == Token.TYPEOF)
					decompiler.AddToken (tt);
				else if (tt == Token.ADD)
					decompiler.AddToken (Token.POS);
				else if (tt == Token.SUB)
					decompiler.AddToken (Token.NEG);
				else 
					decompiler.AddToken (tt);

				Unary u = new Unary (parent, ToJSToken (tt), new Location (ts.SourceName, ts.LineNumber));
				u.operand = UnaryExpr (u);
				return u;
			} else if (tt == Token.INC || tt == Token.DEC) {
				decompiler.AddToken (tt);
				return new PostOrPrefixOperator (parent, MemberExpr (parent, true), ToJSToken (tt), true,
							 new Location (ts.SourceName, ts.LineNumber));
			} else if (tt == Token.ERROR) {
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
					decompiler.AddToken (pf);
					return new PostOrPrefixOperator (parent, pn, ToJSToken (peeked), false,
								 new Location (ts.SourceName, ts.LineNumber));
				}
				return pn;
			}
			return new StringLiteral (null, "Error", new Location (ts.SourceName, ts.LineNumber));  // Only reached on error.  Try to continue.
		}

		JSToken ToJSToken (int tt)
		{
			if (tt == Token.DELPROP)
				return JSToken.Delete;
			else if (tt == Token.VOID)
				return JSToken.Void;
			else if (tt == Token.TYPEOF)
				return JSToken.Typeof;
			else if (tt == Token.INC)
				return JSToken.Increment;
			else if (tt == Token.DEC)
				return JSToken.Decrement;
			else if (tt == Token.ADD)
				return JSToken.Plus;
			else if (tt == Token.SUB)
				return JSToken.Minus;
			else if (tt == Token.BITNOT)
				return JSToken.BitwiseNot;
			else if (tt == Token.NOT)
				return JSToken.LogicalNot;
			else if (tt == Token.EQ)
				return JSToken.Equal;
			else if (tt == Token.NE)
				return JSToken.NotEqual;
			else if (tt == Token.SHEQ)
				return JSToken.StrictEqual;
			else if (tt == Token.SHNE)
				return JSToken.StrictNotEqual;
			else if (tt == Token.MUL)
				return JSToken.Multiply;
			else if (tt == Token.DIV)
				return JSToken.Divide;
			else if (tt == Token.MOD)
				return JSToken.Modulo;
			else if (tt == Token.IN)
				return JSToken.In;
			else if (tt == Token.INSTANCEOF)
				return JSToken.Instanceof;
			else if (tt == Token.LE)
				return JSToken.LessThanEqual;
			else if (tt == Token.LT)
				return JSToken.LessThan;
			else if (tt == Token.GE)
				return JSToken.GreaterThanEqual;
			else if (tt == Token.GT)
				return JSToken.GreaterThan;
			else
				throw new NotImplementedException ();
		}

		//
		// Takes care of +=, -=
		//
		JSToken ToJSToken (int left, int right)
		{
			if (right == Token.ASSIGNOP) {
				if  (left == Token.ADD)
					return JSToken.PlusAssign;
				else if (left == Token.SUB)
					return JSToken.MinusAssign;
				else if (left == Token.MUL)
					return JSToken.MultiplyAssign;
				else if (left == Token.DIV)
					return JSToken.DivideAssign;
				else if (left == Token.BITAND)
					return JSToken.BitwiseAndAssign;
				else if (left == Token.BITOR)
					return JSToken.BitwiseOrAssign;
				else if (left == Token.BITXOR)
					return JSToken.BitwiseXorAssign;
				else if (left == Token.MOD)
					return JSToken.ModuloAssign;
				else if (left == Token.LSH)
					return JSToken.LeftShiftAssign;
				else if (left == Token.RSH)
					return JSToken.RightShiftAssign;
				else if (left == Token.URSH)
					return JSToken.UnsignedRightShiftAssign;
			}
			throw new NotImplementedException ();
		}

		void ArgumentList (AST parent, ICallable list) 
		{
			bool matched;
			ts.allow_reg_exp = true;
			matched = ts.MatchToken (Token.RP);
			ts.allow_reg_exp = false;

			if (!matched) {
				bool first = true;
				do {
					if (!first)
						decompiler.AddToken (Token.COMMA);
					first = false;
					list.AddArg (AssignExpr (parent, false));
				} while (ts.MatchToken (Token.COMMA));				
				MustMatchToken (Token.RP, "msg.no.paren.arg");
			}
			decompiler.AddToken (Token.RP);
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
				decompiler.AddToken (Token.NEW);

				/* Make a NEW node to append to. */
				pn = new New (parent, MemberExpr (parent, false), new Location (ts.SourceName, ts.LineNumber));

				if (ts.MatchToken (Token.LP)) {
					decompiler.AddToken (Token.LP);
					ArgumentList (parent, (ICallable) pn);
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
				if (tt == Token.LC)
					pn = PrimaryExpr (parent);
			} else
				pn = PrimaryExpr (parent);
			return MemberExprTail (pn, allow_call_syntax, pn);
		}

		AST MemberExprTail (AST parent, bool allow_call_syntax, AST pn)
		{
			int tt;

			while ((tt = ts.GetToken ()) > Token.EOF) {
				if (tt == Token.DOT) {
					decompiler.AddToken (Token.DOT);
					MustMatchToken (Token.NAME, "msg.no.name.after.dot");
					string s = ts.GetString;
					decompiler.AddName (s);
					// FIXME: is 'new Identifier' appropriate here?
					pn = new Binary (parent, pn, 
						 new Identifier (parent, ts.GetString, new Location (ts.SourceName, ts.LineNumber)),
						 JSToken.AccessField, new Location (ts.SourceName, ts.LineNumber));
				} else if (tt == Token.LB) {
					decompiler.AddToken (Token.LB);
					Binary b = new Binary (parent, pn, JSToken.LeftBracket, 
						       new Location (ts.SourceName, ts.LineNumber));
					b.right = Expr (b, false);
					pn = b;
					MustMatchToken (Token.RB, "msg.no.bracket.index");
					decompiler.AddToken (Token.RB);
				} else if (allow_call_syntax && tt == Token.LP) {
					/* make a call node */
					decompiler.AddToken (Token.LP);
					pn = new Call (parent, pn, new Location (ts.SourceName, ts.LineNumber));
					
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
				ASTList elems = new ASTList (parent, new Location (ts.SourceName, ts.LineNumber));
				int skip_count = 0;
				decompiler.AddToken (Token.LB);
				bool after_lb_or_comma = true;
				for (;;) {
					ts.allow_reg_exp = true;
					tt = ts.PeekToken ();
					ts.allow_reg_exp = false;
					
					if (tt == Token.COMMA) {
						ts.GetToken ();
						decompiler.AddToken (Token.COMMA);
						if (!after_lb_or_comma) 
							after_lb_or_comma = true;
						else {
							elems.Add (null);
							++skip_count;
						}
					} else if (tt == Token.RB) {
						ts.GetToken ();
						decompiler.AddToken (Token.RB);
						break;
					} else {
						if (!after_lb_or_comma) 
							ReportError ("msg.no.bracket.arg");
						elems.Add (AssignExpr (parent, false));
						after_lb_or_comma = false;
					}
				}
				// FIXME: pass a real Context
				return new ArrayLiteral (null, elems, skip_count, new Location (ts.SourceName, ts.LineNumber));
			} else if (tt == Token.LC) {
				Location location = new Location (ts.SourceName, ts.LineNumber);
				ArrayList elems = new ArrayList ();
				decompiler.AddToken (Token.LC);

				if (!ts.MatchToken (Token.RC)) {
					bool first = true;
					
					commaloop: {
					do {
						ObjectLiteralItem property;

						if (!first)
							decompiler.AddToken (Token.COMMA);
						else
							first = false;
						
						tt = ts.GetToken ();
						
						if (tt == Token.NAME || tt == Token.STRING) {
							string s = ts.GetString;
							if (tt == Token.NAME)
								decompiler.AddName (s);
							else
								decompiler.AddString (s);
							property = new ObjectLiteralItem (s);
						} else if (tt == Token.NUMBER) {
							double n = ts.GetNumber;
							decompiler.AddNumber (n);
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
						// OBJLIT is used as ':' in object literal for
						// decompilation to solve spacing ambiguity.
						decompiler.AddToken (Token.OBJECTLIT);
						property.exp = AssignExpr (parent, false);
						elems.Add (property);
					} while (ts.MatchToken (Token.COMMA));
					MustMatchToken (Token.RC, "msg.no.brace.prop");
					}
					leave_commaloop:
					;
				}
				return new ObjectLiteral (elems, location);
			} else if (tt == Token.LP) {
				decompiler.AddToken (Token.LP);
				pn = Expr (parent, false);
				decompiler.AddToken (Token.RP);
				MustMatchToken (Token.RP, "msg.no.paren");
				decompiler.AddToken (Token.RP);
				return pn;
			} else if (tt == Token.NAME) {
				string name = ts.GetString;
				decompiler.AddName (name);
				return new Identifier (parent, name, new Location (ts.SourceName, ts.LineNumber));
			} else if (tt == Token.NUMBER) {
				double n = ts.GetNumber;
				decompiler.AddNumber (n);

				Location location = new Location (ts.SourceName, ts.LineNumber);

				if (HasNoDecimals (n)) {
					if (InRangeOf (n, Byte.MinValue, Byte.MaxValue))
						return new ByteConstant (parent, (byte) n, location);
					else if (InRangeOf (n, Int16.MinValue, Int16.MaxValue))
						return new ShortConstant (parent, (short) n, location);
					else if (InRangeOf (n, Int32.MinValue, Int32.MaxValue))
						return new IntConstant (parent, (int) n, location);
					else if (InRangeOf (n, Int64.MinValue, Int64.MaxValue))
						return new LongConstant (parent, (long) n, location);
					else
						return new DoubleConstant (parent, n, location);
				} else {
					if (InRangeOf (n, Single.MinValue, Single.MaxValue))
						return new FloatConstant (parent, (float) n, location);
					else if (InRangeOf (n, Double.MinValue, Double.MaxValue))
						return new DoubleConstant (parent, n, location);
					else
						return new DoubleConstant (parent, n, location);
				}
			} else if (tt == Token.STRING) {
				string s = ts.GetString;
				decompiler.AddString (s);
				return new StringLiteral (null, s, new Location (ts.SourceName, ts.LineNumber));
			} else if (tt == Token.REGEXP) {
				string flags = ts.reg_exp_flags;
				ts.reg_exp_flags = null;
				string re = ts.GetString;
				decompiler.AddRegexp (re, flags);
				return new RegExpLiteral (parent, re, flags, new Location (ts.SourceName, ts.LineNumber));
			} else if (tt == Token.NULL) {
				decompiler.AddToken (tt);
				// FIXME, build the null object;
				return null;
			} else if (tt ==  Token.THIS) {
				decompiler.AddToken (tt);
				return new This (parent, new Location (ts.SourceName, ts.LineNumber));
			} else if (tt == Token.FALSE || tt == Token.TRUE) {
				decompiler.AddToken (tt);
				bool v;
				if (tt == Token.FALSE)
					v = false;
				else
					v = true;
				return new BooleanConstant (null, v, new Location (ts.SourceName, ts.LineNumber));
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
