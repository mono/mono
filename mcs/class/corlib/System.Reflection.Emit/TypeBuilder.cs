//
// System.Reflection.Emit/TypeBuilder.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Reflection.Emit {
	public sealed class TypeBuilder : Type {
	private string tname;
	private string nspace;
	private Type parent;
	private Type[] interfaces;
	private MethodBuilder[] methods;
	private ConstructorBuilder[] ctors;
	private PropertyBuilder[] properties;
	private FieldBuilder[] fields;
	private TypeAttributes attrs;
	private int table_idx;
	private ModuleBuilder pmodule;
	private PackingSize packing_size;

	public const int UnspecifiedTypeSize = -1;

		internal override TypeAttributes AttributesImpl {
			get {return attrs;}
		}
		
		internal TypeBuilder (ModuleBuilder mb, string name, TypeAttributes attr, Type parent, Type[] interfaces) {
			int sep_index;
			this.parent = parent;
			this.attrs = attr;
			packing_size = PackingSize.Unspecified;
			sep_index = name.LastIndexOf('.');
			if (sep_index != -1) {
				this.tname = name.Substring (sep_index + 1);
				this.nspace = name.Substring (0, sep_index);
			} else {
				this.tname = name;
				this.nspace = "";
			}
			if (interfaces != null) {
				this.interfaces = new Type[interfaces.Length];
				System.Array.Copy (interfaces, this.interfaces, interfaces.Length);
			}
			pmodule = mb;
			// skip .<Module> ?
			table_idx = mb.get_next_table_index (0x02, true);
		}

		public override Assembly Assembly {get {return null;}}
		public override string AssemblyQualifiedName {get {return null;}}
		public override Type BaseType {get {return parent;}}
		public override Type DeclaringType {get {return null;}}
		public override Type UnderlyingSystemType {
			get {return null;}
		}

		public override string FullName {
			get {
				if (nspace != null)
					return String.Concat (nspace, ".", tname);
				return tname;
			}
		}
		//public override Guid GUID {get {return null;}}
		public override Module Module {
			get {return pmodule;}
		}
		public override string Name {
			get {return tname;}
		}
		public override string Namespace {
			get {return nspace;}
		}
		public PackingSize PackingSize {
			get {return packing_size;}
		}
		public override Type ReflectedType {get {return null;}}
		public override MemberTypes MemberType { 
			get {return MemberTypes.TypeInfo;}
		}

		public override bool IsDefined( Type attributeType, bool inherit) {
			return false;
		}
		public override object[] GetCustomAttributes(bool inherit) {
			return null;
		}
		public override object[] GetCustomAttributes(Type attributeType, bool inherit) {
			return null;
		}

		[MonoTODO]
		public TypeBuilder DefineNestedType (string name) {
			// FIXME: LAMESPEC: what other attributes should we use here as default?
			return DefineNestedType (name, TypeAttributes.Public, typeof(object), null);
		}

		public TypeBuilder DefineNestedType (string name, TypeAttributes attr) {
			return DefineNestedType (name, attr, typeof(object), null);
		}

		public TypeBuilder DefineNestedType (string name, TypeAttributes attr, Type parent) {
			return DefineNestedType (name, attr, parent, null);
		}

		public TypeBuilder DefineNestedType (string name, TypeAttributes attr, Type parent, Type[] interfaces) {
			TypeBuilder res = new TypeBuilder (pmodule, name, attr, parent, interfaces);
			/*if (types != null) {
				TypeBuilder[] new_types = new TypeBuilder [types.Length];
				System.Array.Copy (types, new_types, types.Length);
				new_types [types.Length] = res;
				types = new_types;
			} else {
				types = new TypeBuilder [1];
				types [0] = res;
			}*/
			return res;
		}

		public TypeBuilder DefineNestedType (string name, TypeAttributes attr, Type parent, int typesize) {
			return DefineNestedType (name, attr, parent, null);
		}

		public TypeBuilder DefineNestedType (string name, TypeAttributes attr, Type parent, PackingSize packsize) {
			return DefineNestedType (name, attr, parent, null);
		}

		public TypeBuilder DefineNestedType (string name, TypeAttributes attr, Type parent, PackingSize packsize, int typesize) {
			return DefineNestedType (name, attr, parent, null);
		}

		public ConstructorBuilder DefineConstructor( MethodAttributes attributes, CallingConventions callingConvention, Type[] parameterTypes) {
			ConstructorBuilder cb = new ConstructorBuilder (this, attributes, callingConvention, parameterTypes);
			if (ctors != null) {
				ConstructorBuilder[] new_ctors = new ConstructorBuilder [ctors.Length+1];
				System.Array.Copy (ctors, new_ctors, ctors.Length);
				new_ctors [ctors.Length] = cb;
				ctors = new_ctors;
			} else {
				ctors = new ConstructorBuilder [1];
				ctors [0] = cb;
			}
			return cb;
		}

		public ConstructorBuilder DefineDefaultConstructor( MethodAttributes attributes) {
			return DefineConstructor (attributes, CallingConventions.Standard, null);
		}

		public MethodBuilder DefineMethod( string name, MethodAttributes attributes, Type returnType, Type[] parameterTypes) {
			return DefineMethod (name, attributes, CallingConventions.Standard, returnType, parameterTypes);
		}

		private void append_method (MethodBuilder mb) {
			if (methods != null) {
				MethodBuilder[] new_methods = new MethodBuilder [methods.Length+1];
				System.Array.Copy (methods, new_methods, methods.Length);
				new_methods [methods.Length] = mb;
				methods = new_methods;
			} else {
				methods = new MethodBuilder [1];
				methods [0] = mb;
			}
		}

		public MethodBuilder DefineMethod( string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes) {
			MethodBuilder res = new MethodBuilder (this, name, attributes, callingConvention, returnType, parameterTypes);
			append_method (res);
			return res;
		}

		public MethodBuilder DefinePInvokeMethod (string name, string dllName, string entryName, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, CallingConvention nativeCallConv, CharSet nativeCharSet) {
			MethodBuilder res = new MethodBuilder (this, name, attributes, callingConvention, returnType, parameterTypes,
				dllName, entryName, nativeCallConv, nativeCharSet);
			append_method (res);
			return res;
		}

		public MethodBuilder DefinePInvokeMethod (string name, string dllName, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, CallingConvention nativeCallConv, CharSet nativeCharSet) {
			return DefinePInvokeMethod (name, dllName, name, attributes, callingConvention, returnType, parameterTypes,
				nativeCallConv, nativeCharSet);
		}

		public void DefineMethodOverride( MethodInfo methodInfoBody, MethodInfo methodInfoDeclaration) {
			if (methodInfoBody is MethodBuilder) {
				MethodBuilder mb = (MethodBuilder)methodInfoBody;
				mb.set_override (methodInfoDeclaration);
			}
		}

		public FieldBuilder DefineField( string fieldName, Type type, FieldAttributes attributes) {
			FieldBuilder res = new FieldBuilder (this, fieldName, type, attributes);
			if (fields != null) {
				FieldBuilder[] new_fields = new FieldBuilder [fields.Length+1];
				System.Array.Copy (fields, new_fields, fields.Length);
				new_fields [fields.Length] = res;
				fields = new_fields;
			} else {
				fields = new FieldBuilder [1];
				fields [0] = res;
			}
			return res;
		}

		public PropertyBuilder DefineProperty( string name, PropertyAttributes attributes, Type returnType, Type[] parameterTypes) {
			PropertyBuilder res = new PropertyBuilder (this, name, attributes, returnType, parameterTypes);

			if (properties != null) {
				PropertyBuilder[] new_properties = new PropertyBuilder [properties.Length+1];
				System.Array.Copy (properties, new_properties, properties.Length);
				new_properties [properties.Length] = res;
				properties = new_properties;
			} else {
				properties = new PropertyBuilder [1];
				properties [0] = res;
			}
			return res;
		}

		public Type CreateType() {
			if (methods != null) {
				foreach (MethodBuilder method in methods) {
					method.fixup ();
				}
			}
			if (ctors != null) {
				foreach (ConstructorBuilder ctor in ctors) {
					ctor.fixup ();
				}
			}
			return null;
		}

		public override Type GetElementType () { return null; }

		public override Type[] GetInterfaces () { return null; }

		public override RuntimeTypeHandle TypeHandle { get { return _impl; } }

		public void SetCustomAttribute( CustomAttributeBuilder customBuilder) {
		}
		public void SetCustomAttribute( ConstructorInfo con, byte[] binaryAttribute) {
		}

		public EventBuilder DefineEvent( string name, EventAttributes attributes, Type eventtype) {
			return null;
		}

		public FieldBuilder DefineInitializedData( string name, byte[] data, FieldAttributes attributes) {
			return null;
		}

		public FieldBuilder DefineUninitializedData( string name, int size, FieldAttributes attributes) {
			return null;
		}

		public void SetParent (Type parentType) {
			parent = parentType;
		}
		internal int get_next_table_index (int table, bool inc) {
			return pmodule.get_next_table_index (table, inc);
		}

	}
}
