//
// CodeGenerator.cs
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, 2004 Cesar Lopez Nataren
// (C) 2005, Novell, Inc. (http://novell.com)
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
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Microsoft.JScript.Vsa;
using System.Collections;

namespace Microsoft.JScript {

	internal class EmitContext {

		internal TypeBuilder type_builder;
		internal ILGenerator ig;
		internal ModuleBuilder mod_builder;
		internal MethodBuilder global_code;

		internal Label LoopBegin, LoopEnd;

		internal EmitContext (TypeBuilder type_builder, ModuleBuilder mod_builder, ILGenerator ig)
		{
			this.type_builder = type_builder;
			this.mod_builder = mod_builder;
			this.ig = ig;
		}
	}

	internal class CodeGenerator {

		private static string MODULE = "JScript Module";
		private static string CORLIB = typeof (object).Assembly.FullName;

		internal static string mod_name;
		internal static AppDomain app_domain;
		internal static AssemblyName assembly_name;
		internal static AssemblyBuilder assembly_builder;
		internal static ModuleBuilder module_builder;
		private static int next_type = 0;
		private static ArrayList global_types = new ArrayList ();
		private static Hashtable global_methods = new Hashtable ();
		private static Hashtable source_file_to_type = new Hashtable ();

		private static string NextType {
			get { return "JScript " + next_type++; }
		}

		internal static string Basename (string name)
		{
			int pos = name.LastIndexOf ('/');

			if (pos != -1)
				return name.Substring (pos + 1);

			pos = name.LastIndexOf ('\\');
			if (pos != -1)
				return name.Substring (pos + 1);

			return name;
		}

		internal static string Dirname (string name)
		{
			int pos = name.LastIndexOf ('/');

                        if (pos != -1)
                                return name.Substring (0, pos);

                        pos = name.LastIndexOf ('\\');
                        if (pos != -1)
                                return name.Substring (0, pos);

                        return ".";
		}

		internal static void Init (string file_name)
		{
			app_domain = Thread.GetDomain ();

			assembly_name = new AssemblyName ();
			assembly_name.Name = Path.GetFileNameWithoutExtension (file_name);
			mod_name = MODULE;

			assembly_builder = app_domain.DefineDynamicAssembly (
					     assembly_name,
					     AssemblyBuilderAccess.RunAndSave,
					     Dirname (file_name));

			ConstructorInfo ctr_info = typeof (Microsoft.JScript.ReferenceAttribute).GetConstructor (new Type [] { typeof (string) });
			assembly_builder.SetCustomAttribute (new CustomAttributeBuilder (ctr_info, new object [] {CORLIB}));

			module_builder = assembly_builder.DefineDynamicModule (
					       mod_name,
					       Basename (assembly_name.Name + ".exe"),
					       false);
		}

 		internal static string trim_extension (string file_name)
		{
			int index = file_name.LastIndexOf ('.');

			if (index < 0)
				return file_name;
			else
				return file_name.Substring (0, index);
		}

		internal static void Save (string target_name)
		{
			assembly_builder.Save (CodeGenerator.Basename (target_name));
		}

		internal static void EmitDecls (ScriptBlock prog)
		{
			if (prog == null)
				return;

			string next_type = CodeGenerator.NextType;

			prog.InitTypeBuilder (module_builder, next_type);
			prog.InitGlobalCode ();

			global_types.Add (next_type);
			global_methods.Add (next_type, prog.GlobalCode);
			source_file_to_type.Add (prog.Location.SourceName, next_type);

			prog.EmitDecls (module_builder);
		}

		internal static void emit_jscript_main (TypeBuilder tb)
		{
			emit_jscript_main_constructor (tb);
			emit_jscript_main_entry_point (tb);
		}

		internal static void emit_jscript_main_constructor (TypeBuilder tb)
		{
			ConstructorBuilder cons = tb.DefineConstructor (MethodAttributes.Public, 
									CallingConventions.Standard,
									new Type [] {});
			ILGenerator ig = cons.GetILGenerator ();
			ig.Emit (OpCodes.Ldarg_0);
			ig.Emit (OpCodes.Call, typeof (Object).GetConstructor (new Type [] {}));
			ig.Emit (OpCodes.Ret);
		}

