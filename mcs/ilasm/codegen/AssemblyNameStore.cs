//
// Mono.ILASM.AssemblyNameStore
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;
using System.Reflection;
using System.Collections;

namespace Mono.ILASM {

	public class AssemblyNameStore {
	
		private Hashtable name_store;

		public AssemblyNameStore ()
		{

		}

		public void Add (AssemblyName assembly_name) 
		{
			if (name_store == null)
				name_store = new Hashtable ();
			name_store.Add (assembly_name.Name, assembly_name);
		}

		public Assembly Get (string name)
		{
			AssemblyName assembly_name;

			assembly_name = (AssemblyName)name_store[name];

			if (assembly_name == null)
				return null;
				
			return Assembly.Load (assembly_name);
		}
	}

}

