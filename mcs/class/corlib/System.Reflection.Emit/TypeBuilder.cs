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

namespace System.Reflection.Emit {
	public sealed class TypeBuilder : Type {
	private string name;
	private string nspace;
	private Type parent;
	private Type[] interfaces;
	private MethodBuilder[] methods;
	private PropertyBuilder[] properties;
	private FieldBuilder[] fields;
	private TypeAttributes attrs;
	private int table_idx;
	internal ModuleBuilder module;

	internal TypeBuilder (ModuleBuilder mb, string name, TypeAttributes attr, Type parent, Type[] interfaces) {
			int sep_index;
			this.parent = parent;
			this.attrs = attr;
			sep_index = name.LastIndexOf('.');
			if (sep_index != -1) {
				this.name = name.Substring (sep_index + 1);
				this.nspace = name.Substring (0, sep_index);
			} else {
				this.name = name;
				this.nspace = "";
			}
			if (interfaces != null) {
				this.interfaces = new Type[interfaces.Length];
				System.Array.Copy (interfaces, this.interfaces, interfaces.Length);
			}
			module = mb;
			table_idx = mb.assemblyb.get_next_table_index (0x02, true);
	}

	public const int UnspecifiedTypeSize = 1; // FIXME: check the real value
		
		public override Assembly Assembly {get {return null;}}
		public override string AssemblyQualifiedName {get {return null;}}
		public override Type BaseType {get {return null;}}
		public override Type DeclaringType {get {return null;}}
		public override string FullName {get {return null;}}
		//public override Guid GUID {get {return null;}}
		//public override Module Module {get {return null;}}
		public override string Name {get {return null;}}
		//public override string Namespace {get {return null;}}
		public PackingSize PackingSize {get {return (PackingSize)0;}}
		public override Type ReflectedType {get {return null;}}
		public override MemberTypes MemberType { get {return (MemberTypes)0;}}

		public override bool IsDefined( Type attributeType, bool inherit) {
			return false;
		}
		public override object[] GetCustomAttributes(bool inherit) {
			return null;
		}
		public override object[] GetCustomAttributes(Type attributeType, bool inherit) {
			return null;
		}

		public MethodBuilder DefineMethod( string name, MethodAttributes attributes, Type returnType, Type[] parameterTypes) {
			return DefineMethod (name, attributes, CallingConventions.Standard, returnType, parameterTypes);
		}

		public MethodBuilder DefineMethod( string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes) {
			MethodBuilder res = new MethodBuilder (this, name, attributes, callingConvention, returnType, parameterTypes);
			if (methods != null) {
				MethodBuilder[] new_methods = new MethodBuilder [methods.Length+1];
				System.Array.Copy (methods, new_methods, methods.Length);
				new_methods [methods.Length] = res;
				methods = new_methods;
			} else {
				methods = new MethodBuilder [1];
				methods [0] = res;
			}
			return res;
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
			return null;
		}

		public override Type GetElementType () { return null; }

		public override Type[] GetInterfaces () { return null; }

		public override RuntimeTypeHandle TypeHandle { get { return _impl; } }
	}
}