		internal static void emit_jscript_main_entry_point (TypeBuilder tb)
		{
			MethodBuilder method;
			method = tb.DefineMethod ("Main", 
						  MethodAttributes.Public | MethodAttributes.Static,
						  typeof (void), new Type [] {typeof (String [])});

			method.SetCustomAttribute (new CustomAttributeBuilder 
						   (typeof (STAThreadAttribute).GetConstructor (
											new Type [] {}),
						     new object [] {}));

			ILGenerator ig = method.GetILGenerator ();

			ig.DeclareLocal (typeof (GlobalScope));

			ig.Emit (OpCodes.Ldc_I4_1);
			ig.Emit (OpCodes.Ldc_I4_1);
			ig.Emit (OpCodes.Newarr, typeof (string));
			ig.Emit (OpCodes.Dup);
			ig.Emit (OpCodes.Ldc_I4_0);

			ig.Emit (OpCodes.Ldstr, CORLIB);

			ig.Emit (OpCodes.Stelem_Ref);

			ig.Emit (OpCodes.Call,
				 typeof (VsaEngine).GetMethod ("CreateEngineAndGetGlobalScope", 
							       new Type [] {typeof (bool), 
									    typeof (string [])}));	
			ig.Emit (OpCodes.Stloc_0);

			foreach (string type_name in global_types) {
				ig.Emit (OpCodes.Ldloc_0);
				ig.Emit (OpCodes.Newobj, assembly_builder.GetType (type_name).GetConstructor (
									      new Type [] {typeof (GlobalScope)})); 
				ig.Emit (OpCodes.Call, (MethodInfo) global_methods [type_name]);
				ig.Emit (OpCodes.Pop);
			}
			ig.Emit (OpCodes.Ret);

			assembly_builder.SetEntryPoint (method);
		}

		public static void Run (string file_name, ScriptBlock [] blocks)
		{
			CodeGenerator.Init (file_name);

			//
			// Emit first all the declarations (function and variables)
			//
			foreach (ScriptBlock script_block in blocks)
				CodeGenerator.EmitDecls (script_block);

			//
			// emit everything that's not a declaration
			//
			foreach (ScriptBlock script_block in blocks)
				script_block.Emit ();

			//
			// Create the types ('JScript N')
			//
			foreach (ScriptBlock script_block in blocks)
				script_block.CreateType ();
			
			//
			// Build the default 'JScript Main' class
			//
			TypeBuilder main_type_builder = module_builder.DefineType ("JScript Main");
			emit_jscript_main (main_type_builder);
			main_type_builder.CreateType ();

			CodeGenerator.Save (trim_extension (file_name) + ".exe");
		}

		static void emit_default_case (EmitContext ec, AST ast, OpCode op, Label lbl)
		{
			ast.Emit (ec);
			if (need_convert_to_boolean (ast))
				emit_to_boolean (ast, ec.ig, 0);
			ec.ig.Emit (op, lbl);
		}

		static void ft_binary_recursion (EmitContext ec, AST ast, Label lbl)
		{
			ILGenerator ig = ec.ig;
			if (ast is Binary) {
				Binary b = ast as Binary;
				switch (b.op) {
				case JSToken.LogicalOr:
					Label ftLb = ig.DefineLabel ();
					fall_false (ec, b.left, ftLb);
					fall_true (ec, b.right, lbl);
					ig.MarkLabel (ftLb);
					break;
				case JSToken.LogicalAnd:
					fall_true (ec, b.left, lbl);
					fall_true (ec, b.right, lbl);
					break;

				case JSToken.LessThan:
					ig.Emit (OpCodes.Ldc_I4_0);
					ig.Emit (OpCodes.Conv_R8);
					ig.Emit (OpCodes.Blt, lbl);
					break;

				default:
					ast.Emit (ec);
					ig.Emit (OpCodes.Ldc_I4_1);
					ig.Emit (OpCodes.Call, typeof (Convert).GetMethod ("ToBoolean", new Type [] {typeof (object), typeof (bool)}));
					ig.Emit (OpCodes.Brfalse, lbl);
					break;
				}
			}
		}

		static void ft_emit_equality (EmitContext ec, AST ast, Label lbl)
		{
			ILGenerator ig = ec.ig;
			BinaryOp eq = null;

			if (ast is Equality)
				eq = (Equality) ast;
			else if (ast is StrictEquality)
				eq = (StrictEquality) ast;

			eq.Emit (ec);
			switch (eq.op) {
			case JSToken.NotEqual:
			case JSToken.StrictNotEqual:
				ig.Emit (OpCodes.Brtrue, lbl);
				break;
			case JSToken.Equal:
			case JSToken.StrictEqual:
				ig.Emit (OpCodes.Brfalse, lbl);
				break;
			}
		}

