
//
// System.Reflection.Emit/ModuleBuilder.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Reflection.Emit {
	public class ModuleBuilder : Module {
		private TypeBuilder[] types;
		private CustomAttributeBuilder[] cattrs;
		private int table_idx;
		private AssemblyBuilder assemblyb;

		internal ModuleBuilder (AssemblyBuilder assb, string name, string fullyqname) {
			this.name = this.scopename = name;
			this.fqname = fullyqname;
			this.assembly = this.assemblyb = assb;
			table_idx = get_next_table_index (0x00, true);
		}
	
		public override string FullyQualifiedName {get { return fqname;}}

		[MonoTODO]
		public TypeBuilder DefineType (string name) {
			// FIXME: LAMESPEC: what other attributes should we use here as default?
			return DefineType (name, TypeAttributes.Public, typeof(object), null);
		}

		public TypeBuilder DefineType (string name, TypeAttributes attr) {
			return DefineType (name, attr, typeof(object), null);
		}

		public TypeBuilder DefineType (string name, TypeAttributes attr, Type parent) {
			return DefineType (name, attr, parent, null);
		}

		public TypeBuilder DefineType (string name, TypeAttributes attr, Type parent, Type[] interfaces) {
			TypeBuilder res = new TypeBuilder (this, name, attr, parent, interfaces);
			if (types != null) {
				TypeBuilder[] new_types = new TypeBuilder [types.Length + 1];
				System.Array.Copy (types, new_types, types.Length);
				new_types [types.Length] = res;
				types = new_types;
			} else {
				types = new TypeBuilder [1];
				types [0] = res;
			}
			return res;
		}

		public TypeBuilder DefineType (string name, TypeAttributes attr, Type parent, int typesize) {
			return DefineType (name, attr, parent, null);
		}

		public TypeBuilder DefineType (string name, TypeAttributes attr, Type parent, PackingSize packsize) {
			return DefineType (name, attr, parent, null);
		}

		public TypeBuilder DefineType (string name, TypeAttributes attr, Type parent, PackingSize packsize, int typesize) {
			return DefineType (name, attr, parent, null);
		}

		public MethodInfo GetArrayMethod( Type arrayClass, string methodName, CallingConventions callingConvention, Type returnType, Type[] parameterTypes) {
			return null;
		}

		public EnumBuilder DefineEnum( string name, TypeAttributes visibility, Type underlyingType) {
			EnumBuilder eb = new EnumBuilder (this, name, visibility, underlyingType);
			return eb;
		}

		public override Type GetType( string className) {
			return GetType (className, false, false);
		}
		
		public override Type GetType( string className, bool ignoreCase) {
			return GetType (className, false, ignoreCase);
		}

		private TypeBuilder search_in_array (TypeBuilder[] arr, string className, bool ignoreCase) {
			int i;
			for (i = 0; i < arr.Length; ++i) {
				if (String.Compare (className, arr [i].FullName, ignoreCase) == 0) {
					return arr [i];
				}
			}
			return null;
		}
		
		public override Type GetType( string className, bool throwOnError, bool ignoreCase) {
			int subt;
			TypeBuilder result = null;

			if (types == null && throwOnError)
				throw new TypeLoadException (className);
			subt = className.IndexOf ('+');
			if (subt < 0) {
				result = search_in_array (types, className, ignoreCase);
			} else {
				string pname, rname;
				pname = className.Substring (0, subt);
				rname = className.Substring (subt + 1);
				result = search_in_array (types, pname, ignoreCase);
				if ((result != null) && (result.subtypes != null))
					result = search_in_array (result.subtypes, rname, ignoreCase);
				else
					result = null;
			}
			if ((result == null) && throwOnError)
				throw new TypeLoadException (className);
			return result;
		}

		internal int get_next_table_index (int table, bool inc) {
			return assemblyb.get_next_table_index (table, inc);
		}

		public void SetCustomAttribute( CustomAttributeBuilder customBuilder) {
			if (cattrs != null) {
				CustomAttributeBuilder[] new_array = new CustomAttributeBuilder [cattrs.Length + 1];
				cattrs.CopyTo (new_array, 0);
				new_array [cattrs.Length] = customBuilder;
				cattrs = new_array;
			} else {
				cattrs = new CustomAttributeBuilder [1];
				cattrs [0] = customBuilder;
			}
		}
		public void SetCustomAttribute( ConstructorInfo con, byte[] binaryAttribute) {
			SetCustomAttribute (new CustomAttributeBuilder (con, binaryAttribute));
		}
	}
}
