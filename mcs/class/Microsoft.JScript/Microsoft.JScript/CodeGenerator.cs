//
// CodeGenerator.cs
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
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
		internal bool is_global_code_method;

		internal EmitContext (TypeBuilder type)
		{
			type_builder = type;
			is_global_code_method = false;
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

			module_builder = assembly_builder.DefineDynamicModule (
						mod_name,
						assembly_name.Name + ".exe", 
						false);
		}

 		internal static string trim_extension (string file_name)
		{
			int index = file_name.IndexOf ('.');

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
			type_builder = module_builder.DefineType ("JScript 0");

			type_builder.SetParent (typeof (GlobalScope));
			type_builder.SetCustomAttribute (new CustomAttributeBuilder
							 (typeof (CompilerGlobalScopeAttribute).GetConstructor (new Type [] {}), new object [] {}));

			EmitContext ec = new EmitContext (type_builder);

			//
			// Build the default constructor of the type
			//
			emit_default_script_constructor (ec);

			// here we only emit code for declarations.
			prog.Emit (ec);

			//
			// statements and expression at global scope are emitted 
			// inside 'Global Code' method.
			//
			type_builder = ec.type_builder;
			MethodBuilder method = type_builder.DefineMethod ("Global Code",
									  MethodAttributes.Public,
									  typeof (System.Object),
									  new Type [] {});
									     
			ec.ig = method.GetILGenerator ();
			ec.is_global_code_method = true;

			emit_default_init_global_code (ec.ig);
			prog.Emit (ec);
			emit_default_end_global_code (ec.ig);

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
	}
}
