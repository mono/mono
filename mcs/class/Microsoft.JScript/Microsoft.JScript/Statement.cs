// 
// Statement.cs:
//
// Author:
//	Cesar Octavio Lopez Nataren
//
// (C) 2003, 2004 Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
// Copyright (C) 2005 Novell Inc (http://novell.com)
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
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections;

namespace Microsoft.JScript {

	internal interface ICanModifyContext {
		//
		// Populate the symbol table before resolving references
		//
		void PopulateContext (Environment env, string ns);
		void EmitDecls (EmitContext ec);
	}

	internal class If : AST, ICanModifyContext {

		internal AST cond, true_stm, false_stm;

		internal If (AST parent, AST condition, AST true_stm, AST false_stm, Location location)
			: base (parent, location)
		{
			this.cond = condition;
			this.true_stm = true_stm;
			this.false_stm = false_stm;
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

			if (cond != null)
				sb.Append (cond.ToString ());
			if (true_stm != null)
				sb.Append (true_stm.ToString ());
			if (false_stm != null)
				sb.Append (false_stm.ToString ());
			
			return sb.ToString ();
		}

		void ICanModifyContext.PopulateContext (Environment env, string ns)
		{
			if (true_stm is ICanModifyContext)
				((ICanModifyContext) true_stm).PopulateContext (env, ns);

			if (false_stm is ICanModifyContext)
				((ICanModifyContext) false_stm).PopulateContext (env, ns);
		}
		
		void ICanModifyContext.EmitDecls (EmitContext ec)
		{
			if (true_stm is ICanModifyContext)
				((ICanModifyContext) true_stm).EmitDecls (ec);

			if (false_stm is ICanModifyContext)
				((ICanModifyContext) false_stm).EmitDecls (ec);
		}

		internal override bool Resolve (Environment env)
		{
			bool r = true;

			if (cond != null)
				if (cond is Exp)
					r &= ((Exp) cond).Resolve (env, false);

			if (true_stm != null)
				if (true_stm is Exp)
					r &= ((Exp) true_stm).Resolve (env, true);
				else
					r &= true_stm.Resolve (env);

			if (false_stm != null)
				if (false_stm is Exp)
					r &= ((Exp) false_stm).Resolve (env, true);
				else
					r &= false_stm.Resolve (env);
			return r;
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Label false_lbl = ig.DefineLabel ();
			Label merge_lbl = ig.DefineLabel ();
			CodeGenerator.fall_true (ec, cond, false_lbl);
			CodeGenerator.EmitBox (ig, cond);
			if (true_stm != null)
				true_stm.Emit (ec);
			ig.Emit (OpCodes.Br, merge_lbl);
			ig.MarkLabel (false_lbl);
			if (false_stm != null)
				false_stm.Emit (ec);			
			ig.MarkLabel (merge_lbl);
		}

		internal override void PropagateParent (AST parent)
		{
			base.PropagateParent (parent);
			true_stm.PropagateParent (this);
			false_stm.PropagateParent (this);
		}
 	}

	abstract class Jump : AST {
		protected string label = String.Empty;
		protected object binding;
		
		internal Jump (AST parent, Location location)
			: base (parent, location)
		{
		}

		bool IsLabel (object binding)
		{
			return binding.GetType () == typeof (Labelled);
		}		

		protected bool ValidLabel ()
		{
			binding = SemanticAnalyser.GetLabel (label);
			if (binding == null || !IsLabel (binding))
				throw new Exception ("error JS1026: Label not found");
                        return true;
		}

		internal override void PropagateParent (AST parent)
		{
			base.PropagateParent (parent);
		}
	}

	internal class Continue : Jump {
		internal Continue (AST parent, string label, Location location)
			: base (parent, location)
		{
			this.label += label;
		}

		internal override bool Resolve (Environment env)
                {
                        if (!InLoop)
                                throw new Exception ("A continue can't be outside a iteration stm");
			if (label != String.Empty)
				return ValidLabel ();
			return true;
                }

