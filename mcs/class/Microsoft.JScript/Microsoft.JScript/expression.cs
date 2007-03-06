//
// expression.cs: Everything related to expressions
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, 2004 Cesar Lopez Nataren
// (C) 2005, Novell Inc. (http://novell.com)
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
using Microsoft.JScript.Vsa;

namespace Microsoft.JScript {
	
	public abstract class Exp : AST {
		internal bool no_effect;
		internal abstract bool Resolve (Environment env, bool no_effect);

		internal Exp (AST parent, Location location)
			: base (parent, location)
		{
		}
	}
	
	internal class Unary : UnaryOp {

		private bool deletable = false;
		private bool isCtr = false;

		internal Unary (AST parent, JSToken oper, Location location)
			: base (parent, location)
		{		
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

		internal override bool Resolve (Environment env)
		{
			bool r = false;		       

			if (operand is Binary) {
				Binary bin = (Binary) operand;
				if (oper == JSToken.Delete && bin.AccessField)
					this.deletable = bin.IsDeletable (out isCtr);
				bin.Resolve (env);
			} else if (operand is Exp) {
				if (oper != JSToken.Increment && oper != JSToken.Decrement)
					r = ((Exp) operand).Resolve (env, no_effect);
			} else 
				r = operand.Resolve (env);
			return r;
		}

		internal override bool Resolve (Environment env, bool no_effect)
		{
			this.no_effect = no_effect;
			return Resolve (env);
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			switch (oper) {
			case JSToken.Void:
				operand.Emit (ec);
				ig.Emit (OpCodes.Pop);
				ig.Emit (OpCodes.Ldnull);
				break;

			case JSToken.Typeof:
				operand.Emit (ec);
				CodeGenerator.EmitBox (ig, operand);
				ig.Emit (OpCodes.Call, typeof (Typeof).GetMethod ("JScriptTypeof"));
				break;

			case JSToken.Delete:
				Expression exp = operand as Expression;
				if (exp != null)
					operand = exp.exprs [exp.exprs.Count - 1] as AST;

				Binary arg = operand as Binary;
				if (arg != null) {
					if (arg.op == JSToken.LeftBracket || arg.op == JSToken.AccessField ) {
						if (deletable) {
							arg.left.Emit (ec);
							arg.right.Emit (ec);
							ig.Emit (OpCodes.Ldc_I4_1);
							ig.Emit (OpCodes.Call, typeof (Convert).GetMethod ("ToString",
													   new Type [] { typeof (object), typeof (bool) }));
							ig.Emit (OpCodes.Call, typeof (LateBinding).GetMethod ("DeleteMember",
													       new Type [] { typeof (object), typeof (string) }));
						} else {
							if (isCtr)
								Console.WriteLine ("{0}({1},0) : warning: " +
									   "JS1164: '{2}' is not deletable", 
									   location.SourceName, location.LineNumber, arg.ToString ());

							if (arg.left is Identifier && arg.right is Identifier) {
								//string _base = ((Identifier) arg.left).name.Value;
								string property = ((Identifier) arg.right).name.Value;

								Type lb_type = typeof (LateBinding);
								LocalBuilder lb = ig.DeclareLocal (lb_type);
								
								ig.Emit (OpCodes.Ldstr, property);
								ig.Emit (OpCodes.Newobj, lb_type.GetConstructor (new Type [] { typeof (string) }));
								ig.Emit (OpCodes.Stloc, lb);
								ig.Emit (OpCodes.Ldloc, lb);
								ig.Emit (OpCodes.Dup);
								arg.left.Emit (ec);
								ig.Emit (OpCodes.Stfld, lb_type.GetField ("obj"));
								ig.Emit (OpCodes.Call, lb_type.GetMethod ("Delete"));
							}
						}
					} else {
						Console.WriteLine ("emit_unary_op: Delete: unknown operand type {0}", arg.op);
						throw new NotImplementedException ();
					}
				} else {
					Console.WriteLine ("emit_unary_op: Delete: unknown operand {0} ({1})", operand, operand.GetType ());
					throw new NotImplementedException ();
				}


				if (no_effect)
					ig.Emit (OpCodes.Pop);
				break;

			case JSToken.Plus:
				operand.Emit (ec);
				ig.Emit (OpCodes.Call, typeof (Convert).GetMethod ("ToNumber", new Type [] { typeof (object) }));
				//
				// FIXME: investigate the real
				// discriminate for generating this
				// box.
				if (!no_effect)
					ig.Emit (OpCodes.Box, typeof (double));
				break;

			case JSToken.Minus:
				if (SemanticAnalyser.IsNumericConstant (operand)) {
					operand.Emit (ec);
					ig.Emit (OpCodes.Neg);
				} else if (operand is Identifier && 
					   ((Identifier) operand).name.Value == "Infinity")
					ig.Emit (OpCodes.Ldc_R8, Double.NegativeInfinity);
				else
					emit_non_numeric_unary (ec, operand, (byte) 47);
				break;

			case JSToken.BitwiseNot:
				if (SemanticAnalyser.IsNumericConstant (operand)) {
					operand.Emit (ec);
					ig.Emit (OpCodes.Not);
				} else
					emit_non_numeric_unary (ec, operand, (byte) 40);
				break;

			case JSToken.LogicalNot:
				operand.Emit (ec);
				if (SemanticAnalyser.NeedsToBoolean (operand)) {
					CodeGenerator.EmitBox (ec.ig, operand);
					ig.Emit (OpCodes.Ldc_I4_1);
					ig.Emit (OpCodes.Call, typeof (Convert).GetMethod ("ToBoolean",
										   new Type [] { typeof (object), typeof (bool) }));
				}
				ig.Emit (OpCodes.Ldc_I4_0);
				ig.Emit (OpCodes.Ceq);
				break;

			default:
				Console.WriteLine ("Unimplemented Unary Op: {0}", oper);
				throw new NotImplementedException ();
			}
		}

		private void emit_non_numeric_unary (EmitContext ec, AST operand, byte oper)
		{
			ILGenerator ig = ec.ig;

			Type unary_type = typeof (NumericUnary);
			LocalBuilder unary_builder = ig.DeclareLocal (unary_type);

			ig.Emit (OpCodes.Ldc_I4_S, oper);
			ig.Emit (OpCodes.Newobj, unary_type.GetConstructor (new Type [] { typeof (int) }));
			ig.Emit (OpCodes.Stloc, unary_builder);
			ig.Emit (OpCodes.Ldloc, unary_builder);

			operand.Emit (ec);

			ig.Emit (OpCodes.Call, unary_type.GetMethod ("EvaluateUnary"));
		}
	}

	internal class Binary : BinaryOp, IAssignable {

		internal bool assign, late_bind = false;
		internal AST right_side;

		internal Binary (AST parent, AST left, JSToken op, Location location)
			: this (parent, left, null, op, location)
		{
		}
		
		internal Binary (AST parent, AST left, AST right, JSToken op, Location location)
			: base (parent, left, right, op, location)
		{
		}
		
		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append (left.ToString ());
			
			if (op == JSToken.AccessField)
				sb.Append (".");
			else sb.Append (op);
			
			if (right != null)
				sb.Append (right.ToString ());

			return sb.ToString ();
		}

		internal bool AccessField {
			get { return op == JSToken.AccessField; }
		}

		internal bool LateBinding {
			get { return late_bind; }
		}

		internal override bool Resolve (Environment env)
		{
			bool found = true;

			if (left != null)
				if (op == JSToken.AccessField && left is ICanLookupPrototype) {
					found &= ((ICanLookupPrototype) left).ResolveFieldAccess (right);
					if (!found)
						late_bind = true;
				} else
					found &= left.Resolve (env);
			if (right != null)
				if (op == JSToken.AccessField && right is IAccessible) {
					found &= ((IAccessible) right).ResolveFieldAccess (left);
					if (!found)
						late_bind = true;
				} else
					found &= right.Resolve (env);
			return found;
		}

		internal override bool Resolve (Environment env, bool no_effect)
		{
			this.no_effect = no_effect;
			return Resolve (env);
		}
		
		public bool ResolveAssign (Environment env, AST right_side)
		{
			if (op == JSToken.LeftBracket || op == JSToken.AccessField) {
				this.no_effect = false;
				this.assign = true;
				this.right_side = right_side;
				return Resolve (env);
			} else 
				throw new Exception (location.SourceName + " (" + location.LineNumber + ",0): error JS5008: Illegal assignment");
		}

		internal MemberInfo Binding {
			get { 
				return SemanticAnalyser.get_member (left, right);
			}
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;			
			if (op == JSToken.None) {
				if (left != null)
					left.Emit (ec);
			} else if (op == JSToken.LogicalAnd || op == JSToken.LogicalOr)
				emit_jumping_code (ec);
			else if (op == JSToken.LeftBracket) {
				if (!assign)
					get_default_this (ig);
				if (left != null)
					left.Emit (ec);
				emit_array_access (ec);				
			} else if (op == JSToken.AccessField) {
				if (late_bind)
					emit_late_binding (ec);
				else
					emit_access (left, right, ec);
			} else {
				emit_operator (ig);
				if (left != null) {
					left.Emit (ec);
					CodeGenerator.EmitBox (ig, left);
				}
				if (right != null) {
					right.Emit (ec);
					CodeGenerator.EmitBox (ig, right);
				}
				emit_op_eval (ig);
			}
			if (no_effect)
				ig.Emit (OpCodes.Pop);
		}

