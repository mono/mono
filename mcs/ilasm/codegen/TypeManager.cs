//
// Mono.ILASM.TypeManager.cs
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

	public class TypeManager {

		private Hashtable type_table;

		public TypeManager ()
		{

			type_table = new Hashtable ();
			Hashtable t = type_table;

			// Add the default types
			t ["object"]  = Type.GetType ("System.Object");
			t ["string"]  = Type.GetType ("System.String");
			t ["char"]    = Type.GetType ("System.Char");
			t ["void"]    = Type.GetType ("System.Void");
			t ["bool"]    = Type.GetType ("System.Boolean");
			t ["int8"]    = Type.GetType ("System.Byte");
			t ["int16"]   = Type.GetType ("System.Int16");
			t ["int32"]   = Type.GetType ("System.Int32");
			t ["int64"]   = Type.GetType ("System.Int64");
			t ["float32"] = Type.GetType ("System.Single");
			t ["float64"] = Type.GetType ("System.Double");
			t ["uint8"]   = Type.GetType ("System.SByte");
			t ["uint16"]  = Type.GetType ("System.UInt16");
			t ["uint32"]  = Type.GetType ("System.UInt32");
			t ["uint64"]  = Type.GetType ("System.UInt64");
		}

		public Type this [string type_name] {
			get {
				Type return_type = (Type)type_table[type_name];
					
				if (return_type == null) {
					return_type = LoadType (type_name);
					type_table[type_name] = return_type;
				}
				
				return return_type;
			}
		}

		/// TODO: Use AssemblyStore, and load types in the same assembly
		private Type LoadType (string type_name) {
			string assembly_name;
			Assembly assembly;
			int bracket_start, bracket_end;

			bracket_start = type_name.IndexOf ('[');
			bracket_end = type_name.IndexOf (']');
			
			Console.WriteLine ("Loading Type: {0}", type_name);
			Console.WriteLine ("bracket pos: {0} {1}", bracket_start, bracket_end);

			if ((bracket_start == -1) || (bracket_end == -1))
				return null;

			assembly_name = type_name.Substring (bracket_start, bracket_end);

			assembly = Assembly.LoadWithPartialName (assembly_name);
			
			return assembly.GetType (type_name);
		
		}

	}
		
}

