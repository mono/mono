//
// Mono.ILASM.MethodTable.cs
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

	public class MethodTable {

		private class MethodTableItem {
		
			private static readonly int DefinedFlag = 2;
	
			private int flags;

			public ArrayList LocationList;
			public Method Method;

			public MethodTableItem (Method method, Location location)
			{
				flags = 0;
				Method = method;
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
		
		public MethodTable (ClassDef parent_class)
		{
			this.parent_class = parent_class;
			table = new Hashtable ();
		}

		public Method GetReference (string name, TypeRef return_type, 
			Param[] param_list, TypeRef[] param_type_list, Location location)
		{
			string signature = GetSignature (name, return_type, param_type_list);

			MethodTableItem item = table[signature] as MethodTableItem;
			
			if (item != null) {
				item.LocationList.Add (location);
				return item.Method;
			}
			
			return null;
		}
	
		public MethodDef AddDefinition (MethAttr method_attr, ImplAttr impl_attr, CallConv call_conv, 
			string name, TypeRef return_type, Param[] param_list, 
			TypeRef[] param_type_list, Location location) 
		{
			string signature = GetSignature (name, return_type, param_type_list);

			CheckExists (signature);

			MethodDef method = parent_class.AddMethod (method_attr, impl_attr, name, 
				return_type.Type, param_list);
			method.AddCallConv (call_conv);
			AddDefined (signature, method, location);
			
			return method;
		}
			
		protected string GetSignature (string name, TypeRef return_type, 
			TypeRef[] param_list)
		{
			StringBuilder builder = new StringBuilder ();
			
			builder.Append (return_type.FullName);
			builder.Append ('_');
			builder.Append (name);
			builder.Append ('(');
			
			bool first = true;
			foreach (TypeRef type_ref in param_list) {
				if (!first)
					builder.Append (',');
				builder.Append (type_ref.FullName);
			}
			builder.Append (')');

			return builder.ToString ();
		}

		protected void AddDefined (string signature, MethodDef method, Location location)
		{
			if (table.Contains (signature))
				return; 

			MethodTableItem item = new MethodTableItem (method, location);
			item.Defined = true;

			table[signature] = item;
		}

		/// <summary>
		///  If a method is allready defined throw an Error
		/// </summary>
		protected void CheckExists (string signature) 
		{
			MethodTableItem item = table[signature] as MethodTableItem;
			
			if ((item != null) && (item.Defined)) {
				throw new Exception (String.Format ("Method: {0} defined in multiple locations.", 
					signature));
			}
		}
	}

}

