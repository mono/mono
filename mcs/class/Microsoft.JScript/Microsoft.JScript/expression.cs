//
// Expression.cs: Everything related to expressions
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
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
			if (current_op == JSToken.None) {
				if (left != null)
					left.Emit (ec);
			} else if (current_op == JSToken.LogicalAnd || current_op == JSToken.LogicalOr)
				emit_jumping_code (ec);
			else {
				emit_operator (ig);
				if (left != null)
					left.Emit (ec);
				if (right != null)
					right.Emit (ec);			       
				emit_op_eval (ig);
			}
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
			case JSToken.BitwiseAnd:
			case JSToken.BitwiseXor:
			case JSToken.BitwiseOr:
			case JSToken.LeftShift:
			case JSToken.RightShift:
			case JSToken.UnsignedRightShift:
				ig.Emit (OpCodes.Call, typeof (BitwiseBinary).GetMethod ("EvaluateBitwiseBinary"));
					 break;
			}			
		}

		internal void emit_jumping_code (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Type t = typeof (bool);
			Label false_label = ig.DefineLabel ();
			Label exit_label = ig.DefineLabel ();
			CodeGenerator.fall_true (ec, this, false_label);
			ig.Emit (OpCodes.Ldc_I4_1);			
			ig.Emit (OpCodes.Box, t);			
			ig.Emit (OpCodes.Br, exit_label);
			ig.MarkLabel (false_label);
			ig.Emit (OpCodes.Ldc_I4_0);			
			ig.Emit (OpCodes.Box, t);			
			ig.MarkLabel (exit_label);
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
			} else if (current_op == JSToken.LeftShift) {
				t = typeof (BitwiseBinary);
				local_builder = ig.DeclareLocal (t);
				ig.Emit (OpCodes.Ldc_I4_S, (byte) 61);
			} else if (current_op == JSToken.RightShift) {
				t = typeof (BitwiseBinary);
				local_builder = ig.DeclareLocal (t);
				ig.Emit (OpCodes.Ldc_I4_S, (byte) 62);
			} else if (current_op == JSToken.UnsignedRightShift) {
				t = typeof (BitwiseBinary);
				local_builder = ig.DeclareLocal (t);
				ig.Emit (OpCodes.Ldc_I4_S, (byte) 63);
			} else if (current_op == JSToken.Multiply) {
				t = typeof (NumericBinary);
				local_builder = ig.DeclareLocal (t);
				ig.Emit (OpCodes.Ldc_I4_S, (byte) 64);
			} else if (current_op == JSToken.Divide) {
				t = typeof (NumericBinary);
				local_builder = ig.DeclareLocal (t);
				ig.Emit (OpCodes.Ldc_I4_S, (byte) 65);
			} else if (current_op == JSToken.Modulo) {
				t = typeof (NumericBinary);
				local_builder = ig.DeclareLocal (t);
				ig.Emit (OpCodes.Ldc_I4_S, (byte) 66);
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
			return Resolve (context);
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Label false_label = ig.DefineLabel ();
			Label merge_label = ig.DefineLabel ();
			CodeGenerator.fall_true (ec, cond_exp, false_label);
			if (true_exp != null)
				true_exp.Emit (ec);
			ig.Emit (OpCodes.Br, merge_label);
			ig.MarkLabel (false_label);
			if (false_exp != null)
				false_exp.Emit (ec);
			ig.MarkLabel (merge_label);
			if (no_effect)
				ig.Emit (OpCodes.Pop);
		}
	}

	internal interface ICallable {
		void AddArg (AST arg);
	}
	
	public class Call : AST, ICallable {
		
		internal AST member_exp;		
		internal Args args;

		internal Call (AST parent, AST exp)
		{
			this.parent = parent; 
			this.member_exp = exp;
			this.args = new Args ();
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

			if (member_exp != null)
				sb.Append (member_exp.ToString () + " ");
			if (args != null)
				sb.Append (args.ToString ());
			return sb.ToString ();
		}

		public void AddArg (AST arg)
		{
			args.Add (arg);
		}

		internal override bool Resolve (IdentificationTable context)
		{
			bool r = true;

			if (member_exp != null)
				r &= member_exp.Resolve (context);
			if (args != null)
				r &= args.Resolve (context);
			return r;
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			if (member_exp.ToString () == "print") {				
				AST ast;
				int n = args.Size - 1;				
				Type script_stream = typeof (ScriptStream);
				MethodInfo write = script_stream.GetMethod ("Write");
				MethodInfo writeline = script_stream.GetMethod ("WriteLine");
				MethodInfo to_string = typeof (Convert).GetMethod ("ToString",
										   new Type [] { typeof (object), typeof (bool) });
				for (int i = 0; i <= n; i++) {
					ast = args.get_element (i);
					ast.Emit (ec);

					if (ast is StringLiteral)
						;
					else {
						ig.Emit (OpCodes.Ldc_I4_1);
						ig.Emit (OpCodes.Call, to_string);
					}

					if (i == n)
						ig.Emit (OpCodes.Call, writeline);
					else
						ig.Emit (OpCodes.Call, write);
				}
			}			
		}
	}

	internal class Identifier : Exp, IAssignable {

		internal string name;
		internal AST binding;
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
			object bind = context.Contains (name);
			if (bind == null)
				throw new Exception ("variable not found: " +  name);
			else
				binding = bind as AST;

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

			if (binding is FormalParam) {
				FormalParam f = binding as FormalParam;
				ig.Emit (OpCodes.Ldarg_S, f.pos);
			} else if (binding is VariableDeclaration || binding is Try) {
				FieldInfo field_info = extract_field_info (binding);
				LocalBuilder local_builder = extract_local_builder (binding);
				
				if (field_info != null) {
					if (assign)
						ig.Emit (OpCodes.Stsfld, field_info);
					else
						ig.Emit (OpCodes.Ldsfld, field_info);
				} else if (local_builder != null) {
					if (assign)
						ig.Emit (OpCodes.Stloc, local_builder);
					else
						ig.Emit (OpCodes.Ldloc, local_builder);
				}
			} 
			if (!assign && no_effect)
				ig.Emit (OpCodes.Pop);				
		}

		internal FieldInfo extract_field_info (AST a)
		{
			FieldInfo r = null;
			if (a is VariableDeclaration)
				r = ((VariableDeclaration) a).field_info;
			else if (a is Try)
				r = ((Try) a).field_info;
			return r;
		}
		
		internal LocalBuilder extract_local_builder (AST a)
		{
			LocalBuilder r = null;
			if (a is VariableDeclaration)
				r = ((VariableDeclaration) a).local_builder;
			else if (a is Try)
				r = ((Try) a).local_builder;
			return r;
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
			int i, n = elems.Count;
			AST tmp;
			bool r = true;

			for (i = 0; i < n; i++) {
				tmp = (AST) elems [i];
				r &= tmp.Resolve (context);
			}
			return r;
		}

		internal AST get_element (int i)
		{
			if (i >= 0 && i < elems.Count)
				return (AST) elems [i];
			else
				throw new IndexOutOfRangeException ();
		}

		internal int Size {
			get { return elems.Count; }
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
			else 
				((AST) e).Resolve (context);

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
			return l + " = " + r;			
		}
	}

	internal class New : AST, ICallable {

		AST exp;
		Args args;

		internal New (AST parent, AST exp)
		{
			this.parent = parent;
			this.exp = exp;
			this.args = new Args ();
		}

		public void AddArg (AST arg)
		{
			args.Add (arg);
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
	
	internal interface IAssignable {
		bool ResolveAssign (IdentificationTable context);
	}
}
