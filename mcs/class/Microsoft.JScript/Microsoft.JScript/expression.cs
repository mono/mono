//
// expression.cs: Everything related to expressions
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, 2004 Cesar Lopez Nataren
// (C) 2005, Novell Inc.
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
		internal abstract bool Resolve (IdentificationTable context, bool no_effect);
	}
	
	public class Unary : UnaryOp {

		internal Unary (AST parent, JSToken oper)
		{		
			this.parent = parent;
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
			if (oper != JSToken.Minus)
				emit_unary_op (ec);
		}

		internal void emit_unary_op (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			switch (oper) {
			case JSToken.Typeof:
				ig.Emit (OpCodes.Call, typeof (Typeof).GetMethod ("JScriptTypeof"));
				break;
			}
		}
	}

	internal class Binary : BinaryOp, IAssignable {

		bool assign, late_bind = false;
		AST right_side;

		internal Binary (AST parent, AST left, JSToken op)
			: this (parent, left, null, op)
		{
		}
		
		internal Binary (AST parent, AST left, AST right, JSToken op)
			: base (left, right, op)
		{
			this.parent = parent;
		}
		
		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append (left.ToString () + " ");
			
			if (op != JSToken.None)
				sb.Append (op + " ");
			
			if (right != null)
				sb.Append (right.ToString ());

			return sb.ToString ();
		}

		internal bool AccessField {
			get { return op == JSToken.AccessField; }
		}

		internal override bool Resolve (IdentificationTable context)
		{
			bool r = true;
			if (left != null)
				r &= left.Resolve (context);
			if (right != null) 
				if (op == JSToken.AccessField && right is IAccesible) {
					r &= ((IAccesible) right).ResolveFieldAccess (left);
					if (!r)
						late_bind = true;
				} else
					r &= right.Resolve (context);
			return r;
		}

		internal override bool Resolve (IdentificationTable context, bool no_effect)
		{
			this.no_effect = no_effect;
			return Resolve (context);
		}
		
		public bool ResolveAssign (IdentificationTable context, AST right_side)
		{
			if (op == JSToken.LeftBracket || op == JSToken.AccessField) {
				this.no_effect = false;
				this.assign = true;
				this.right_side = right_side;
				return Resolve (context);
			} else 
				throw new Exception ("error JS5008: Illegal assignment");
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
				if (left != null)
					left.Emit (ec);
				if (right != null)
					right.Emit (ec);			       
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
			default:
				throw new NotImplementedException ();
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
		
		void emit_late_get_or_set (EmitContext ec, LocalBuilder local)
		{
			ILGenerator ig = ec.ig;

			ig.Emit (OpCodes.Ldloc, local);
			ig.Emit (OpCodes.Dup);
			
			left.Emit (ec);

			CodeGenerator.load_engine (InFunction, ec.ig);
			ig.Emit (OpCodes.Call , typeof (Convert).GetMethod ("ToObject"));

			Type lb_type = typeof (LateBinding);
			ig.Emit (OpCodes.Stfld, lb_type.GetField ("obj"));

			if (assign) {
				right_side.Emit (ec);
				ig.Emit (OpCodes.Call, lb_type.GetMethod ("SetValue"));
			} else
				ig.Emit (OpCodes.Call, lb_type.GetMethod ("GetNonMissingValue"));
		}
		
		internal void get_default_this (ILGenerator ig)
		{
			if (InFunction)
				ig.Emit (OpCodes.Ldarg_1);
			else {
				ig.Emit (OpCodes.Ldarg_0);
				ig.Emit (OpCodes.Ldfld, typeof (ScriptObject).GetField ("engine"));
			}
		       
			ig.Emit (OpCodes.Call, typeof (Microsoft.JScript.Vsa.VsaEngine).GetMethod ("ScriptObjectStackTop"));
			Type iact_obj = typeof (IActivationObject);
			ig.Emit (OpCodes.Castclass, iact_obj);
			ig.Emit (OpCodes.Callvirt, iact_obj.GetMethod ("GetDefaultThisObject"));
		}

		internal void emit_array_access (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			ig.Emit (OpCodes.Ldc_I4_1);
			ig.Emit (OpCodes.Newarr, typeof (object));
			ig.Emit (OpCodes.Dup);
			ig.Emit (OpCodes.Ldc_I4_0);
			if (right != null)
				right.Emit (ec);
			ig.Emit (OpCodes.Stelem_Ref);			

			if (assign) {
				if (right_side != null)
					right_side.Emit (ec);
				ig.Emit (OpCodes.Call, typeof (LateBinding).GetMethod ("SetIndexedPropertyValueStatic"));
			} else {
				ig.Emit (OpCodes.Ldc_I4_0);
				ig.Emit (OpCodes.Ldc_I4_1);

				if (InFunction)
					ig.Emit (OpCodes.Ldarg_1);
				else {
					ig.Emit (OpCodes.Ldarg_0);
					ig.Emit (OpCodes.Ldfld, typeof (ScriptObject).GetField ("engine"));
				}
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
	
	public class Call : Exp, ICallable {
		
		internal AST member_exp;		
		internal Args args;
		internal object binding;
		internal Type bind_type;

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
			int n = -1;

			if (member_exp != null) {
				if (member_exp is Identifier) {
					member_exp.Resolve (context);
					binding = context.Get ((member_exp as Identifier).name);
					bind_type = binding.GetType ();

					if (bind_type == typeof (BuiltIn)) {
						BuiltIn built_in = binding as BuiltIn;
						if (!built_in.IsFunction)
							throw new Exception ("error JS5002 A: function expected.");
						if (IsGlobalObjectMethod (built_in)) {
							args.params_info = built_in.Parameters;
							n = built_in.NumOfArgs;
						}
					} else if (bind_type == typeof (FunctionDeclaration) || bind_type == typeof (FunctionExpression)) {
						args.func = (Function) binding;
						n = (binding as Function).NumOfArgs;
					}
				} else if (member_exp is Binary) {
					member_exp.Resolve (context);
					Binary bin = (Binary) member_exp;
					if (bin.AccessField)
						binding = bin.Binding;
					if (binding is MethodInfo) {
						MethodInfo method_info = (MethodInfo) binding;
						args.has_var_args (method_info);
						args.params_info = method_info.GetParameters ();
						if (args.params_info != null)
							n = args.params_info.Length;
						else n = 0;
					}
				} else
					r &= member_exp.Resolve (context);
		
				if (args != null) {
					args.DesiredNumOfArgs = n;
					r &= args.Resolve (context);
				}
			} else
				throw new Exception ("Call.Resolve, member_exp can't be null");
			return r;
		}

		internal override bool Resolve (IdentificationTable context, bool no_effect)
		{
			this.no_effect = no_effect;
			return Resolve (context);
		}

		internal override void Emit (EmitContext ec)
		{
			if (bind_type == typeof (BuiltIn)) {
				BuiltIn b = binding as BuiltIn;
				if (b.IsPrint) {
					emit_print_stm (ec);
					return;
				} else if (IsGlobalObjectMethod (binding)) {
					args.Emit (ec);
					member_exp.Emit (ec);
				} else if (IsConstructorProperty (binding)) {
 					member_exp.Emit (ec);
 					EmitBuiltInArgs (ec);
 					EmitInvoke (ec);					
				}
			} else if (bind_type == typeof (FunctionDeclaration) || bind_type == typeof (FunctionExpression)) {
				MethodBuilder method = (MethodBuilder) TypeManager.Get ((binding as Function).func_obj.name);
				emit_func_call (method, ec);
				return;
			} else if (binding is MemberInfo) {
				MemberInfo minfo = (MemberInfo) binding;
				MemberTypes member_type = minfo.MemberType;
				ILGenerator ig = ec.ig;				
				
				if (member_type == MemberTypes.Method) {
					args.Emit (ec);
					MethodInfo method = (MethodInfo) minfo;
					ig.Emit (OpCodes.Call, method);
					Type return_type = method.ReturnType;
					if (return_type != typeof (void))
						ig.Emit (OpCodes.Box, return_type);
				} else
					throw new NotImplementedException ();
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

		void emit_late_call (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			if (member_exp is Binary) {
				Binary bin = member_exp as Binary;
				if (bin.right is Identifier) {
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
					member_exp.Emit (ec);
					setup_late_call_args (ec);
					ig.Emit (OpCodes.Call, typeof (LateBinding).GetMethod ("CallValue"));
				}
			} else {				
				get_global_scope_or_this (ec.ig);
				member_exp.Emit (ec);
				setup_late_call_args (ec);
				ig.Emit (OpCodes.Call, typeof (LateBinding).GetMethod ("CallValue"));
			}
		}

		internal void get_global_scope_or_this (ILGenerator ig)
		{
			if (InFunction)
				ig.Emit (OpCodes.Ldarg_1);
			else {
				ig.Emit (OpCodes.Ldarg_0);
				ig.Emit (OpCodes.Ldfld, typeof (ScriptObject).GetField ("engine"));
			}

			ig.Emit (OpCodes.Call, typeof (Microsoft.JScript.Vsa.VsaEngine).GetMethod ("ScriptObjectStackTop"));
			Type iact_obj = typeof (IActivationObject);
			ig.Emit (OpCodes.Castclass, iact_obj);

			//
			//FIXME: when is each of them (GetGlobalScope | GetDefaultThisObject?
			//
			// ig.Emit (OpCodes.Callvirt, iact_obj.GetMethod ("GetGlobalScope"));
			ig.Emit (OpCodes.Callvirt, iact_obj.GetMethod ("GetDefaultThisObject"));
		}

		void init_late_binding (EmitContext ec, LocalBuilder local)
		{
			ILGenerator ig = ec.ig;

			ig.Emit (OpCodes.Ldloc, local);
			ig.Emit (OpCodes.Dup);
			
			AST left = (member_exp as Binary).left;
			left.Emit (ec);
			
			ig.Emit (OpCodes.Ldarg_0);
			ig.Emit (OpCodes.Ldfld, typeof (ScriptObject).GetField ("engine"));
			ig.Emit (OpCodes.Call , typeof (Convert).GetMethod ("ToObject"));

			Type lb_type = typeof (LateBinding);

			ig.Emit (OpCodes.Stfld, lb_type.GetField ("obj"));
		}

		void setup_late_call_args (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			int n = args.Size;

			ig.Emit (OpCodes.Ldc_I4, n);
			ig.Emit (OpCodes.Newarr, typeof (object));
			
			for (int i = 0; i < n; i++) {
				ig.Emit (OpCodes.Dup);
				ig.Emit (OpCodes.Ldc_I4, i);
				args.get_element (i).Emit (ec);
				ig.Emit (OpCodes.Stelem_Ref);
			}

			ig.Emit (OpCodes.Ldc_I4_0);
			ig.Emit (OpCodes.Ldc_I4_0);

			if (InFunction)
				ig.Emit (OpCodes.Ldarg_1);
			else {
				ig.Emit (OpCodes.Ldarg_0);
				ig.Emit (OpCodes.Ldfld, typeof (ScriptObject).GetField ("engine"));
			}
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
			
			if (n >= 1 && (member_exp.ToString () == "String" || member_exp.ToString () == "Boolean" || member_exp.ToString () == "Number")) {
				args.get_element (0).Emit (ec);
				return;
			}

			ig.Emit (OpCodes.Ldc_I4, n);
			ig.Emit (OpCodes.Newarr, typeof (object));
			for (int i = 0; i < n; i++) {
				ig.Emit (OpCodes.Dup);
				ig.Emit (OpCodes.Ldc_I4, i);
				args.get_element (i).Emit (ec);
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
	}

	interface IAccesible {
		bool ResolveFieldAccess (AST parent);
	}

	internal class Identifier : Exp, IAssignable, IAccesible {
		
		internal Symbol name;
		internal AST binding;
		internal bool assign;
		AST right_side;
		MemberInfo [] members = null;

		internal Identifier (AST parent, string id)
		{
			this.parent = parent;
			this.name = Symbol.CreateSymbol (id);
		}

		public override string ToString ()
		{
			return name.Value;
		}

		internal override bool Resolve (IdentificationTable context)
		{
			bool contained = context.Contains (this.name);
			if (contained)
				binding = (AST) context.Get (this.name);
			else
				throw new Exception ("variable not found: " +  name);
			return true;
		}

		internal override bool Resolve (IdentificationTable context, bool no_effect)
		{
			this.no_effect = no_effect;
			return Resolve (context);
		}

		public bool ResolveAssign (IdentificationTable context, AST right_side)
		{
			this.assign = true;
			this.no_effect = false;
			this.right_side = right_side;
			if (name.Value != String.Empty)			
				return Resolve (context);
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
			contains_method = SemanticAnalyser.object_contains_method (
					   SemanticAnalyser.map_to_ctr (obj_name), prop_name);
			if (!contains_method)
				throw new Exception ("error: JS0438: Object doesn't support this property or method");
			return true;
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			if (assign && right_side != null)
				right_side.Emit (ec);
			if (binding is FormalParam) {
				FormalParam f = binding as FormalParam;
				if (assign)
					ig.Emit (OpCodes.Starg, (short) f.pos);
				else
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
			} else if (binding is BuiltIn)
				binding.Emit (ec);
			else if (binding is FunctionDeclaration)
				load_script_func (ec, (FunctionDeclaration) binding);
			else if (binding == null) // it got referenced before was declared and initialized
				Console.WriteLine ("id = {0}, Identifier.Emit, binding == null? {1}", name.Value, binding == null);
			if (!assign && no_effect)
				ig.Emit (OpCodes.Pop);				
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
						LocalBuilder local_meth = (LocalBuilder) TypeManager.GetLocal (binding.func_obj.name);
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
		int num_of_args = -1;
		internal Function func;
		internal bool is_print;
		internal ParameterInfo [] params_info;
		internal bool late_bind = false;
		internal bool var_args = false;
		
		internal Args ()
		{
			elems = new ArrayList ();
		}

		internal void Add (AST e)
		{
			elems.Add (e);
		}
		
		internal int DesiredNumOfArgs {
			set { 
				if (!(value < 0))
					num_of_args = value; 
			}
		}
		
		internal bool IsPrint {
			set { is_print = value; }
		}

		internal void has_var_args (MethodInfo minfo)
		{
			JSFunctionAttribute [] custom_attrs = (JSFunctionAttribute []) 
				minfo.GetCustomAttributes (typeof (JSFunctionAttribute), true);

			foreach (JSFunctionAttribute attr in custom_attrs)
				if (attr.GetAttributeValue () == JSFunctionAttributeEnum.HasVarArgs) {
					var_args = true;
					return;
				}
			var_args = false;
		}

		internal override bool Resolve (IdentificationTable context)
		{
			int n = elems.Count;
			AST tmp;
			bool r = true;

			if (!is_print && !var_args &&  num_of_args >= 0 && n > num_of_args)
				Console.WriteLine ("warning JS1148: There are too many arguments. The extra arguments will be ignored");
			for (int i = 0; i < n; i++) {
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
				return null;
		}

		internal int Size {
			get { return elems.Count; }
		}

		internal override void Emit (EmitContext ec)
		{
			int n;
			ILGenerator ig = ec.ig;

			if (var_args) {
				n = elems.Count;
				ig.Emit (OpCodes.Ldc_I4, n);
				ig.Emit (OpCodes.Newarr, typeof (object));
				for (int i = 0; i < n; i++) {
					ig.Emit (OpCodes.Dup);
					ig.Emit (OpCodes.Ldc_I4, i);
					get_element (i).Emit (ec);
					ig.Emit (OpCodes.Stelem_Ref);
				}
			} else {
				n = elems.Count;
				if (num_of_args < 0)
					num_of_args = n;
				AST ast;
				for (int j = 0; j < n && j < num_of_args; j++) {
					ast = get_element (j);
					ast.Emit (ec);
					if (!late_bind && params_info != null)
						force_strong_type (ig, ast, params_info [j]);
				}
				int missing = 0;
				if (func != null) 
					missing = func.NumOfArgs - n;
				else if (params_info != null)
					missing = params_info.Length - n;

				for (int k = 0; k < missing; k++)
					ig.Emit (OpCodes.Ldsfld, typeof (DBNull).GetField ("Value"));
			}
		}

		internal void force_strong_type (ILGenerator ig, AST ast, ParameterInfo pinfo)
		{
			Type param_type = pinfo.ParameterType;
			if (ast.GetType () == typeof (NumericLiteral)) {
				if (param_type == typeof (double))
					ig.Emit (OpCodes.Conv_R8);
				else if (param_type == typeof (object))
					;
				else throw new NotImplementedException ();
			} else {

				if (param_type == typeof (double))
					ig.Emit (OpCodes.Call, typeof (Convert).GetMethod ("ToNumber", new Type [] {typeof (object)}));
				else if (param_type == typeof (string)) {
					ig.Emit (OpCodes.Ldc_I4_1);
					ig.Emit (OpCodes.Call, typeof (Convert).GetMethod ("ToString", new Type [] {typeof (object), typeof (bool)}));
				} else if (param_type == typeof (object))
					;
				else throw new NotImplementedException ();
			}			
		}
	}

	public class Expression : Exp {

		internal ArrayList exprs;

		internal int Size {
			get { return exprs.Count; }
		}

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
			: base (left, right, op)
		{
			this.parent = parent;
			this.is_embedded = is_embedded;
		}

		//
		// after calling Resolve, left contains all the 
		// information about the assignment
		//
		internal override bool Resolve (IdentificationTable context)
		{						
			bool r;

			if (left is IAssignable)
				r = ((IAssignable) left).ResolveAssign (context, right);
			else
				throw new Exception ("(" + line_number + ",0): error JS5008: Illegal assignment");
			if (right is Exp)
				r &=((Exp) right).Resolve (context, false);
			else
				r &= right.Resolve (context);
			return r;
		}

		internal override bool Resolve (IdentificationTable context, bool no_effect)
		{
			return true;
		}

		internal override void Emit (EmitContext ec)
		{
			if (op == JSToken.Assign) {
				if (is_embedded) {
					Console.WriteLine ("embedded assignments not supported yet");
					Environment.Exit (-1);
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
					if (right != null)
						right.Emit (ec);
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

				if (right != null)
					right.Emit (ec);

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
			bool r = true;

			if (exp != null && exp.GetType () == typeof (Identifier)) {
				Identifier id = (Identifier) exp;
				late_bind = !SemanticAnalyser.is_js_object (id.name.Value);
			} 
			exp.Resolve (context);

			if (args != null)
				r &= args.Resolve (context);
			return r;
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			if (exp != null) {
				if (late_bind) {					
					CodeGenerator.emit_get_default_this (ec.ig);
					exp.Emit (ec);

					ig.Emit (OpCodes.Ldc_I4, args.Size);
					ig.Emit (OpCodes.Newarr, typeof (object));

					for (int i = 0; i < args.Size; i++) {
						ig.Emit (OpCodes.Dup);
						ig.Emit (OpCodes.Ldc_I4, i);
						args.get_element (i).Emit (ec);
						ig.Emit (OpCodes.Stelem_Ref);
					}
					
					ig.Emit (OpCodes.Ldc_I4_1);
					ig.Emit (OpCodes.Ldc_I4_0);

					ig.Emit (OpCodes.Ldarg_0);
					ig.Emit (OpCodes.Ldfld, typeof (ScriptObject).GetField ("engine"));

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
			}
		}
		
		void emit_args (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			int n = args.Size;
			ig.Emit (OpCodes.Ldc_I4, args.Size);
			ig.Emit (OpCodes.Newarr, typeof (object));

			for (int i = 0; i < n; i++) {
				ig.Emit (OpCodes.Dup);
				ig.Emit (OpCodes.Ldc_I4, i);
				args.get_element (i).Emit (ec);
				ig.Emit (OpCodes.Stelem_Ref);
			}
		}
	}
	
	internal interface IAssignable {
		bool ResolveAssign (IdentificationTable context, AST right_side);
	}

	internal class BuiltIn : AST {
		string name;
		bool allowed_as_ctr;
		bool allowed_as_func;

		internal BuiltIn (string name, bool allowed_as_ctr, bool allowed_as_func)
		{
			this.name = name;
			this.allowed_as_ctr = allowed_as_ctr;
			this.allowed_as_func = allowed_as_func;
		}

 		internal override bool Resolve (IdentificationTable context)
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
				ig.Emit (OpCodes.Box, typeof (Double));
				break;				
			case "Infinity":
				ig.Emit (OpCodes.Ldc_R8, Double.PositiveInfinity);
				// FIXME: research when not to generate the Boxing
				ig.Emit (OpCodes.Box, typeof (Double));
				break;
			case "undefined":
				ig.Emit (OpCodes.Ldnull);
				break;
			case "null":
				ig.Emit (OpCodes.Ldsfld, typeof (DBNull).GetField ("Value"));
				break;
				
			/* function properties of the Global Object */
			case "eval":
				throw new NotImplementedException ();
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
