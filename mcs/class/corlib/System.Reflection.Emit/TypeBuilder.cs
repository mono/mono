//
// System.Reflection.Emit/TypeBuilder.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Text;
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
	#region Sync with reflection.h
	private string tname;
	private string nspace;
	private Type parent;
	private Type nesting_type;
	private Type[] interfaces;
	private int num_methods;
	private MethodBuilder[] methods;
	private ConstructorBuilder[] ctors;
	private PropertyBuilder[] properties;
	private int num_fields;
	private FieldBuilder[] fields;
	private EventBuilder[] events;
	private CustomAttributeBuilder[] cattrs;
	internal TypeBuilder[] subtypes;
	internal TypeAttributes attrs;
	private int table_idx;
	private ModuleBuilder pmodule;
	private int class_size;
	private PackingSize packing_size;
#if NET_2_0 || BOOTSTRAP_NET_2_0
	private	GenericTypeParameterBuilder[] generic_params;
#else
        private Object generic_params; /* so offsets don't change */
#endif
	private RefEmitPermissionSet[] permissions;	
	#endregion
	private Type created;
	string fullname;

	public const int UnspecifiedTypeSize = 0;

		protected override TypeAttributes GetAttributeFlagsImpl () {
			return attrs;
		}
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern void setup_internal_class (TypeBuilder tb);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern void create_internal_class (TypeBuilder tb);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern void setup_generic_class (TypeBuilder tb);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern EventInfo get_event_info (EventBuilder eb);

		internal TypeBuilder (ModuleBuilder mb, TypeAttributes attr) {
			this.parent = null;
			this.attrs = attr;
			this.class_size = UnspecifiedTypeSize;
			this.table_idx = 1;
			fullname = this.tname = "<Module>";
			this.nspace = "";
			pmodule = mb;
			setup_internal_class (this);
		}

		internal TypeBuilder (ModuleBuilder mb, string name, TypeAttributes attr, Type parent, Type[] interfaces, PackingSize packing_size, int type_size, Type nesting_type) {
			int sep_index;
			this.parent = parent;
			this.attrs = attr;
			this.class_size = type_size;
			this.packing_size = packing_size;
			this.nesting_type = nesting_type;
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
			table_idx = mb.get_next_table_index (this, 0x02, true);
			setup_internal_class (this);
			fullname = GetFullName ();
		}

		public override Assembly Assembly {
			get {return pmodule.Assembly;}
		}

		public override string AssemblyQualifiedName {
			get {
				return fullname + ", " + Assembly.GetName().FullName;
			}
		}
		public override Type BaseType {
			get {
				return parent;
			}
		}
		public override Type DeclaringType {get {return nesting_type;}}

