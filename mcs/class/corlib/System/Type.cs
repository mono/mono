//
// System.Type.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
// TODO: Mucho left to implement.
//

using System.Reflection;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Globalization;

namespace System {

	//
	// FIXME: Implement the various IReflect dependencies
	//

	[MonoTODO]
	public abstract class Type : MemberInfo, IReflect {
		
		internal RuntimeTypeHandle _impl;

		public static readonly char Delimiter = '.';
		public static readonly Type[] EmptyTypes = {};
		public static readonly MemberFilter FilterAttribute = new MemberFilter (FilterAttribute_impl);
		public static readonly MemberFilter FilterName = new MemberFilter (FilterName_impl);
		public static readonly MemberFilter FilterNameIgnoreCase = new MemberFilter (FilterNameIgnoreCase_impl);
		public static readonly object Missing;

		/* implementation of the delegates for MemberFilter */
		static bool FilterName_impl (MemberInfo m, object filterCriteria) {
			string name = (string) filterCriteria;
			return name.Equals (m.Name);
		}
		
		static bool FilterNameIgnoreCase_impl (MemberInfo m, object filterCriteria) {
			string name = (string) filterCriteria;
			return String.Compare (name, m.Name, true) == 0;
		}
		
		static bool FilterAttribute_impl (MemberInfo m, object filterCriteria) {
			throw new NotImplementedException ("FilterAttribute_impl");
		}

		protected Type () {
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
				throw new NotImplementedException ();
			}
		}