		void emit_access (AST obj, AST prop_name, EmitContext ec)
		{
			MemberInfo minfo = SemanticAnalyser.get_member (obj, prop_name);	
			ILGenerator ig = ec.ig;
			MemberTypes minfo_type = minfo.MemberType;

			switch (minfo_type) {
			case MemberTypes.Field:
				FieldInfo finfo = (FieldInfo) minfo;
				object value = finfo.GetValue (finfo);
				Type type = value.GetType ();
				if (type == typeof (double))
					ig.Emit (OpCodes.Ldc_R8, (double) value);
				break;
			case MemberTypes.Property:
				PropertyInfo property = (PropertyInfo) minfo;
				Type decl_type = property.DeclaringType;
				Type t = null;

				if (decl_type == typeof (RegExpConstructor)) {
					t = typeof (GlobalObject);
					ig.Emit (OpCodes.Call, t.GetProperty (FieldName (decl_type)).GetGetMethod ());
					CodeGenerator.load_engine (InFunction, ig);
					ig.Emit (OpCodes.Call, typeof (Convert).GetMethod ("ToObject2"));
					ig.Emit (OpCodes.Castclass, decl_type);
				} else if (decl_type == typeof (ScriptFunction) ||
					   decl_type == typeof (ScriptObject)) {
					t = typeof (GlobalObject);
					ig.Emit (OpCodes.Call, t.GetProperty (((Identifier) obj).name.Value).GetGetMethod ());
				}
				ig.Emit (OpCodes.Call, decl_type.GetProperty (property.Name).GetGetMethod ());
				break;
			default:
				Type lb_type = typeof (LateBinding);
				LocalBuilder lateBinder = ig.DeclareLocal (lb_type);
				Identifier prop = (Identifier) prop_name;

				ig.Emit (OpCodes.Ldstr, prop.name.Value);
				ig.Emit (OpCodes.Newobj, lb_type.GetConstructor (new Type [] { typeof (string)} ));
				ig.Emit (OpCodes.Stloc, lateBinder);
				ig.Emit (OpCodes.Ldloc, lateBinder);
				ig.Emit (OpCodes.Dup);

				Identifier obj_name = (Identifier) obj;

				ig.Emit (OpCodes.Call, typeof (GlobalObject).GetProperty (obj_name.name.Value).GetGetMethod ());
				ig.Emit (OpCodes.Stfld, lb_type.GetField ("obj"));
				ig.Emit (OpCodes.Call, lb_type.GetMethod ("GetNonMissingValue"));

				break;

			}
			emit_box (ig, minfo);
		}			

		void emit_box (ILGenerator ig, MemberInfo info)
		{
			MemberTypes member_type = info.MemberType;
			Type type = null;

			switch (member_type) {
			case MemberTypes.Field:
				type = ((FieldInfo) info).FieldType;
				break;
			}
			if (type != null)
				ig.Emit (OpCodes.Box, type);
		}

		private string FieldName (Type type)
		{
			if (type == typeof (RegExpConstructor))
				return "RegExp";
			throw new NotImplementedException ();
		}

		void emit_late_binding (EmitContext ec)			
		{
			LocalBuilder local_lb = init_late_binding (ec);
			emit_late_get_or_set (ec, local_lb);
		}

		LocalBuilder init_late_binding (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Type lb_type = typeof (LateBinding);

			LocalBuilder local = ig.DeclareLocal (lb_type);
			
			string prop_name = (right as Identifier).name.Value;
			ig.Emit (OpCodes.Ldstr, prop_name);
			ig.Emit (OpCodes.Newobj, lb_type.GetConstructor (new Type [] {typeof (string)}));
			ig.Emit (OpCodes.Stloc, local);			

			return local;
		}
		
		void emit_late_get_or_set (EmitContext ec, LocalBuilder lb_builder)
		{
			ILGenerator ig = ec.ig;
			LocalBuilder right_obj = ig.DeclareLocal (typeof (object));
			//
			// If right_side isn't supplied we take the new value from the stack.
			// This case is used by PostOrPrefixOperator.Emit when it encounters a
			// Binary node as its operand.
			//
			if (right_side == null && assign) {
				ig.Emit (OpCodes.Box, typeof (object));
				ig.Emit (OpCodes.Stloc, right_obj);
			}
			Type type = SemanticAnalyser.IsLiteral (left);

			if (type == null) {
				ig.Emit (OpCodes.Ldloc, lb_builder);
				ig.Emit (OpCodes.Dup);
			}				
			left.Emit (ec);

			LocalBuilder local_literal = null;

			//
			// If the left hand side is as literal we must create a local
			// var where is kept for future use
			//
			if (type != null) {
				local_literal = ig.DeclareLocal (type);
				ig.Emit (OpCodes.Dup);
				ig.Emit (OpCodes.Stloc, local_literal);
				
				ig.Emit (OpCodes.Ldloc, lb_builder);
				ig.Emit (OpCodes.Dup);
				ig.Emit (OpCodes.Ldloc, local_literal);
			} else {
				CodeGenerator.load_engine (InFunction, ec.ig);
				ig.Emit (OpCodes.Call , typeof (Convert).GetMethod ("ToObject"));
			}
			Type lb_type = typeof (LateBinding);
			ig.Emit (OpCodes.Stfld, lb_type.GetField ("obj"));
			
			if (assign) {
				if (right_side != null)
					right_side.Emit (ec);
				else
					ig.Emit (OpCodes.Ldloc, right_obj);
				CodeGenerator.EmitBox (ig, right_side);
				ig.Emit (OpCodes.Call, lb_type.GetMethod ("SetValue"));
			} else
				ig.Emit (OpCodes.Call, lb_type.GetMethod ("GetNonMissingValue"));
		}
		
		internal void get_default_this (ILGenerator ig)
		{
			CodeGenerator.load_engine (InFunction, ig);
			ig.Emit (OpCodes.Call, typeof (Microsoft.JScript.Vsa.VsaEngine).GetMethod ("ScriptObjectStackTop"));
			Type iact_obj = typeof (IActivationObject);
			ig.Emit (OpCodes.Castclass, iact_obj);
			ig.Emit (OpCodes.Callvirt, iact_obj.GetMethod ("GetDefaultThisObject"));
		}

		internal void emit_array_access (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			LocalBuilder right_obj = ig.DeclareLocal (typeof (object));
			if (right_side == null && assign) {
				LocalBuilder val_obj = ig.DeclareLocal (typeof (object));
				ig.Emit (OpCodes.Stloc, val_obj);
				ig.Emit (OpCodes.Box, typeof (object));
				ig.Emit (OpCodes.Stloc, right_obj);
				ig.Emit (OpCodes.Ldloc, val_obj);
			}
			ig.Emit (OpCodes.Ldc_I4_1);
			ig.Emit (OpCodes.Newarr, typeof (object));
			ig.Emit (OpCodes.Dup);
			ig.Emit (OpCodes.Ldc_I4_0);
			if (right != null) {
				right.Emit (ec);
				CodeGenerator.EmitBox (ig, right);
			}
			ig.Emit (OpCodes.Stelem_Ref);			

			if (assign) {
				if (right_side != null) {
					right_side.Emit (ec);
					CodeGenerator.EmitBox (ig, right_side);
				}
				else
					ig.Emit (OpCodes.Ldloc, right_obj);
				ig.Emit (OpCodes.Call, typeof (LateBinding).GetMethod ("SetIndexedPropertyValueStatic"));
			} else {
				ig.Emit (OpCodes.Ldc_I4_0);
				ig.Emit (OpCodes.Ldc_I4_1);
				CodeGenerator.load_engine (InFunction, ig);
				ig.Emit (OpCodes.Call, typeof (LateBinding).GetMethod ("CallValue"));
			}
		}

		internal void emit_op_eval (ILGenerator ig)
		{
			switch (op) {
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
			
			if (op == JSToken.Plus) {
				t = typeof (Plus);
				local_builder = ig.DeclareLocal (t);				
				ig.Emit (OpCodes.Newobj, t.GetConstructor (new Type [] {}));
				ig.Emit (OpCodes.Stloc, local_builder);
				ig.Emit (OpCodes.Ldloc, local_builder);
				return;
			} else if (op == JSToken.Minus) {
				t = typeof (NumericBinary);
				local_builder = ig.DeclareLocal (t);
				ig.Emit (OpCodes.Ldc_I4_S, (byte) 47);
			} else if (op == JSToken.LeftShift) {
				t = typeof (BitwiseBinary);
				local_builder = ig.DeclareLocal (t);
				ig.Emit (OpCodes.Ldc_I4_S, (byte) 61);
			} else if (op == JSToken.RightShift) {
				t = typeof (BitwiseBinary);
				local_builder = ig.DeclareLocal (t);
				ig.Emit (OpCodes.Ldc_I4_S, (byte) 62);
			} else if (op == JSToken.UnsignedRightShift) {
				t = typeof (BitwiseBinary);
				local_builder = ig.DeclareLocal (t);
				ig.Emit (OpCodes.Ldc_I4_S, (byte) 63);
			} else if (op == JSToken.Multiply) {
				t = typeof (NumericBinary);
				local_builder = ig.DeclareLocal (t);
				ig.Emit (OpCodes.Ldc_I4_S, (byte) 64);
			} else if (op == JSToken.Divide) {
				t = typeof (NumericBinary);
				local_builder = ig.DeclareLocal (t);
				ig.Emit (OpCodes.Ldc_I4_S, (byte) 65);
			} else if (op == JSToken.Modulo) {
				t = typeof (NumericBinary);
				local_builder = ig.DeclareLocal (t);
				ig.Emit (OpCodes.Ldc_I4_S, (byte) 66);
			} else if (op == JSToken.BitwiseOr) {
				t = typeof (BitwiseBinary);
				local_builder = ig.DeclareLocal (t);
				ig.Emit (OpCodes.Ldc_I4_S, (byte) 50);
			} else if (op == JSToken.BitwiseXor) {
				t = typeof (BitwiseBinary);
				local_builder = ig.DeclareLocal (t);
				ig.Emit (OpCodes.Ldc_I4_S, (byte) 51);				
			} else if (op == JSToken.BitwiseAnd) {
				t = typeof (BitwiseBinary);
				local_builder = ig.DeclareLocal (t);
				ig.Emit (OpCodes.Ldc_I4_S, (byte) 52);
			}
			ig.Emit (OpCodes.Newobj, t.GetConstructor (new Type [] {typeof (int)}));
			ig.Emit (OpCodes.Stloc, local_builder);
			ig.Emit (OpCodes.Ldloc, local_builder);
		}

		internal bool IsDeletable (out bool isCtr)
		{
			if (left is Identifier && right is Identifier)
				return SemanticAnalyser.IsDeletable ((Identifier) left, (Identifier) right, out isCtr);
			isCtr = false;
			return false;
		}
	}