		internal override void Emit (EmitContext ec)
		{
			if (label == String.Empty) {
				ec.ig.Emit (OpCodes.Br, ec.LoopBegin);
				return;
			}
			ec.ig.Emit (OpCodes.Br, (binding as Labelled).InitAddrs);
		}
	}

	internal class Break : Jump {

		internal Break (AST parent, string label, Location location)
			: base (parent, location)
		{
			this.label += label;
		}

		internal override bool Resolve (Environment env)
                {
                        if (!InLoop && !InSwitch)
				throw new Exception ("A break statement can't be outside a switch or iteration stm");
			if (label != String.Empty)
				return ValidLabel ();
			return true;
                }

		internal override void Emit (EmitContext ec)
		{
			if (label == String.Empty) {
				ec.ig.Emit (OpCodes.Br, ec.LoopEnd);
				return;
			}
			ec.ig.Emit (OpCodes.Br, (binding as Labelled).EndAddrs);
		}

		internal override void PropagateParent (AST parent)
		{
			base.PropagateParent (parent);
		}
	}

	internal class NotVoidReturnEventArgs : EventArgs {
	}
		
	internal delegate void NotVoidReturnEventHandler (object sender, NotVoidReturnEventArgs args);
	
	internal class Return : AST, ICanModifyContext {

		internal AST expression;
		private bool exp_returns_void = false;
		public event NotVoidReturnEventHandler not_void_return;
	
		internal Return (Location location)
			: base (null, location)
		{
		}
		
		internal void OnNotVoidReturn (NotVoidReturnEventArgs args)
		{
			if (not_void_return != null)
				not_void_return (this, args);
		}

		internal void Init (AST parent, AST exp)
		{
			this.parent = parent;
			expression = exp;
			Function cont_func = GetContainerFunction;
			this.not_void_return = new NotVoidReturnEventHandler (cont_func.NotVoidReturnHappened);
		}

		public override string ToString ()
		{
			if (expression != null)
				return expression.ToString ();
			else 
				return String.Empty;
		}

		void ICanModifyContext.PopulateContext (Environment env, string ns)
		{
			if (expression is ICanModifyContext)
				((ICanModifyContext) expression).PopulateContext (env, ns);
		}

		void ICanModifyContext.EmitDecls (EmitContext ec)
		{
			if (expression is ICanModifyContext)
				((ICanModifyContext) expression).EmitDecls (ec);
		}

		internal override bool Resolve (Environment env)
		{
			if (!InFunction)
				throw new Exception ("error JS1018: 'return' statement outside of function");
			if (expression != null) {
				if (expression is Expression) {
					AST ast = ((Expression) expression).Last;
					if (ast is Call) {
						Call call = (Call) ast;
						if (call.member_exp is Identifier) {
							object obj = env.Get (String.Empty, ((Identifier) call.member_exp).name);
							if (obj is Function)
								exp_returns_void = ((Function) obj).HandleReturnType == typeof (void);
						}
					}
				}					
				OnNotVoidReturn (null);
				return expression.Resolve (env);
			} else 
				return true;
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Label lbl = ig.DefineLabel ();
			LocalBuilder loc = null;

			if (expression != null) {
				expression.Emit (ec);
				loc = ig.DeclareLocal (typeof (object));

				if (exp_returns_void)
					ig.Emit (OpCodes.Ldnull);

				ig.Emit (OpCodes.Stloc, loc);
			}
					 
			ig.Emit (OpCodes.Br, lbl);
			ig.MarkLabel (lbl);

			if (loc != null)
				ig.Emit (OpCodes.Ldloc, loc);
		}
	}

	internal class DoWhile : AST, ICanModifyContext {

		AST stm, exp;

		internal DoWhile (Location location)
			: base (null, location)
		{
		}

		internal void Init (AST parent, AST stm, AST exp)
		{
			this.parent = parent;
			this.stm = stm;
			this.exp = exp;
		}