/*		public override bool IsSubclassOf (Type c)
		{
			Type t;
			if (c == null)
				return false;
			if (c == this)
				return false;
			t = parent;
			while (t != null) {
				if (c == t)
					return true;
				t = t.BaseType;
			}
			return false;
		}*/

		[MonoTODO]
		public override Type UnderlyingSystemType {
			get {
				// This should return the type itself for non-enum types but 
				// that breaks mcs.
				if (fields != null) {
					foreach (FieldBuilder f in fields) {
						if ((f != null) && (f.Attributes & FieldAttributes.Static) == 0)
							return f.FieldType;
					}
				}
				throw new InvalidOperationException ("Underlying type information on enumeration is not specified.");
			}
		}

		string GetFullName () {
			if (nesting_type != null)
				return String.Concat (nesting_type.FullName, "+", tname);
			if ((nspace != null) && (nspace.Length > 0))
				return String.Concat (nspace, ".", tname);
			return tname;
		}
	
		public override string FullName {
			get {
				return fullname;
			}
		}
	
		public override Guid GUID {
			get {
			    throw not_supported ();
			}
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
		public int Size {
			get { return class_size; }
		}
		public override Type ReflectedType {get {return nesting_type;}}

		public void AddDeclarativeSecurity( SecurityAction action, PermissionSet pset) {
			if (pset == null)
				throw new ArgumentNullException ("pset");
			if ((action == SecurityAction.RequestMinimum) ||
				(action == SecurityAction.RequestOptional) ||
				(action == SecurityAction.RequestRefuse))
				throw new ArgumentException ("Request* values are not permitted", "action");

			if (is_created)
				throw not_after_created ();

			if (permissions != null) {
				/* Check duplicate actions */
				foreach (RefEmitPermissionSet set in permissions)
					if (set.action == action)
						throw new InvalidOperationException ("Multiple permission sets specified with the same SecurityAction.");

				RefEmitPermissionSet[] new_array = new RefEmitPermissionSet [permissions.Length + 1];
				permissions.CopyTo (new_array, 0);
				permissions = new_array;
			}
			else
				permissions = new RefEmitPermissionSet [1];

			permissions [permissions.Length - 1] = new RefEmitPermissionSet (action, pset.ToXml ().ToString ());
			attrs |= TypeAttributes.HasSecurity;
		}

		public void AddInterfaceImplementation( Type interfaceType) {
			if (interfaceType == null)
				throw new ArgumentNullException ("interfaceType");
			if (is_created)
				throw not_after_created ();

			if (interfaces != null) {
				// Check for duplicates
				foreach (Type t in interfaces)
					if (t == interfaceType)
						return;

				Type[] ifnew = new Type [interfaces.Length + 1];
				interfaces.CopyTo (ifnew, 0);
				ifnew [interfaces.Length] = interfaceType;
				interfaces = ifnew;
			} else {
				interfaces = new Type [1];
				interfaces [0] = interfaceType;
			}
		}

		[MonoTODO]
		protected override ConstructorInfo GetConstructorImpl (BindingFlags bindingAttr, Binder binder,
								       CallingConventions callConvention, Type[] types,
								       ParameterModifier[] modifiers)
		{
			if (ctors == null)
				return null;

			ConstructorBuilder found = null;
			int count = 0;
			
			foreach (ConstructorBuilder cb in ctors){
				if (callConvention != CallingConventions.Any && cb.CallingConvention != callConvention)
					continue;
				found = cb;
				count++;
			}

			if (count == 0)
				return null;
			if (types == null){
				if (count > 1)
					throw new AmbiguousMatchException ();
				return found;
			}
			MethodBase[] match = new MethodBase [count];
			if (count == 1)
				match [0] = found;
			else {
				count = 0;
				foreach (ConstructorInfo m in ctors) {
					if (callConvention != CallingConventions.Any && m.CallingConvention != callConvention)
						continue;
					match [count++] = m;
				}
			}
			if (binder == null)
				binder = Binder.DefaultBinder;
			return (ConstructorInfo)binder.SelectMethod (bindingAttr, match, types, modifiers);
		}

		public override bool IsDefined( Type attributeType, bool inherit)
		{
			/*
			 * MS throws NotSupported here, but we can't because some corlib
			 * classes make calls to IsDefined.
			 */
			return MonoCustomAttrs.IsDefined (this, attributeType, inherit);
		}
		
		public override object[] GetCustomAttributes(bool inherit)
		{
			throw not_supported ();
		}
		
		public override object[] GetCustomAttributes(Type attributeType, bool inherit)
		{
			throw not_supported ();
		}

		public TypeBuilder DefineNestedType (string name) {
			return DefineNestedType (name, TypeAttributes.NestedPrivate, pmodule.assemblyb.corlib_object_type, null);
		}

		public TypeBuilder DefineNestedType (string name, TypeAttributes attr) {
			return DefineNestedType (name, attr, pmodule.assemblyb.corlib_object_type, null);
		}

		public TypeBuilder DefineNestedType (string name, TypeAttributes attr, Type parent) {
			return DefineNestedType (name, attr, parent, null);
		}

		private TypeBuilder DefineNestedType (string name, TypeAttributes attr, Type parent, Type[] interfaces,
						      PackingSize packsize, int typesize)
		{
			check_name ("name", name);
			// Visibility must be NestedXXX
			/* This breaks mcs
			if (((attrs & TypeAttributes.VisibilityMask) == TypeAttributes.Public) ||
				((attrs & TypeAttributes.VisibilityMask) == TypeAttributes.NotPublic))
				throw new ArgumentException ("attr", "Bad type flags for nested type.");
			*/
			if (interfaces != null)
				foreach (Type iface in interfaces)
					if (iface == null)
						throw new ArgumentNullException ("interfaces");

			TypeBuilder res = new TypeBuilder (pmodule, name, attr, parent, interfaces, packsize, typesize, this);
			res.fullname = res.GetFullName ();
			pmodule.RegisterTypeName (res, res.fullname);
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

		public TypeBuilder DefineNestedType (string name, TypeAttributes attr, Type parent, Type[] interfaces) {
			return DefineNestedType (name, attr, parent, interfaces, PackingSize.Unspecified, UnspecifiedTypeSize);
		}

		public TypeBuilder DefineNestedType (string name, TypeAttributes attr, Type parent, int typesize) {
			return DefineNestedType (name, attr, parent, null, PackingSize.Unspecified, typesize);
		}

		public TypeBuilder DefineNestedType (string name, TypeAttributes attr, Type parent, PackingSize packsize) {
			return DefineNestedType (name, attr, parent, null, packsize, UnspecifiedTypeSize);
		}

		public ConstructorBuilder DefineConstructor (MethodAttributes attributes, CallingConventions callingConvention, Type[] parameterTypes) {
			return DefineConstructor (attributes, callingConvention, parameterTypes, null, null);
		}

#if NET_2_0 || BOOTSTRAP_NET_2_0
		public
#else
		internal
#endif
		ConstructorBuilder DefineConstructor (MethodAttributes attributes, CallingConventions callingConvention, Type[] parameterTypes, Type[][] requiredCustomModifiers, Type[][] optionalCustomModifiers)
		{
			if (is_created)
				throw not_after_created ();
			ConstructorBuilder cb = new ConstructorBuilder (this, attributes, callingConvention, parameterTypes, requiredCustomModifiers, optionalCustomModifiers);
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

		public ConstructorBuilder DefineDefaultConstructor (MethodAttributes attributes)
		{
			ConstructorBuilder cb = DefineConstructor (attributes, CallingConventions.Standard, new Type [0]);

			Type parent_type;

			if (parent != null)
				parent_type = parent;
			else
				parent_type = pmodule.assemblyb.corlib_object_type;

			ConstructorInfo parent_constructor =
				parent_type.GetConstructor (
					BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
					null, Type.EmptyTypes, null);

			ILGenerator ig = cb.GetILGenerator ();
			if (parent_constructor != null){
				ig.Emit (OpCodes.Ldarg_0);
				ig.Emit (OpCodes.Call, parent_constructor);
			}
			ig.Emit (OpCodes.Ret);
			return cb;
		}

		public MethodBuilder DefineMethod( string name, MethodAttributes attributes, Type returnType, Type[] parameterTypes) {
			return DefineMethod (name, attributes, CallingConventions.Standard, returnType, parameterTypes);
		}

		private void append_method (MethodBuilder mb) {
			if (methods != null) {
				if (methods.Length == num_methods) {
					MethodBuilder[] new_methods = new MethodBuilder [methods.Length * 2];
					System.Array.Copy (methods, new_methods, num_methods);
					methods = new_methods;
				}
			} else {
				methods = new MethodBuilder [1];
			}
			methods [num_methods] = mb;
			num_methods ++;
		}

		public MethodBuilder DefineMethod( string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes) {
			return DefineMethod (name, attributes, callingConvention, returnType, null, null, parameterTypes, null, null);
		}

#if NET_2_0 || BOOTSTRAP_NET_2_0
		public
#else
		internal
#endif
		MethodBuilder DefineMethod( string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers) {
			check_name ("name", name);
			if (is_created)
				throw not_after_created ();
			if (IsInterface && (
				!((attributes & MethodAttributes.Abstract) != 0) || 
				!((attributes & MethodAttributes.Virtual) != 0)))
				throw new ArgumentException ("attributes", "Interface method must be abstract and virtual.");

			if (returnType == null)
				returnType = pmodule.assemblyb.corlib_void_type;
			MethodBuilder res = new MethodBuilder (this, name, attributes, callingConvention, returnType, returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers, parameterTypes, parameterTypeRequiredCustomModifiers, parameterTypeOptionalCustomModifiers);
			append_method (res);
			return res;
		}

		public MethodBuilder DefinePInvokeMethod (string name, string dllName, string entryName, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, CallingConvention nativeCallConv, CharSet nativeCharSet) {
			return DefinePInvokeMethod (name, dllName, entryName, attributes, callingConvention, returnType, null, null, parameterTypes, null, null, nativeCallConv, nativeCharSet);
		}

#if NET_2_0 || BOOTSTRAP_NET_2_0
		public
#else
		internal
#endif
		MethodBuilder DefinePInvokeMethod (
						string name, 
						string dllName, 
						string entryName, MethodAttributes attributes, 
						CallingConventions callingConvention, 
						Type returnType, 
						Type[] returnTypeRequiredCustomModifiers, 
						Type[] returnTypeOptionalCustomModifiers, 
						Type[] parameterTypes, 
						Type[][] parameterTypeRequiredCustomModifiers, 
						Type[][] parameterTypeOptionalCustomModifiers, 
						CallingConvention nativeCallConv, 
						CharSet nativeCharSet) {
			check_name ("name", name);
			check_name ("dllName", dllName);
			check_name ("entryName", entryName);
			if ((attributes & MethodAttributes.Abstract) != 0)
				throw new ArgumentException ("attributes", "PInvoke methods must be static and native and cannot be abstract.");
			if (IsInterface)
				throw new ArgumentException ("PInvoke methods cannot exist on interfaces.");		
			if (is_created)
				throw not_after_created ();

			MethodBuilder res 
				= new MethodBuilder (
						this, 
						name, 
						attributes, 
						callingConvention,
						returnType, 
						returnTypeRequiredCustomModifiers, 
						returnTypeOptionalCustomModifiers, 
						parameterTypes, 
						parameterTypeRequiredCustomModifiers, 
						parameterTypeOptionalCustomModifiers,
						dllName, 
						entryName, 
						nativeCallConv, 
						nativeCharSet);
			append_method (res);
			return res;
		}

		public MethodBuilder DefinePInvokeMethod (string name, string dllName, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, CallingConvention nativeCallConv, CharSet nativeCharSet) {
			return DefinePInvokeMethod (name, dllName, name, attributes, callingConvention, returnType, parameterTypes,
				nativeCallConv, nativeCharSet);
		}

		public void DefineMethodOverride( MethodInfo methodInfoBody, MethodInfo methodInfoDeclaration) {
			if (methodInfoBody == null)
				throw new ArgumentNullException ("methodInfoBody");
			if (methodInfoDeclaration == null)
				throw new ArgumentNullException ("methodInfoDeclaration");
			if (is_created)
				throw not_after_created ();

			if (methodInfoBody is MethodBuilder) {
				MethodBuilder mb = (MethodBuilder)methodInfoBody;
				mb.set_override (methodInfoDeclaration);
			}
		}

		public FieldBuilder DefineField( string fieldName, Type type, FieldAttributes attributes) {
			return DefineField (fieldName, type, null, null, attributes);
		}

#if NET_2_0 || BOOTSTRAP_NET_2_0
		public
#else
		internal
#endif
	    FieldBuilder DefineField( string fieldName, Type type, Type[] requiredCustomAttributes, Type[] optionalCustomAttributes, FieldAttributes attributes) {
			check_name ("fieldName", fieldName);
			if (type == typeof (void))
				throw new ArgumentException ("type",  "Bad field type in defining field.");
			if (is_created)
				throw not_after_created ();

			FieldBuilder res = new FieldBuilder (this, fieldName, type, attributes, requiredCustomAttributes, optionalCustomAttributes);
			if (fields != null) {
				if (fields.Length == num_fields) {
					FieldBuilder[] new_fields = new FieldBuilder [fields.Length * 2];
					System.Array.Copy (fields, new_fields, num_fields);
					fields = new_fields;
				}
				fields [num_fields] = res;
				num_fields ++;
			} else {
				fields = new FieldBuilder [1];
				fields [0] = res;
				num_fields ++;
				create_internal_class (this);
			}
			return res;
		}

		public PropertyBuilder DefineProperty( string name, PropertyAttributes attributes, Type returnType, Type[] parameterTypes) {
			check_name ("name", name);
			if (parameterTypes != null)
				foreach (Type param in parameterTypes)
					if (param == null)
						throw new ArgumentNullException ("parameterTypes");
			if (is_created)
				throw not_after_created ();

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
			return DefineConstructor (MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, CallingConventions.Standard, null);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern Type create_runtime_class (TypeBuilder tb);

		private bool is_nested_in (Type t) {
			while (t != null) {
				if (t == this)
					return true;
				else
					t = t.DeclaringType;
			}
			return false;
		}
		
		public Type CreateType() {
			/* handle nesting_type */
			if (is_created)
				throw not_after_created ();

			// Fire TypeResolve events for fields whose type is an unfinished
			// value type.
			if (fields != null) {
				foreach (FieldBuilder fb in fields) {
					if (fb == null)
						continue;
					Type ft = fb.FieldType;
					if (!fb.IsStatic && (ft is TypeBuilder) && ft.IsValueType && (ft != this) && is_nested_in (ft)) {
						TypeBuilder tb = (TypeBuilder)ft;
						if (!tb.is_created) {
							AppDomain.CurrentDomain.DoTypeResolve (tb);
							if (!tb.is_created) {
								// FIXME: We should throw an exception here,
								// but mcs expects that the type is created
								// even if the exception is thrown
								//throw new TypeLoadException ("Could not load type " + tb);
							}
						}
					}
				}
			}

			if (methods != null) {
				for (int i = 0; i < num_methods; ++i)
					((MethodBuilder)(methods[i])).fixup ();
			}

			//
			// On classes, define a default constructor if not provided
			//
			if (!(IsInterface || IsValueType) && (ctors == null) && (tname != "<Module>") && 
				(GetAttributeFlagsImpl () & TypeAttributes.Abstract | TypeAttributes.Sealed) != (TypeAttributes.Abstract | TypeAttributes.Sealed))
				DefineDefaultConstructor (MethodAttributes.Public);

			if (ctors != null){
				foreach (ConstructorBuilder ctor in ctors) 
					ctor.fixup ();
			}

			created = create_runtime_class (this);
			if (created != null)
				return created;
			return this;
		}

		public override ConstructorInfo[] GetConstructors (BindingFlags bindingAttr)
		{
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

		public override Type GetElementType () { 
			throw not_supported ();
		}

		public override EventInfo GetEvent (string name, BindingFlags bindingAttr) {
			throw not_supported ();
		}

		/* Needed to keep signature compatibility with MS.NET */
		public override EventInfo[] GetEvents ()
		{
			return GetEvents (DefaultBindingFlags);
		}

		public override EventInfo[] GetEvents (BindingFlags bindingAttr) {
			// FIXME: Under MS.NET, this throws a NotImplementedException
			// But mcs calls this method. How can that be?
			return new EventInfo [0];
		}

		// This is only used from MonoGenericInst.initialize().
		internal EventInfo[] GetEvents_internal (BindingFlags bindingAttr)
		{
			if (events == null)
				return new EventInfo [0];
			ArrayList l = new ArrayList ();
			bool match;
			MethodAttributes mattrs;
			MethodInfo accessor;

			foreach (EventBuilder eb in events) {
				if (eb == null)
					continue;
				EventInfo c = get_event_info (eb);
				match = false;
				accessor = c.GetAddMethod (true);
				if (accessor == null)
					accessor = c.GetRemoveMethod (true);
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
			EventInfo[] result = new EventInfo [l.Count];
			l.CopyTo (result);
			return result;
		}

		public override FieldInfo GetField( string name, BindingFlags bindingAttr) {
			if (fields == null)
				return null;

			bool match;
			FieldAttributes mattrs;
			
			foreach (FieldInfo c in fields) {
				if (c == null)
					continue;
				if (c.Name != name)
					continue;
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
				return c;
			}
			return null;
		}

		public override FieldInfo[] GetFields (BindingFlags bindingAttr) {
			if (fields == null)
				return new FieldInfo [0];
			ArrayList l = new ArrayList ();
			bool match;
			FieldAttributes mattrs;
			
			foreach (FieldInfo c in fields) {
				if (c == null)
					continue;
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
			throw not_supported ();
		}
		
		public override Type[] GetInterfaces () {
			if (interfaces != null) {
				Type[] ret = new Type [interfaces.Length];
				interfaces.CopyTo (ret, 0);
				return ret;
			} else {
				return Type.EmptyTypes;
			}
		}

		public override MemberInfo[] GetMember (string name, MemberTypes type,
												BindingFlags bindingAttr) {
			throw not_supported ();
		}

		public override MemberInfo[] GetMembers (BindingFlags bindingAttr) {
			throw not_supported ();
		}

		private MethodInfo[] GetMethodsByName (string name, BindingFlags bindingAttr, bool ignoreCase, Type reflected_type) {
			MethodInfo[] candidates;
			if (((bindingAttr & BindingFlags.DeclaredOnly) == 0) && (parent != null)) {
				MethodInfo[] parent_methods = parent.GetMethods (bindingAttr);
				if (methods == null)
					candidates = parent_methods;
				else {
					candidates = new MethodInfo [methods.Length + parent_methods.Length];
					parent_methods.CopyTo (candidates, 0);
					methods.CopyTo (candidates, parent_methods.Length);
				}
			}
			else
				candidates = methods;
					
			if (candidates == null)
				return new MethodInfo [0];

			ArrayList l = new ArrayList ();
			bool match;
			MethodAttributes mattrs;

			foreach (MethodInfo c in candidates) {
				if (c == null)
					continue;
				if (name != null) {
					if (String.Compare (c.Name, name, ignoreCase) != 0)
						continue;
				}
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

		public override MethodInfo[] GetMethods (BindingFlags bindingAttr) {
			return GetMethodsByName (null, bindingAttr, false, this);
		}

		protected override MethodInfo GetMethodImpl (string name, BindingFlags bindingAttr,
							     Binder binder,
							     CallingConventions callConvention,
							     Type[] types, ParameterModifier[] modifiers)
		{
			if (!is_created)
				/* MS.Net throws this exception if the type is unfinished... */
				throw not_supported ();

			bool ignoreCase = ((bindingAttr & BindingFlags.IgnoreCase) != 0);
			MethodInfo[] methods = GetMethodsByName (name, bindingAttr, ignoreCase, this);
			MethodInfo found = null;
			MethodBase[] match;
			int typesLen = (types != null) ? types.Length : 0;
			int count = 0;
			
			foreach (MethodInfo m in methods) {
				// Under MS.NET, Standard|HasThis matches Standard...
				if (callConvention != CallingConventions.Any && ((m.CallingConvention & callConvention) != callConvention))
					continue;
				found = m;
				count++;
			}

			if (count == 0)
				return null;
			
			if (count == 1 && typesLen == 0) 
				return found;

			match = new MethodBase [count];
			if (count == 1)
				match [0] = found;
			else {
				count = 0;
				foreach (MethodInfo m in methods) {
					if (callConvention != CallingConventions.Any && ((m.CallingConvention & callConvention) != callConvention))
						continue;
					match [count++] = m;
				}
			}
			
			if (types == null) 
				return (MethodInfo) Binder.FindMostDerivedMatch (match);

			if (binder == null)
				binder = Binder.DefaultBinder;
			
			return (MethodInfo)binder.SelectMethod (bindingAttr, match, types, modifiers);
		}

		public override Type GetNestedType( string name, BindingFlags bindingAttr) {
			throw not_supported ();
		}

		public override Type[] GetNestedTypes (BindingFlags bindingAttr) {
			bool match;
			ArrayList result = new ArrayList ();

			if (subtypes == null)
				return Type.EmptyTypes;
			foreach (TypeBuilder t in subtypes) {
				match = false;
				if ((t.attrs & TypeAttributes.VisibilityMask) == TypeAttributes.NestedPublic) {
					if ((bindingAttr & BindingFlags.Public) != 0)
						match = true;
				} else {
					if ((bindingAttr & BindingFlags.NonPublic) != 0)
						match = true;
				}
				if (!match)
					continue;
				result.Add (t);
			}
			Type[] r = new Type [result.Count];
			result.CopyTo (r);
			return r;
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
			throw not_supported ();
		}

		protected override bool HasElementTypeImpl () {
			// According to the MSDN docs, this is supported for TypeBuilders,
			// but in reality, it is not
			throw not_supported ();
			//			return IsArrayImpl() || IsByRefImpl() || IsPointerImpl ();
		}

		public override object InvokeMember( string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters) {
			throw not_supported ();
		}

		protected override bool IsArrayImpl ()
		{
			return Type.IsArrayImpl (this);
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
			return ((type_is_subtype_of (this, pmodule.assemblyb.corlib_value_type, false) || type_is_subtype_of (this, typeof(System.ValueType), false)) &&
				this != pmodule.assemblyb.corlib_value_type &&
				this != pmodule.assemblyb.corlib_enum_type);
		}
		
		public override RuntimeTypeHandle TypeHandle { 
			get { 
				throw not_supported (); 
			} 
		}

		public void SetCustomAttribute( CustomAttributeBuilder customBuilder) {
			if (customBuilder == null)
				throw new ArgumentNullException ("customBuilder");

			string attrname = customBuilder.Ctor.ReflectedType.FullName;
			if (attrname == "System.Runtime.InteropServices.StructLayoutAttribute") {
				byte[] data = customBuilder.Data;
				int layout_kind; /* the (stupid) ctor takes a short or an int ... */
				layout_kind = (int)data [2];
				layout_kind |= ((int)data [3]) << 8;
				attrs &= ~TypeAttributes.LayoutMask;
				switch ((LayoutKind)layout_kind) {
				case LayoutKind.Auto:
					attrs |= TypeAttributes.AutoLayout;
					break;
				case LayoutKind.Explicit:
					attrs |= TypeAttributes.ExplicitLayout;
					break;
				case LayoutKind.Sequential:
					attrs |= TypeAttributes.SequentialLayout;
					break;
				default:
					// we should ignore it since it can be any value anyway...
					throw new Exception ("Error in customattr");
				}
				string first_type_name = customBuilder.Ctor.GetParameters()[0].ParameterType.FullName;
				int pos = 6;
				if (first_type_name == "System.Int16")
					pos = 4;
				int nnamed = (int)data [pos++];
				nnamed |= ((int)data [pos++]) << 8;
				for (int i = 0; i < nnamed; ++i) {
					byte named_type = data [pos++];
					byte type = data [pos++];
					int len;
					string named_name;

					if (type == 0x55) {
						len = CustomAttributeBuilder.decode_len (data, pos, out pos);
						string named_typename = CustomAttributeBuilder.string_from_bytes (data, pos, len);
						pos += len;
						// FIXME: Check that 'named_type' and 'named_typename' match, etc.
						//        See related code/FIXME in mono/mono/metadata/reflection.c
					}

					len = CustomAttributeBuilder.decode_len (data, pos, out pos);
					named_name = CustomAttributeBuilder.string_from_bytes (data, pos, len);
					pos += len;
					/* all the fields are integers in StructLayout */
					int value = (int)data [pos++];
					value |= ((int)data [pos++]) << 8;
					value |= ((int)data [pos++]) << 16;
					value |= ((int)data [pos++]) << 24;
					switch (named_name) {
					case "CharSet":
						switch ((CharSet)value) {
						case CharSet.None:
						case CharSet.Ansi:
							break;
						case CharSet.Unicode:
							attrs |= TypeAttributes.UnicodeClass;
							break;
						case CharSet.Auto:
							attrs |= TypeAttributes.AutoClass;
							break;
						default:
							break; // error out...
						}
						break;
					case "Pack":
						packing_size = (PackingSize)value;
						break;
					case "Size":
						class_size = value;
						break;
					default:
						break; // error out...
					}
				}
				return;
			} else if (attrname == "System.SerializableAttribute") {
				attrs |= TypeAttributes.Serializable;
				return;
			}
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
			check_name ("name", name);
			if (eventtype == null)
				throw new ArgumentNullException ("eventtype");
			if (is_created)
				throw not_after_created ();

			EventBuilder res = new EventBuilder (this, name, attributes, eventtype);
			if (events != null) {
				EventBuilder[] new_events = new EventBuilder [events.Length+1];
				System.Array.Copy (events, new_events, events.Length);
				new_events [events.Length] = res;
				events = new_events;
			} else {
				events = new EventBuilder [1];
				events [0] = res;
			}
			return res;
		}

		public FieldBuilder DefineInitializedData( string name, byte[] data, FieldAttributes attributes) {
			if (data == null)
				throw new ArgumentNullException ("data");
			if ((data.Length == 0) || (data.Length > 0x3f0000))
				throw new ArgumentException ("data", "Data size must be > 0 and < 0x3f0000");

			FieldBuilder res = DefineUninitializedData (name, data.Length, attributes);
			res.SetRVAData (data);

			return res;
		}

		static int UnmanagedDataCount = 0;
		
		public FieldBuilder DefineUninitializedData( string name, int size, FieldAttributes attributes) {
			check_name ("name", name);
			if ((size <= 0) || (size > 0x3f0000))
				throw new ArgumentException ("size", "Data size must be > 0 and < 0x3f0000");
			if (is_created)
				throw not_after_created ();

			string s = "$ArrayType$"+UnmanagedDataCount.ToString();
			UnmanagedDataCount++;
			TypeBuilder datablobtype = DefineNestedType (s,
				TypeAttributes.NestedPrivate|TypeAttributes.ExplicitLayout|TypeAttributes.Sealed,
				pmodule.assemblyb.corlib_value_type, null, PackingSize.Size1, size);
			datablobtype.CreateType ();
			return DefineField (name, datablobtype, attributes|FieldAttributes.Static|FieldAttributes.HasFieldRVA);
		}

		public TypeToken TypeToken {
			get {
				return new TypeToken (0x02000000 | table_idx);
			}
		}
		public void SetParent (Type parentType) {
			if (parentType == null)
				throw new ArgumentNullException ("parentType");
			if (is_created)
				throw not_after_created ();

			parent = parentType;
			// will just set the parent-related bits if called a second time
			setup_internal_class (this);
		}
		internal int get_next_table_index (object obj, int table, bool inc) {
			return pmodule.get_next_table_index (obj, table, inc);
		}

		public override InterfaceMapping GetInterfaceMap (Type interfaceType)
		{
			if (created == null)
				throw new NotSupportedException ("This method is not implemented for incomplete types.");

			return created.GetInterfaceMap (interfaceType);
		}

		internal bool is_created {
			get {
				return created != null;
			}
		}

		private Exception not_supported ()
		{
			return new NotSupportedException ("The invoked member is not supported in a dynamic module.");
		}

		private Exception not_after_created ()
		{
			return new InvalidOperationException ("Unable to change after type has been created.");
		}

		private void check_name (string argName, string name)
		{
			if (name == null)
				throw new ArgumentNullException (argName);
			if (name == "")
				throw new ArgumentException (argName, "Empty name is not legal.");
			if (name.IndexOf ((char)0) != -1)
				throw new ArgumentException (argName, "Illegal name.");
		}

		public override String ToString ()
		{
			return FullName;
		}

		[MonoTODO]
		public override bool IsAssignableFrom (Type c)
		{
			return base.IsAssignableFrom (c);
		}

		[MonoTODO]
		public override bool IsSubclassOf (Type c)
		{
			return base.IsSubclassOf (c);
		}

#if NET_2_0 || BOOTSTRAP_NET_2_0
		public bool IsCreated () {
			return is_created;
		}

		public override Type[] GetGenericArguments ()
		{
			if (generic_params != null)
				return generic_params;

			throw new InvalidOperationException ();
		}

		public override Type GetGenericTypeDefinition ()
		{
			setup_generic_class (this);

			return base.GetGenericTypeDefinition ();
		}

		public override bool HasGenericArguments {
			get {
				throw new NotImplementedException ();
			}
		}

		public override bool ContainsGenericParameters {
			get {
				return generic_params != null;
			}
		}

		public extern override bool IsGenericParameter {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}

		public override int GenericParameterPosition {
			get {
				throw new NotImplementedException ();
			}
		}

		public override MethodInfo DeclaringMethod {
			get {
				throw new NotImplementedException ();
			}
		}

		public GenericTypeParameterBuilder[] DefineGenericParameters (string[] names)
		{
			generic_params = new GenericTypeParameterBuilder [names.Length];
			for (int i = 0; i < names.Length; i++)
				generic_params [i] = new GenericTypeParameterBuilder (
					this, null, names [i], i);

			return generic_params;
		}

		public MethodBuilder DefineGenericMethod (string name, MethodAttributes attributes)
		{
			return DefineMethod (name, attributes, CallingConventions.Standard, null, null);
		}
#endif
	}
}