		internal static void fall_true (EmitContext ec, AST ast, Label lbl)
		{
			Type type = ast.GetType ();

			if (type == typeof (Expression)) {
				Expression exp = ast as Expression;
				AST last_exp = (AST) exp.exprs [exp.exprs.Count - 1];
				if (exp.exprs.Count >= 2)
					exp.Emit (ec);
				fall_true (ec, last_exp, lbl);
			} else if (type == typeof (Binary))
				ft_binary_recursion (ec, ast, lbl);
			else if (type == typeof (Equality) || type == typeof (StrictEquality))
				ft_emit_equality (ec, ast, lbl);
			else if (type == typeof (Relational))
				ft_emit_relational (ec, (Relational) ast, lbl);
			else
				emit_default_case (ec, ast, OpCodes.Brfalse, lbl);
		}

		static void ff_emit_relational (EmitContext ec, AST ast, Label lbl)
		{
			ILGenerator ig = ec.ig;
			Relational r = ast as Relational;
			r.Emit (ec);

			OpCode opcode;

			switch (r.op) {
			case JSToken.LessThan:
				opcode = OpCodes.Blt;
				break;

			case JSToken.GreaterThan:
				opcode = OpCodes.Bgt;
				break;

			case JSToken.LessThanEqual:
				opcode = OpCodes.Ble;
				break;

			case JSToken.GreaterThanEqual:
				opcode = OpCodes.Bge;
				break;

			default:
				throw new Exception ("unexpected token");
			}

			ig.Emit (OpCodes.Ldc_I4_0);
			ig.Emit (OpCodes.Conv_R8);
			ig.Emit (opcode, lbl);
		}

		static void ft_emit_relational (EmitContext ec, Relational re, Label lbl)
		{
			ILGenerator ig = ec.ig;

			re.Emit (ec);
			JSToken op = re.op;
			
			OpCode opcode;

			switch (op) {
			case JSToken.LessThan:
				opcode = OpCodes.Bge_Un;
				break;

			case JSToken.GreaterThan:
				opcode = OpCodes.Ble_Un;
				break;

			case JSToken.LessThanEqual:
				opcode = OpCodes.Bgt_Un;
				break;

			case JSToken.GreaterThanEqual:
				opcode = OpCodes.Blt_Un;
				break;

			default:
				Console.WriteLine (re.Location.LineNumber);
				throw new NotImplementedException ();
			}

			ig.Emit (OpCodes.Ldc_I4_0);
			ig.Emit (OpCodes.Conv_R8);
			ig.Emit (opcode, lbl);
		}

		static void ff_binary_recursion (EmitContext ec, AST ast, Label lbl)
		{
			ILGenerator ig = ec.ig;
			Binary b = ast as Binary;

			switch (b.op) {
			case JSToken.LogicalOr:
				fall_false (ec, b.left, lbl);
				fall_false (ec, b.right, lbl);
				break;

			case JSToken.LogicalAnd:
				Label ftLb = ig.DefineLabel ();
				fall_true (ec, b.left, ftLb);
				fall_false (ec, b.right, lbl);
				ig.MarkLabel (ftLb);
				break;
			}
		}

		static void ff_emit_equality_cond (EmitContext ec, AST ast, Label lbl)
		{
			ILGenerator ig = ec.ig;
			Equality eq = ast as Equality;
			eq.Emit (ec);

			switch (eq.op) {
			case JSToken.NotEqual:
			case JSToken.Equal:
				ig.Emit (OpCodes.Brfalse, lbl);
				break;
			}
		}
			
		internal static void fall_false (EmitContext ec, AST ast, Label lbl)
		{
			Type type = ast.GetType ();

			if (type == typeof (Expression)) {  
				Expression exp = ast as Expression;

				if (exp.Size > 1)
					exp.Emit (ec);

				AST last_exp = (AST) exp.exprs [exp.exprs.Count - 1];

				if (last_exp is Relational)
					ff_emit_relational (ec, last_exp, lbl);
				else if (last_exp is Binary)
					ff_binary_recursion (ec, last_exp, lbl);
				else if (last_exp is Identifier || last_exp is BooleanConstant)
					emit_default_case (ec, last_exp, OpCodes.Brtrue, lbl);
 				else if (last_exp is Equality) 
					ff_emit_equality_cond (ec, last_exp, lbl);
				else {
					Console.WriteLine ("WARNING: fall_false, last_exp.GetType () == {0}, {1}", last_exp, ast.Location.LineNumber);
				}
			} else if (type == typeof (Binary))
				ff_binary_recursion (ec, ast, lbl);
			else if (type == typeof (Relational))
				ff_emit_relational (ec, ast, lbl);
			else 
				emit_default_case (ec, ast, OpCodes.Brtrue, lbl);
		}

