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

namespace Microsoft.JScript {

	internal class EmitContext {

		internal TypeBuilder type_builder;

		internal EmitContext (TypeBuilder type)
		{
			type_builder = type;
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
			EmitContext ec = new EmitContext (type_builder);

			prog.Emit (ec);

			ec.type_builder.CreateType ();
		}

		public static void Run (string file_name, AST prog)
		{
			CodeGenerator.Init (file_name);
			CodeGenerator.Emit (prog);

			CodeGenerator.Save (trim_extension (file_name) + 
					    ".exe");
		}	
	}
}
