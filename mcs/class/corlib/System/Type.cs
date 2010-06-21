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
using System.Reflection.Emit;
using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Globalization;

namespace System {

	[Serializable]
	[ClassInterface (ClassInterfaceType.None)]
	[ComVisible (true)]
	[ComDefaultInterface (typeof (_Type))]
	public abstract class Type : MemberInfo, IReflect, _Type {
		
		internal RuntimeTypeHandle _impl;

		public static readonly char Delimiter = '.';
		public static readonly Type[] EmptyTypes = {};
		public static readonly MemberFilter FilterAttribute = new MemberFilter (FilterAttribute_impl);
		public static readonly MemberFilter FilterName = new MemberFilter (FilterName_impl);
		public static readonly MemberFilter FilterNameIgnoreCase = new MemberFilter (FilterNameIgnoreCase_impl);
		public static readonly object Missing = System.Reflection.Missing.Value;

		internal const BindingFlags DefaultBindingFlags =
		BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;

		/* implementation of the delegates for MemberFilter */
		static bool FilterName_impl (MemberInfo m, object filterCriteria)
		{
			string name = (string) filterCriteria;
			if (name == null || name.Length == 0 )
				return false; // because m.Name cannot be null or empty
				
			if (name [name.Length-1] == '*')
				return string.CompareOrdinal (name, 0, m.Name, 0, name.Length-1) == 0;

           	return name.Equals (m.Name);            	
		}

