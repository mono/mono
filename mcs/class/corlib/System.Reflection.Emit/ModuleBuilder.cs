
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
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Diagnostics.SymbolStore;
using System.IO;

namespace System.Reflection.Emit {
	public class ModuleBuilder : Module {
		private TypeBuilder[] types;
		private CustomAttributeBuilder[] cattrs;
		private int table_idx;
		private AssemblyBuilder assemblyb;
		private ISymbolWriter symbol_writer;
		Hashtable name_cache;

		internal ModuleBuilder (AssemblyBuilder assb, string name, string fullyqname, bool emitSymbolInfo) {
			this.name = this.scopename = name;
			this.fqname = fullyqname;
			this.assembly = this.assemblyb = assb;
			table_idx = get_next_table_index (0x00, true);
			name_cache = new Hashtable ();

			if (emitSymbolInfo)
				symbol_writer = GetSymbolWriter (fullyqname);
		}

		internal ISymbolWriter GetSymbolWriter (string filename)
		{
			Assembly assembly;
			try {
				assembly = Assembly.Load ("Mono.CSharp.Debugger");
			} catch (FileNotFoundException) {
				return null;
			}

			Type type = assembly.GetType ("Mono.CSharp.Debugger.MonoSymbolWriter");
			if (type == null)
				return null;

			Type[] arg_types = new Type [1];
			arg_types [0] = typeof (string);
			ConstructorInfo constructor = type.GetConstructor (arg_types);

			object[] args = new object [1];
			args [0] = filename;

			if (constructor == null)
				return null;

			Object instance = constructor.Invoke (args);
			if (instance == null)
				return null;

			if (!(instance is ISymbolWriter))
				return null;

			return (ISymbolWriter) instance;
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
			name_cache.Add (name, res);
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
			if (arr == types && !ignoreCase)
				return (TypeBuilder)name_cache [className];
			for (i = 0; i < arr.Length; ++i) {
				if (String.Compare (className, arr [i].FullName, ignoreCase) == 0) {
					return arr [i];
				}
			}
			return null;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern Type create_modified_type (TypeBuilder tb, string modifiers);

		static char[] type_modifiers = {'&', '[', '*'};
		
		public override Type GetType( string className, bool throwOnError, bool ignoreCase) {
			int subt;
			string modifiers;
			TypeBuilder result = null;

			if (types == null && throwOnError)
				throw new TypeLoadException (className);

			subt = className.IndexOfAny (type_modifiers);
			if (subt >= 0) {
				modifiers = className.Substring (subt);
				className = className.Substring (0, subt);
			} else
				modifiers = null;
			
			subt = className.IndexOf ('+');
			if (subt < 0) {
				if (types != null)
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
			if (result != null && (modifiers != null))
				return create_modified_type (result, modifiers);
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

		public ISymbolWriter GetSymWriter () {
			return symbol_writer;
		}
	}
}
