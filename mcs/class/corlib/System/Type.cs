//
// System.Type.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
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

using System.Diagnostics;
using System.Reflection;
using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Globalization;

namespace System {

	[Serializable]
	[ClassInterface (ClassInterfaceType.AutoDual)]
	public abstract class Type : MemberInfo, IReflect {
		
		internal RuntimeTypeHandle _impl;

		public static readonly char Delimiter = '.';
		public static readonly Type[] EmptyTypes = {};
		public static readonly MemberFilter FilterAttribute = new MemberFilter (FilterAttribute_impl);
		public static readonly MemberFilter FilterName = new MemberFilter (FilterName_impl);
		public static readonly MemberFilter FilterNameIgnoreCase = new MemberFilter (FilterNameIgnoreCase_impl);
		public static readonly object Missing;

		protected const BindingFlags DefaultBindingFlags =
		BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;

		/* implementation of the delegates for MemberFilter */
		static bool FilterName_impl (MemberInfo m, object filterCriteria)
		{
			string name = (string) filterCriteria;
			return name.Equals (m.Name);
		}

		static bool FilterNameIgnoreCase_impl (MemberInfo m, object filterCriteria)
		{
			string name = (string) filterCriteria;
			if (name.Length != m.Name.Length)
				return false;

			return String.Compare (name, m.Name, true, CultureInfo.InvariantCulture) == 0;
		}

		static bool FilterAttribute_impl (MemberInfo m, object filterCriteria)
		{
			int flags = ((IConvertible)filterCriteria).ToInt32 (null);
			if (m is MethodInfo)
				return ((int)((MethodInfo)m).Attributes & flags) != 0;
			if (m is FieldInfo)
				return ((int)((FieldInfo)m).Attributes & flags) != 0;
			if (m is PropertyInfo)
				return ((int)((PropertyInfo)m).Attributes & flags) != 0;
			if (m is EventInfo)
				return ((int)((EventInfo)m).Attributes & flags) != 0;
			return false;
		}

		protected Type ()
		{
		}

		/// <summary>
		///   The assembly where the type is defined.
		/// </summary>
		public abstract Assembly Assembly {
			get;
		}

		/// <summary>
		///   Gets the fully qualified name for the type including the
		///   assembly name where the type is defined.
		/// </summary>
		public abstract string AssemblyQualifiedName {
			get;
		}

		/// <summary>
		///   Returns the Attributes associated with the type.
		/// </summary>
		public TypeAttributes Attributes {
			get {
				return GetAttributeFlagsImpl ();
			}
		}

		/// <summary>
		///   Returns the basetype for this type
		/// </summary>
		public abstract Type BaseType {
			get;
		}

		/// <summary>
		///   Returns the class that declares the member.
		/// </summary>
		public override Type DeclaringType {
			get {
				return null;
			}
		}

		/// <summary>
		///
		/// </summary>
		public static Binder DefaultBinder {
			get {
				return Binder.DefaultBinder;
			}
		}

		/// <summary>
		///    The full name of the type including its namespace
		/// </summary>
		public abstract string FullName {
			get;
		}

		public abstract Guid GUID {
			get;
		}

		public bool HasElementType {
			get {
				return HasElementTypeImpl ();
			}
		}

		public bool IsAbstract {
			get {
				return (Attributes & TypeAttributes.Abstract) != 0;
			}
		}

		public bool IsAnsiClass {
			get {
				return (Attributes & TypeAttributes.StringFormatMask)
				== TypeAttributes.AnsiClass;
			}
		}

		public bool IsArray {
			get {
				return IsArrayImpl ();
			}
		}

		public bool IsAutoClass {
			get {
				return (Attributes & TypeAttributes.StringFormatMask) == TypeAttributes.AutoClass;
			}
		}

		public bool IsAutoLayout {
			get {
				return (Attributes & TypeAttributes.LayoutMask) == TypeAttributes.AutoLayout;
			}
		}

		public bool IsByRef {
			get {
				return IsByRefImpl ();
			}
		}

		public bool IsClass {
			get {
				//
				// This code used to probe for "this == typeof (System.Enum)", but in
				// The .NET Framework 1.0, the above test return false
				//
				if (this == typeof (System.ValueType))
					return true;
				if (IsInterface)
					return false;
				return !type_is_subtype_of (this, typeof (System.ValueType), false);
			}
		}