	internal class Conditional : Exp {

		AST cond_exp, true_exp, false_exp;

		internal Conditional (AST parent, AST expr, AST  trueExpr, AST falseExpr, Location location)
			: base (parent, location)
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

		internal override bool Resolve (Environment env)
		{
			bool r = true;
			if (cond_exp != null)
				r &= cond_exp.Resolve (env);
			if (true_exp != null)
				r &= true_exp.Resolve (env);
			if (false_exp != null)
				r &= false_exp.Resolve (env);
			return r;
		}

		internal override bool Resolve (Environment env, bool no_effect)
		{
			this.no_effect = no_effect;
			return Resolve (env);
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Label false_label = ig.DefineLabel ();
			Label merge_label = ig.DefineLabel ();
			CodeGenerator.fall_true (ec, cond_exp, false_label);
			if (true_exp != null) {
				true_exp.Emit (ec);
				CodeGenerator.EmitBox (ig, true_exp);
			}
			ig.Emit (OpCodes.Br, merge_label);
			ig.MarkLabel (false_label);
			if (false_exp != null) {
				false_exp.Emit (ec);
				CodeGenerator.EmitBox (ig, false_exp);
			}
			ig.MarkLabel (merge_label);
			if (no_effect)
				ig.Emit (OpCodes.Pop);
		}
	}

	internal interface ICallable {
		void AddArg (AST arg);
	}
	
	internal class Call : Exp, ICallable {
		
		internal AST member_exp;		
		internal Args args;
		internal object binding;
		internal Type bind_type;
		private bool is_dynamic_function = false;
		private bool need_this = false;

