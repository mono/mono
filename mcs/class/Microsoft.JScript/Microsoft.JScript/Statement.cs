// 
// Statement.cs:
//
// Author:
//	Cesar Octavio Lopez Nataren
//
// (C) 2003, 2004 Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
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

	internal class If : AST {

		internal AST cond, true_stm, false_stm;

		internal If (AST parent, AST condition, AST true_stm, AST false_stm, int line_number)
		{
			this.cond = condition;
			this.true_stm = true_stm;
			this.false_stm = false_stm;
			this.line_number = line_number;
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
		
		internal override bool Resolve (IdentificationTable context)
		{
			bool r = true;

			if (cond != null)
				if (cond is Exp)
					r &= ((Exp) cond).Resolve (context, false);

			if (true_stm != null)
				if (true_stm is Exp)
					r &= ((Exp) true_stm).Resolve (context, true);
				else
					r &= true_stm.Resolve (context);			

			if (false_stm != null)
				if (false_stm is Exp)
					r &= ((Exp) false_stm).Resolve (context, true);
				else
					r &= false_stm.Resolve (context);			
			return r;
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Label false_lbl = ig.DefineLabel ();
			Label merge_lbl = ig.DefineLabel ();
			CodeGenerator.fall_true (ec, cond, false_lbl);
			if (true_stm != null)
				true_stm.Emit (ec);			
			ig.Emit (OpCodes.Br, merge_lbl);
			ig.MarkLabel (false_lbl);
			if (false_stm != null)
				false_stm.Emit (ec);			
			ig.MarkLabel (merge_lbl);
		}
 	}

	abstract class Jump : AST {
		protected string label = String.Empty;
		protected object binding;

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
	}

	internal class Continue : Jump {
		internal Continue (AST parent, string label, int line_number)
		{
			this.parent = parent;
			Console.WriteLine ("Continue.parent = {0}", this.parent);
			this.label += label;
			this.line_number = line_number;
		}

		internal override bool Resolve (IdentificationTable context)
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

		internal Break (AST parent, string label, int line_number)
		{
			this.parent = parent;
			Console.WriteLine ("Break.parent = {0}", this.parent);
			this.label += label;
			this.line_number = line_number;
		}

		 internal override bool Resolve (IdentificationTable context)
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
	}

	internal class NotVoidReturnEventArgs : EventArgs {
	}
		
	internal delegate void NotVoidReturnEventHandler (object sender, NotVoidReturnEventArgs args);
	
	internal class Return : AST {

		internal AST expression;		
		public event NotVoidReturnEventHandler not_void_return;
		
		
		internal void OnNotVoidReturn (NotVoidReturnEventArgs args)
		{
			if (not_void_return != null)
				not_void_return (this, args);
		}

		internal Return (AST parent, AST exp, int line_number)
		{
			this.parent = parent;
			expression = exp;
			this.line_number = line_number;
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

		internal override bool Resolve (IdentificationTable context)
		{
			if (!InFunction)
				throw new Exception ("error JS1018: 'return' statement outside of function");
			if (expression != null) {
				OnNotVoidReturn (null);
				return expression.Resolve (context);
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
				ig.Emit (OpCodes.Stloc, loc);
			}
					 
			ig.Emit (OpCodes.Br, lbl);
			ig.MarkLabel (lbl);

			if (loc != null)
				ig.Emit (OpCodes.Ldloc, loc);
		}
	}

	internal class DoWhile : AST {

		AST stm, exp;
		
		internal void Init (AST parent, AST stm, AST exp, int line_number)
		{
			this.parent = parent;
			Console.WriteLine ("DoWhile.parent = {0}", this.parent);
			this.stm = stm;
			this.exp = exp;
			this.line_number = line_number;
		}

		internal override bool Resolve (IdentificationTable context)
		{
			bool r = true;

			if (stm != null)
				r &= stm.Resolve (context);
			if (exp != null)
				r &= exp.Resolve (context);
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


	internal class While : AST {		
		AST exp, stm;

		internal void Init (AST parent, AST exp, AST stm, int line_number)
		{
			this.parent = parent;
			this.exp = exp;
			this.stm = stm;
			this.line_number = line_number;
		}

		internal override bool Resolve (IdentificationTable context)
		{
			bool r = true;
			if (exp != null)
				if (exp is Exp)
					r &= ((Exp) exp).Resolve (context, false);
				else 
					r &= exp.Resolve (context);
			if (stm != null)
				r &= stm.Resolve (context);
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

	internal class For : AST {
		
		AST [] exprs = new AST [3];
		AST stms;

		internal For (AST parent, int line_number, AST init, AST test, AST incr, AST body)
		{
			this.parent = parent;
			this.line_number = line_number;
			exprs [0] = init;
			exprs [1] = test;
			exprs [2] = incr;
			stms = body;
		}

		internal override bool Resolve (IdentificationTable context)
		{
			bool r = true;
			foreach (AST ast in exprs)
				if (ast != null)
					r &= ast.Resolve (context);
			if (stms != null)
				r &= stms.Resolve (context);
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
					ig.Emit (OpCodes.Ldc_I4_0);
					ig.Emit (OpCodes.Conv_R8);
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

	public class Switch : AST {	       

		internal AST exp;
		internal ArrayList case_clauses;
		internal ArrayList default_clauses;
		internal ArrayList sec_case_clauses;

		internal Switch (AST parent, int line_number)
		{
			this.parent = parent;
			this.line_number = line_number;
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

		internal override bool Resolve (IdentificationTable context)
		{
			bool r = true;
			if (exp != null)
				r &= exp.Resolve (context);
			if (case_clauses != null)
				foreach (Clause c in case_clauses)
					r &= c.Resolve (context);
			if (default_clauses != null)
				foreach (AST dc in default_clauses)
					r &= dc.Resolve (context);
			if (sec_case_clauses != null)
				foreach (Clause sc in sec_case_clauses)
					r &= sc.Resolve (context);
			return r;
		}

		internal override void Emit (EmitContext ec)
		{
			if (exp != null)
				exp.Emit (ec);

			ILGenerator ig = ec.ig;
			Label init_default = ig.DefineLabel ();
			Label end_of_default = ig.DefineLabel ();
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
		}
	}

	public class Clause : AST {
		internal AST exp;
		internal ArrayList stm_list;
		internal Label matched_block;

		public Clause (AST parent)
		{
			this.parent = parent;
			stm_list = new ArrayList ();
		}

		internal void AddStm (AST stm)
		{
			stm_list.Add (stm);
		}

		internal override bool Resolve (IdentificationTable context)
		{
			bool r = true;
			if (exp != null)
				r &= exp.Resolve (context);
			foreach (AST ast in stm_list)
				r &= ast.Resolve (context);
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
			ILGenerator ig = ec.ig;
			foreach (AST ast in stm_list)
				ast.Emit (ec);
		}

		internal override void Emit (EmitContext ec)
		{
		}	
	}

	internal class Catch : AST {
		internal string id;
		internal AST catch_cond;
		internal AST stms;

		FieldBuilder field_info;
		LocalBuilder local_builder;

		internal Catch (string id, AST catch_cond, AST stms, AST parent, int line_number)
		{
			this.id = id;
			this.catch_cond = catch_cond;
			this.stms = stms;
			this.line_number = line_number;
		}

		internal override bool Resolve (IdentificationTable context)
		{
			bool r = true;
			if (stms != null)
				r &= stms.Resolve (context);
			return r;
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Type t = typeof (object);
			bool not_in_func = parent == null;

			if (not_in_func)
				field_info = ec.type_builder.DefineField (mangle_id (id), t, FieldAttributes.Public | FieldAttributes.Static);
			else
				local_builder = ig.DeclareLocal (t);

			ig.BeginCatchBlock (typeof (Exception));
			if (not_in_func) {
				ig.Emit (OpCodes.Ldarg_0);
				ig.Emit (OpCodes.Ldfld, typeof (ScriptObject).GetField ("engine"));
				ig.Emit (OpCodes.Call, typeof (Try).GetMethod ("JScriptExceptionValue"));
				ig.Emit (OpCodes.Stsfld, field_info);
			} else {
				ig.Emit (OpCodes.Ldarg_1);
				ig.Emit (OpCodes.Call, typeof (Try).GetMethod ("JScriptExceptionValue"));
				ig.Emit (OpCodes.Stloc, local_builder);
			}
			stms.Emit (ec);			
		}

		internal string mangle_id (string id)
		{
			return id + ":0";
		}
	}

	internal class Labelled : AST {
		string name;
		Label init_addrs; 
		Label end_addrs;
		AST stm;

		internal Label InitAddrs {
			set { init_addrs = value; }
			get { return init_addrs; }
		}
		
		internal Label EndAddrs {
			set { end_addrs = value; }
			get { return end_addrs; }
		}

		internal void Init (AST parent, string name, AST stm, int line_number)
		{
			this.parent = parent;
			Console.WriteLine ("labelled.parent = {0}", this.parent);
			this.name = name;
			this.stm = stm;
			this.line_number = line_number;
		}

		internal override bool Resolve (IdentificationTable context)
		{
			try {
				SemanticAnalyser.AddLabel (name, this);
			} catch (ArgumentException e) {
				throw new Exception ("error JS1025: Label redefined");
			}
			if (stm != null)
				stm.Resolve (context);
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

