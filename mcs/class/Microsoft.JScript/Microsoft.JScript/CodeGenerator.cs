//
// CodeGenerator.cs
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
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Microsoft.JScript.Vsa;
using System.Runtime.CompilerServices;

namespace Microsoft.JScript {

	internal class EmitContext {

		internal TypeBuilder type_builder;
		internal ILGenerator ig;
		internal ModuleBuilder mod_builder;

		internal Label LoopBegin, LoopEnd;

		internal EmitContext (TypeBuilder type)
		{
			type_builder = type;

			if (type_builder != null) {
				MethodBuilder global_code =  type_builder.DefineMethod (
									"Global Code",
									MethodAttributes.Public,
									typeof (System.Object),
									new Type [] {});
				ig = global_code.GetILGenerator ();
			}
		}

		internal EmitContext (TypeBuilder type_builder, ModuleBuilder mod_builder, ILGenerator ig)
		{
			this.type_builder = type_builder;
			this.mod_builder = mod_builder;
			this.ig = ig;
		}
	}

	public class CodeGenerator {

		private static string MODULE = "JScript Module";

		internal static string mod_name;
		internal static AppDomain app_domain;
		internal static AssemblyName assembly_name;
		internal static AssemblyBuilder assembly_builder;
		internal static ModuleBuilder module_builder;

		internal static void Init (string file_name)
		{
			app_domain = Thread.GetDomain ();

			assembly_name = new AssemblyName ();
			assembly_name.Name =  trim_extension (file_name);

			mod_name = MODULE;

			assembly_builder = app_domain.DefineDynamicAssembly (
					     assembly_name,
					     AssemblyBuilderAccess.RunAndSave);

			ConstructorInfo ctr_info = typeof (Microsoft.JScript.ReferenceAttribute).GetConstructor (new Type [] { typeof (string) });
			// FIXME: find out which is the blob.
			byte [] blob  = new byte [] {};
			assembly_builder.SetCustomAttribute (ctr_info, blob); 

			module_builder = assembly_builder.DefineDynamicModule (
						mod_name,
						assembly_name.Name + ".exe", 
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
			assembly_builder.Save (target_name);
		}

		internal static void Emit (AST prog)
		{
			if (prog == null)
				return;

			TypeBuilder type_builder;
			type_builder = module_builder.DefineType ("JScript 0", TypeAttributes.Public);

			type_builder.SetParent (typeof (GlobalScope));
			type_builder.SetCustomAttribute (new CustomAttributeBuilder
							 (typeof (CompilerGlobalScopeAttribute).GetConstructor (new Type [] {}), new object [] {}));

			EmitContext ec = new EmitContext (type_builder);
			ec.mod_builder = module_builder;
			ILGenerator global_code = ec.ig;

			emit_default_script_constructor (ec);
			emit_default_init_global_code (global_code);
			prog.Emit (ec);
			emit_default_end_global_code (global_code);
			ec.type_builder.CreateType ();

			//
			// Build the default 'JScript Main' class
			//
			ec.type_builder = module_builder.DefineType ("JScript Main");
			emit_jscript_main (ec.type_builder);
			ec.type_builder.CreateType ();
		}

		internal static void emit_default_init_global_code (ILGenerator ig)
		{
			ig.Emit (OpCodes.Ldarg_0);
			ig.Emit (OpCodes.Ldfld, typeof (ScriptObject).GetField ("engine"));
			ig.Emit (OpCodes.Ldarg_0);
			ig.Emit (OpCodes.Call,
				typeof (VsaEngine).GetMethod ("PushScriptObject",
							      new Type [] { typeof (ScriptObject)}));
		}

		internal static void emit_default_end_global_code (ILGenerator ig)
		{
			ig.Emit (OpCodes.Ldnull);
			ig.Emit (OpCodes.Ldarg_0);
			ig.Emit (OpCodes.Ldfld, typeof (ScriptObject).GetField ("engine"));
			ig.Emit (OpCodes.Call, typeof (VsaEngine).GetMethod ("PopScriptObject"));
			ig.Emit (OpCodes.Pop);
			ig.Emit (OpCodes.Ret);		
		}

		internal static void emit_default_script_constructor (EmitContext ec)
		{
			ConstructorBuilder cons_builder;
			TypeBuilder tb = ec.type_builder;
			cons_builder = tb.DefineConstructor (MethodAttributes.Public,
							     CallingConventions.Standard,
							     new Type [] { typeof (GlobalScope) });

			ILGenerator ig = cons_builder.GetILGenerator ();
			ig.Emit (OpCodes.Ldarg_0);
			ig.Emit (OpCodes.Ldarg_1);
			ig.Emit (OpCodes.Dup);
			ig.Emit (OpCodes.Ldfld,
				 typeof (ScriptObject).GetField ("engine"));
			
			ig.Emit (OpCodes.Call, 
				 typeof (GlobalScope).GetConstructor (new Type [] {typeof (GlobalScope), 
										   typeof (VsaEngine)}));
			ig.Emit (OpCodes.Ret);
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

			ig.Emit (OpCodes.Ldstr,
				 "mscorlib, Version=1.0.3300.0, Culture=neutral, Pub" + 
				 "licKeyToken=b77a5c561934e089");

			ig.Emit (OpCodes.Stelem_Ref);

			ig.Emit (OpCodes.Call,
				 typeof (VsaEngine).GetMethod ("CreateEngineAndGetGlobalScope", 
							       new Type [] {typeof (bool), 
									    typeof (string [])}));	
			ig.Emit (OpCodes.Stloc_0);
			ig.Emit (OpCodes.Ldloc_0);
			
			ig.Emit (OpCodes.Newobj,
				 assembly_builder.GetType ("JScript 0").GetConstructor (
									new Type [] {typeof (GlobalScope)})); 
			ig.Emit (OpCodes.Call, 
				 assembly_builder.GetType ("JScript 0").GetMethod (
									   "Global Code", new Type [] {}));
			ig.Emit (OpCodes.Pop);
			ig.Emit (OpCodes.Ret);

			assembly_builder.SetEntryPoint (method);
		}

		public static void Run (string file_name, AST prog)
		{
			CodeGenerator.Init (file_name);
			CodeGenerator.Emit (prog);
			CodeGenerator.Save (trim_extension (file_name) + ".exe");
		}

		internal static void fall_true (EmitContext ec, AST ast, Label lbl)
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
				}
			} else {
				ast.Emit (ec);
				if (need_convert_to_boolean (ast))
					emit_to_boolean (ast, ig, 0);
				ig.Emit (OpCodes.Brfalse, lbl);
			}
		}

		internal static void fall_false (EmitContext ec, AST ast, Label lbl)
		{
			ILGenerator ig = ec.ig;
			if (ast is Binary) {
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
			} else {			        
				ast.Emit (ec);
				if (need_convert_to_boolean (ast))
					emit_to_boolean (ast, ig, 0);
				ig.Emit (OpCodes.Brtrue, lbl);
			}
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
				if (tmp is Equality || tmp is Relational || tmp is BooleanLiteral)
					return false;
				else
					return true;
			} else 
				return false;
		}
	}
}
