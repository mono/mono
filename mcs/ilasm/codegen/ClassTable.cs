//
// Mono.ILASM.ClassTable.cs
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//

using PEAPI;
using System;
using System.Collections;

namespace Mono.ILASM {

	public class ClassTable {

		private class ClassTableItem {
		
			private static readonly int DefinedFlag = 2;
	
			private int flags;

			public ArrayList LocationList;
			public Class Class;

			public ClassTableItem (Class klass, Location location)
			{
				flags = 0;
				Class = klass;
				LocationList = new ArrayList ();
				LocationList.Add (location);
			}
		
			public bool Defined {
				get { return ((flags & DefinedFlag) != 0); }
				set {
					if (value)
						flags |= DefinedFlag;
					else
						flags ^= DefinedFlag;
				}
			}
		}

		protected readonly TypeAttr DefaultAttr;
		protected Hashtable table;
		protected PEFile pefile;
		
		public ClassTable (PEFile pefile)
		{
			DefaultAttr = TypeAttr.Public;
			this.pefile = pefile;
			table = new Hashtable ();
		}

		public Class Get (string full_name)
		{
			ClassTableItem item = table[full_name] as ClassTableItem;
			
			if (item == null)
				return null;

			return item.Class;
		}
		
		public Class GetReference (string full_name, Location location)
		{
			ClassTableItem item = table[full_name] as ClassTableItem;
			
			if (item != null) {
				item.LocationList.Add (location);
				return item.Class;
			}
			
			string name_space, name;			
			GetNameAndNamespace (full_name, out name_space, out name);
			Class klass = pefile.AddClass (DefaultAttr, name_space, name);
			AddReference (full_name, klass, location);
			
	
			return klass;
		}

		public ClassDef AddDefinition (string name_space, string name, 
			TypeAttr attr, Location location) 
		{
			string full_name = String.Format ("{0}.{1}", name_space, name);
			ClassTableItem item = table[full_name] as ClassTableItem;
			
			if ((item != null) && (item.Defined)) {
				throw new Exception (String.Format ("Class: {0} defined in multiple locations.", 
					full_name));
			}
			
			ClassDef klass = pefile.AddClass (attr, name_space, name);
			AddDefined (full_name, klass, location);

			return klass;
		}

		/// <summary>
		///  When there is no code left to compile, check to make sure referenced types where defined
		///  TODO: Proper error reporting
		/// </summary>
		public void CheckForUndefined ()
		{
			foreach (DictionaryEntry dic_entry in table) {
				ClassTableItem table_item = (ClassTableItem) dic_entry.Value;
				if (table_item.Defined)
					continue;
				throw new Exception (String.Format ("Type: {0} is not defined.", dic_entry.Key));
			}
		}

		protected void AddDefined (string full_name, Class klass, Location location)
		{
			if (table.Contains (full_name))
				return; 

			ClassTableItem item = new ClassTableItem (klass, location);
			item.Defined = true;

			table[full_name] = item;
		}

		protected void AddReference (string full_name, Class klass, Location location)
		{
			if (table.Contains (full_name))
				return;

			ClassTableItem item = new ClassTableItem (klass, location);
			
			table[full_name] = item;
		}

		protected void GetNameAndNamespace (string full_name,
			out string name_space, out string name) {
			
			int last_dot = full_name.LastIndexOf ('.');
	
			if (last_dot < 0) {
				name_space = String.Empty;
				name = full_name;
				return;
			}
				
			name_space = full_name.Substring (0, last_dot);
			name = full_name.Substring (last_dot + 1);
		}

	}

}