		internal static void emit_to_boolean (AST ast, ILGenerator ig, int i)
		{
			ig.Emit (OpCodes.Ldc_I4, i);
			ig.Emit (OpCodes.Call, typeof (Convert).GetMethod ("ToBoolean", 
									   new Type [] { typeof (object), typeof (Boolean)}));
		}

		internal static bool need_convert_to_boolean (AST ast)
		{
			if (ast == null)
				return false;

			if (ast is Identifier)
				return true;
			else if (ast is Expression) {
				Expression exp = ast as Expression;
				int n = exp.exprs.Count - 1;
				AST tmp = (AST) exp.exprs [n];
				if (tmp is Equality || tmp is Relational || tmp is BooleanConstant)
					return false;
				else
					return true;
			} else
				return false;
		}
		
		//
		// Loads a current VsaEngine
		//
		internal static void load_engine (bool in_function, ILGenerator ig)
		{
			//
			// If we are in a function declaration at global level,
			// we must load the engine associated to the current 'JScript N' instance,
			// otherwise pick up the engine at second place in method's signature.
			//
			if (in_function)
				ig.Emit (OpCodes.Ldarg_1);
			else {
				ig.Emit (OpCodes.Ldarg_0);
				ig.Emit (OpCodes.Ldfld, typeof (ScriptObject).GetField ("engine"));
			}
		}
		
		internal static void emit_get_default_this (ILGenerator ig, bool inFunction) 
		{
			CodeGenerator.load_engine (inFunction, ig);
			ig.Emit (OpCodes.Call, typeof (VsaEngine).GetMethod ("ScriptObjectStackTop"));
			Type iact_obj = typeof (IActivationObject);
			ig.Emit (OpCodes.Castclass, iact_obj);
			ig.Emit (OpCodes.Callvirt, iact_obj.GetMethod ("GetDefaultThisObject"));
		}

		internal static object variable_defined_in_current_scope (string id)
		{
			return TypeManager.defined_in_current_scope (id);
		}

