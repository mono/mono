//
// Mono.ILASM.CodeGen.cs
//
// Author(s):
//  Sergey Chaban (serge@wildwestsoftware.com)
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) Sergey Chaban
// (C) 2003 Jackson Harper, All rights reserved
//

using PEAPI;
using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.ILASM {

	public class CodeGen {
		
		private PEFile pefile;

		private string current_namespace;
		private ClassDef current_class;
		private MethodDef current_method;
		
		private ClassTable class_table;
		private ExternTable extern_table;

		public CodeGen (string output_file, bool is_dll, bool is_assembly)
		{
			pefile = new PEFile (output_file, is_dll, is_assembly);
			class_table = new ClassTable (pefile);
			extern_table = new ExternTable (pefile);
		}
	
		public PEFile PEFile {
			get { return pefile; }
		}

		public string CurrentNameSpace {
			get { return current_namespace; }
			set { current_namespace = value; }
		}

		public ClassDef CurrentClass {
			get { return current_class; }
			set { current_class = value; }
		}
		
		public MethodDef CurrentMethod {
			get { return current_method; }
			set { current_method = value; }
		}

		public ClassTable ClassTable {
			get { return class_table; }
		}

		public ExternTable ExternTable {
			get { return extern_table; }
		}

		public void AddClass (TypeAttr at, string name, Location location)
		{
			current_class = class_table.AddDefinition (current_namespace, name, at, location);
		}
		
		public void AddClass (TypeAttr at, string name, Class parent, Location location)
		{
		 	current_class = class_table.AddDefinition (current_namespace, name, 
				at, parent, location);
		}
		
		public void AddMethod (MethAttr method_attr, ImplAttr impl_attr, CallConv call_conv, string name, 
			TypeRef return_type, Param[] param_list, TypeRef[] param_type_list, Location location) 
		{
			MethodTable method_table = class_table.GetMethodTable (current_class.Name, location);
			current_method = method_table.AddDefinition (method_attr, impl_attr, call_conv, name, 
				return_type, param_list, param_type_list, location);
		}
	}

}