		/// <summary>
		///
		/// </summary>
		public static Binder DefaultBinder {
			get {
				throw new NotImplementedException ();
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

		public bool HasEmelentType {
			get {return false;} // FIXME
		}

		public bool IsAbstract {
			get {
				return (Attributes & TypeAttributes.Abstract) != 0;
			}
		}

		public bool IsAnsiClass {
			get {
				return (Attributes & TypeAttributes.StringFormatMask) == TypeAttributes.AnsiClass;
			}
		}

		public bool IsArray {
			get {
				return IsArrayImpl ();
			}
		}

		public bool IsAuto {
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
				if (this == typeof (System.Enum) || this == typeof (System.ValueType))
					return true;
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
				return (Attributes & TypeAttributes.LayoutMask) == TypeAttributes.LayoutSequential;
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
				return !IsPublic;
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

		[MonoTODO]
		public bool IsPublic {
			get {
				// FIXME: handle nestedpublic, too?
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
				return (Attributes & TypeAttributes.Serializable) != 0;
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
			get {return MemberTypes.TypeInfo;} // FIXME
		}

		public abstract Module Module {get;}
	
		public abstract string Namespace {get;}

		public override Type ReflectedType {
			get {
				throw new NotImplementedException ();
			}
		}

		public abstract RuntimeTypeHandle TypeHandle {get;}

		public ConstructorInfo TypeInitializer {
			get {
				throw new NotImplementedException ();
			}
		}

		public abstract Type UnderlyingSystemType {get;}

		public override bool Equals (object o) {
			if (o == null)
				return false;
			Type cmp = o as Type;
			if (cmp == null)
				return false;
			return Equals (cmp);
		}
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern bool Equals (Type type);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern Type internal_from_handle (RuntimeTypeHandle handle);
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern Type internal_from_name (string name);
		
		public static Type GetType(string typeName)
		{
			return internal_from_name (typeName);
		}

		public static Type GetType(string typeName, bool throwOnError)
		{
			// LAMESPEC: what kinds of errors cause exception to be thrown?
			return internal_from_name (typeName);
		}

		public static Type GetType(string typeName, bool throwOnError, bool ignoreCase)
		{
			throw new NotImplementedException ();
		}

		public static Type[] GetTypeArray (object[] args) {
			Type[] ret;

			ret = new Type [args.Length];
			for (int i = 0; i < args.Length; ++i)
				ret [i] = args[i].GetType ();
			return ret;
		}

		public static TypeCode GetTypeCode( Type type)
		{
			// FIXME
			return TypeCode.Empty;
		}

		public static Type GetTypeFromCLSID (Guid clsid) {
			throw new NotImplementedException ();
		}

		public static Type GetTypeFromCLSID (Guid clsid, bool throwOnError) {
			throw new NotImplementedException ();
		}

		public static Type GetTypeFromCLSID (Guid clsid, string server) {
			throw new NotImplementedException ();
		}

		public static Type GetTypeFromCLSID (Guid clsid, string server, bool throwOnError) {
			throw new NotImplementedException ();
		}

		public static Type GetTypeFromHandle (RuntimeTypeHandle handle)
		{ 
			return internal_from_handle (handle);
		}

		public static Type GetTypeFromProgID (string progID) {
			throw new NotImplementedException ();
		}

		public static Type GetTypeFromProgID (string progID, bool throwOnError) {
			throw new NotImplementedException ();
		}

		public static Type GetTypeFromProgID (string progID, string server) {
			throw new NotImplementedException ();
		}

		public static Type GetTypeFromProgID (string progID, string server, bool throwOnError) {
			throw new NotImplementedException ();
		}

		public static RuntimeTypeHandle GetTypeHandle (object o) {
			return o.GetType().TypeHandle;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		protected static extern bool type_is_subtype_of (Type a, Type b, bool check_interfaces);
		
		public bool IsSubclassOf (Type c)
		{
			return type_is_subtype_of (this, c, false);
		}

		[MonoTODO]
		public virtual Type[] FindInterfaces (TypeFilter filter, object filterCriteria)
		{
			// FIXME
			throw new NotImplementedException ();
		}
		
		public Type GetInterface (string name) {
			return GetInterface (name, false);
		}

		public abstract Type GetInterface (string name, bool ignoreCase);

		public virtual InterfaceMapping GetInterfaceMap (Type interfaceType) {
			throw new NotImplementedException ();
		}

		public abstract Type[] GetInterfaces ();

		[MonoTODO]
		public virtual bool IsAssignableFrom (Type c)
		{
			// FIXME
			return type_is_subtype_of (c, this, true);
		}

		public virtual bool IsInstanceOfType (object o) {
			if (o != null) {
				return o.GetType().IsSubclassOf (this);
			}
			return false;
		}

		[MonoTODO]
		public virtual int GetArrayRank ()
		{
			// FIXME
			throw new NotImplementedException ();
		}

		public abstract Type GetElementType ();

		public EventInfo GetEvent (string name) {
			throw new NotImplementedException ();
		}

		public abstract EventInfo GetEvent (string name, BindingFlags bindingAttr);

		public virtual EventInfo[] GetEvents () {
			return GetEvents (BindingFlags.Public|BindingFlags.Static|BindingFlags.Instance);
		}

		public abstract EventInfo[] GetEvents (BindingFlags bindingAttr);

		public FieldInfo GetField( string name) {
			return GetField (name, BindingFlags.Public);
		}

		public abstract FieldInfo GetField( string name, BindingFlags bindingAttr);

		public FieldInfo[] GetFields ()
		{
			return GetFields (BindingFlags.Public|BindingFlags.Instance|BindingFlags.Static);
		}

		public abstract FieldInfo[] GetFields (BindingFlags bindingAttr);
		
		public override int GetHashCode() {
			return (int)_impl.Value;
		}

		public MemberInfo[] GetMember( string name) {
			return GetMember (name, BindingFlags.Public);
		}
		
		public virtual MemberInfo[] GetMember( string name, BindingFlags bindingAttr) {
			// FIXME
			throw new NotImplementedException ();
		}

		public virtual MemberInfo[] GetMember( string name, MemberTypes type, BindingFlags bindingAttr) {
			// FIXME
			throw new NotImplementedException ();
		}

		public MemberInfo[] GetMembers() {
			return GetMembers (BindingFlags.Public|BindingFlags.Static|BindingFlags.Instance);
		}

		public abstract MemberInfo[] GetMembers( BindingFlags bindingAttr);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern MethodInfo get_method (Type type, string name, Type[] types);

		public MethodInfo GetMethod( string name) {
			return GetMethod (name, BindingFlags.Public);
		}

		public MethodInfo GetMethod( string name, BindingFlags bindingAttr) {
			// FIXME
			throw new NotImplementedException ();
		}
		
		public MethodInfo GetMethod (string name, Type[] types)
		{
			return get_method (this, name, types);
		}

		public MethodInfo GetMethod( string name, Type[] types, ParameterModifier[] modifiers) {
			// FIXME
			throw new NotImplementedException ();
		}

		public MethodInfo GetMethod( string name, BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers) {
			// FIXME
			throw new NotImplementedException ();
		}

		public MethodInfo GetMethod( string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers) {
			// FIXME
			throw new NotImplementedException ();
		}

		protected abstract MethodInfo GetMethodImpl( string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers);

		public MethodInfo[] GetMethods ()
		{
			return GetMethods (BindingFlags.Public|BindingFlags.Instance|BindingFlags.Static);
		}

		public abstract MethodInfo[] GetMethods (BindingFlags bindingAttr);

		public Type GetNestedType( string name) {
			return GetNestedType (name, BindingFlags.Public);
		}

		public abstract Type GetNestedType( string name, BindingFlags bindingAttr);

		public Type[] GetNestedTypes () {
			return GetNestedTypes (BindingFlags.Public);
		}

		public abstract Type[] GetNestedTypes (BindingFlags bindingAttr);

		public PropertyInfo[] GetProperties ()
		{
			return GetProperties (BindingFlags.Public|BindingFlags.Static|BindingFlags.Instance);
		}

		public abstract PropertyInfo[] GetProperties( BindingFlags bindingAttr);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern PropertyInfo get_property (Type type, string name, Type[] types);
		
		public PropertyInfo GetProperty (string name)
		{
			return GetProperty (name, BindingFlags.Public);
		}

		public PropertyInfo GetProperty( string name, BindingFlags bindingAttr) {
			// FIXME
			throw new NotImplementedException ();
		}

		public PropertyInfo GetProperty( string name, Type returnType) {
			// FIXME
			throw new NotImplementedException ();
		}

		public PropertyInfo GetProperty (string name, Type[] types)
		{
			return get_property (this, name, types);
		}

		public PropertyInfo GetProperty (string name, Type returnType, Type[] types)
		{
			// FIXME
			throw new NotImplementedException ();
		}

		public PropertyInfo GetProperty( string name, Type returnType, Type[] types, ParameterModifier[] modifiers) {
			// FIXME
			throw new NotImplementedException ();
		}

		public PropertyInfo GetProperty( string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers) {
			// FIXME
			throw new NotImplementedException ();
		}

		protected abstract PropertyInfo GetPropertyImpl( string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern ConstructorInfo get_constructor (Type type, Type[] types);

		protected abstract ConstructorInfo GetConstructorImpl (BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers);

		protected abstract TypeAttributes GetAttributeFlagsImpl ();
		protected abstract bool HasElementTypeImpl ();
		protected abstract bool IsArrayImpl ();
		protected abstract bool IsByRefImpl ();
		protected abstract bool IsCOMObjectImpl ();
		protected virtual bool IsContextfulImpl () {
			return typeof (ContextBoundObject).IsAssignableFrom (this);
		}
		protected virtual bool IsMarshalByRefImpl () {
			// FIXME
			return false;
		}
		protected abstract bool IsPointerImpl ();
		protected abstract bool IsPrimitiveImpl ();
		protected abstract bool IsValueTypeImpl ();
		
		public ConstructorInfo GetConstructor (Type[] types)
		{
			return get_constructor (this, types);
		}

		public ConstructorInfo GetConstructor (BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers) {
			throw new NotImplementedException ();
		}
		public ConstructorInfo GetConstructor( BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers) {
			throw new NotImplementedException ();
		}

		public ConstructorInfo[] GetConstructors () {
			return GetConstructors (BindingFlags.Public|BindingFlags.Static|BindingFlags.Instance);
		}
		
		public abstract ConstructorInfo[] GetConstructors (BindingFlags bindingAttr);

		public virtual MemberInfo[] GetDefaultMembers () {
			throw new NotImplementedException ();
		}

		public virtual MemberInfo[] FindMembers( MemberTypes memberType, BindingFlags bindingAttr,
							 MemberFilter filter, object filterCriteria) {
			MemberInfo[] result;
			ArrayList l = new ArrayList ();

			//Console.WriteLine ("FindMembers for {0} (Type: {1}): {2}", this.FullName, this.GetType().FullName, this.obj_address());

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
				PropertyInfo[] c = GetProperties (bindingAttr);
				if (filter != null) {
					foreach (MemberInfo m in c) {
						if (filter (m, filterCriteria))
							l.Add (m);
					}
				} else {
					l.AddRange (c);
				}
			}
			result = new MemberInfo [l.Count];
			l.CopyTo (result);
			return result;
		}

		public object InvokeMember( string name, BindingFlags invokeAttr, Binder binder, object target, object[] args) {
			// FIXME
			return null;
		}

		public object InvokeMember( string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, CultureInfo culture) {
			// FIXME
			return null;
		}

		public abstract object InvokeMember( string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters);

		public override string ToString()
		{
			string res = FullName;
			if (IsArray)
				res = res + "[]";
			if (IsByRef)
				res = res + "&";
			if (IsPointer)
				res = res + "*";
			return res;
		}
	}
}