		void ICanModifyContext.PopulateContext (Environment env, string ns)
		{
			if (stm is ICanModifyContext)
				((ICanModifyContext) stm).PopulateContext (env, ns);
		}

		void ICanModifyContext.EmitDecls (EmitContext ec)
		{
			if (stm is ICanModifyContext)
				((ICanModifyContext) stm).EmitDecls (ec);
		}

		internal override bool Resolve (Environment env)
		{
			bool r = true;

			if (stm != null)
				if (stm is Exp)
					r &= ((Exp) stm).Resolve (env, true);
				else
					r &= stm.Resolve (env);
			if (exp != null)
				r &= exp.Resolve (env);
			return r;
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Label old_begin = ec.LoopBegin;
			Label old_end = ec.LoopEnd;
			Label body_label = ig.DefineLabel ();

			ec.LoopBegin = ig.DefineLabel ();
			ec.LoopEnd = ig.DefineLabel ();

			ig.MarkLabel (body_label);

			if (stm != null)
				stm.Emit (ec);

			ig.MarkLabel (ec.LoopBegin);

			if (parent.GetType () == typeof (Labelled))
				ig.MarkLabel ((parent as Labelled).InitAddrs);

			CodeGenerator.fall_false (ec, exp, body_label);
			ig.MarkLabel (ec.LoopEnd);

			ec.LoopBegin = old_begin;
			ec.LoopEnd = old_end;
		}	       
	}


	internal class While : AST, ICanModifyContext {
		AST exp, stm;

		internal While (Location location)
			: base (null, location)
		{
		}

		internal void Init (AST parent, AST exp, AST stm)
		{
			this.parent = parent;
			this.exp = exp;
			this.stm = stm;
		}

		void ICanModifyContext.PopulateContext (Environment env, string ns)
		{
			if (stm is ICanModifyContext)
				((ICanModifyContext) stm).PopulateContext (env, ns);
		}

		void ICanModifyContext.EmitDecls (EmitContext ec)
		{
			if (stm is ICanModifyContext)
				((ICanModifyContext) stm).EmitDecls (ec);
		}
		
		internal override bool Resolve (Environment env)
		{
			bool r = true;
			if (exp != null)
				if (exp is Exp)
					r &= ((Exp) exp).Resolve (env, false);
				else 
					r &= exp.Resolve (env);
			if (stm != null)
				if (stm is Exp)
					r &= ((Exp) stm).Resolve (env, true);
				else
					r &= stm.Resolve (env);
			return r;
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Label old_begin = ec.LoopBegin;
			Label old_end = ec.LoopEnd;

			ec.LoopBegin = ig.DefineLabel ();
                        ec.LoopEnd = ig.DefineLabel ();

			Label body_label = ig.DefineLabel ();

			ig.Emit (OpCodes.Br, ec.LoopBegin);
			ig.MarkLabel (body_label);
			
			if (stm != null)
				stm.Emit (ec);
			
			ig.MarkLabel (ec.LoopBegin);
			
			if (parent.GetType () == typeof (Labelled))
				ig.MarkLabel ((parent as Labelled).InitAddrs);

			CodeGenerator.fall_false (ec, exp, body_label);
			ig.MarkLabel (ec.LoopEnd);

			ec.LoopBegin = old_begin;
			ec.LoopEnd = old_end;
		}

		public override string ToString ()
		{
			return "while";
		}
	}

	internal class For : AST, ICanModifyContext {
		
		AST [] exprs = new AST [3];
		AST stms;

		internal For (AST parent, AST init, AST test, AST incr, AST body, Location location)
			: base (parent, location)
		{
			exprs [0] = init;
			exprs [1] = test;
			exprs [2] = incr;
			stms = body;
		}

		void ICanModifyContext.PopulateContext (Environment env, string ns)
		{
			foreach (AST ast in exprs)
				if (ast is ICanModifyContext)
					((ICanModifyContext) ast).PopulateContext (env, ns);

			if (stms is ICanModifyContext)
				((ICanModifyContext) stms).PopulateContext (env, ns);
		}
		