		internal Call (AST parent, AST exp, Location location)
			: base (parent, location)
		{
			this.member_exp = exp;
			this.args = new Args (location);
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

		internal override bool Resolve (Environment env)
		{
			bool r = true;

			if (member_exp != null) {
				if (member_exp is Identifier) {
					member_exp.Resolve (env);
					binding = env.Get (String.Empty, (member_exp as Identifier).name);
					bind_type = binding.GetType ();

					if (bind_type == typeof (BuiltIn)) {
						BuiltIn built_in = binding as BuiltIn;
						if (!built_in.IsFunction)
							throw new Exception ("error JS5002 A: function expected.");
						if (IsGlobalObjectMethod (built_in)) {
							//
							// If a method contains an eval invocation
							// we must generate proper code for accessing 
							// the local vars in the StackFrame.
							//
							if (((Identifier) member_exp).name.Value == "eval") {
								Function cont_func = GetContainerFunction;
								if (cont_func != null)
									SemanticAnalyser.AddMethodWithEval (cont_func.func_obj.name);
							}
						}
					}
				} else if (member_exp is Binary) {
					member_exp.Resolve (env);
					Binary bin = (Binary) member_exp;
					if (bin.AccessField)
						binding = bin.Binding;
					if (binding is MethodInfo)
						NeedThis ((MethodInfo) binding);
				} else
					r &= member_exp.Resolve (env);

				args.BoundToMethod = binding;

				if (args != null)
					r &= args.Resolve (env);
			} else
				throw new Exception ("Call.Resolve, member_exp can't be null");
			return r;
		}

		private void NeedThis (MethodInfo method)
		{
			need_this = SemanticAnalyser.Needs (JSFunctionAttributeEnum.HasThisObject, method);
		}

		internal override bool Resolve (Environment env, bool no_effect)
		{
			this.no_effect = no_effect;
			return Resolve (env);
		}

		internal override void Emit (EmitContext ec)
		{
			if (bind_type == typeof (BuiltIn)) {
				BuiltIn b = binding as BuiltIn;
				if (b.IsPrint) {
					emit_print_stm (ec);
					return;
				} else if (IsGlobalObjectMethod (binding)) {
					bool eval = IsEval (binding);
					bool in_func = InFunction;
					
					if (eval && in_func)
						CodeGenerator.load_local_vars (ec.ig, in_func);
					
					args.Emit (ec);

					if (eval) {
#if (!NET_1_0)
						ec.ig.Emit (OpCodes.Ldnull);
#endif
						CodeGenerator.load_engine (in_func, ec.ig);
					}
					member_exp.Emit (ec);
					
					if (eval && no_effect && in_func)
						ec.ig.Emit (OpCodes.Pop);

					if (eval && in_func) {
						set_local_vars (ec.ig);
						return;
					}
				} else if (IsConstructorProperty (binding)) {
 					member_exp.Emit (ec);
 					EmitBuiltInArgs (ec);
 					EmitInvoke (ec);					
				}
			} else if (bind_type == typeof (FunctionDeclaration) || bind_type == typeof (FunctionExpression)) {
				Function function = binding as Function;
				MethodBuilder method = (MethodBuilder) TypeManager.Get (function.func_obj.name);

				if (SemanticAnalyser.MethodContainsEval (function.func_obj.name) ||
				    SemanticAnalyser.MethodReferenceOutterScopeVar (function.func_obj.name) ||
				    SemanticAnalyser.MethodVarsUsedNested (function.func_obj.name)) {
					if (InFunction)
						CodeGenerator.load_local_vars (ec.ig, true);
					emit_late_call (ec);
					if (no_effect)
						ec.ig.Emit (OpCodes.Pop);
					if (InFunction)
						set_local_vars (ec.ig);
					return;
				} else {
					emit_func_call (method, ec);
					return;
				}
			} else if (binding is MemberInfo) {
				MemberInfo minfo = (MemberInfo) binding;
				MemberTypes member_type = minfo.MemberType;
				ILGenerator ig = ec.ig;				
				
				if (member_type == MemberTypes.Method) {
					if (member_exp is Binary) {
						Binary bin = (Binary) member_exp;
						if (bin.left is ICanLookupPrototype && need_this ) {
							bin.left.Emit (ec);
							CodeGenerator.EmitBox (ig, bin.left);
						}
					}						
					args.Emit (ec);
					MethodInfo method = (MethodInfo) minfo;
					ig.Emit (OpCodes.Call, method);
					Type return_type = method.ReturnType;
					if (return_type != typeof (void) && return_type != typeof (string))
						ig.Emit (OpCodes.Box, return_type);
				} else if (member_type == MemberTypes.Property) {
					if (member_exp is Binary) {
						Binary bin = (Binary) member_exp;

						bin.left.Emit (ec);

						bool in_func = InFunction;
						CodeGenerator.load_engine (in_func, ig);
						ig.Emit (OpCodes.Call, typeof (Convert).GetMethod ("ToObject2"));

						if (bin.left is Identifier) {
							Type coerce_to = null;
							if (((Identifier) bin.left).name.Value == "Function")
								coerce_to = typeof (FunctionConstructor);

							LocalBuilder ctr = null;

							if (coerce_to != null) {
								ig.Emit (OpCodes.Castclass, coerce_to);
								ctr = ig.DeclareLocal (coerce_to);
							}

							ig.Emit (OpCodes.Dup);
							ig.Emit (OpCodes.Stloc, ctr);
							bin.Emit (ec);

							ig.Emit (OpCodes.Ldc_I4, args.Size);
							ig.Emit (OpCodes.Newarr, typeof (object));

							ig.Emit (OpCodes.Ldc_I4_0);
							ig.Emit (OpCodes.Ldc_I4_0);

							CodeGenerator.load_engine (in_func, ig);

							ig.Emit (OpCodes.Call, typeof (LateBinding).GetMethod ("CallValue"));
						}
					}
				} else {
					Console.WriteLine ("member_type = {0}", member_type);
					Console.WriteLine ("member_exp = {0}", member_exp);
					Console.WriteLine ("line_number = {0}", location.LineNumber);
					throw new NotImplementedException ();
				}
			} else
				emit_late_call (ec);
			
			if (no_effect)
				ec.ig.Emit (OpCodes.Pop);			
		}

		internal void emit_print_stm (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			
			if (args == null || args.Size == 0) {
				ig.Emit (OpCodes.Ldstr, "");
				ig.Emit (OpCodes.Call, typeof (ScriptStream).GetMethod ("WriteLine"));
				return;
			}

			Type script_stream = typeof (ScriptStream);
			MethodInfo write = script_stream.GetMethod ("Write");
			MethodInfo writeline = script_stream.GetMethod ("WriteLine");
			MethodInfo to_string = typeof (Convert).GetMethod ("ToString",
									   new Type [] { typeof (object), typeof (bool) });
			AST ast;
			int n = args.Size - 1;

			for (int i = 0; i <= n; i++) {
				ast = args.get_element (i);

				if (ast is Assign)
					CodeGenerator.EmitAssignAsExp (ec, ast);
				else
					ast.Emit (ec);

				if (ast is Relational) 
					CodeGenerator.EmitRelationalComp (ig, (Relational) ast);
				else
					CodeGenerator.EmitBox (ig, ast);

				if (!(ast is StringLiteral)){
					ig.Emit (OpCodes.Ldc_I4_1);
					ig.Emit (OpCodes.Call, to_string);
				}
					
				if (i == n)
					ig.Emit (OpCodes.Call, writeline);
				else
					ig.Emit (OpCodes.Call, write);
			}
			
		}

		void emit_late_call (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			if (member_exp is Binary) {
				Binary bin = member_exp as Binary;
				if (SemanticAnalyser.IsLiteral (bin.left) != null) {
					member_exp.Emit (ec);
					CodeGenerator.EmitBox (ig, member_exp);
					setup_late_call_args (ec);
					ig.Emit (OpCodes.Call, typeof (LateBinding).GetMethod ("CallValue"));
				} else if (bin.right is Identifier) {
					Identifier rside = bin.right as Identifier;
					Type lb_type = typeof (LateBinding);
				
					LocalBuilder lb = ig.DeclareLocal (lb_type);

					ig.Emit (OpCodes.Ldstr, rside.name.Value);
					ig.Emit (OpCodes.Newobj , lb_type.GetConstructor (new Type [] {typeof (string)}));
					ig.Emit (OpCodes.Stloc, lb);
					init_late_binding (ec, lb);
					setup_late_call_args (ec);
					ig.Emit (OpCodes.Call, typeof (LateBinding).GetMethod ("Call"));
				} else {
					bin.left.Emit (ec);
					CodeGenerator.EmitBox (ig, bin.left);

					member_exp.Emit (ec);
					CodeGenerator.EmitBox (ig, member_exp);

					setup_late_call_args (ec);
					ig.Emit (OpCodes.Call, typeof (LateBinding).GetMethod ("CallValue"));
				}
			} else {
				get_global_scope_or_this (ec.ig);
				member_exp.Emit (ec);
				CodeGenerator.EmitBox (ig, member_exp);
				setup_late_call_args (ec);
				ig.Emit (OpCodes.Call, typeof (LateBinding).GetMethod ("CallValue"));
			}
		}

		internal void get_global_scope_or_this (ILGenerator ig)
		{
			is_dynamic_function = parent is FunctionExpression && parent == member_exp;

			if (is_dynamic_function) {
				ig.Emit (OpCodes.Ldarg_0);
				ig.Emit (OpCodes.Ldfld, typeof (ScriptObject).GetField ("engine"));
			} else
				CodeGenerator.load_engine (InFunction, ig);
			ig.Emit (OpCodes.Call, typeof (Microsoft.JScript.Vsa.VsaEngine).GetMethod ("ScriptObjectStackTop"));
			Type iact_obj = typeof (IActivationObject);
			ig.Emit (OpCodes.Castclass, iact_obj);

			//
			// FIXME: Find out the exact discrimination
			// for: GetGlobalScope and GetDefaultThisObject.
			// For example, in program: print (function () { return 1; } ());
			// we invoke GetGlobalScope, in other cases GetDefaultThisObject 
			//
			if (member_exp is FunctionExpression)
				ig.Emit (OpCodes.Callvirt, iact_obj.GetMethod ("GetGlobalScope"));
			else
				ig.Emit (OpCodes.Callvirt, iact_obj.GetMethod ("GetDefaultThisObject"));
		}

		void init_late_binding (EmitContext ec, LocalBuilder local)
		{
			ILGenerator ig = ec.ig;

			ig.Emit (OpCodes.Ldloc, local);
			ig.Emit (OpCodes.Dup);
			
			AST left = (member_exp as Binary).left;
			left.Emit (ec);

			CodeGenerator.load_engine (InFunction, ig);

			ig.Emit (OpCodes.Call , typeof (Convert).GetMethod ("ToObject"));

			Type lb_type = typeof (LateBinding);

			ig.Emit (OpCodes.Stfld, lb_type.GetField ("obj"));
		}

		void setup_late_call_args (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			int n = args.Size;
			AST ast = null;

			ig.Emit (OpCodes.Ldc_I4, n);
			ig.Emit (OpCodes.Newarr, typeof (object));
			
			for (int i = 0; i < n; i++) {
				ig.Emit (OpCodes.Dup);
				ig.Emit (OpCodes.Ldc_I4, i);
				ast = args.get_element (i);
				ast.Emit (ec);
				CodeGenerator.EmitBox (ig, ast);
				ig.Emit (OpCodes.Stelem_Ref);
			}

			ig.Emit (OpCodes.Ldc_I4_0);
			ig.Emit (OpCodes.Ldc_I4_0);

			if (is_dynamic_function) {
				ig.Emit (OpCodes.Ldarg_0);
				ig.Emit (OpCodes.Ldfld, typeof (ScriptObject).GetField ("engine"));
			} else
				CodeGenerator.load_engine (InFunction, ig);
		}

		void emit_func_call (MethodBuilder mb, EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			CodeGenerator.load_engine (InFunction, ig);
			ig.Emit (OpCodes.Call, typeof (VsaEngine).GetMethod ("ScriptObjectStackTop"));
			Type iact_obj = typeof (IActivationObject);
			ig.Emit (OpCodes.Castclass, iact_obj);
			ig.Emit (OpCodes.Callvirt, iact_obj.GetMethod ("GetDefaultThisObject"));
			CodeGenerator.load_engine (InFunction, ig);
			args.Emit (ec);
			ig.Emit (OpCodes.Call, mb);

			if (!return_void (mb) && no_effect)
				ig.Emit (OpCodes.Pop);
 		}

		bool return_void (MethodBuilder mb)
		{
			return mb.ReturnType == typeof (void);
		}

		void EmitBuiltInArgs (EmitContext ec)
		{
			if (member_exp.ToString () == "Date")
				return;

			ILGenerator ig = ec.ig;
			int n = args.Size;
			AST ast = null;
			
			if (n >= 1 && (member_exp.ToString () == "String" || member_exp.ToString () == "Boolean" || member_exp.ToString () == "Number")) {
				ast = args.get_element (0);

				if (ast is Assign)
					CodeGenerator.EmitAssignAsExp (ec, ast);
				else {
					ast.Emit (ec);
					CodeGenerator.EmitBox (ig, ast);
				}
				return;
			}

			ig.Emit (OpCodes.Ldc_I4, n);
			ig.Emit (OpCodes.Newarr, typeof (object));

			for (int i = 0; i < n; i++) {
				ig.Emit (OpCodes.Dup);
				ig.Emit (OpCodes.Ldc_I4, i);
				ast = args.get_element (i);

				if (ast is Assign)
					CodeGenerator.EmitAssignAsExp (ec, ast);
				else {
					ast.Emit (ec);
					CodeGenerator.EmitBox (ig, ast);
				}

				ig.Emit (OpCodes.Stelem_Ref);
			}
		}

		void EmitInvoke (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			string name = member_exp.ToString ();
			Type type = null;
			bool boolean = false;
			bool number = false;

			switch (name) {
			case "Object":
				type = typeof (ObjectConstructor);
				break;
			case "Function":
				type = typeof (FunctionConstructor);
				break;
			case "Array":
				type = typeof (ArrayConstructor);
				break;
			case "String":
				type = typeof (StringConstructor);
				break;
			case "Boolean":
				type = typeof (BooleanConstructor);
				boolean = true;
				break;
			case "Number":
				type = typeof (NumberConstructor);
				number = true;
				break;
			case "Date":
				type = typeof (DateConstructor);
				break;
			case "RegExp":
				type = typeof (RegExpConstructor);
				break;
			case "Error":
			case "EvalError":
			case "RangeError":
			case "ReferenceError":
			case "SyntaxError":
			case "TypeError":
			case "URIError":
				type = typeof (ErrorConstructor);
				break;
			}
			ig.Emit (OpCodes.Call, type.GetMethod ("Invoke"));
			if (boolean)
				ig.Emit (OpCodes.Box, typeof (Boolean));
			if (number)
				ig.Emit (OpCodes.Box, typeof (Double));
		}

		bool IsConstructorProperty (object binding)
		{
			if (!(binding is BuiltIn))
				return false;

			string name = (binding as BuiltIn).Name;
			
			return (name == "Object" || name == "Function" || name == "Array" || name == "String" || name == "Boolean" || name == "Number" || name == "Date" || name == "RegExp" || name == "Error" || name == "EvalError" || name == "RangeError" || name == "ReferenceError" || name == "SyntaxError" || name == "TypeError" || name == "URIError");
		}

		bool IsGlobalObjectMethod (object binding)
		{
			if (binding == null || binding.GetType () != typeof (BuiltIn))
				return false;
			
			BuiltIn bind = binding as BuiltIn;
			switch (bind.Name) {
			case "eval":
			case "parseInt":
			case "parseFloat":
			case "isNaN":
			case "isFinite":
			case "decodeURI":
			case "decodeURIComponent":
			case "encodeURI":
			case "encodeURIComponent":
			case "escape":
			case "unescape":
				return true;
			default:
				return false;
			}
		}

		bool IsEval (object binding)
		{
			if (binding == null || binding.GetType () != typeof (BuiltIn))
				return false;
			BuiltIn bind = (BuiltIn) binding;
			return bind.Name == "eval";
		}

		internal void set_local_vars (ILGenerator ig)
		{
			int n = 0;
			Type stack_frame = typeof (StackFrame);

			CodeGenerator.load_engine (InFunction, ig);

			ig.Emit (OpCodes.Call, typeof (VsaEngine).GetMethod ("ScriptObjectStackTop"));
			ig.Emit (OpCodes.Castclass, stack_frame);
			ig.Emit (OpCodes.Ldfld, stack_frame.GetField ("localVars"));

			object [] locals = TypeManager.CurrentLocals;
			n = locals != null ? locals.Length : 0;
			object local = null;

			for (int i = 0; i < n; i++) {
				local = locals [i];
				if (local is LocalBuilder) {
					ig.Emit (OpCodes.Dup);
					ig.Emit (OpCodes.Ldc_I4, i);
					ig.Emit (OpCodes.Ldelem_Ref);
					ig.Emit (OpCodes.Stloc, (LocalBuilder) local);
				}
			}
			ig.Emit (OpCodes.Pop);
		}
	}