		public bool IsCOMObject {
			get {
				return IsCOMObjectImpl ();
			}
		}

		public bool IsContextful {
			get {
				return IsContextfulImpl ();
			}
		}

		public bool IsEnum {
			get {
				return type_is_subtype_of (this, typeof (System.Enum), false) &&
					this != typeof (System.Enum);
			}
		}

		public bool IsExplicitLayout {
			get {
				return (Attributes & TypeAttributes.LayoutMask) == TypeAttributes.ExplicitLayout;
			}
		}

		public bool IsImport {
			get {
				return (Attributes & TypeAttributes.Import) != 0;
			}
		}

		public bool IsInterface {
			get {
				return (Attributes & TypeAttributes.ClassSemanticsMask) == TypeAttributes.Interface;
			}
		}

		public bool IsLayoutSequential {
			get {
				return (Attributes & TypeAttributes.LayoutMask) == TypeAttributes.SequentialLayout;
			}
		}

		public bool IsMarshalByRef {
			get {
				return IsMarshalByRefImpl ();
			}
		}

		public bool IsNestedAssembly {
			get {
				return (Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.NestedAssembly;
			}
		}

		public bool IsNestedFamANDAssem {
			get {
				return (Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.NestedFamANDAssem;
			}
		}

		public bool IsNestedFamily {
			get {
				return (Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.NestedFamily;
			}
		}

		public bool IsNestedFamORAssem {
			get {
				return (Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.NestedFamORAssem;
			}
		}

		public bool IsNestedPrivate {
			get {
				return (Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.NestedPrivate;
			}
		}

		public bool IsNestedPublic {
			get {
				return (Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.NestedPublic;
			}
		}

		public bool IsNotPublic {
			get {
				return (Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.NotPublic;
			}
		}

		public bool IsPointer {
			get {
				return IsPointerImpl ();
			}
		}

		public bool IsPrimitive {
			get {
				return IsPrimitiveImpl ();
			}
		}

		public bool IsPublic {
			get {
				return (Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.Public;
			}
		}

		public bool IsSealed {
			get {
				return (Attributes & TypeAttributes.Sealed) != 0;
			}
		}

		public bool IsSerializable {
			get {
				// Enums and delegates are always serializable
				return (Attributes & TypeAttributes.Serializable) != 0 || IsEnum || 
					type_is_subtype_of (this, typeof (System.Delegate), false);
			}
		}

		public bool IsSpecialName {
			get {
				return (Attributes & TypeAttributes.SpecialName) != 0;
			}
		}

		public bool IsUnicodeClass {
			get {
				return (Attributes & TypeAttributes.StringFormatMask) == TypeAttributes.UnicodeClass;
			}
		}

		public bool IsValueType {
			get {
				return IsValueTypeImpl ();
			}
		}

		public override MemberTypes MemberType {
			get {return MemberTypes.TypeInfo;}
		}

#if NET_2_0 || BOOTSTRAP_NET_2_0
		override
#endif
		public abstract Module Module {get;}
	
		public abstract string Namespace {get;}

		public override Type ReflectedType {
			get {
				return null;
			}
		}

		public abstract RuntimeTypeHandle TypeHandle {get;}

		public ConstructorInfo TypeInitializer {
			get {
				return GetConstructorImpl (
					BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
					null,
					CallingConventions.Any,
					EmptyTypes,
					null);
			}
		}

		public abstract Type UnderlyingSystemType {get;}

		public override bool Equals (object o)
		{
			if (o == null)
				return false;
			
			// TODO: return UnderlyingSystemType == o.UnderlyingSystemType;
			Type cmp = o as Type;
			if (cmp == null)
				return false;
			return Equals (cmp);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern bool Equals (Type type);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern Type internal_from_handle (IntPtr handle);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern Type internal_from_name (string name, bool throwOnError, bool ignoreCase);

		public static Type GetType(string typeName)
		{
			if (typeName == null)
				throw new ArgumentNullException ("typeName");

			return internal_from_name (typeName, false, false);
		}

		public static Type GetType(string typeName, bool throwOnError)
		{
			if (typeName == null)
				throw new ArgumentNullException ("typeName");

			Type type = internal_from_name (typeName, throwOnError, false);
			if (throwOnError && type == null)
				throw new TypeLoadException ("Error loading '" + typeName + "'");

			return type;
		}

		public static Type GetType(string typeName, bool throwOnError, bool ignoreCase)
		{
			if (typeName == null)
				throw new ArgumentNullException ("typeName");

			Type t = internal_from_name (typeName, throwOnError, ignoreCase);
			if (throwOnError && t == null)
				throw new TypeLoadException ("Error loading '" + typeName + "'");

			return t;
		}

		public static Type[] GetTypeArray (object[] args) {
			if (args == null)
				throw new ArgumentNullException ("args");

			Type[] ret;
			ret = new Type [args.Length];
			for (int i = 0; i < args.Length; ++i)
				ret [i] = args[i].GetType ();
			return ret;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static TypeCode GetTypeCode (Type type);

		[MonoTODO]
		public static Type GetTypeFromCLSID (Guid clsid)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static Type GetTypeFromCLSID (Guid clsid, bool throwOnError)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static Type GetTypeFromCLSID (Guid clsid, string server)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static Type GetTypeFromCLSID (Guid clsid, string server, bool throwOnError)
		{
			throw new NotImplementedException ();
		}

		public static Type GetTypeFromHandle (RuntimeTypeHandle handle)
		{ 
			return internal_from_handle (handle.Value);
		}

		[MonoTODO]
		public static Type GetTypeFromProgID (string progID)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static Type GetTypeFromProgID (string progID, bool throwOnError)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static Type GetTypeFromProgID (string progID, string server)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static Type GetTypeFromProgID (string progID, string server, bool throwOnError)
		{
			throw new NotImplementedException ();
		}

		public static RuntimeTypeHandle GetTypeHandle (object o)
		{
			return o.GetType().TypeHandle;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern bool type_is_subtype_of (Type a, Type b, bool check_interfaces);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern bool type_is_assignable_from (Type a, Type b);
		
		public virtual bool IsSubclassOf (Type c)
		{
			if (c == null)
				return false;

			return (this != c) && type_is_subtype_of (this, c, false);
		}

		public virtual Type[] FindInterfaces (TypeFilter filter, object filterCriteria)
		{
			if (filter == null)
				throw new ArgumentNullException ("filter");

			ArrayList ifaces = new ArrayList ();
			foreach (Type iface in GetInterfaces ()) {
				if (filter (iface, filterCriteria))
					ifaces.Add (iface);
			}

			return (Type []) ifaces.ToArray (typeof (Type));
		}
		
		public Type GetInterface (string name) {
			return GetInterface (name, false);
		}

		public abstract Type GetInterface (string name, bool ignoreCase);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern void GetInterfaceMapData (Type t, Type iface, out MethodInfo[] targets, out MethodInfo[] methods);
		
		public virtual InterfaceMapping GetInterfaceMap (Type interfaceType) {
			InterfaceMapping res;
			if (interfaceType == null)
				throw new ArgumentNullException ("interfaceType");
			if (!interfaceType.IsInterface)
				throw new ArgumentException (Locale.GetText ("Argument must be an interface."), "interfaceType");
			res.TargetType = this;
			res.InterfaceType = interfaceType;
			GetInterfaceMapData (this, interfaceType, out res.TargetMethods, out res.InterfaceMethods);
			if (res.TargetMethods == null)
				throw new ArgumentException (Locale.GetText ("Interface not found"), "interfaceType");

			return res;
		}

		public abstract Type[] GetInterfaces ();

		public virtual bool IsAssignableFrom (Type c)
		{
			if (c == null)
				return false;

			if (Equals (c))
				return true;

			return type_is_assignable_from (this, c);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern virtual bool IsInstanceOfType (object o);

		public virtual int GetArrayRank ()
		{
			throw new NotSupportedException ();	// according to MSDN
		}

		public abstract Type GetElementType ();

		public EventInfo GetEvent (string name)
		{
			return GetEvent (name, DefaultBindingFlags);
		}

		public abstract EventInfo GetEvent (string name, BindingFlags bindingAttr);

		public virtual EventInfo[] GetEvents ()
		{
			return GetEvents (DefaultBindingFlags);
		}

		public abstract EventInfo[] GetEvents (BindingFlags bindingAttr);

		public FieldInfo GetField( string name)
		{
			return GetField (name, DefaultBindingFlags);
		}

		public abstract FieldInfo GetField( string name, BindingFlags bindingAttr);

		public FieldInfo[] GetFields ()
		{
			return GetFields (DefaultBindingFlags);
		}

		public abstract FieldInfo[] GetFields (BindingFlags bindingAttr);
		
		public override int GetHashCode()
		{
			return (int)_impl.Value;
		}

		public MemberInfo[] GetMember (string name)
		{
			return GetMember (name, DefaultBindingFlags);
		}
		
		public virtual MemberInfo[] GetMember (string name, BindingFlags bindingAttr)
		{
			return GetMember (name, MemberTypes.All, bindingAttr);
		}

		public virtual MemberInfo[] GetMember (string name, MemberTypes type, BindingFlags bindingAttr)
		{
			if ((bindingAttr & BindingFlags.IgnoreCase) != 0)
				return FindMembers (type, bindingAttr, FilterNameIgnoreCase, name);
			else
				return FindMembers (type, bindingAttr, FilterName, name);
		}

		public MemberInfo[] GetMembers ()
		{
			return GetMembers (DefaultBindingFlags);
		}

		public abstract MemberInfo[] GetMembers (BindingFlags bindingAttr);

		public MethodInfo GetMethod (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			return GetMethodImpl (name, DefaultBindingFlags, null, CallingConventions.Any, null, null);
		}

		public MethodInfo GetMethod (string name, BindingFlags bindingAttr)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			
			return GetMethodImpl (name, bindingAttr, null, CallingConventions.Any, null, null);
		}
		
		public MethodInfo GetMethod (string name, Type[] types)
		{
			return GetMethod (name, DefaultBindingFlags, null, CallingConventions.Any, types, null);
		}

		public MethodInfo GetMethod (string name, Type[] types, ParameterModifier[] modifiers)
		{
			return GetMethod (name, DefaultBindingFlags, null, CallingConventions.Any, types, modifiers);
		}

		public MethodInfo GetMethod (string name, BindingFlags bindingAttr, Binder binder,
		                             Type[] types, ParameterModifier[] modifiers)
		{
			
			return GetMethod (name, bindingAttr, binder, CallingConventions.Any, types, modifiers);
		}

		public MethodInfo GetMethod (string name, BindingFlags bindingAttr, Binder binder,
		                             CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (types == null)
				throw new ArgumentNullException ("types");

			for (int i = 0; i < types.Length; i++) 
				if (types[i] == null)
					throw new ArgumentNullException ("types");

			return GetMethodImpl (name, bindingAttr, binder, callConvention, types, modifiers);
		}

		protected abstract MethodInfo GetMethodImpl (string name, BindingFlags bindingAttr, Binder binder,
		                                             CallingConventions callConvention, Type[] types,
		                                             ParameterModifier[] modifiers);

		public MethodInfo[] GetMethods ()
		{
			return GetMethods (DefaultBindingFlags);
		}

		public abstract MethodInfo[] GetMethods (BindingFlags bindingAttr);

		public Type GetNestedType (string name)
		{
			return GetNestedType (name, DefaultBindingFlags);
		}

		public abstract Type GetNestedType (string name, BindingFlags bindingAttr);

		public Type[] GetNestedTypes ()
		{
			return GetNestedTypes (DefaultBindingFlags);
		}

		public abstract Type[] GetNestedTypes (BindingFlags bindingAttr);


		public PropertyInfo[] GetProperties ()
		{
			return GetProperties (DefaultBindingFlags);
		}

		public abstract PropertyInfo[] GetProperties (BindingFlags bindingAttr);


		public PropertyInfo GetProperty (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			return GetPropertyImpl (name, DefaultBindingFlags, null, null, new Type[0], null);
		}

		public PropertyInfo GetProperty (string name, BindingFlags bindingAttr)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			return GetPropertyImpl (name, bindingAttr, null, null, new Type[0], null);
		}

		public PropertyInfo GetProperty (string name, Type returnType)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			return GetPropertyImpl (name, DefaultBindingFlags, null, returnType, new Type[0], null);
		}

		public PropertyInfo GetProperty (string name, Type[] types)
		{
			return GetProperty (name, DefaultBindingFlags, null, null, types, null);
		}

		public PropertyInfo GetProperty (string name, Type returnType, Type[] types)
		{
			return GetProperty (name, DefaultBindingFlags, null, returnType, types, null);
		}

		public PropertyInfo GetProperty( string name, Type returnType, Type[] types, ParameterModifier[] modifiers)
		{
			return GetProperty (name, DefaultBindingFlags, null, returnType, types, modifiers);
		}

		public PropertyInfo GetProperty (string name, BindingFlags bindingAttr, Binder binder, Type returnType,
		                                 Type[] types, ParameterModifier[] modifiers)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (types == null)
				throw new ArgumentNullException ("types");

			return GetPropertyImpl (name, bindingAttr, binder, returnType, types, modifiers);
		}

		protected abstract PropertyInfo GetPropertyImpl (string name, BindingFlags bindingAttr, Binder binder,
		                                                 Type returnType, Type[] types, ParameterModifier[] modifiers);

		protected abstract ConstructorInfo GetConstructorImpl (BindingFlags bindingAttr,
								       Binder binder,
								       CallingConventions callConvention,
								       Type[] types,
								       ParameterModifier[] modifiers);

		protected abstract TypeAttributes GetAttributeFlagsImpl ();
		protected abstract bool HasElementTypeImpl ();
		protected abstract bool IsArrayImpl ();
		protected abstract bool IsByRefImpl ();
		protected abstract bool IsCOMObjectImpl ();
		protected abstract bool IsPointerImpl ();
		protected abstract bool IsPrimitiveImpl ();
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern bool IsArrayImpl (Type type);

		protected virtual bool IsValueTypeImpl ()
		{
			if (this == typeof (Enum) || this == typeof (ValueType))
				return false;

			return IsSubclassOf (typeof (ValueType));
		}
		
		protected virtual bool IsContextfulImpl ()
		{
			return typeof (ContextBoundObject).IsAssignableFrom (this);
		}

		protected virtual bool IsMarshalByRefImpl ()
		{
			return typeof (MarshalByRefObject).IsAssignableFrom (this);
		}

		public ConstructorInfo GetConstructor (Type[] types)
		{
			return GetConstructorImpl (
				DefaultBindingFlags, null, CallingConventions.Any, types, null);
		}

		public ConstructorInfo GetConstructor (BindingFlags bindingAttr, Binder binder,
						       Type[] types, ParameterModifier[] modifiers)
		{
			return GetConstructorImpl (
				bindingAttr, binder, CallingConventions.Any, types, modifiers);
		}

		public ConstructorInfo GetConstructor (BindingFlags bindingAttr, Binder binder,
						       CallingConventions callConvention,
						       Type[] types, ParameterModifier[] modifiers)
		{
			if (types == null)
				throw new ArgumentNullException ("types");

			return GetConstructorImpl (bindingAttr, binder, callConvention, types, modifiers);
		}

		public ConstructorInfo[] GetConstructors ()
		{
			return GetConstructors (BindingFlags.Public | BindingFlags.Instance);
		}
		
		public abstract ConstructorInfo[] GetConstructors (BindingFlags bindingAttr);

		public virtual MemberInfo[] GetDefaultMembers ()
		{
			object [] att = GetCustomAttributes (typeof (DefaultMemberAttribute), true);
			if (att.Length == 0)
				return new MemberInfo [0];

			MemberInfo [] member = GetMember (((DefaultMemberAttribute) att [0]).MemberName);
			return (member != null) ? member : new MemberInfo [0];
		}

		public virtual MemberInfo[] FindMembers (MemberTypes memberType, BindingFlags bindingAttr,
							 MemberFilter filter, object filterCriteria)
		{
			MemberInfo[] result;
			ArrayList l = new ArrayList ();

			// Console.WriteLine ("FindMembers for {0} (Type: {1}): {2}",
			// this.FullName, this.GetType().FullName, this.obj_address());

			if ((memberType & MemberTypes.Constructor) != 0) {
				ConstructorInfo[] c = GetConstructors (bindingAttr);
				if (filter != null) {
					foreach (MemberInfo m in c) {
						if (filter (m, filterCriteria))
							l.Add (m);
					}
				} else {
					l.AddRange (c);
				}
			}
			if ((memberType & MemberTypes.Event) != 0) {
				EventInfo[] c = GetEvents (bindingAttr);
				if (filter != null) {
					foreach (MemberInfo m in c) {
						if (filter (m, filterCriteria))
							l.Add (m);
					}
				} else {
					l.AddRange (c);
				}
			}
			if ((memberType & MemberTypes.Field) != 0) {
				FieldInfo[] c = GetFields (bindingAttr);
				if (filter != null) {
					foreach (MemberInfo m in c) {
						if (filter (m, filterCriteria))
							l.Add (m);
					}
				} else {
					l.AddRange (c);
				}
			}
			if ((memberType & MemberTypes.Method) != 0) {
				MethodInfo[] c = GetMethods (bindingAttr);
				if (filter != null) {
					foreach (MemberInfo m in c) {
						if (filter (m, filterCriteria))
							l.Add (m);
					}
				} else {
					l.AddRange (c);
				}
			}
			if ((memberType & MemberTypes.Property) != 0) {
				PropertyInfo[] c;
				int count = l.Count;
				Type ptype;
				if (filter != null) {
					ptype = this;
					while ((l.Count == count) && (ptype != null)) {
						c = ptype.GetProperties (bindingAttr);
						foreach (MemberInfo m in c) {
							if (filter (m, filterCriteria))
								l.Add (m);
						}
						ptype = ptype.BaseType;
					}
				} else {
					c = GetProperties (bindingAttr);
					l.AddRange (c);
				}
			}
			if ((memberType & MemberTypes.NestedType) != 0) {
				Type[] c = GetNestedTypes (bindingAttr);
				if (filter != null) {
					foreach (MemberInfo m in c) {
						if (filter (m, filterCriteria)) {
							l.Add (m);
						}
					}
				} else {
					l.AddRange (c);
				}
			}
			result = new MemberInfo [l.Count];
			l.CopyTo (result);
			return result;
		}

		[DebuggerHidden]
		[DebuggerStepThrough] 
		public object InvokeMember (string name, BindingFlags invokeAttr, Binder binder, object target, object[] args)
		{
			return InvokeMember (name, invokeAttr, binder, target, args, null, null, null);
		}

		[DebuggerHidden]
		[DebuggerStepThrough] 
		public object InvokeMember (string name, BindingFlags invokeAttr, Binder binder,
					    object target, object[] args, CultureInfo culture)
		{
			return InvokeMember (name, invokeAttr, binder, target, args, null, culture, null);
		}

		public abstract object InvokeMember (string name, BindingFlags invokeAttr,
						     Binder binder, object target, object[] args,
						     ParameterModifier[] modifiers,
						     CultureInfo culture, string[] namedParameters);

		public override string ToString()
		{
			return FullName;
		}

#if NET_2_0 || BOOTSTRAP_NET_2_0
		public abstract Type[] GetGenericArguments ();

		public abstract bool HasGenericArguments {
			get;
		}

		public abstract bool ContainsGenericParameters {
			get;
		}

		public extern bool IsGenericTypeDefinition {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern Type GetGenericTypeDefinition_impl ();

		public virtual Type GetGenericTypeDefinition ()
		{
			Type res = GetGenericTypeDefinition_impl ();
			if (res == null)
				throw new InvalidOperationException ();

			return res;
		}

		public extern bool IsGenericInstance {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern Type BindGenericParameters (Type gt, Type [] types);
		
		public Type BindGenericParameters (Type [] types)
		{
			if (types == null)
				throw new ArgumentNullException ("types");
			foreach (Type t in types) {
				if (t == null)
					throw new ArgumentNullException ("types");
			}
			Type res = BindGenericParameters (this, types);
			if (res == null)
				throw new TypeLoadException ();
			return res;
		}

		public abstract bool IsGenericParameter {
			get;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern int GetGenericParameterPosition ();
		
		public virtual int GenericParameterPosition {
			get {
				int res = GetGenericParameterPosition ();
				if (res < 0)
					throw new InvalidOperationException ();
				return res;
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern GenericParameterAttributes GetGenericParameterAttributes ();

		public virtual GenericParameterAttributes GenericParameterAttributes {
			get {
				if (!IsGenericParameter)
					throw new InvalidOperationException ();

				return GetGenericParameterAttributes ();
			}
		}

		public abstract MethodInfo DeclaringMethod {
			get;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern Type make_array_type (int rank);

		public virtual Type MakeArrayType ()
		{
			return MakeArrayType (1);
		}

		public virtual Type MakeArrayType (int rank)
		{
			if (rank < 1)
				throw new IndexOutOfRangeException ();
			return make_array_type (rank);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern Type make_byref_type ();

		public virtual Type MakeByRefType ()
		{
			return make_byref_type ();
		}
#endif
	}
}