		void ICanModifyContext.EmitDecls (EmitContext ec)
		{
			foreach (AST ast in exprs)
				if (ast is ICanModifyContext)
					((ICanModifyContext) ast).EmitDecls (ec);

			if (stms is ICanModifyContext)
				((ICanModifyContext) stms).EmitDecls (ec);
		}

		internal override bool Resolve (Environment env)
		{
			bool r = true;

			for (int i = 0; i < 3; ++i) {
				AST e = exprs[i];
				if (e is Exp)
					r &= ((Exp) e).Resolve (env, i != 1);
				else
					r &= e.Resolve (env);
			}

			if (stms != null)
				r &= stms.Resolve (env);
			return true;
		}

		internal override void Emit (EmitContext ec)
		{
			AST tmp;
			ILGenerator ig = ec.ig;
			Label old_begin = ec.LoopBegin;
			Label old_end = ec.LoopEnd;
			Label back = ig.DefineLabel ();
			Label forward = ig.DefineLabel ();


			/* emit init expr */
			tmp = exprs [0];
			if (tmp != null)
				tmp.Emit (ec);

                        ec.LoopBegin = ig.DefineLabel ();
			ec.LoopEnd = ig.DefineLabel ();
			
			ig.MarkLabel (back);
			ig.MarkLabel (ec.LoopBegin);

			/* emit condition */
			tmp = exprs [1];
			if (tmp != null)
				tmp.Emit (ec);

			if (tmp != null && tmp is Expression) {
				ArrayList t = ((Expression) tmp).exprs;
				AST a = (AST) t [t.Count - 1];
				if (a is Equality)
					ig.Emit (OpCodes.Brfalse, forward);
				else if (a is Relational) {
					Relational rel = (Relational) a;
					ig.Emit (OpCodes.Ldc_I4_0);
					ig.Emit (OpCodes.Conv_R8);

					if (rel.op == JSToken.GreaterThan)
						ig.Emit (OpCodes.Ble,  forward);
					else
						ig.Emit (OpCodes.Bge, forward);
				}
			}
			/* emit stms */
			if (stms != null)
				stms.Emit (ec);

			if (parent.GetType () == typeof (Labelled))
				ig.MarkLabel ((parent as Labelled).InitAddrs);

			tmp = exprs [2];
			/* emit increment */
			if (tmp != null)
				tmp.Emit (ec);

			ig.Emit (OpCodes.Br, back);
			ig.MarkLabel (forward);
			ig.MarkLabel (ec.LoopEnd);

			ec.LoopBegin = old_begin;
			ec.LoopEnd = old_end;
		}
	}

	internal class Switch : AST, ICanModifyContext {

		internal AST exp;
		internal ArrayList case_clauses;
		internal ArrayList default_clauses;
		internal ArrayList sec_case_clauses;

		internal Switch (AST parent, Location location)
			: base (parent, location)
		{
			case_clauses = new ArrayList ();
			default_clauses = new ArrayList ();
			sec_case_clauses = new ArrayList ();
		}

		internal void AddClause (Clause clause, ClauseType clause_type)
		{
			if (clause_type == ClauseType.Case)
				case_clauses.Add (clause);
			else if (clause_type == ClauseType.CaseAfterDefault)
				sec_case_clauses.Add (clause);
		}

		void ICanModifyContext.PopulateContext (Environment env, string ns)
		{
			foreach (AST ast in case_clauses)
				if (ast is ICanModifyContext)
					((ICanModifyContext) ast).PopulateContext (env, ns);

			foreach (AST ast in default_clauses)
				if (ast is ICanModifyContext)
					((ICanModifyContext) ast).PopulateContext (env, ns);

			foreach (AST ast in sec_case_clauses)
				if (ast is ICanModifyContext)
					((ICanModifyContext) ast).PopulateContext (env, ns);
		}
		