	interface IAccessible {
		bool ResolveFieldAccess (AST parent);
	}

	internal interface ICanLookupPrototype {
		bool ResolveFieldAccess (AST parent);
	}

	internal class Identifier : Exp, IAssignable, IAccessible {
		
		internal Symbol name;
		internal AST binding;
		internal bool assign;
		AST right_side;
		
		int lexical_difference;
		const int MINIMUM_DIFFERENCE = 1;
		bool no_field;
		LocalBuilder local_builder;

		private bool undeclared = false;

		internal Identifier (AST parent, string id, Location location)
			: base (parent, location)
		{
			this.name = Symbol.CreateSymbol (id);
		}

		bool NeedParents {
			get { return lexical_difference >= MINIMUM_DIFFERENCE && no_field; }
		}

		public override string ToString ()
		{
			return name.Value;
		}

		internal override bool Resolve (Environment env)
		{
			bool contained = env.Contains (String.Empty, this.name);
			if (contained) {
				binding = (AST) env.Get (String.Empty, this.name);
				if (binding is VariableDeclaration) {
					VariableDeclaration decl = (VariableDeclaration) binding;
					lexical_difference = env.Depth (String.Empty) - decl.lexical_depth;
					no_field = decl.lexical_depth != 0;

					if (no_field && lexical_difference > 0 && !env.CatchScope (String.Empty)) {
						Function container_func = GetContainerFunction;
						SemanticAnalyser.AddMethodReferenceOutterScopeVar (container_func.func_obj.name, decl);
						env.Enter (String.Empty, Symbol.CreateSymbol (decl.id), decl);
						SemanticAnalyser.AddMethodVarsUsedNested (decl.GetContainerFunction.func_obj.name, decl);
					}
				}
			} else if (SemanticAnalyser.NoFast) {
				undeclared = true;
				Console.WriteLine ("warning JS1135: Variable '" + name + "' has not been declared");
			} else {
				//
				// Identifier not into stand-alone JScript, we'll search into 
				// referenced assemblies and namespaces 
				//
				throw new Exception (location.SourceName + "(" + location.LineNumber + 
					     ",0) : error JS1135: Variable '" + name + "' has not been declared");
			}
			return true;
		}

		internal override bool Resolve (Environment env, bool no_effect)
		{
			this.no_effect = no_effect;
			return Resolve (env);
		}

		public bool ResolveAssign (Environment env, AST right_side)
		{
			this.assign = true;
			this.no_effect = false;
			this.right_side = right_side;
			if (name.Value != String.Empty)			
				return Resolve (env);
			return true;
		}

		//
		// Throws an exception if parent is a native object
		// and does not contains name as member.
		// Returns true if parent contains name as member. 
		// Returns false if parent is not a native object,
		// this indicate that late binding code must get generated.
		//
		public bool ResolveFieldAccess (AST parent)
		{
			if (parent is Identifier) {
				Identifier p = parent as Identifier;
				return is_static_property (p.name.Value, name.Value);
			}
			//
			// Return false so late binding will take place
			//
			return false;
		}

		//
		// If obj_name is a native object, search in its
		// constructor if contains a prop_name, otherwise return
		// false letting late binding taking place
		//
		bool is_static_property (string obj_name, string prop_name)
		{
			bool native_obj = SemanticAnalyser.is_js_object (obj_name);

			if (!native_obj)
				return false;

			bool contains_method;
			contains_method = SemanticAnalyser.object_contains (
					   SemanticAnalyser.map_to_ctr (obj_name), LateBinding.MapToInternalName (prop_name));
			if (!contains_method)
				throw new Exception ("error: JS0438: Object " + obj_name + " doesn't support this property or method:" + prop_name);
			return true;
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			if (assign && right_side != null && !undeclared) {
				right_side.Emit (ec);
				CodeGenerator.EmitBox (ig, right_side);
			}
			if (binding is FormalParam) {
				FormalParam f = binding as FormalParam;
				if (assign)
					ig.Emit (OpCodes.Starg, (short) f.pos);
				else
					ig.Emit (OpCodes.Ldarg_S, f.pos);
			} else if (binding is VariableDeclaration || binding is Try || binding is Catch) {
				if (NeedParents) {
					CodeGenerator.emit_parents (InFunction, lexical_difference, ig);
					store_stack_frame_into_locals (ec.ig);
					if (this.local_builder != null)
						if (assign)
							ig.Emit (OpCodes.Stloc, local_builder);
						else
							ig.Emit (OpCodes.Ldloc, local_builder);
				} else {
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
			} else if (binding is BuiltIn)
				binding.Emit (ec);
			else if (binding is FunctionDeclaration)
				load_script_func (ec, (FunctionDeclaration) binding);
			else if (binding == null) { // it got referenced before was declared and initialized
				if (undeclared) {
					if (assign)
						emit_undeclared_assignment (ec);
					else
						emit_undeclared_use (ig);
				}
			} else
				Console.WriteLine ("Identifier.Emit, binding.GetType = {0}", binding.GetType ());

			if (!assign && no_effect && !undeclared)
				ig.Emit (OpCodes.Pop);
  		}
  
		private void emit_undeclared_assignment (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			string name = this.name.Value;
			Type lb_type = typeof (LateBinding);
			bool in_function = InFunction;

			MethodInfo script_object_stack_top = typeof (VsaEngine).GetMethod ("ScriptObjectStackTop");
			Type iactivation_obj = typeof (IActivationObject);

			Label label_no_field = ig.DefineLabel ();
			Label label_end = ig.DefineLabel ();

			LocalBuilder lb_local = ig.DeclareLocal (lb_type);
			LocalBuilder field_builder = ig.DeclareLocal (typeof (FieldInfo));
			LocalBuilder tmp_helper = ig.DeclareLocal (typeof (object));

			ig.Emit (OpCodes.Ldstr, name);
			CodeGenerator.load_engine (in_function, ig);
			ig.Emit (OpCodes.Call, script_object_stack_top);
			ig.Emit (OpCodes.Newobj, lb_type.GetConstructor (new Type [] { typeof (string), typeof (object) }));
			ig.Emit (OpCodes.Stloc, lb_local);
			CodeGenerator.load_engine (in_function, ig);
			ig.Emit (OpCodes.Call, script_object_stack_top);
			ig.Emit (OpCodes.Castclass, iactivation_obj);
			ig.Emit (OpCodes.Ldstr, name);

			// FIXME: compute the needed lexical level, for now is hardcoded
			ig.Emit (OpCodes.Ldc_I4_0);
			ig.Emit (OpCodes.Callvirt, iactivation_obj.GetMethod ("GetField"));
			ig.Emit (OpCodes.Stloc, field_builder);

			ig.Emit (OpCodes.Ldloc, lb_local);

			right_side.Emit (ec);
			CodeGenerator.EmitBox (ec.ig, right_side);

			ig.Emit (OpCodes.Ldloc, field_builder);		
			ig.Emit (OpCodes.Ldnull);
			ig.Emit (OpCodes.Beq_S, label_no_field);

			ig.Emit (OpCodes.Stloc, tmp_helper); // Store value
			ig.Emit (OpCodes.Pop); // Pop local builder
			
			ig.Emit (OpCodes.Ldloc, field_builder);
			ig.Emit (OpCodes.Ldstr, name);
			ig.Emit (OpCodes.Ldloc, tmp_helper);
			ig.Emit (OpCodes.Callvirt, typeof (FieldInfo).GetMethod ("SetValue", new Type [] { typeof (object), typeof (object) }));
			
			ig.Emit (OpCodes.Br_S, label_end);

			ig.MarkLabel (label_no_field);
			ig.Emit (OpCodes.Call, lb_type.GetMethod ("SetValue"));
			ig.MarkLabel (label_end);
		}

		private void emit_undeclared_use (ILGenerator ig)
		{
			Label merge = ig.DefineLabel ();

			string name = this.name.Value;
			Type lb_type = typeof (LateBinding);
			LocalBuilder lb_local = ig.DeclareLocal (lb_type);
			bool in_function = InFunction;
			MethodInfo script_object_stack_top = typeof (VsaEngine).GetMethod ("ScriptObjectStackTop");
			Type iactivation_obj = typeof (IActivationObject);

			ig.Emit (OpCodes.Ldstr, name);
			CodeGenerator.load_engine (in_function, ig);
			ig.Emit (OpCodes.Call, script_object_stack_top);
			ig.Emit (OpCodes.Newobj, lb_type.GetConstructor (new Type [] { typeof (string), typeof (object) }));
			ig.Emit (OpCodes.Stloc, lb_local);
			CodeGenerator.load_engine (in_function, ig);
			ig.Emit (OpCodes.Call, script_object_stack_top);
			ig.Emit (OpCodes.Castclass, iactivation_obj);
			ig.Emit (OpCodes.Ldstr, name);

			// FIXME: compute the needed lexical level, for now is hardcoded
			ig.Emit (OpCodes.Ldc_I4_0);

			ig.Emit (OpCodes.Callvirt, iactivation_obj.GetMethod ("GetMemberValue"));
			ig.Emit (OpCodes.Dup);
			ig.Emit (OpCodes.Call, typeof (Binding).GetMethod ("IsMissing"));

			ig.Emit (OpCodes.Brfalse, merge);

			ig.Emit (OpCodes.Pop);

			ig.Emit (OpCodes.Ldloc, lb_local);
			ig.Emit (OpCodes.Call, lb_type.GetMethod ("GetValue2"));
			ig.MarkLabel (merge);
		}

		internal void EmitStore (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			if (binding is FormalParam) {
				FormalParam f = binding as FormalParam;
				ig.Emit (OpCodes.Starg, (short) f.pos);
			} else if (binding is VariableDeclaration || binding is Try) {
				FieldInfo fb = extract_field_info (binding);
				LocalBuilder local = extract_local_builder (binding);
				if (fb == null)
					ig.Emit (OpCodes.Stloc, local);
				else
					ig.Emit (OpCodes.Stsfld, fb);					
			}
		}

		internal void EmitLoad (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			if (binding is FormalParam) {
				FormalParam f = binding as FormalParam;
				ig.Emit (OpCodes.Ldarg_S, f.pos);
			} else if (binding is VariableDeclaration || binding is Try) {
				FieldInfo fb = extract_field_info (binding);
				LocalBuilder local = extract_local_builder (binding);
				if (fb == null)
					ig.Emit (OpCodes.Ldloc, local);
				else
					ig.Emit (OpCodes.Ldsfld, fb);
			}
		}

		void load_script_func (EmitContext ec, FunctionDeclaration binding)
		{
			object bind = TypeManager.Get (binding.func_obj.name);
						       
			if (bind != null) {
				if (bind is MethodBuilder) {
					TypeBuilder type = ec.type_builder;
					if (binding.InFunction) {
						LocalBuilder local_meth = (LocalBuilder) TypeManager.GetLocalScriptFunction (binding.func_obj.name);
						ec.ig.Emit (OpCodes.Ldloc, local_meth);
					} else {
						FieldInfo method = type.GetField (binding.func_obj.name);
						ec.ig.Emit (OpCodes.Ldsfld, method);
					}
				} else if (bind is LocalBuilder)
					ec.ig.Emit (OpCodes.Ldloc, (LocalBuilder) bind);
				else throw new Exception ("load_script_func");
			}
		}

		internal FieldInfo extract_field_info (AST a)
		{
			FieldInfo r = null;

			if (a is VariableDeclaration)
				r = ((VariableDeclaration) a).field_info;
			else if (a is Try)
				r = ((Try) a).field_info;
			else if (a is Catch)
				r = ((Catch) a).field_info;
			return r;
		}
		
		internal LocalBuilder extract_local_builder (AST a)
		{
			LocalBuilder r = null;
			if (a is VariableDeclaration)
				r = ((VariableDeclaration) a).local_builder;
			else if (a is Try)
				r = ((Try) a).local_builder;
			else if (a is Catch)
				r = ((Catch) a).local_builder;
			return r;
		}
		
		//
		// FIXME: Only must store the extern variables which are used.
		//
		internal void store_stack_frame_into_locals (ILGenerator ig)
		{
			ig.Emit (OpCodes.Dup);

			Type stack_frame = typeof (StackFrame);
			ig.Emit (OpCodes.Castclass, stack_frame);
			ig.Emit (OpCodes.Ldfld, stack_frame.GetField ("localVars"));
			
			DictionaryEntry [] locals = TypeManager.LocalsAtDepth (((VariableDeclaration) binding).lexical_depth);

			int i = 0;
			LocalBuilder local = null;

			foreach (DictionaryEntry entry in locals) {
				if (entry.Value is LocalBuilder) {
					local = ig.DeclareLocal (typeof (object));
					if (entry.Key == name.Value)
						this.local_builder = local;
					ig.Emit (OpCodes.Dup);
					ig.Emit (OpCodes.Ldc_I4, i++);
					ig.Emit (OpCodes.Ldelem_Ref);
					ig.Emit (OpCodes.Stloc, (LocalBuilder) local);
				}
			}
			ig.Emit (OpCodes.Pop);

			//
			// FIXME: what does it determine this?
			//
			ig.Emit (OpCodes.Call, typeof (ScriptObject).GetMethod ("GetParent"));
			ig.Emit (OpCodes.Pop);			
		}
	}

