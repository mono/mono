//
// Statement.cs:
//
// Author:
//	Cesar Octavio Lopez Nataren
//
// (C) 2003, 2004 Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

using System;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections;

namespace Microsoft.JScript {

	internal class If : AST {

		internal AST cond, true_stm, false_stm;

		internal If (AST parent, AST condition, AST true_stm, AST false_stm)
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
			Label true_lbl = ig.DefineLabel ();
			Label merge_lbl = ig.DefineLabel ();
			CodeGenerator.fall_false (ec, cond, true_lbl);			
			if (true_stm != null)
				true_stm.Emit (ec);			
			ig.Emit (OpCodes.Br, merge_lbl);
			ig.MarkLabel (true_lbl);
			if (false_stm != null)
				false_stm.Emit (ec);			
			ig.MarkLabel (merge_lbl);
		}
 	}

	internal class Continue : AST {

		internal string identifier;

		public override string ToString ()
		{
			return identifier;
		}

		internal override bool Resolve (IdentificationTable context)
		{
			throw new NotImplementedException ();
		}

		internal override void Emit (EmitContext ec)
		{
			throw new NotImplementedException ();
		}
	}

	internal class Break : AST {

		internal string identifier;

		public override string ToString ()
		{
			return identifier;
		}

		internal override bool Resolve (IdentificationTable context)
		{
			throw new NotImplementedException ();
		}

		internal override void Emit (EmitContext ec)
		{
			throw new NotImplementedException ();
		}
	}

	internal class Return : AST {

		internal AST expression;

		internal Return (AST parent, AST exp)
		{
			this.parent = parent;
			expression = exp;
		}

		public override string ToString ()
		{
			return expression.ToString ();
		}

		internal override bool Resolve (IdentificationTable context)
		{
			if (parent == null)
				throw new Exception ("error JS1018: 'return' statement outside of function");

			return expression.Resolve (context);
		}

		internal override void Emit (EmitContext ec)
		{
			throw new NotImplementedException ();
		}
	}

	internal class DoWhile : AST {

		AST stm, exp;
		
		internal DoWhile (AST parent, AST stm, AST exp)
		{
			this.parent = parent;
			this.stm = stm;
			this.exp = exp;
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
			Label body_label = ig.DefineLabel ();
			ig.MarkLabel (body_label);
			if (stm != null)
				stm.Emit (ec);
			CodeGenerator.fall_false (ec, exp, body_label);			
		}
	}


	internal class While : AST {
		
		AST exp, stm;

		internal While (AST parent, AST exp, AST stm)
		{
			this.parent = parent;
			this.exp = exp;
			this.stm = stm;
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
			Label body_label = ig.DefineLabel ();
			Label cond_label = ig.DefineLabel ();
			ig.Emit (OpCodes.Br, cond_label);
			ig.MarkLabel (body_label);
			if (stm != null)
				stm.Emit (ec);
			ig.MarkLabel (cond_label);
			CodeGenerator.fall_false (ec, exp, body_label);
			
		}
	}

	internal class For : AST {
		
		AST [] exprs;
		AST stms;

		internal For (AST parent, AST [] exprs, AST stms)
		{
			this.parent = parent;
			this.exprs = exprs;
			this.stms = stms;
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
			Label back = ig.DefineLabel ();
			Label forward = ig.DefineLabel ();

			/* emit init expr */
			tmp = exprs [0];
			if (tmp != null)
				tmp.Emit (ec);
			ig.MarkLabel (back);

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
			/* emit stm */
			if (stms != null)
				stms.Emit (ec);
			tmp = exprs [2];
			if (tmp != null)
				tmp.Emit (ec);
			
			ig.Emit (OpCodes.Br, back);
			ig.MarkLabel (forward);
		}
	}

	public class Switch : AST {	       

		internal AST exp;
		internal ArrayList case_clauses;
		internal ArrayList default_clauses;
		internal ArrayList sec_case_clauses;

		internal Switch (AST parent)
		{
			this.parent = parent;
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
			LocalBuilder loc = ig.DeclareLocal (typeof (object));
			ig.Emit (OpCodes.Stloc, loc);
			foreach (Clause c in case_clauses) {
				ig.Emit (OpCodes.Ldloc, loc);
				c.EmitConditional (ec);
			}
			Label end = ig.DefineLabel ();
			ig.Emit (OpCodes.Br, end);
			foreach (Clause c in case_clauses) {
				ig.MarkLabel (c.matched_block);
				c.EmitStms (ec);				
			}
			ig.MarkLabel (end);
			foreach (AST ast in default_clauses)
				ast.Emit (ec);
			// FIXME: clauses after default clause are not being generated.
			foreach (Clause sc in sec_case_clauses)
				sc.Emit (ec);
		}
	}

	public class Clause : AST {
		internal AST exp;
		internal ArrayList stm_list;
		internal Label matched_block;

		public Clause (AST parent)
		{
			this.parent = parent;
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
}