		internal static void load_local_vars (ILGenerator ig, bool inFunction)
		{
			int n = 0;
			Type stack_frame = typeof (StackFrame);

			CodeGenerator.load_engine (inFunction, ig);
						
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
					ig.Emit (OpCodes.Ldloc, (LocalBuilder) local);
					ig.Emit (OpCodes.Stelem_Ref);
				}
			}
			ig.Emit (OpCodes.Pop);
		}
			
		internal static void locals_to_stack_frame (ILGenerator ig, int lexical_depth, int lexical_difference, bool inFunction)
		{
			CodeGenerator.emit_parents (inFunction, lexical_difference, ig);
			ig.Emit (OpCodes.Dup);
			
			Type stack_frame = typeof (StackFrame);
			ig.Emit (OpCodes.Castclass, stack_frame);
			ig.Emit (OpCodes.Ldfld, stack_frame.GetField ("localVars"));
			
			DictionaryEntry [] locals = TypeManager.LocalsAtDepth (lexical_depth);

			int i = 0;
			foreach (DictionaryEntry entry in locals) {
				if (entry.Value is LocalBuilder) {
					ig.Emit (OpCodes.Dup);
					ig.Emit (OpCodes.Ldc_I4, i);
					ig.Emit (OpCodes.Ldloc, (short) i++);
					ig.Emit (OpCodes.Stelem_Ref);
				}
			}
			ig.Emit (OpCodes.Pop);
			//
			// FIXME: what determine this?
			//
			ig.Emit (OpCodes.Call, typeof (ScriptObject).GetMethod ("GetParent"));
			ig.Emit (OpCodes.Pop);
		}

		internal static void emit_parents (bool inFunction, int lexical_difference, ILGenerator ig)
		{
			CodeGenerator.load_engine (inFunction, ig);
			ig.Emit (OpCodes.Call, typeof (VsaEngine).GetMethod ("ScriptObjectStackTop"));
			for (int i = 0; i < lexical_difference; i++)
				ig.Emit (OpCodes.Call, typeof (ScriptObject).GetMethod ("GetParent"));
		}

		internal static void EmitBox (ILGenerator ig, object obj)
		{
			if (obj == null) 
				return;

			Type box_type = GetBoxType (obj);

			if (box_type != null)
				ig.Emit (OpCodes.Box, box_type);
		}

		internal static void EmitConv (ILGenerator ig, Type type)
		{
			TypeCode tc = Type.GetTypeCode (type);
			
			switch (tc) {
			case TypeCode.Double:
				ig.Emit (OpCodes.Conv_R8);
				break;

			default:
				throw new NotImplementedException ();
			}
		}

		private static Type GetBoxType (object obj)
		{
			if (obj is ByteConstant || obj is ShortConstant || obj is IntConstant)
				return typeof (int);
			else if (obj is LongConstant)
				return typeof (long);
			else if (obj is FloatConstant || obj is DoubleConstant)
				return typeof (double);
			else if (obj is BooleanConstant || obj is StrictEquality || obj is Equality)
				return typeof (bool);
			else if (obj is Unary) {
				Unary unary = (Unary) obj;
				JSToken oper = unary.oper;
				AST operand = unary.operand;
				
				if (oper == JSToken.Minus || oper == JSToken.Plus ||
				    oper == JSToken.Increment || oper == JSToken.Decrement ||
				    oper == JSToken.BitwiseNot)
					return GetBoxType (operand);
				else if (oper == JSToken.LogicalNot || oper == JSToken.Delete)
					return typeof (bool);
			} else if (obj is Identifier) {
				Identifier id = (Identifier) obj;
				string name = id.name.Value;
				if  (name == "NaN" || name == "Infinity")
					return typeof (double);
			} else if (obj is Binary) {
				Binary bin = obj as Binary;
				if (bin.AccessField && !bin.LateBinding) {
					MemberInfo binding = bin.Binding;
					MemberTypes member_type = binding.MemberType;
					if (member_type == MemberTypes.Property)
						return ((PropertyInfo) binding).PropertyType;
				}
			} else if (obj is Relational) {
				Relational re = (Relational) obj;
				if (re.op == JSToken.In)
					return typeof (bool);
			}
			return null;
		}

		internal static void emit_default_value (ILGenerator ig, ParameterInfo param)
		{
			Type param_type = param.ParameterType;

			if (param_type == typeof (Double))
				ig.Emit (OpCodes.Ldc_R8, GlobalObject.NaN);
			else if (param_type == typeof (object))
				ig.Emit (OpCodes.Ldsfld, typeof (Missing).GetField ("Value"));
			else
				throw new NotImplementedException ();
		}

		internal static void EmitRelationalComp (ILGenerator ig, Relational re)
		{
			JSToken op = re.op;

			if (op == JSToken.Instanceof)
				return;
			else if (op == JSToken.In) {
				ig.Emit (OpCodes.Box, typeof (bool));
				return;
			}

			Label true_case = ig.DefineLabel ();
			Label box_to_bool = ig.DefineLabel ();

			ig.Emit (OpCodes.Ldc_I4_0);
			ig.Emit (OpCodes.Conv_R8);

			OpCode opcode;

			switch (op) {
			case JSToken.LessThan:
				opcode = OpCodes.Blt;
				break;

			case JSToken.LessThanEqual:
				opcode = OpCodes.Ble;
				break;

			case JSToken.GreaterThan:
				opcode = OpCodes.Bgt;
				break;

			case JSToken.GreaterThanEqual:
				opcode = OpCodes.Bge;
				break;

			default:
				Console.WriteLine (re.Location.LineNumber);
				throw new NotImplementedException ();
			}

			ig.Emit (opcode, true_case);
			ig.Emit (OpCodes.Ldc_I4_0);
			ig.Emit (OpCodes.Br, box_to_bool);
			ig.MarkLabel (true_case);
			ig.Emit (OpCodes.Ldc_I4_1);
			ig.MarkLabel (box_to_bool);
			ig.Emit (OpCodes.Box, typeof (bool));
		}

		internal static string GetTypeName (string srcName)
		{
			return (string) source_file_to_type [srcName];
		}

		internal static void EmitAssignAsExp (EmitContext ec, AST ast)
		{
			Assign assign = (Assign) ast;
			LocalBuilder builder = assign.EmitAndReturnBuilder (ec);
			ec.ig.Emit (OpCodes.Ldloc, builder);
		}
	}
}