		void ICanModifyContext.EmitDecls (EmitContext ec)
		{
			foreach (AST ast in case_clauses)
				if (ast is ICanModifyContext)
					((ICanModifyContext) ast).EmitDecls (ec);

			foreach (AST ast in default_clauses)
				if (ast is ICanModifyContext)
					((ICanModifyContext) ast).EmitDecls (ec);

			foreach (AST ast in sec_case_clauses)
				if (ast is ICanModifyContext)
					((ICanModifyContext) ast).EmitDecls (ec);
		}

		internal override bool Resolve (Environment env)
		{
			bool r = true;
			if (exp != null)
				r &= exp.Resolve (env);
			if (case_clauses != null)
				foreach (Clause c in case_clauses)
					r &= c.Resolve (env);
			if (default_clauses != null)
				foreach (AST dc in default_clauses) {
					if (dc is Exp)
						r &= ((Exp) dc).Resolve (env, true);
					else
						r &= dc.Resolve (env);
				}
			if (sec_case_clauses != null)
				foreach (Clause sc in sec_case_clauses)
					r &= sc.Resolve (env);
			return r;
		}

		internal override void Emit (EmitContext ec)
		{
			if (exp != null)
				exp.Emit (ec);

			ILGenerator ig = ec.ig;
			Label init_default = ig.DefineLabel ();
			Label end_of_default = ig.DefineLabel ();
			Label old_end = ec.LoopEnd;
			ec.LoopEnd = ig.DefineLabel ();

			LocalBuilder loc = ig.DeclareLocal (typeof (object));
			ig.Emit (OpCodes.Stloc, loc);

			foreach (Clause c in case_clauses) {
				ig.Emit (OpCodes.Ldloc, loc);
				c.EmitConditional (ec);
			}

			/* emit conditionals from clauses that come after the default clause */
			if (sec_case_clauses != null && sec_case_clauses.Count > 0) {
				foreach (Clause c in sec_case_clauses) {
					ig.Emit (OpCodes.Ldloc, loc);
					c.EmitConditional (ec);
				}
			} 
			ig.Emit (OpCodes.Br, init_default);

			/* emit the stms from case_clauses */
			foreach (Clause c in case_clauses) {
				ig.MarkLabel (c.matched_block);
				c.EmitStms (ec);				
			}
			
			ig.MarkLabel (init_default);
			foreach (AST ast in default_clauses)
				ast.Emit (ec);
			ig.MarkLabel (end_of_default);

			if (sec_case_clauses != null && sec_case_clauses.Count > 0) {
				foreach (Clause c in sec_case_clauses) {
					ig.MarkLabel (c.matched_block);
					c.EmitStms (ec);				
				}
			} 
			ig.MarkLabel (ec.LoopEnd);
			ec.LoopEnd = old_end;
		}
	}

	internal class Clause : AST, ICanModifyContext {
		internal AST exp;
		internal ArrayList stm_list;
		internal Label matched_block;

		internal Clause (AST parent, Location location)
			: base (parent, location)
		{
			stm_list = new ArrayList ();
		}

		internal void AddStm (AST stm)
		{
			stm_list.Add (stm);
		}

		void ICanModifyContext.PopulateContext (Environment env, string ns)
		{
			foreach (AST ast in stm_list)
				if (ast is ICanModifyContext)
					((ICanModifyContext) ast).PopulateContext (env, ns);
		}
		

		void ICanModifyContext.EmitDecls (EmitContext ec)
		{
			foreach (AST ast in stm_list)
				if (ast is ICanModifyContext)
					((ICanModifyContext) ast).EmitDecls (ec);
		}

		internal override bool Resolve (Environment env)
		{
			bool r = true;
			if (exp != null)
				r &= exp.Resolve (env);
			foreach (AST ast in stm_list) {
				if (ast is Exp)
					r &= ((Exp) ast).Resolve (env, true);
				else
					r &= ast.Resolve (env);
			}
			return r;			
		}

