//
// Expression.cs: Everything related to expressions
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

using System;
using System.Text;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace Microsoft.JScript {
	
	public abstract class Exp : AST {
		internal bool no_effect;
		internal abstract bool Resolve (IdentificationTable context, bool no_effect);
	}
	
	public class Unary : UnaryOp {
		
		internal Unary (AST parent, AST operand, JSToken oper)
		{			
			this.parent = parent;
			this.operand = operand;
			this.oper = oper;
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			
			if (oper != JSToken.None)
				sb.Append (oper + " ");
			
			if (operand != null)
				sb.Append (operand.ToString ());
			
			return sb.ToString ();
		}

		internal override bool Resolve (IdentificationTable context)
		{
			bool r = false;		       
			
			if (operand is Exp)
				if (oper != JSToken.Increment && oper != JSToken.Decrement)
					r = ((Exp) operand).Resolve (context, no_effect);
			r = ((AST) operand).Resolve (context);			
			return r;
		}

		internal override bool Resolve (IdentificationTable context, bool no_effect)
		{
			this.no_effect = no_effect;
			return Resolve (context);
		}

		internal override void Emit (EmitContext ec)
		{
			if (operand != null)
				operand.Emit (ec);
		}			
	}

	public class Binary : BinaryOp {

		internal Binary (AST parent, AST left, AST right, JSToken op)
		{
			this.parent = parent;
			this.left = left;
			this.right = right;
			this.current_op = op;	
		}
		
		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append (left.ToString () + " ");
			
			if (current_op != JSToken.None)
				sb.Append (current_op + " ");
			
			if (right != null)
				sb.Append (right.ToString ());

			return sb.ToString ();
		}

		internal override bool Resolve (IdentificationTable context)
		{
			bool r = true;
			if (left != null)
				r &= left.Resolve (context);
			if (right != null)
				r &= right.Resolve (context);
			return r;
		}

		internal override bool Resolve (IdentificationTable context, bool no_effect)
		{
			this.no_effect = no_effect;
			return Resolve (context);
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			if (current_op != JSToken.None)
				emit_operator (ig);
			if (left != null)
				left.Emit (ec);
			if (right != null)
				right.Emit (ec);			       
			emit_op_eval (ig);

			if (no_effect)
				ig.Emit (OpCodes.Pop);
		}

		internal void emit_op_eval (ILGenerator ig)
		{
			switch (current_op) {
			case JSToken.Plus:
				ig.Emit (OpCodes.Callvirt, typeof (Plus).GetMethod ("EvaluatePlus"));
				break;
			case JSToken.Minus:
			case JSToken.Divide:
			case JSToken.Modulo:
			case JSToken.Multiply:
				ig.Emit (OpCodes.Call, typeof (NumericBinary).GetMethod ("EvaluateNumericBinary"));
				break;
			case JSToken.Equal:
				ig.Emit (OpCodes.Call, typeof (Equality).GetMethod ("EvaluateEquality"));
				Label t = ig.DefineLabel ();
				Label f = ig.DefineLabel ();
				ig.Emit (OpCodes.Brtrue_S, t);
				ig.Emit (OpCodes.Ldc_I4_0);
				ig.Emit (OpCodes.Br_S, f);
				ig.MarkLabel (t);
				ig.Emit (OpCodes.Ldc_I4_1);
				ig.MarkLabel (f);
				ig.Emit (OpCodes.Pop);
				break;
			}
		}

		internal void emit_operator (ILGenerator ig)
		{
			LocalBuilder local_builder = null;
			Type t = null;

			if (current_op == JSToken.Plus) {
				t = typeof (Plus);
				local_builder = ig.DeclareLocal (t);				
				ig.Emit (OpCodes.Newobj, t.GetConstructor (new Type [] {}));
				ig.Emit (OpCodes.Stloc, local_builder);
				ig.Emit (OpCodes.Ldloc, local_builder);
				return;
			} else if (current_op == JSToken.Minus) {
				t = typeof (NumericBinary);
				local_builder = ig.DeclareLocal (t);
				ig.Emit (OpCodes.Ldc_I4_S, (byte) 47);
			} else if (current_op == JSToken.Multiply) {
				t = typeof (NumericBinary);
				local_builder = ig.DeclareLocal (t);
				ig.Emit (OpCodes.Ldc_I4_S, (byte) 64);
			} else if (current_op == JSToken.Divide) {
				t = typeof (NumericBinary);
				local_builder = ig.DeclareLocal (t);
				ig.Emit (OpCodes.Ldc_I4_S, (byte) 65);
			} else if (current_op == JSToken.BitwiseOr) {
				t = typeof (BitwiseBinary);
				local_builder = ig.DeclareLocal (t);
				ig.Emit (OpCodes.Ldc_I4_S, (byte) 50);
			} else if (current_op == JSToken.BitwiseXor) {
				t = typeof (BitwiseBinary);
				local_builder = ig.DeclareLocal (t);
				ig.Emit (OpCodes.Ldc_I4_S, (byte) 51);				
			} else if (current_op == JSToken.BitwiseAnd) {
				t = typeof (BitwiseBinary);
				local_builder = ig.DeclareLocal (t);
				ig.Emit (OpCodes.Ldc_I4_S, (byte) 52);				
			} else if (current_op == JSToken.Equal) {
				t = typeof (Equality);
				local_builder = ig.DeclareLocal (t);
				ig.Emit (OpCodes.Ldc_I4_S, (byte) 53);
			} else if (current_op == JSToken.NotEqual) {
				t = typeof (Equality);
				local_builder = ig.DeclareLocal (t);
				ig.Emit (OpCodes.Ldc_I4_S, (byte) 54);
			}
			ig.Emit (OpCodes.Newobj, t.GetConstructor (new Type [] {typeof (int)}));
			ig.Emit (OpCodes.Stloc, local_builder);
			ig.Emit (OpCodes.Ldloc, local_builder);
		}
	}

	public class Conditional : Exp {

		AST cond_exp, true_exp, false_exp;

		internal Conditional (AST parent, AST expr, AST  trueExpr, AST falseExpr)
		{
			this.cond_exp = expr;
			this.true_exp = trueExpr;
			this.false_exp = falseExpr;
		}
		
		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

			if (cond_exp != null)
				sb.Append (cond_exp.ToString () + " ");
			if (true_exp != null)
				sb.Append (true_exp.ToString () + " ");
			if (false_exp != null)
				sb.Append (false_exp.ToString ());

			return sb.ToString ();
		}

		internal override bool Resolve (IdentificationTable context)
		{
			bool r = true;
			if (cond_exp != null)
				r &= cond_exp.Resolve (context);
			if (true_exp != null)
				r &= true_exp.Resolve (context);
			if (false_exp != null)
				r &= false_exp.Resolve (context);
			return r;
		}

		internal override bool Resolve (IdentificationTable context, bool no_effect)
		{
			this.no_effect = no_effect;
			return true;
		}

		internal override void Emit (EmitContext ec)
		{
			throw new NotImplementedException ();
		}
	}

	public class Call : AST {

		internal AST member_exp;
		internal AST args1;
		internal AST args2;

		public Call (AST parent, AST member_exp, AST args1, AST args2)
		{
			this.parent = parent;
			this.member_exp = member_exp;
			this.args1 = args1;
			this.args2 = args2;
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

			if (member_exp != null)
				sb.Append (member_exp.ToString () + " ");
			if (args1 != null)
				sb.Append (args1.ToString ());
			if (args2 != null)
				sb.Append (args2.ToString ());

			return sb.ToString ();
		}

		internal override bool Resolve (IdentificationTable context)
		{
			if (member_exp != null)
				member_exp.Resolve (context);

			if (args1 != null)
				args1.Resolve (context);

			if (args2 != null)
				args2.Resolve (context);

			return true;
		}

		internal override void Emit (EmitContext ec)
		{
			throw new NotImplementedException ();
		}
	}

	internal class Identifier : Exp, IAssignable {

		internal string name;
		internal Decl binding;
		internal bool assign;

		internal Identifier (AST parent, string id)
		{
			this.parent = parent;
			this.name = id;
		}

		public override string ToString ()
		{
			return name;
		}

		internal override bool Resolve (IdentificationTable context)
		{
			if (name == "print")
				return SemanticAnalyser.print;
			
			Decl bind = (Decl) context.Contains (name);

			if (bind == null)
				throw new Exception ("variable not found: " +  name);
			else
				binding = bind;

			return true;
		}

		internal override bool Resolve (IdentificationTable context, bool no_effect)
		{
			this.no_effect = no_effect;
			return Resolve (context);
		}

		public bool ResolveAssign (IdentificationTable context)
		{
			this.assign = true;
			this.no_effect = false;
			if (name != String.Empty)			
				return Resolve (context);
			return true;
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			FieldInfo field_info = binding.field_info;
			LocalBuilder local_builder = binding.local_builder;
			
			if (field_info != null) {
				if (assign)
					ig.Emit (OpCodes.Stsfld, binding.field_info);
				else {
					ig.Emit (OpCodes.Ldsfld, binding.field_info);
					if (no_effect)
						ig.Emit (OpCodes.Pop);
				}
			} else if (local_builder != null) {
				if (assign)
					ig.Emit (OpCodes.Stloc, binding.local_builder);
				else {
					ig.Emit (OpCodes.Ldloc, binding.local_builder);
					if (no_effect)
						ig.Emit (OpCodes.Pop);
				}				
			}
		}
	}

	public class Args : AST {

		internal ArrayList elems;

		internal Args ()
		{
			elems = new ArrayList ();
		}

		internal void Add (AST e)
		{
			elems.Add (e);
		}

		internal override bool Resolve (IdentificationTable context)
		{
			int i, size = elems.Count;
			AST tmp;

			for (i = 0; i < size; i++) {
				tmp = (AST) elems [i];
				tmp.Resolve (context);
			}
			return true;
		}

		internal override void Emit (EmitContext ec)
		{
			throw new NotImplementedException ();
		}
	}

	public class Expression : Exp {

		internal ArrayList exprs;

		internal Expression (AST parent)
		{
			this.parent = parent;
			exprs = new ArrayList ();
		}

		internal void Add (AST a)
		{
			exprs.Add (a);
		}

		public override string ToString ()
		{
			int size = exprs.Count;		

			if (size > 0) {
				int i;
				StringBuilder sb = new StringBuilder ();

				for (i = 0; i < size; i++)
					sb.Append (exprs [i].ToString ());
					sb.Append ("\n");
				return sb.ToString ();

			} else return String.Empty;
		}

		internal override bool Resolve (IdentificationTable context)
		{
			int i, n;
			object e;
			bool r = true;
			
			n = exprs.Count - 1;

			for (i = 0; i < n; i++) {
				e = exprs [i];
				if (e is Exp)
					r &= ((Exp) e).Resolve (context, true);
				else
					r &= ((AST) e).Resolve (context);
			}
			e = exprs [n];

			if (e is Exp)
				if (e is Assign)
					r &= ((Assign) e).Resolve (context);
				else
					r &= ((Exp) e).Resolve (context, no_effect);
			return r;
		}

			internal override bool Resolve (IdentificationTable context, bool no_effect)
		{
			this.no_effect = no_effect;
			return Resolve (context);
		}

		internal override void Emit (EmitContext ec)
		{
			int i, n = exprs.Count;
			AST exp;

			for (i = 0; i < n; i++) {
				exp = (AST) exprs [i];
				exp.Emit (ec);
			}
		}
	}

	internal class Assign : BinaryOp {

		internal bool is_embedded;

		internal Assign (AST parent, AST left, AST right, JSToken op, bool is_embedded)
		{
			this.parent = parent;
			this.left = left;
			this.right = right;
			this.is_embedded = is_embedded;
			current_op = op;
		}

		internal override bool Resolve (IdentificationTable context)
		{
			bool r;
			if (left is IAssignable)
				r = ((IAssignable) left).ResolveAssign (context);
			else 
				return false;

			if (right is Exp)
				r &=((Exp) right).Resolve (context, false);

			return r;
		}

		internal override bool Resolve (IdentificationTable context, bool no_effect)
		{
			return true;
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			if (is_embedded) {
				Console.WriteLine ("embedded assignments not supported yet");
				Environment.Exit (-1);
			} else {
				right.Emit (ec);
				left.Emit (ec);
			}			
		}
		
		public override string ToString ()
		{
			string l = left.ToString ();
			string r = right.ToString ();
			return l + " " + r;			
		}
	}
	
	internal interface IAssignable {
		bool ResolveAssign (IdentificationTable context);
	}
}
