//
// Mono.ILASM.FieldTable.cs
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//

using PEAPI;
using System;
using System.Text;
using System.Collections;

namespace Mono.ILASM {

	public class FieldTable {

		private class FieldTableItem {
		
			private static readonly int DefinedFlag = 2;
	
			private int flags;

			public ArrayList LocationList;
			public FieldDef Field;

			public FieldTableItem (FieldDef field, Location location)
			{
				flags = 0;
				Field = field;
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

		protected Hashtable table;
		protected ClassDef parent_class;
		
		public FieldTable (ClassDef parent_class)
		{
			this.parent_class = parent_class;
			table = new Hashtable ();
		}

		public Field GetReference (TypeRef type, string name, Location location)
		{
			FieldTableItem item = table[name] as FieldTableItem;
			
			if (item != null) {
				item.LocationList.Add (location);
				return item.Field;
			}
			
			FieldDef field = parent_class.AddField (name, type.Type);
			AddReferenced (name, field, location);

			return field;
		}
	
		public FieldDef AddDefinition (FieldAttr field_attr, string name, 
			TypeRef type, Location location) 
		{
			FieldTableItem item = (FieldTableItem) table[name];
			
			if (item == null) {
				FieldDef field = parent_class.AddField (field_attr, name, type.Type);
				AddDefined (name, field, location);
				return field;
			}
			
			item.Field.AddFieldAttr (field_attr);
			item.Defined = true;
			
			return item.Field;
		}

		public bool CheckDefined ()
		{
			foreach (DictionaryEntry dic_entry in table) {
				FieldTableItem table_item = (FieldTableItem) dic_entry.Value;
				if (table_item.Defined)
					continue;
				Report.Error (String.Format ("Field: {0} is not defined.", dic_entry.Key));
			}
			return true;
		}
			
		protected void AddDefined (string signature, FieldDef field, Location location)
		{
			if (table.Contains (signature))
				return; 

			FieldTableItem item = new FieldTableItem (field, location);
			item.Defined = true;

			table[signature] = item;
		}

		protected void AddReferenced (string signature, FieldDef field, Location location)
		{
			FieldTableItem item = new FieldTableItem (field, location);
			
			table[signature] = item;
		}

		/// <summary>
		///  If a field is allready defined throw an Error
		/// </summary>
		protected void CheckExists (string signature) 
		{
			FieldTableItem item = table[signature] as FieldTableItem;
			
			if ((item != null) && (item.Defined))
				Report.Error ("Field: " + signature + " defined in multiple locations.");
		}
	}

}