		static bool FilterNameIgnoreCase_impl (MemberInfo m, object filterCriteria)
		{
			string name = (string) filterCriteria;
			if (name == null || name.Length == 0 )
				return false; // because m.Name cannot be null or empty
				
			if (name [name.Length-1] == '*')
				return string.Compare (name, 0, m.Name, 0, name.Length-1, StringComparison.OrdinalIgnoreCase) == 0;

			return string.Equals (name, m.Name, StringComparison.OrdinalIgnoreCase);
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
				if (IsInterface)
					return false;

				return !IsValueType;
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

		public
#if NET_4_0
		virtual
#endif
		bool IsEnum {
			get {
				return IsSubclassOf (typeof (Enum));
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

		public
#if NET_4_0
		virtual
#endif
		bool IsSerializable {
			get {
				if ((Attributes & TypeAttributes.Serializable) != 0)
					return true;

				// Enums and delegates are always serializable

				Type type = UnderlyingSystemType;
				if (type == null)
					return false;

				// Fast check for system types
				if (type.IsSystemType)
					return type_is_subtype_of (type, typeof (Enum), false) || type_is_subtype_of (type, typeof (Delegate), false);

				// User defined types depend on this behavior
				do {
					if ((type == typeof (Enum)) || (type == typeof (Delegate)))
						return true;

					type = type.BaseType;
				} while (type != null);

				return false;
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

		override
		public abstract Module Module {get;}
	
		public abstract string Namespace {get;}

		public override Type ReflectedType {
			get {
				return null;
			}
		}

		public virtual RuntimeTypeHandle TypeHandle {
			get { throw new ArgumentException ("Derived class must provide implementation."); }
		}

		[ComVisible (true)]
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

		/*
		 * This has NOTHING to do with getting the base type of an enum. Use
		 * Enum.GetUnderlyingType () for that.
		 */
		public abstract Type UnderlyingSystemType {get;}

		public override bool Equals (object o)
		{
#if NET_4_0
			return Equals (o as Type);
#else
			if (o == this)
				return true;

			Type me = UnderlyingSystemType;
			if (me == null)
				return false;
			return me.EqualsInternal (o as Type);
#endif
		}

#if NET_4_0
		public virtual bool Equals (Type o)
		{
			if ((object)o == this)
				return true;
			if ((object)o == null)
				return false;
			Type me = UnderlyingSystemType;
			if ((object)me == null)
				return false;

			o = o.UnderlyingSystemType;
			if ((object)o == null)
				return false;
			if ((object)o == this)
				return true;
			return me.EqualsInternal (o);
		}		
#else
		public bool Equals (Type o)
		{

			if (o == this)
				return true;
			if (o == null)
				return false;
			Type me = UnderlyingSystemType;
			if (me == null)
				return false;
			return me.EqualsInternal (o.UnderlyingSystemType);
		}
#endif
#if NET_4_0
		[MonoTODO ("Implement it properly once 4.0 impl details are known.")]
		public static bool operator == (Type left, Type right)
		{
			return Object.ReferenceEquals (left, right);
		}

		[MonoTODO ("Implement it properly once 4.0 impl details are known.")]
		public static bool operator != (Type left, Type right)
		{
			return !Object.ReferenceEquals (left, right);
		}

		[MonoInternalNote ("Reimplement this in MonoType for bonus speed")]
		public virtual Type GetEnumUnderlyingType () {
			if (!IsEnum)
				throw new ArgumentException ("Type is not an enumeration", "enumType");

			var fields = GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			if (fields == null || fields.Length != 1)
				throw new ArgumentException ("An enum must have exactly one instance field", "enumType");

			return fields [0].FieldType;
		}

		[MonoInternalNote ("Reimplement this in MonoType for bonus speed")]
		public virtual string[] GetEnumNames () {
			if (!IsEnum)
				throw new ArgumentException ("Type is not an enumeration", "enumType");

			var fields = GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

			string [] result = new string [fields.Length];
			for (int i = 0; i < fields.Length; ++i)
				result [i] = fields [i].Name;

			return result;
		}

		NotImplementedException CreateNIE () {
			return new NotImplementedException ();
		}

		public virtual Array GetEnumValues () {
			if (!IsEnum)
				throw new ArgumentException ("Type is not an enumeration", "enumType");

			throw CreateNIE ();
		}

		bool IsValidEnumType (Type type) {
			return (type.IsPrimitive && type != typeof (bool) && type != typeof (double) && type != typeof (float)) || type.IsEnum;
		}

		[MonoInternalNote ("Reimplement this in MonoType for bonus speed")]
		public virtual string GetEnumName (object value) {
			if (value == null)
				throw new ArgumentException ("Value is null", "value");
			if (!IsValidEnumType (value.GetType ()))
				throw new ArgumentException ("Value is not the enum or a valid enum underlying type", "value");
			if (!IsEnum)
				throw new ArgumentException ("Type is not an enumeration", "enumType");

			object obj = null;
			var fields = GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			
			for (int i = 0; i < fields.Length; ++i) {
				var fv = fields [i].GetValue (null);
				if (obj == null) {
					try {
						//XXX we can't use 'this' as argument as it might be an UserType
						obj = Enum.ToObject (fv.GetType (), value);
					} catch (OverflowException) {
						return null;
					} catch (InvalidCastException) {
						throw new ArgumentException ("Value is not valid", "value");
					}
				}
				if (fv.Equals (obj))
					return fields [i].Name;
			}

			return null;
		}

		[MonoInternalNote ("Reimplement this in MonoType for bonus speed")]
		public virtual bool IsEnumDefined (object value) {
			if (value == null)
				throw new ArgumentException ("Value is null", "value");
			if (!IsEnum)
				throw new ArgumentException ("Type is not an enumeration", "enumType");

			Type vt = value.GetType ();
			if (!IsValidEnumType (vt) && vt != typeof (string))
				throw new InvalidOperationException ("Value is not the enum or a valid enum underlying type");

			var fields = GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

			if (value is string) {
				for (int i = 0; i < fields.Length; ++i) {
					if (fields [i].Name.Equals (value))
						return true;
				}
			} else {
				if (vt != this && vt != GetEnumUnderlyingType ())
					throw new ArgumentException ("Value is not the enum or a valid enum underlying type", "value");

				object obj = null;
				for (int i = 0; i < fields.Length; ++i) {
					var fv = fields [i].GetValue (null);
					if (obj == null) {
						try {
							//XXX we can't use 'this' as argument as it might be an UserType
							obj = Enum.ToObject (fv.GetType (), value);
						} catch (OverflowException) {
							return false;
						} catch (InvalidCastException) {
							throw new ArgumentException ("Value is not valid", "value");
						}
					}
					if (fv.Equals (obj))
						return true;
				}
			}
			return false;
		}
	
		public static Type GetType (string typeName, Func<AssemblyName,Assembly> assemblyResolver, Func<Assembly,string,bool,Type> typeResolver)
		{
			return GetType (typeName, assemblyResolver, typeResolver, false, false);
		}
	
		public static Type GetType (string typeName, Func<AssemblyName,Assembly> assemblyResolver, Func<Assembly,string,bool,Type> typeResolver, bool throwOnError)
		{
			return GetType (typeName, assemblyResolver, typeResolver, throwOnError, false);
		}
	
		public static Type GetType (string typeName, Func<AssemblyName,Assembly> assemblyResolver, Func<Assembly,string,bool,Type> typeResolver, bool throwOnError, bool ignoreCase)
		{
			TypeSpec spec = TypeSpec.Parse (typeName);
			return spec.Resolve (assemblyResolver, typeResolver, throwOnError, ignoreCase);
		}

		public virtual bool IsSecurityTransparent
		{
			get { throw CreateNIE (); }
		}

		public virtual bool IsSecurityCritical
		{
			get { throw CreateNIE (); }
		}

		public virtual bool IsSecuritySafeCritical
		{
			get { throw CreateNIE (); }
		}
#endif
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern bool EqualsInternal (Type type);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern Type internal_from_handle (IntPtr handle);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern Type internal_from_name (string name, bool throwOnError, bool ignoreCase);

		public static Type GetType(string typeName)
		{
			if (typeName == null)
				throw new ArgumentNullException ("TypeName");

			return internal_from_name (typeName, false, false);
		}

		public static Type GetType(string typeName, bool throwOnError)
		{
			if (typeName == null)
				throw new ArgumentNullException ("TypeName");

			Type type = internal_from_name (typeName, throwOnError, false);
			if (throwOnError && type == null)
				throw new TypeLoadException ("Error loading '" + typeName + "'");

			return type;
		}

		public static Type GetType(string typeName, bool throwOnError, bool ignoreCase)
		{
			if (typeName == null)
				throw new ArgumentNullException ("TypeName");

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
		internal extern static TypeCode GetTypeCodeInternal (Type type);

#if NET_4_0
		protected virtual
#endif
		TypeCode GetTypeCodeImpl () {
			Type type = this;
			if (type is MonoType)
				return GetTypeCodeInternal (type);

			type = type.UnderlyingSystemType;

			if (!type.IsSystemType)
				return TypeCode.Object;
			else
				return GetTypeCodeInternal (type);
		}

		public static TypeCode GetTypeCode (Type type) {
			if (type == null)
				/* MS.NET returns this */
				return TypeCode.Empty;
			return type.GetTypeCodeImpl ();
		}

		[MonoTODO("This operation is currently not supported by Mono")]
		public static Type GetTypeFromCLSID (Guid clsid)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("This operation is currently not supported by Mono")]
		public static Type GetTypeFromCLSID (Guid clsid, bool throwOnError)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("This operation is currently not supported by Mono")]
		public static Type GetTypeFromCLSID (Guid clsid, string server)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("This operation is currently not supported by Mono")]
		public static Type GetTypeFromCLSID (Guid clsid, string server, bool throwOnError)
		{
			throw new NotImplementedException ();
		}

		public static Type GetTypeFromHandle (RuntimeTypeHandle handle)
		{
			if (handle.Value == IntPtr.Zero)
				// This is not consistent with the other GetXXXFromHandle methods, but
				// MS.NET seems to do this
				return null;

			return internal_from_handle (handle.Value);
		}

		[MonoTODO("Mono does not support COM")]
		public static Type GetTypeFromProgID (string progID)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Mono does not support COM")]
		public static Type GetTypeFromProgID (string progID, bool throwOnError)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Mono does not support COM")]
		public static Type GetTypeFromProgID (string progID, string server)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Mono does not support COM")]
		public static Type GetTypeFromProgID (string progID, string server, bool throwOnError)
		{
			throw new NotImplementedException ();
		}

		public static RuntimeTypeHandle GetTypeHandle (object o)
		{
			if (o == null)
				throw new ArgumentNullException ();

			return o.GetType().TypeHandle;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern bool type_is_subtype_of (Type a, Type b, bool check_interfaces);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern bool type_is_assignable_from (Type a, Type b);

		public new Type GetType ()
		{
			return base.GetType ();
		}

		[ComVisible (true)]
		public virtual bool IsSubclassOf (Type c)
		{
			if (c == null || c == this)
				return false;

			// Fast check for system types
			if (IsSystemType)
				return c.IsSystemType && type_is_subtype_of (this, c, false);

			// User defined types depend on this behavior
			for (Type type = BaseType; type != null; type = type.BaseType)
				if (type == c)
					return true;

			return false;
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

		[ComVisible (true)]
		public virtual InterfaceMapping GetInterfaceMap (Type interfaceType) {
			if (!IsSystemType)
				throw new NotSupportedException ("Derived classes must provide an implementation.");
			if (!interfaceType.IsSystemType)
				throw new ArgumentException ("interfaceType", "Type is an user type");
			InterfaceMapping res;
			if (interfaceType == null)
				throw new ArgumentNullException ("interfaceType");
			if (!interfaceType.IsInterface)
				throw new ArgumentException (Locale.GetText ("Argument must be an interface."), "interfaceType");
			if (IsInterface)
				throw new ArgumentException ("'this' type cannot be an interface itself");
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

			if (c is TypeBuilder)
				return ((TypeBuilder)c).IsAssignableTo (this);

			/* Handle user defined type classes */
			if (!IsSystemType) {
				Type systemType = UnderlyingSystemType;
				if (!systemType.IsSystemType)
					return false;

				Type other = c.UnderlyingSystemType;
				if (!other.IsSystemType)
					return false;

				return systemType.IsAssignableFrom (other);
			}

			if (!c.IsSystemType) {
				Type underlyingType = c.UnderlyingSystemType;
				if (!underlyingType.IsSystemType)
					return false;
				return IsAssignableFrom (underlyingType);
			}

			return type_is_assignable_from (this, c);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static bool IsInstanceOfType (Type type, object o);

		public virtual bool IsInstanceOfType (object o)
		{
			Type type = UnderlyingSystemType;
			if (!type.IsSystemType)
				return false;
			return IsInstanceOfType (type, o);
		}

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
			Type t = UnderlyingSystemType;
			if (t != null && t != this)
				return t.GetHashCode ();
			return (int)_impl.Value;
		}

		public MemberInfo[] GetMember (string name)
		{
			return GetMember (name, MemberTypes.All, DefaultBindingFlags);
		}
		
		public virtual MemberInfo[] GetMember (string name, BindingFlags bindingAttr)
		{
			return GetMember (name, MemberTypes.All, bindingAttr);
		}

		public virtual MemberInfo[] GetMember (string name, MemberTypes type, BindingFlags bindingAttr)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
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

		internal MethodInfo GetMethodImplInternal (string name, BindingFlags bindingAttr, Binder binder,
															CallingConventions callConvention, Type[] types,
															ParameterModifier[] modifiers)
		{
			return GetMethodImpl (name, bindingAttr, binder, callConvention, types, modifiers);
		}

		internal virtual MethodInfo GetMethod (MethodInfo fromNoninstanciated)
                {
			throw new System.InvalidOperationException ("can only be called in generic type");
                }

		internal virtual ConstructorInfo GetConstructor (ConstructorInfo fromNoninstanciated)
                {
			throw new System.InvalidOperationException ("can only be called in generic type");
                }

		internal virtual FieldInfo GetField (FieldInfo fromNoninstanciated)
                {
			throw new System.InvalidOperationException ("can only be called in generic type");
                }

		
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

			return GetPropertyImpl (name, DefaultBindingFlags, null, null, null, null);
		}

		public PropertyInfo GetProperty (string name, BindingFlags bindingAttr)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			return GetPropertyImpl (name, bindingAttr, null, null, null, null);
		}

		public PropertyInfo GetProperty (string name, Type returnType)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			return GetPropertyImpl (name, DefaultBindingFlags, null, returnType, null, null);
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

			foreach (Type t in types) {
				if (t == null)
					throw new ArgumentNullException ("types");
			}

			return GetPropertyImpl (name, bindingAttr, binder, returnType, types, modifiers);
		}

		protected abstract PropertyInfo GetPropertyImpl (string name, BindingFlags bindingAttr, Binder binder,
		                                                 Type returnType, Type[] types, ParameterModifier[] modifiers);

		internal PropertyInfo GetPropertyImplInternal (string name, BindingFlags bindingAttr, Binder binder,
													   Type returnType, Type[] types, ParameterModifier[] modifiers)
		{
			return GetPropertyImpl (name, bindingAttr, binder, returnType, types, modifiers);
		}

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
			if (this == typeof (ValueType) || this == typeof (Enum))
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

		[ComVisible (true)]
		public ConstructorInfo GetConstructor (Type[] types)
		{
			return GetConstructor (BindingFlags.Public|BindingFlags.Instance, null, CallingConventions.Any, types, null);
		}

		[ComVisible (true)]
		public ConstructorInfo GetConstructor (BindingFlags bindingAttr, Binder binder,
						       Type[] types, ParameterModifier[] modifiers)
		{
			return GetConstructor (bindingAttr, binder, CallingConventions.Any, types, modifiers);
		}

		[ComVisible (true)]
		public ConstructorInfo GetConstructor (BindingFlags bindingAttr, Binder binder,
						       CallingConventions callConvention,
						       Type[] types, ParameterModifier[] modifiers)
		{
			if (types == null)
				throw new ArgumentNullException ("types");

			foreach (Type t in types) {
				if (t == null)
					throw new ArgumentNullException ("types");
			}

			return GetConstructorImpl (bindingAttr, binder, callConvention, types, modifiers);
		}

		[ComVisible (true)]
		public ConstructorInfo[] GetConstructors ()
		{
			return GetConstructors (BindingFlags.Public | BindingFlags.Instance);
		}

		[ComVisible (true)]
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

			switch (memberType) {
			case MemberTypes.Constructor :
				result = new ConstructorInfo [l.Count];
				break;
			case MemberTypes.Event :
				result = new EventInfo [l.Count];
				break;
			case MemberTypes.Field :
				result = new FieldInfo [l.Count];
				break;
			case MemberTypes.Method :
				result = new MethodInfo [l.Count];
				break;
			case MemberTypes.NestedType :
			case MemberTypes.TypeInfo :
				result = new Type [l.Count];
				break;
			case MemberTypes.Property :
				result = new PropertyInfo [l.Count];
				break;
			default :
				result = new MemberInfo [l.Count];
				break;
			}
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

		internal virtual bool IsCompilerContext {
			get {
				AssemblyBuilder builder = Assembly as AssemblyBuilder;
				return builder != null && builder.IsCompilerContext;
			}
		}

		internal virtual Type InternalResolve ()
		{
			return UnderlyingSystemType;
		}

		internal bool IsSystemType {
			get {
				return _impl.Value != IntPtr.Zero;
			}
		}

		public virtual Type[] GetGenericArguments ()
		{
			throw new NotSupportedException ();
		}

		public virtual bool ContainsGenericParameters {
			get { return false; }
		}

		public virtual extern bool IsGenericTypeDefinition {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern Type GetGenericTypeDefinition_impl ();

		public virtual Type GetGenericTypeDefinition ()
		{
			throw new NotSupportedException ("Derived classes must provide an implementation.");
		}

		public virtual extern bool IsGenericType {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern Type MakeGenericType (Type gt, Type [] types);

		static AssemblyBuilder PeelAssemblyBuilder (Type type)
		{
			if (type.Assembly is AssemblyBuilder)
				return (AssemblyBuilder)type.Assembly;

			if (type.HasElementType)
				return PeelAssemblyBuilder (type.GetElementType ());

			if (!type.IsGenericType || type.IsGenericParameter || type.IsGenericTypeDefinition)
				return null;

			foreach (Type arg in type.GetGenericArguments ()) {
				AssemblyBuilder ab = PeelAssemblyBuilder (arg);
				if (ab != null)
					return ab;
			}
			return null;
		}

		public virtual Type MakeGenericType (params Type[] typeArguments)
		{
			if (IsUserType)
				throw new NotSupportedException ();
			if (!IsGenericTypeDefinition)
				throw new InvalidOperationException ("not a generic type definition");
			if (typeArguments == null)
				throw new ArgumentNullException ("typeArguments");
			if (GetGenericArguments().Length != typeArguments.Length)
				throw new ArgumentException (String.Format ("The type or method has {0} generic parameter(s) but {1} generic argument(s) where provided. A generic argument must be provided for each generic parameter.", GetGenericArguments ().Length, typeArguments.Length), "typeArguments");

			bool hasUserType = false;
			AssemblyBuilder compilerContext = null;

			Type[] systemTypes = new Type[typeArguments.Length];
			for (int i = 0; i < typeArguments.Length; ++i) {
				Type t = typeArguments [i];
				if (t == null)
					throw new ArgumentNullException ("typeArguments");

				if (!(t is MonoType))
					hasUserType = true;
				if (t.IsCompilerContext)
					compilerContext = PeelAssemblyBuilder (t);
				systemTypes [i] = t;
			}

			if (hasUserType) {
				if (compilerContext != null)
					return compilerContext.MakeGenericType (this, typeArguments);
				return new MonoGenericClass (this, typeArguments);
			}

			Type res = MakeGenericType (this, systemTypes);
			if (res == null)
				throw new TypeLoadException ();
			return res;
		}

		public virtual bool IsGenericParameter {
			get {
				return false;
			}
		}

		public bool IsNested {
			get {
				return DeclaringType != null;
			}
		}

		public bool IsVisible {
			get {
				if (IsNestedPublic)
					return DeclaringType.IsVisible;

				return IsPublic;
			}
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
				if (!IsSystemType)
					throw new NotSupportedException ("Derived classes must provide an implementation.");

				if (!IsGenericParameter)
					throw new InvalidOperationException ();

				return GetGenericParameterAttributes ();
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern Type[] GetGenericParameterConstraints_impl ();

		public virtual Type[] GetGenericParameterConstraints ()
		{
			if (!IsSystemType)
				throw new InvalidOperationException ();

			if (!IsGenericParameter)
				throw new InvalidOperationException ();

			return GetGenericParameterConstraints_impl ();
		}

		public virtual MethodBase DeclaringMethod {
			get {
				return null;
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern Type make_array_type (int rank);

		public virtual Type MakeArrayType ()
		{
			if (!IsSystemType)
				throw new NotSupportedException ("Derived classes must provide an implementation.");
			return make_array_type (0);
		}

		public virtual Type MakeArrayType (int rank)
		{
			if (!IsSystemType)
				throw new NotSupportedException ("Derived classes must provide an implementation.");
			if (rank < 1 || rank > 255)
				throw new IndexOutOfRangeException ();
			return make_array_type (rank);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern Type make_byref_type ();

		public virtual Type MakeByRefType ()
		{
			if (!IsSystemType)
				throw new NotSupportedException ("Derived classes must provide an implementation.");
			if (IsByRef)
				throw new TypeLoadException ("Can not call MakeByRefType on a ByRef type");
			return make_byref_type ();
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern Type MakePointerType (Type type);

		public virtual Type MakePointerType ()
		{
			if (!IsSystemType)
				throw new NotSupportedException ("Derived classes must provide an implementation.");
			return MakePointerType (this);
		}

		public static Type ReflectionOnlyGetType (string typeName, 
							  bool throwIfNotFound, 
							  bool ignoreCase)
		{
			if (typeName == null)
				throw new ArgumentNullException ("typeName");
			int idx = typeName.IndexOf (',');
			if (idx < 0 || idx == 0 || idx == typeName.Length - 1)
				throw new ArgumentException ("Assembly qualifed type name is required", "typeName");
			string an = typeName.Substring (idx + 1);
			Assembly a;
			try {
				a = Assembly.ReflectionOnlyLoad (an);
			} catch {
				if (throwIfNotFound)
					throw;
				return null;
			}
			return a.GetType (typeName.Substring (0, idx), throwIfNotFound, ignoreCase);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern void GetPacking (out int packing, out int size);		

		public virtual StructLayoutAttribute StructLayoutAttribute {
			get {
#if NET_4_0
				throw CreateNIE ();
#else
				return GetStructLayoutAttribute ();
#endif
			}
		}
		
		internal StructLayoutAttribute GetStructLayoutAttribute ()
		{
			LayoutKind kind;

			if (IsLayoutSequential)
				kind = LayoutKind.Sequential;
			else if (IsExplicitLayout)
				kind = LayoutKind.Explicit;
			else
				kind = LayoutKind.Auto;

			StructLayoutAttribute attr = new StructLayoutAttribute (kind);

			if (IsUnicodeClass)
				attr.CharSet = CharSet.Unicode;
			else if (IsAnsiClass)
				attr.CharSet = CharSet.Ansi;
			else
				attr.CharSet = CharSet.Auto;

			if (kind != LayoutKind.Auto)
				GetPacking (out attr.Pack, out attr.Size);

			return attr;
		}

		internal object[] GetPseudoCustomAttributes ()
		{
			int count = 0;

			/* IsSerializable returns true for delegates/enums as well */
			if ((Attributes & TypeAttributes.Serializable) != 0)
				count ++;
			if ((Attributes & TypeAttributes.Import) != 0)
				count ++;

			if (count == 0)
				return null;
			object[] attrs = new object [count];
			count = 0;

			if ((Attributes & TypeAttributes.Serializable) != 0)
				attrs [count ++] = new SerializableAttribute ();
			if ((Attributes & TypeAttributes.Import) != 0)
				attrs [count ++] = new ComImportAttribute ();

			return attrs;
		}			


#if NET_4_0 || BOOTSTRAP_NET_4_0
		public virtual bool IsEquivalentTo (Type other)
		{
			return this == other;
		}
#endif

		/* 
		 * Return whenever this object is an instance of a user defined subclass
		 * of System.Type or an instance of TypeDelegator.
		 */
		internal bool IsUserType {
			get {
				/* 
				 * subclasses cannot modify _impl so if it is zero, it means the
				 * type is not created by the runtime.
				 */
				return _impl.Value == IntPtr.Zero &&
					(GetType ().Assembly != typeof (Type).Assembly || GetType () == typeof (TypeDelegator));
			}
		}

		void _Type.GetIDsOfNames ([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException ();
		}

		void _Type.GetTypeInfo (uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException ();
		}

		void _Type.GetTypeInfoCount (out uint pcTInfo)
		{
			throw new NotImplementedException ();
		}

		void _Type.Invoke (uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException ();
		}
	}
}
