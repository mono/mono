//
// Mono.ILASM.ExternTable.cs
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//

using System;
using System.Collections;
using System.Reflection;

namespace Mono.ILASM {

	public class ExternTable {

		protected class ExternAssembly {
			
			public PEAPI.AssemblyRef AssemblyRef;
			public Assembly Assembly;
			
			protected PEAPI.PEFile pefile;
			protected Hashtable type_table;

			public ExternAssembly (PEAPI.PEFile pefile, string name, 
				AssemblyName asmb_name)
			{
				type_table = new Hashtable ();
				this.pefile = pefile;
				AssemblyRef = pefile.AddExternAssembly (name);
				Assembly = Assembly.Load (asmb_name);
			}

			public PEAPI.Class GetType (string name_space, string name)
			{
				string full_name = String.Format ("{0}.{1}",
					name_space, name);
				PEAPI.Class klass = type_table[full_name] as PEAPI.Class;
				
				if (klass != null)
					return klass;

				// Make sure the type exists
				Type type = Assembly.GetType (full_name);

				if (type == null) {
					throw new Exception (String.Format ("Assembly {0} " + 
					" does not contain type {1}", AssemblyName.Name, full_name));
				}

				klass = AssemblyRef.AddClass (name_space, name);
				type_table[full_name] = klass;
		
				return klass;
			}
		}
 		
		PEAPI.PEFile pefile;
		Hashtable assembly_table;
		Hashtable type_table;
		Hashtable method_table;
		Hashtable member_table;

		public ExternTable (PEAPI.PEFile pefile)
		{
			this.pefile = pefile;
		}

		public void AddAssembly (string name, AssemblyName asmb_name)
		{
			if (assembly_table == null) {
				assembly_table = new Hashtable ();
			} else if (assembly_table.Contains (name)) {
				// Maybe this is an error??
				return;
			}
			
			assembly_table[name] = new ExternAssembly (pefile, name, asmb_name);
		}

		public PEAPI.Class GetClass (string asmb_name, string name_space, string name)
		{
			ExternAssembly ext_asmb;
			ext_asmb = assembly_table[asmb_name] as ExternAssembly;

			if (ext_asmb == null)
				throw new Exception (String.Format ("Assembly {0} not defined.", asmb_name));
			
			
			return ext_asmb.GetType (name_space, name);
		}
	}
}