	internal class Args : AST {

		private ArrayList elems;

		//
		// BoundToMethod can be of type:
		// - Function (when we have a function expression or declaration)
		// - MethodInfo (prototype's method invoked through a
		//   literal or access to a method from any built-in
		// object.
		// - BuiltIn for methods from the GlobalObject
		//
		internal object BoundToMethod;

		private bool BoundToDeclaredFunction {
			get { return BoundToMethod is Function; }
		}

		private bool in_new = false;
		internal bool InNew {
			set { in_new = value; }
			get { return in_new; }
		}

		private int expected_args = 0;
		private bool has_this = false;
		private bool var_args = false;
		private bool has_engine = false;

		internal Args (Location location)
			: base (null, location)
		{
			elems = new ArrayList ();
		}

		internal void Add (AST e)
		{
			elems.Add (e);
		}

		internal AST get_element (int i)
		{
			if (0 <= i && i < elems.Count)
				return (AST) elems [i];
			return null;
		}

		internal int Size {
			get {
				if (elems == null)
					return 0;
				return elems.Count;
			}
		}

		internal override bool Resolve (Environment env)
		{
			AST tmp;
			bool r = true;

			if (BoundToMethod is Function)
				expected_args = ((Function) BoundToMethod).NumOfArgs;
			if (BoundToMethod is MethodInfo) {
				MethodInfo method = (MethodInfo) BoundToMethod;
				has_this = SemanticAnalyser.Needs (JSFunctionAttributeEnum.HasThisObject, method);
				var_args = SemanticAnalyser.Needs (JSFunctionAttributeEnum.HasVarArgs, method);
				has_engine = SemanticAnalyser.Needs (JSFunctionAttributeEnum.HasEngine, method);
				expected_args = method.GetParameters ().Length;
			} else if (BoundToMethod is BuiltIn) {
				BuiltIn built_in = (BuiltIn) BoundToMethod;
				if (built_in.IsConstructor || InNew)
					expected_args = (elems == null) ? 0 : elems.Count;
				else
					expected_args = ((BuiltIn) BoundToMethod).NumOfArgs;
			}

			if (elems == null)
				return true;
			
			int n = elems.Count;

			for (int i = 0; i < n; i++) {
				tmp = (AST) elems [i];
				if (tmp != null)
					if (tmp is Exp)
						r &= ((Exp) tmp).Resolve (env, false);
					else
						r &= tmp.Resolve (env);
			}
			return r;
		}

		internal override void Emit (EmitContext ec)
		{
			bool strong_type = BoundToMethod is MethodInfo;
			ParameterInfo [] parameters = null;

			// We may be called more than once, so avoid modifying fields
			int expected_args = this.expected_args;

			if (!BoundToDeclaredFunction) {
				if (has_this)
					expected_args--;
				if (has_engine)
					expected_args--;
				if (var_args)
					expected_args--;
			}

			if (expected_args < 0) {
				if (BoundToDeclaredFunction)
					expected_args = ((Function) BoundToMethod).NumOfArgs;
				else
					throw new Exception ("expected_args can't be negative");
			}

			//
			// When BoundToMethod is null and the semantic
			// analysis passed means that the method is 'print'
			//
			// This should be on Resolve but it's here as
			// a work around of the various passes that
			// get performed of Resolve that affect the
			// state of expected_args.
			//
			// When the fix for calling just one time
			// Resolve is applied, this must be moved back
			// to Resolve.
			//
			if (BoundToMethod != null && !var_args) {
				if (elems.Count > expected_args) {
					Console.WriteLine (
					   location.SourceName + "(" + location.LineNumber + ",0) : " +
					   "warning JS1148: There are too many arguments. The extra arguments will be ignored");
				}
				if (elems.Count < expected_args) {
					string name = String.Empty;

					if (BoundToMethod is MethodInfo)
						name = ((MethodInfo) BoundToMethod).Name;
					else if (BoundToDeclaredFunction)
						name = ((Function) BoundToMethod).func_obj.name;

					Console.WriteLine (location.SourceName + "(" + location.LineNumber + ",0) : " +
						   "warning JS1204: Not all required arguments have been supplied" + 
						   " to method " + name);
				}
			}

			if (BoundToMethod is MethodInfo)
				parameters = ((MethodInfo) BoundToMethod).GetParameters ();

			if (has_engine)
				CodeGenerator.load_engine (InFunction, ec.ig);

			if (var_args) {
				if (expected_args > 1)
					emit_default_args_case (ec, expected_args, strong_type, parameters);
				
				ILGenerator ig = ec.ig;

				int remains = elems.Count - expected_args;
				
				if (remains >= 0)
					ig.Emit (OpCodes.Ldc_I4, remains);
				else
					ig.Emit (OpCodes.Ldc_I4_0);

				ig.Emit (OpCodes.Newarr, typeof (object));

				int n = elems.Count;
				AST ast = null;

				for (int j = expected_args, k = 0; j < n; j++, k++) {
					ast = get_element (j);
					if (ast != null) {
						ig.Emit (OpCodes.Dup);
						ig.Emit (OpCodes.Ldc_I4, k);
						if (ast is Assign)
							CodeGenerator.EmitAssignAsExp (ec, ast);
						else
							ast.Emit (ec);

						if (ast is Relational)
							CodeGenerator.EmitRelationalComp (ig, (Relational) ast);
						else
							CodeGenerator.EmitBox (ig, ast);

						ig.Emit (OpCodes.Stelem_Ref);
					}
				}
			} else
				emit_default_args_case (ec, expected_args, strong_type, parameters);
		}

