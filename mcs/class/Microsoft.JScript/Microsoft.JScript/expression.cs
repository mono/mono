//
// Expression.cs: Everything related to expressions
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, 2004 Cesar Lopez Nataren
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
		}			
	}

	internal class Binary : BinaryOp, IAssignable {

		bool assign;
		AST right_side;

		internal Binary (AST parent, AST left, JSToken op)
			: this (parent, left, null, op)
		{
		}

		internal Binary (AST parent, AST left, AST right, JSToken op)
		{
			this.parent = parent;
			this.left = left;
			this.right = right;
			this.op = op;	
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

		internal override bool Resolve (IdentificationTable context)
		{
			bool r = true;
			if (left != null)
				r &= left.Resolve (context);
			if (right != null) 
				if (op == JSToken.AccessField && right is IAccesible)
					r &= ((IAccesible) right).ResolveFieldAccess (left);
				else
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
				emit_access (ec);				
			} else if (op == JSToken.AccessField) {
				if (left is Identifier) {
					Identifier parent = left as Identifier;
					switch (parent.name) {
					case "Math":
						MathObject math_obj = new MathObject ();
						Type math_obj_type = math_obj.GetType ();
						Identifier property = right as Identifier;
						double v = 0;
						switch (property.name) {
						/* value properties of the math object */
						case "E":
							v = math_obj.E;
							break;
						case "LN10":
							v = math_obj.LN10;
							break;
						case "LN2":
							v = math_obj.LN2;
							break;
						case "LOG2E":
							v = math_obj.LOG2E;
							break;
						case "LOG10E":
							v = math_obj.LOG10E;
							break;
						case "PI":
							v = math_obj.PI;
							break;
						case "SQRT1_2":
							v = math_obj.SQRT1_2;
							break;
						case "SQRT2":
							v = math_obj.SQRT2;
							break;
						
						/* function properties of the math object */
						case "abs":
						case "acos":
						case "asin":
						case "atan":
						case "atan2":
						case "sin":
						case "cos":
							ig.Emit (OpCodes.Call, typeof (Convert).GetMethod ("ToNumber", new Type [] { typeof (object) }));
							ig.Emit (OpCodes.Call, math_obj_type.GetMethod (property.name));
							ig.Emit (OpCodes.Box, typeof (double));
							return;
						}
						ig.Emit (OpCodes.Ldc_R8, v);
						ig.Emit (OpCodes.Box, typeof (Double));
						break;
					}
				}
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

		internal void get_default_this (ILGenerator ig)
		{
			if (parent == null || parent.GetType () == typeof (ScriptBlock))
				ig.Emit (OpCodes.Ldarg_0);
			else
				ig.Emit (OpCodes.Ldarg_1);
			
			ig.Emit (OpCodes.Ldfld, typeof (ScriptObject).GetField ("engine"));
			ig.Emit (OpCodes.Call, typeof (Microsoft.JScript.Vsa.VsaEngine).GetMethod ("ScriptObjectStackTop"));
			Type iact_obj = typeof (IActivationObject);
			ig.Emit (OpCodes.Castclass, iact_obj);
			ig.Emit (OpCodes.Callvirt, iact_obj.GetMethod ("GetDefaultThisObject"));
		}

		internal void emit_access (EmitContext ec)
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

				if (parent == null || parent.GetType () == typeof (ScriptBlock)) {
					ig.Emit (OpCodes.Ldarg_0);
					ig.Emit (OpCodes.Ldfld, typeof (ScriptObject).GetField ("engine"));
				} else
					ig.Emit (OpCodes.Ldarg_1);
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
				BuiltIn binding = (BuiltIn) SemanticAnalyser.ObjectSystemContains (member_exp.ToString ());
				if (binding != null) {
					if (!binding.IsFunction)
						throw new Exception ("error JS5002: function expected.");
					if (!binding.IsConstructor)
						n = binding.NumOfArgs;
				}
				r &= member_exp.Resolve (context);
			}
			if (args != null) {
				args.DesiredNumOfArgs = n;
				if (member_exp.ToString () == "print")
					args.IsPrint = true;
				r &= args.Resolve (context);
			}
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
			} else {
				BuiltIn binding = (BuiltIn) SemanticAnalyser.ObjectSystemContains (member_exp.ToString ());
				if (binding == null || IsGlobalObjectMethod (binding)) {
					args.Emit (ec);
					member_exp.Emit (ec);
				} else {
					member_exp.Emit (ec);
					EmitBuiltInArgs (ec);
					EmitInvoke (ec);
				}
				if (no_effect)
					ec.ig.Emit (OpCodes.Pop);
			}
		}

		void EmitBuiltInArgs (EmitContext ec)
		{
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

		bool IsGlobalObjectMethod (BuiltIn binding)
		{
			switch (binding.Name) {
			case "eval":
			case "parseInt":
			case "parseFloat":
			case "isNaN":
			case "isFinite":
			case "decodeURI":
			case "decodeURIComponent":
			case "encodeURI":
			case "encodeURIComponent":				
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

		internal string name;
		internal AST binding;
		internal bool assign;
		AST right_side;
		MemberInfo [] members = null;

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
				bind = SemanticAnalyser.ObjectSystemContains (name);
			if (bind == null)
				throw new Exception ("variable not found: " +  name);
			binding = bind as AST;
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
			if (name != String.Empty)			
				return Resolve (context);
			return true;
		}

		public bool ResolveFieldAccess (AST parent)
		{
			if (parent is Identifier) {
				Identifier p = parent as Identifier;

				Console.WriteLine ("ResolveFieldAccess: p.name = {0}", p.name);
				Console.WriteLine ("ResolveFieldAccess: name = {0}", name);

				AST binding = (AST) SemanticAnalyser.ObjectSystemContains (p.name);
				if (binding != null && binding is BuiltIn)
					return IsBuiltInObjectProperty (p.name, name);
			}
			return false;
		}

		bool IsBuiltInObjectProperty (string obj_name, string prop_name)
		{
			Type type;
			if (obj_name == "Math") {
				type = typeof (MathObject);
				//FieldInfo prop = type.GetField (prop_name);
				members = type.FindMembers (MemberTypes.Field | MemberTypes.Method,
									  BindingFlags.Public | BindingFlags.Static,
									  Type.FilterName, prop_name);
				if (members != null && members.Length > 0) {
					Console.WriteLine ("found property {0}", prop_name);
					return true;
				} else
					throw new Exception ("error: JS0438: Object doesn't support this property or method");
			}
			return false;
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			if (assign && right_side != null)
				right_side.Emit (ec);
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
			} else if (binding is BuiltIn)
				binding.Emit (ec);
			else
				Console.WriteLine ("Identifier.Emit, DID NOT EMIT ANYTHING, binding is {0}", binding);

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
		int num_of_args = -1;
		internal bool is_print;

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

		internal override bool Resolve (IdentificationTable context)
		{
			int i, n = elems.Count;
			AST tmp;
			bool r = true;

			if (!is_print && num_of_args >= 0 && n > num_of_args)
				Console.WriteLine ("warning JS1148: There are too many arguments. The extra arguments will be ignored");
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
				return null;
		}

		internal int Size {
			get { return elems.Count; }
		}

		internal override void Emit (EmitContext ec)
		{
			int i = 0, n = elems.Count;
			//Console.WriteLine ("n = {0}", n);
			//Console.WriteLine ("num_of_args = {0}", num_of_args);
			AST ast;
			do {
				//Console.WriteLine ("Args.Emit, i = {0}", i);
				ast = get_element (i);
				if (ast != null)
					ast.Emit (ec);
				i++;
			} while (i < n || i < num_of_args);

			if (num_of_args > n) {
				ILGenerator ig = ec.ig;
				for (int j = 0; j < num_of_args - 1; j++)
					ig.Emit (OpCodes.Ldsfld, typeof (Missing).GetField ("Value"));;
			}
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
			this.op = op;
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
			} 
			left.Emit (ec);
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
			bool r = true;
			if (exp != null)
				r &= exp.Resolve (context);
			if (args != null)
				r &= args.Resolve (context);
			return r;
		}

		internal override void Emit (EmitContext ec)
		{
			throw new NotImplementedException ();
		}
		
	}
	
	internal interface IAssignable {
		bool ResolveAssign (IdentificationTable context, AST right_side);
	}

	internal class BuiltIn : AST {
		string name;
		bool allowed_as_const;
		bool allowed_as_func;

		internal BuiltIn (string name, bool allowed_as_const, bool allowed_as_func)
		{
			this.name = name;
			this.allowed_as_const = allowed_as_const;
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
			get { return allowed_as_const; }
		}

		internal bool IsFunction {
			get { return allowed_as_func; }
		}
		
		internal int NumOfArgs {
			get {
				Type global_object = typeof (GlobalObject);
				MethodInfo method = global_object.GetMethod (name);
				return method.GetParameters ().Length;
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
				throw new NotImplementedException ();
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
