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
using System.Globalization;
using System.Collections;
using System.Security;
using System.Security.Permissions;

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
	private CustomAttributeBuilder[] cattrs;
	internal TypeBuilder[] subtypes;
	private TypeAttributes attrs;
	private int table_idx;
	private ModuleBuilder pmodule;
	private int class_size;
	private PackingSize packing_size;

	public const int UnspecifiedTypeSize = -1;

		protected override TypeAttributes GetAttributeFlagsImpl () {
			return attrs;
		}
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern void setup_internal_class (TypeBuilder tb);
		
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
			setup_internal_class (this);
		}

		public override Assembly Assembly {
			get {return pmodule.Assembly;}
		}
		public override string AssemblyQualifiedName {get {return null;}}
		public override Type BaseType {get {return parent;}}
		public override Type DeclaringType {get {return null;}}
		public override Type UnderlyingSystemType {
			get {return null;}
		}

		public override string FullName {
			get {
				if ((nspace != null) && (nspace.Length > 0))
					return String.Concat (nspace, ".", tname);
				return tname;
			}
		}
	
		public override Guid GUID {
			get {return Guid.Empty;}
		}

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
		public override Type ReflectedType {get {return parent;}}
		public override MemberTypes MemberType { 
			get {return MemberTypes.TypeInfo;}
		}

		public void AddDeclarativeSecurity( SecurityAction action, PermissionSet pset) {
			throw new NotImplementedException ();
		}

		public void AddInterfaceImplementation( Type interfaceType) {
			throw new NotImplementedException ();
		}

		protected override ConstructorInfo GetConstructorImpl (BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers) {
			throw new NotImplementedException ();
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
			if (subtypes != null) {
				TypeBuilder[] new_types = new TypeBuilder [subtypes.Length + 1];
				System.Array.Copy (subtypes, new_types, subtypes.Length);
				new_types [subtypes.Length] = res;
				subtypes = new_types;
			} else {
				subtypes = new TypeBuilder [1];
				subtypes [0] = res;
			}
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

		public ConstructorBuilder DefineTypeInitializer() {
			throw new NotImplementedException ();
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
			return this;
		}

		public override ConstructorInfo[] GetConstructors (BindingFlags bindingAttr) {
			if (ctors == null)
				return new ConstructorInfo [0];
			ArrayList l = new ArrayList ();
			bool match;
			MethodAttributes mattrs;
			
			foreach (ConstructorBuilder c in ctors) {
				match = false;
				mattrs = c.Attributes;
				if ((mattrs & MethodAttributes.MemberAccessMask) == MethodAttributes.Public) {
					if ((bindingAttr & BindingFlags.Public) != 0)
						match = true;
				} else {
					if ((bindingAttr & BindingFlags.NonPublic) != 0)
						match = true;
				}
				if (!match)
					continue;
				match = false;
				if ((mattrs & MethodAttributes.Static) != 0) {
					if ((bindingAttr & BindingFlags.Static) != 0)
						match = true;
				} else {
					if ((bindingAttr & BindingFlags.Instance) != 0)
						match = true;
				}
				if (!match)
					continue;
				l.Add (c);
			}
			ConstructorInfo[] result = new ConstructorInfo [l.Count];
			l.CopyTo (result);
			return result;
		}

		public override Type GetElementType () { return null; }

		public override EventInfo GetEvent (string name, BindingFlags bindingAttr) {
			throw new NotImplementedException ();
		}

		public override EventInfo[] GetEvents (BindingFlags bindingAttr) {
			return new EventInfo [0];
		}

		public override FieldInfo GetField( string name, BindingFlags bindingAttr) {
			//FIXME
			throw new NotImplementedException ();
		}

		public override FieldInfo[] GetFields (BindingFlags bindingAttr) {
			if (fields == null)
				return new FieldInfo [0];
			ArrayList l = new ArrayList ();
			bool match;
			FieldAttributes mattrs;
			
			foreach (FieldInfo c in fields) {
				match = false;
				mattrs = c.Attributes;
				if ((mattrs & FieldAttributes.FieldAccessMask) == FieldAttributes.Public) {
					if ((bindingAttr & BindingFlags.Public) != 0)
						match = true;
				} else {
					if ((bindingAttr & BindingFlags.NonPublic) != 0)
						match = true;
				}
				if (!match)
					continue;
				match = false;
				if ((mattrs & FieldAttributes.Static) != 0) {
					if ((bindingAttr & BindingFlags.Static) != 0)
						match = true;
				} else {
					if ((bindingAttr & BindingFlags.Instance) != 0)
						match = true;
				}
				if (!match)
					continue;
				l.Add (c);
			}
			FieldInfo[] result = new FieldInfo [l.Count];
			l.CopyTo (result);
			return result;
		}

		public override Type GetInterface (string name, bool ignoreCase) {
			throw new NotImplementedException ();
		}
		
		public override Type[] GetInterfaces () {
			if (interfaces != null) {
				Type[] ret = new Type [interfaces.Length];
				System.Array.Copy (interfaces, ret, ret.Length);
				return ret;
			} else {
				return Type.EmptyTypes;
			}
		}

		public override MemberInfo[] GetMembers( BindingFlags bindingAttr) {
			// FIXME
			throw new NotImplementedException ();
		}

		public override MethodInfo[] GetMethods (BindingFlags bindingAttr) {
			if (methods == null)
				return new MethodInfo [0];
			ArrayList l = new ArrayList ();
			bool match;
			MethodAttributes mattrs;

			foreach (MethodInfo c in methods) {
				match = false;
				mattrs = c.Attributes;
				if ((mattrs & MethodAttributes.MemberAccessMask) == MethodAttributes.Public) {
					if ((bindingAttr & BindingFlags.Public) != 0)
						match = true;
				} else {
					if ((bindingAttr & BindingFlags.NonPublic) != 0)
						match = true;
				}
				if (!match)
					continue;
				match = false;
				if ((mattrs & MethodAttributes.Static) != 0) {
					if ((bindingAttr & BindingFlags.Static) != 0)
						match = true;
				} else {
					if ((bindingAttr & BindingFlags.Instance) != 0)
						match = true;
				}
				if (!match)
					continue;
				l.Add (c);
			}
			MethodInfo[] result = new MethodInfo [l.Count];
			l.CopyTo (result);
			return result;
		}

		protected override MethodInfo GetMethodImpl( string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers) {
			// FIXME
			throw new NotImplementedException ();
			return null;
		}
		
		public override Type GetNestedType( string name, BindingFlags bindingAttr) {
			// FIXME
			throw new NotImplementedException ();
			return null;
		}

		public override Type[] GetNestedTypes (BindingFlags bindingAttr) {
			// FIXME
			throw new NotImplementedException ();
			return null;
		}

		public override PropertyInfo[] GetProperties( BindingFlags bindingAttr) {
			if (properties == null)
				return new PropertyInfo [0];
			ArrayList l = new ArrayList ();
			bool match;
			MethodAttributes mattrs;
			MethodInfo accessor;
			
			foreach (PropertyInfo c in properties) {
				match = false;
				accessor = c.GetGetMethod (true);
				if (accessor == null)
					accessor = c.GetSetMethod (true);
				if (accessor == null)
					continue;
				mattrs = accessor.Attributes;
				if ((mattrs & MethodAttributes.MemberAccessMask) == MethodAttributes.Public) {
					if ((bindingAttr & BindingFlags.Public) != 0)
						match = true;
				} else {
					if ((bindingAttr & BindingFlags.NonPublic) != 0)
						match = true;
				}
				if (!match)
					continue;
				match = false;
				if ((mattrs & MethodAttributes.Static) != 0) {
					if ((bindingAttr & BindingFlags.Static) != 0)
						match = true;
				} else {
					if ((bindingAttr & BindingFlags.Instance) != 0)
						match = true;
				}
				if (!match)
					continue;
				l.Add (c);
			}
			PropertyInfo[] result = new PropertyInfo [l.Count];
			l.CopyTo (result);
			return result;
		}
		
		protected override PropertyInfo GetPropertyImpl( string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers) {
			// FIXME
			throw new NotImplementedException ();
			return null;
		}

		protected override bool HasElementTypeImpl () {
			return IsArrayImpl() || IsByRefImpl() || IsPointerImpl ();
		}

		public override object InvokeMember( string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters) {
			// FIXME
			throw new NotImplementedException ();
			return null;
		}

		protected override bool IsArrayImpl () {
			// FIXME
			return false;
		}
		protected override bool IsByRefImpl () {
			// FIXME
			return false;
		}
		protected override bool IsCOMObjectImpl () {
			return false;
		}
		protected override bool IsPointerImpl () {
			// FIXME
			return false;
		}
		protected override bool IsPrimitiveImpl () {
			// FIXME
			return false;
		}
		protected override bool IsValueTypeImpl () {
			// test this one
			return type_is_subtype_of (this, typeof (System.ValueType), false);
		}
		
		public override RuntimeTypeHandle TypeHandle { get { return _impl; } }

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

		public EventBuilder DefineEvent( string name, EventAttributes attributes, Type eventtype) {
			throw new NotImplementedException ();
		}

		static int InitializedDataCount = 0;
		
		public FieldBuilder DefineInitializedData( string name, byte[] data, FieldAttributes attributes) {
			TypeBuilder datablobtype = pmodule.DefineType ("$ArrayType$"+InitializedDataCount.ToString(),
				TypeAttributes.Public|TypeAttributes.ExplicitLayout|TypeAttributes.Sealed,
				typeof (System.ValueType), PackingSize.Size1, data.Length);
			datablobtype.packing_size = PackingSize.Size1;
			datablobtype.class_size = data.Length;
			datablobtype.CreateType ();
			FieldBuilder res = DefineField (name, datablobtype, attributes|FieldAttributes.Assembly|FieldAttributes.Static|FieldAttributes.HasFieldRVA);
			res.SetRVAData (data);
			InitializedDataCount++;
			return res;
		}

		public FieldBuilder DefineUninitializedData( string name, int size, FieldAttributes attributes) {
			throw new NotImplementedException ();
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
