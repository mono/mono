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

		public CodeGen (string output_file, bool is_dll, bool is_assembly)
		{
			pefile = new PEFile (output_file, is_dll, is_assembly);
			class_table = new ClassTable (pefile);
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
			set { class_table = value; }
		}

		public void AddClass (TypeAttr at, string name)
		{
			current_class = pefile.AddClass (at, current_namespace, name);
		}

		public void AddClass (TypeAttr at, string name, Class parent)
		{
			current_class = pefile.AddClass (at, current_namespace, name, parent);
		}
	}

}