		internal void emit_default_args_case (EmitContext ec, int n, bool strong_type,
					      ParameterInfo [] parameters)
		{
			AST ast = null;
			ILGenerator ig = ec.ig;

			// tracks the proper index of the formal params
			// from the strong typed method
			int j = 0;

			if (!BoundToDeclaredFunction) {
				if (has_this)
					j++;
				if (has_engine)
					j++;
			}

			for (int i = 0; i < n; i++, j++) {
				ast = get_element (i);
				if (ast != null) {
					if (ast is Assign) {
						CodeGenerator.EmitAssignAsExp (ec, ast);
						continue;
					} else
						ast.Emit (ec);

					if (ast is Relational)
						CodeGenerator.EmitRelationalComp (ig, (Relational) ast);
					else if (strong_type)
						force_strong_type (ig, ast, parameters [j]);
					else
						CodeGenerator.EmitBox (ig, ast);
				} else {
					//
					// ast was null and we need
					// to provide a parameter
					//
					if (strong_type)
						CodeGenerator.emit_default_value (ig, parameters [j]);
					else
						ig.Emit (OpCodes.Ldsfld, typeof (Missing).GetField ("Value"));
				}
			}
		}

		internal void force_strong_type (ILGenerator ig, AST ast, object obj)
		{
			Type param_type = null;
			if (obj is ParameterInfo)
				param_type = ((ParameterInfo) obj).ParameterType;
			else
				param_type = obj.GetType ();

			if (SemanticAnalyser.IsNumericConstant (ast)) {
				if (param_type == typeof (double))
					CodeGenerator.EmitConv (ig, param_type);
				else if (param_type == typeof (object))
					CodeGenerator.EmitBox (ig, ast);
				else
					throw new NotImplementedException ();
			} else {
				if (ast is Unary) {
					Unary unary = (Unary) ast;
					force_strong_type (ig, unary.operand, obj);
				} else if (param_type == typeof (double))
					ig.Emit (OpCodes.Call, typeof (Convert).GetMethod ("ToNumber",
										   new Type [] { typeof (object) }));
			}
		}
	}

	internal class Expression : Exp {

		internal ArrayList exprs;

		internal int Size {
			get { return exprs.Count; }
		}

		internal Expression (AST parent, Location location)
			: base (parent, location)
		{
			exprs = new ArrayList ();
		}

		internal void Add (AST a)
		{
			exprs.Add (a);
		}

		internal AST Last {
			get { return (AST) exprs [Size - 1]; }
		}

		public override string ToString ()
		{
			int size = exprs.Count;		

			if (size > 0) {
				int i;
				StringBuilder sb = new StringBuilder ();

				for (i = 0; i < size; i++)
					sb.Append (exprs [i].ToString ());
				return sb.ToString ();
			} else return String.Empty;
		}

		internal override bool Resolve (Environment env)
		{
			int n = exprs.Count - 1;
			bool r = true;
			object e;

			for (int i = 0; i <= n; i++) {
				e = exprs [i];
				if (e is Exp)
					r &= ((Exp) e).Resolve (env, i < n || no_effect);
				else
					r &= ((AST) e).Resolve (env);
			}

			return r;
		}

		internal override bool Resolve (Environment env, bool no_effect)
		{
			this.no_effect = no_effect;
			return Resolve (env);
		}

		internal override void Emit (EmitContext ec)
		{
			int i, n = exprs.Count - 1;
			AST exp;

			for (i = 0; i < n; i++) {
				exp = (AST) exprs [i];
				exp.Emit (ec);
			}
			if (n >= 0)
			{
				exp = (AST) exprs [n];
				if (exp is Assign) {
					if (no_effect)
						exp.Emit (ec);
					else
						CodeGenerator.EmitAssignAsExp (ec, exp);
				}
				else {
					exp.Emit (ec);
					if (!no_effect)
						CodeGenerator.EmitBox (ec.ig, exp);
				}
			}
		}
	}

	internal class Assign : BinaryOp {

		internal bool is_embedded;

		internal Assign (AST parent, JSToken op, Location location)
			: base (parent, null, null, op, location)
		{
		}

		internal void Init (AST left, AST right, bool is_embedded)
		{
			this.left = left;
			this.right = right;
			this.is_embedded = is_embedded;
		}

		//
		// after calling Resolve, left contains all the 
		// information about the assignment
		//
		internal override bool Resolve (Environment env)
		{
			bool r;

			if (left is IAssignable)
				r = ((IAssignable) left).ResolveAssign (env, right);
			else
				throw new Exception (location.SourceName + " (" + location.LineNumber + ",0): error JS5008: Illegal assignment");
			if (right is Exp)
				r &=((Exp) right).Resolve (env, false);
			else
				r &= right.Resolve (env);
			return r;
		}

		internal override bool Resolve (Environment env, bool no_effect)
		{
			this.no_effect = no_effect;
			return Resolve (env);
		}

		internal LocalBuilder EmitAndReturnBuilder (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			LocalBuilder builder = ig.DeclareLocal (typeof (object));
			Emit (ec);
			right.Emit (ec);
			CodeGenerator.EmitBox (ig, right);
			ig.Emit (OpCodes.Stloc, builder);
			return builder;
		}

		internal override void Emit (EmitContext ec)
		{
			if (op == JSToken.Assign) {
				if (is_embedded) {
					Console.WriteLine ("embedded assignments not supported yet");
					System.Environment.Exit (-1);
				} 
				left.Emit (ec);
			} else {
				ILGenerator ig = ec.ig;
				Type type = null;
				LocalBuilder local = null;
				LocalBuilder aux = ig.DeclareLocal (typeof (object));
				
				switch (op) {
				case JSToken.PlusAssign:
					type = typeof (Plus);
					local = ig.DeclareLocal (type);
					ig.Emit (OpCodes.Newobj, type.GetConstructor (new Type [] {}));
					ig.Emit (OpCodes.Stloc, local);
					if (left is Identifier)
						((Identifier) left).EmitLoad (ec);
					ig.Emit (OpCodes.Stloc, aux);
					ig.Emit (OpCodes.Ldloc, local);
					ig.Emit (OpCodes.Ldloc, aux);
					if (right != null) {
						right.Emit (ec);
						CodeGenerator.EmitBox (ig, right);
					}
					ig.Emit (OpCodes.Call, type.GetMethod ("EvaluatePlus"));
					if (left is Identifier)
						((Identifier) left).EmitStore (ec);
					return;
				case JSToken.MinusAssign:
				case JSToken.MultiplyAssign:
				case JSToken.DivideAssign:
				case JSToken.ModuloAssign:
					type = typeof (NumericBinary);
					break;
				case JSToken.BitwiseAndAssign:
				case JSToken.BitwiseOrAssign:
				case JSToken.BitwiseXorAssign:
				case JSToken.LeftShiftAssign:
				case JSToken.RightShiftAssign:
				case JSToken.UnsignedRightShiftAssign:
					type = typeof (BitwiseBinary);
					break;			       
				}
				local = ig.DeclareLocal (type);
				load_parameter (ig, op);

				ig.Emit (OpCodes.Newobj, type.GetConstructor (new Type [] {typeof (int)}));
				ig.Emit (OpCodes.Stloc, local);

				if (left is Identifier)
					((Identifier) left).EmitLoad (ec);

				ig.Emit (OpCodes.Stloc, aux);
				ig.Emit (OpCodes.Ldloc, local);
				ig.Emit (OpCodes.Ldloc, aux);

				if (right != null) {
					right.Emit (ec);
					CodeGenerator.EmitBox (ig, right);
				}

				emit_evaluation (op, type, ig);
				
				if (left is Identifier)
					((Identifier) left).EmitStore (ec);
			}
		}

		void load_parameter (ILGenerator ig, JSToken op)
		{
			switch (op) {
			case JSToken.MinusAssign: 
				ig.Emit (OpCodes.Ldc_I4_S, 47);
				break;
			case JSToken.BitwiseOrAssign:
				ig.Emit (OpCodes.Ldc_I4_S, 50);
				break;
			case JSToken.BitwiseXorAssign:
				ig.Emit (OpCodes.Ldc_I4_S, 51);
				break;
			case JSToken.BitwiseAndAssign:
				ig.Emit (OpCodes.Ldc_I4_S, 52);
				break;
			case JSToken.LeftShiftAssign:
				ig.Emit (OpCodes.Ldc_I4_S, 61);
				break;
			case JSToken.RightShiftAssign:
				ig.Emit (OpCodes.Ldc_I4_S, 62);
				break;				
			case JSToken.UnsignedRightShiftAssign:
				ig.Emit (OpCodes.Ldc_I4_S, 63);
				break;
			case JSToken.MultiplyAssign:
				ig.Emit (OpCodes.Ldc_I4_S, 64);
				break;
			case JSToken.DivideAssign:
				ig.Emit (OpCodes.Ldc_I4_S, 65);
				break;
			case JSToken.ModuloAssign:
				ig.Emit (OpCodes.Ldc_I4_S, 66);
				break;
			default:
				throw new NotImplementedException ();
			}
		}

		void emit_evaluation (JSToken op, Type type, ILGenerator ig)
		{
			switch (op) {
			case JSToken.MinusAssign:
			case JSToken.MultiplyAssign:
			case JSToken.DivideAssign:
			case JSToken.ModuloAssign:
				ig.Emit (OpCodes.Call, type.GetMethod ("EvaluateNumericBinary"));
				break;
			case JSToken.BitwiseAndAssign:
			case JSToken.BitwiseOrAssign:
			case JSToken.BitwiseXorAssign:
			case JSToken.LeftShiftAssign:
			case JSToken.RightShiftAssign:
			case JSToken.UnsignedRightShiftAssign:
				ig.Emit (OpCodes.Call, type.GetMethod ("EvaluateBitwiseBinary"));
				break;
			default:
				throw new NotImplementedException ();
			}

		}