		internal void EmitConditional (EmitContext ec)
		{
			if (exp != null)
				exp.Emit (ec);
			ILGenerator ig = ec.ig;
			matched_block = ig.DefineLabel ();
			ig.Emit (OpCodes.Call, typeof (StrictEquality).GetMethod ("JScriptStrictEquals"));
			ig.Emit (OpCodes.Brtrue, matched_block);
		}
		
		internal void EmitStms (EmitContext ec)
		{
			foreach (AST ast in stm_list)
				ast.Emit (ec);
		}

		internal override void Emit (EmitContext ec)
		{
		}	
	}

	internal class Catch : AST, ICanModifyContext {
		internal string id;
		internal AST catch_cond;
		internal AST stms;

		internal FieldBuilder field_info;
		internal LocalBuilder local_builder;

		internal Catch (string id, AST catch_cond, AST stms, AST parent, Location location)
			: base (parent, location)
		{
			this.id = id;
			this.catch_cond = catch_cond;
			this.stms = stms;
		}

		void ICanModifyContext.PopulateContext (Environment env, string ns)
		{
			if (stms is ICanModifyContext)
				((ICanModifyContext) stms).PopulateContext (env, ns);
		}
		
		void ICanModifyContext.EmitDecls (EmitContext ec)
		{
			if (stms is ICanModifyContext)
				((ICanModifyContext) stms).EmitDecls (ec);
		}

		internal override bool Resolve (Environment env)
		{
			bool r = true;
			if (stms != null)
				r &= stms.Resolve (env);
			return r;
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Type t = typeof (object);
			bool in_function = InFunction;

			if (in_function)
				local_builder = ig.DeclareLocal (t);
			else
				field_info = ec.type_builder.DefineField (mangle_id (id), t, FieldAttributes.Public | FieldAttributes.Static);

			ig.BeginCatchBlock (typeof (Exception));
			CodeGenerator.load_engine (in_function, ig);
			ig.Emit (OpCodes.Call, typeof (Try).GetMethod ("JScriptExceptionValue"));
			
			if (in_function)
				ig.Emit (OpCodes.Stloc, local_builder);
			else
				ig.Emit (OpCodes.Stsfld, field_info);

			stms.Emit (ec);			
		}

		internal string mangle_id (string id)
		{
			return id + ":0";
		}
	}

	internal class Labelled : AST, ICanModifyContext {
		string name;
		Label init_addrs; 
		Label end_addrs;
		AST stm;

		internal Labelled (AST parent, Location location)
			: base (parent, location)
		{
		}

		internal Label InitAddrs {
			set { init_addrs = value; }
			get { return init_addrs; }
		}
		
		internal Label EndAddrs {
			set { end_addrs = value; }
			get { return end_addrs; }
		}

		internal void Init (AST parent, string name, AST stm, Location location)
		{
			this.parent = parent;
			this.name = name;
			this.stm = stm;
			this.location = location;
		}

		void ICanModifyContext.PopulateContext (Environment env, string ns)
		{
			if (stm is ICanModifyContext)
				((ICanModifyContext) stm).PopulateContext (env, ns);
		}
		
		void ICanModifyContext.EmitDecls (EmitContext ec)
		{
			if (stm is ICanModifyContext)
				((ICanModifyContext) stm).EmitDecls (ec);
		}

		internal override bool Resolve (Environment env)
		{
			try {
				SemanticAnalyser.AddLabel (name, this);
			} catch (ArgumentException) {
				throw new Exception ("error JS1025: Label redefined");
			}
			if (stm != null)
				stm.Resolve (env);
			SemanticAnalyser.RemoveLabel (name);
			return true;
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			init_addrs = ig.DefineLabel ();
			end_addrs = ig.DefineLabel ();

			if (!IsLoop (stm))
				ig.MarkLabel (init_addrs);

			stm.Emit (ec);

			ig.MarkLabel (end_addrs);
		}

		bool IsLoop (AST ast)
		{
			Type t = ast.GetType ();
			return t == typeof (For) || t == typeof (While) || t == typeof (DoWhile) || t == typeof (ForIn);
		}

		public override string ToString ()
		{
			return "Labelled";
		}
	}
}

