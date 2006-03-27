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


	public delegate void MethodDefinedEvent (object sender, MethodDefinedEventArgs args);
	public delegate void MethodReferencedEvent (object sender, MethodReferencedEventArgs args);

	public class MethodEventArgs : EventArgs {
		
		public readonly string Signature;
		public readonly string Name;
		public readonly TypeRef ReturnType;
		public readonly Param[] ParamList;
		public readonly bool IsInTable;

		public MethodEventArgs (string signature, string name,
			TypeRef return_type, Param[] param_list, bool is_in_table) 
		{
			Signature = signature;
			Name = name;
			ReturnType = return_type;
			ParamList = param_list;
			IsInTable = is_in_table;
		}
	}

	public class MethodDefinedEventArgs : MethodEventArgs {

		public readonly MethAttr MethodAttributes;
		public readonly ImplAttr ImplAttributes;
		public readonly CallConv CallConv;
		
		public MethodDefinedEventArgs (string signature, string name, 
			TypeRef return_type, Param[] param_list, bool is_in_table, MethAttr method_attr, 
			ImplAttr impl_attr, CallConv call_conv) : base (signature, name, 
			return_type, param_list, is_in_table)
		{
			MethodAttributes = method_attr;
			ImplAttributes = impl_attr;
			CallConv = call_conv;
		}
	}

	public class MethodReferencedEventArgs : MethodEventArgs {
		
		public MethodReferencedEventArgs (string signature, string name, 
			TypeRef return_type, Param[] param_list, bool is_in_table) : base (signature, name, 
			return_type, param_list, is_in_table)
		{

		}
	}


	public class MethodTable {

		private class MethodTableItem {
		
			private static readonly int DefinedFlag = 2;
	
			private int flags;

			public ArrayList LocationList;
			public MethodDef Method;

			public MethodTableItem (MethodDef method, Location location)
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
		
		public static event MethodReferencedEvent MethodReferencedEvent;
		public static event MethodDefinedEvent MethodDefinedEvent;

		public MethodTable (ClassDef parent_class)
		{
			this.parent_class = parent_class;
			table = new Hashtable ();
		}

		public Method GetReference (string name, TypeRef return_type, 
			Param[] param_list, TypeRef[] param_type_list, Location location)
		{
			string signature = GetSignature (name, return_type, param_type_list);

			if (MethodReferencedEvent != null)
				MethodReferencedEvent (this, new MethodReferencedEventArgs (signature, name,
					return_type, param_list, table.Contains (signature)));

			MethodTableItem item = table[signature] as MethodTableItem;
			
			if (item != null) {
				item.LocationList.Add (location);
				return item.Method;
			}
			
			MethodDef method = parent_class.AddMethod (name, return_type.Type,
				param_list);
			
			AddReferenced (signature, method, location);

			return method;
		}
	
		public MethodDef AddDefinition (MethAttr method_attr, ImplAttr impl_attr, CallConv call_conv, 
			string name, TypeRef return_type, Param[] param_list, 
			TypeRef[] param_type_list, Location location) 
		{
			string signature = GetSignature (name, return_type, param_type_list);

			if (MethodDefinedEvent != null)
				MethodDefinedEvent (this, new MethodDefinedEventArgs (signature, name,
					return_type, param_list, table.Contains (signature), method_attr, 
					impl_attr, call_conv));

			MethodTableItem item = (MethodTableItem) table[signature];
			
			if (item == null) {
				MethodDef method = parent_class.AddMethod (method_attr, impl_attr, name, 
					return_type.Type, param_list);
				method.AddCallConv (call_conv);
				AddDefined (signature, method, location);
				return method;
			}
			
			item.Method.AddMethAttribute (method_attr);
			item.Method.AddImplAttribute (impl_attr);
			item.Method.AddCallConv (call_conv);
			item.Defined = true;
		
			return item.Method;
		}

		public bool CheckDefined ()
		{
			foreach (DictionaryEntry dic_entry in table) {
				MethodTableItem table_item = (MethodTableItem) dic_entry.Value;
				if (table_item.Defined)
					continue;
				Report.Error (String.Format ("Method: {0} is not defined.", dic_entry.Key));
			}
			return true;
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

		protected void AddReferenced (string signature, MethodDef method, Location location)
		{
			MethodTableItem item = new MethodTableItem (method, location);
			
			table[signature] = item;
		}

		/// <summary>
		///  If a method is allready defined throw an Error
		/// </summary>
		protected void CheckExists (string signature) 
		{
			MethodTableItem item = table[signature] as MethodTableItem;
			
			if ((item != null) && (item.Defined)) {
				Report.Error (String.Format ("Method: {0} defined in multiple locations.", 
					signature));
			}
		}
	}

}