		public override string ToString ()
		{
			string l = left.ToString ();
			string r = right.ToString ();
			return l + " " + op.ToString () + " " + r;
		}
	}

	internal class New : AST, ICallable {

		AST exp;
		Args args;
		bool late_bind = false;

		internal New (AST parent, AST exp, Location location)
			: base (parent, location)
		{
			this.exp = exp;
			this.args = new Args (location);
		}

		public void AddArg (AST arg)
		{
			args.Add (arg);
		}
		
		internal override bool Resolve (Environment env)
		{
			bool r = true;

			if (exp != null && exp.GetType () == typeof (Identifier)) {
				Identifier id = (Identifier) exp;
				late_bind = !SemanticAnalyser.is_js_object (id.name.Value);
			} 
			exp.Resolve (env);

			if (args != null) {
				args.InNew = true;
				r &= args.Resolve (env);
			}
			return r;
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			if (exp != null) {
				if (late_bind) {
					AST ast = null;
					CodeGenerator.emit_get_default_this (ec.ig, InFunction);
					exp.Emit (ec);

					ig.Emit (OpCodes.Ldc_I4, args.Size);
					ig.Emit (OpCodes.Newarr, typeof (object));

					for (int i = 0; i < args.Size; i++) {
						ig.Emit (OpCodes.Dup);
						ig.Emit (OpCodes.Ldc_I4, i);
						ast = args.get_element (i);

						if (ast is Assign)
							CodeGenerator.EmitAssignAsExp (ec, ast);
						else {
							ast.Emit (ec);
							CodeGenerator.EmitBox (ig, ast);
						}
						ig.Emit (OpCodes.Stelem_Ref);
					}

					ig.Emit (OpCodes.Ldc_I4_1);
					ig.Emit (OpCodes.Ldc_I4_0);

					CodeGenerator.load_engine (InFunction, ig);

					ig.Emit (OpCodes.Call, typeof (LateBinding).GetMethod ("CallValue"));
				} else {
					if (exp != null)
						exp.Emit (ec);
					if (args != null)
						emit_args (ec);
					emit_create_instance (ec);
				}
			}
		}
		
		void emit_create_instance (EmitContext ec)
		{
			if (exp is Identifier) {
				ILGenerator ig = ec.ig;
				Type type = null;
				switch ((exp as Identifier).name.Value) {
				case "Array":					
					type = typeof (ArrayConstructor);
					break;
				case "Date":
					type = typeof (DateConstructor);
					break;
				case "Number":
					type = typeof (NumberConstructor);
					break;
				case "Object":
					type = typeof (ObjectConstructor);
					break;
				case "RegExp":
					type = typeof (RegExpConstructor);
					break;
				case "String":
					type = typeof (StringConstructor);
					break;
				case "Boolean":
					type = typeof (BooleanConstructor);
					break;
				case "Function":
					type = typeof (FunctionConstructor);
					break;
				}
				if (type != null)
					ig.Emit (OpCodes.Call, type.GetMethod ("CreateInstance"));
				else
					throw new NotImplementedException (String.Format (
						"Should emit LateBinding.CallValue logic for unknown constructor {0}",
						(exp as Identifier).name.Value));
			}
		}
		
		void emit_args (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			int n = args.Size;
			ig.Emit (OpCodes.Ldc_I4, args.Size);
			ig.Emit (OpCodes.Newarr, typeof (object));

			AST ast = null;

			for (int i = 0; i < n; i++) {
				ig.Emit (OpCodes.Dup);
				ig.Emit (OpCodes.Ldc_I4, i);
				ast = args.get_element (i);

				if (ast is Assign)
					CodeGenerator.EmitAssignAsExp (ec, ast);
				else {
					ast.Emit (ec);
					CodeGenerator.EmitBox (ig, ast);
				}
				ig.Emit (OpCodes.Stelem_Ref);
			}
		}
	}
	
	internal interface IAssignable {
		bool ResolveAssign (Environment env, AST right_side);
	}

	internal class BuiltIn : AST {
		string name;
		bool allowed_as_ctr;
		bool allowed_as_func;

		internal BuiltIn (string name, bool allowed_as_ctr, bool allowed_as_func)
			: base (null, null)
		{
			this.name = name;
			this.allowed_as_ctr = allowed_as_ctr;
			this.allowed_as_func = allowed_as_func;
		}

 		internal override bool Resolve (Environment env)
		{
			return true;
		}
		
		internal string Name {
			get { return name; }
		}
		
		internal bool IsConstructor {
			get { return allowed_as_ctr; }
		}

		internal bool IsFunction {
			get { return allowed_as_func; }
		}
		
		internal bool IsPrint {
			get { return String.Equals (name, "print"); }
		}
		internal int NumOfArgs {
			get {
				if (name == "print")
					return -1;

				Type global_object = typeof (GlobalObject);
				MethodInfo method = global_object.GetMethod (name);
				return method.GetParameters ().Length;
			}
		}

		internal ParameterInfo [] Parameters {
			get {
				Type global_obj = typeof (GlobalObject);
				return global_obj.GetMethod (name).GetParameters ();
			}
		}

 		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
 			Type go = typeof (GlobalObject);
			switch (name) {
			/* value properties of the Global Object */
			case "NaN":
				ig.Emit (OpCodes.Ldc_R8, Double.NaN);
				break;				

			case "Infinity":
				ig.Emit (OpCodes.Ldc_R8, Double.PositiveInfinity);
				break;

			case "undefined":
				ig.Emit (OpCodes.Ldnull);
				break;

			case "null":
				ig.Emit (OpCodes.Ldsfld, typeof (DBNull).GetField ("Value"));
				break;
				
			/* function properties of the Global Object */
			case "eval":
				Type [] method_args = null;
#if NET_1_0
				method_args = new Type [] {typeof (object), typeof (VsaEngine)};
#else
				method_args = new Type [] {typeof (object), typeof (object), typeof (VsaEngine)};
#endif
				ig.Emit (OpCodes.Call, typeof (Eval).GetMethod ("JScriptEvaluate", method_args));
				break;

			case "parseInt":				
				ig.Emit (OpCodes.Call, go.GetMethod ("parseInt"));
				ig.Emit (OpCodes.Box, typeof (Double));
				break;

			case "parseFloat":
				ig.Emit (OpCodes.Call, go.GetMethod ("parseFloat"));
				ig.Emit (OpCodes.Box, typeof (Double));
				break;

			case "isNaN":
				ig.Emit (OpCodes.Call, go.GetMethod ("isNaN"));
				ig.Emit (OpCodes.Box, typeof (bool));
				break;

			case "isFinite":
				ig.Emit (OpCodes.Call, go.GetMethod ("isFinite"));
				ig.Emit (OpCodes.Box, typeof (bool));
				break;

			case "decodeURI":
				ig.Emit (OpCodes.Call, go.GetMethod ("decodeURI"));
				break;

			case "decodeURIComponent":
				ig.Emit (OpCodes.Call, go.GetMethod ("decodeURIComponent"));
				break;

			case "encodeURI":
				ig.Emit (OpCodes.Call, go.GetMethod ("encodeURI"));
				break;

			case "encodeURIComponent":
				ig.Emit (OpCodes.Call, go.GetMethod ("encodeURIComponent"));
				break;

			case "escape":
				ig.Emit (OpCodes.Call, go.GetMethod ("escape"));
				break;

			case "unescape":
				ig.Emit (OpCodes.Call, go.GetMethod ("unescape"));
				break;

			/* constructor properties of the Global object */
			case "Object":
				ig.Emit (OpCodes.Call, go.GetProperty ("Object").GetGetMethod ());
				break;

			case "Function":
				ig.Emit (OpCodes.Call, go.GetProperty ("Function").GetGetMethod ());
				break;

			case "Array":
				ig.Emit (OpCodes.Call, go.GetProperty ("Array").GetGetMethod ());
				break;

			case "String":
				ig.Emit (OpCodes.Call, go.GetProperty ("String").GetGetMethod ());
				break;

			case "Boolean":
				ig.Emit (OpCodes.Call, go.GetProperty ("Boolean").GetGetMethod ());
				break;

			case "Number":
				ig.Emit (OpCodes.Call, go.GetProperty ("Number").GetGetMethod ());
				break;

			case "Date":
				ig.Emit (OpCodes.Call, go.GetProperty ("Date").GetGetMethod ());
				break;

			case "RegExp":
				ig.Emit (OpCodes.Call, go.GetProperty ("RegExp").GetGetMethod ());
				break;

			case "Error":
				ig.Emit (OpCodes.Call, go.GetProperty ("Error").GetGetMethod ());
				break;

			case "EvalError":
				ig.Emit (OpCodes.Call, go.GetProperty ("EvalError").GetGetMethod ());
				break;

			case "RangeError":
				ig.Emit (OpCodes.Call, go.GetProperty ("RangeError").GetGetMethod ());
				break;

			case "ReferenceError":
				ig.Emit (OpCodes.Call, go.GetProperty ("ReferenceError").GetGetMethod ());
				break;

			case "SyntaxError":
				ig.Emit (OpCodes.Call, go.GetProperty ("SyntaxError").GetGetMethod ());
				break;

			case "TypeError":
				ig.Emit (OpCodes.Call, go.GetProperty ("TypeError").GetGetMethod ());
				break;

			case "URIError":
				ig.Emit (OpCodes.Call, go.GetProperty ("URIError").GetGetMethod ());
				break;
			/* other properties of the Global object */
			case "Math":
				ig.Emit (OpCodes.Call, go.GetProperty ("Math").GetGetMethod ());
				break;
			default:
				throw new Exception ("This is BuiltIn " + name);
			}
		}
	}
}
