//
// cil-codegen.cs: The CIL code generation interface
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

using System;
using System.Reflection;
using System.Reflection.Emit;

public class CilCodeGen {
	AppDomain current_domain;
	AssemblyBuilder assembly_builder;
	ModuleBuilder   module_builder;

	public CilCodeGen (string name, string output)
	{
		AssemblyName an;
		
		an = new AssemblyName ();
		an.Name = "AssemblyName";
		current_domain = AppDomain.CurrentDomain;
		assembly_builder = current_domain.DefineDynamicAssembly (
			an, AssemblyBuilderAccess.RunAndSave);
		
		module_builder = assembly_builder.DefineDynamicModule (name, output);
	}
	
	public AssemblyBuilder AssemblyBuilder {
		get {
			return assembly_builder;
		}
	}
	
	public ModuleBuilder ModuleBuilder {
		get {
			return module_builder;
		}
	}

	public void Save (string name)
	{
		assembly_builder.Save (name);
	}
}
