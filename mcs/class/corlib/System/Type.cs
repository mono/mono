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
using System.Runtime.CompilerServices;

namespace System {

	//
	// FIXME: Implement the various IReflect dependencies
	//
	
	public abstract class Type : MemberInfo /* IReflect */ {
		
		internal RuntimeTypeHandle _impl;

		public static readonly char Delimiter = '.';
		public static readonly Type[] EmptyTypes = {};
		public static readonly MemberFilter FilterAttribute;
		public static readonly MemberFilter FilterName;
		public static readonly MemberFilter FilterNameIgnoreCase;
		public static readonly object Missing;

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

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern TypeAttributes get_attributes (Type type);
		
		/// <summary>
		///   Returns the Attributes associated with the type.
		/// </summary>
		public TypeAttributes Attributes {
			get {
				return get_attributes (this);
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

		public abstract Type UnderlyingSystemType {get;}
		
		/// <summary>
		///
		/// </summary>
		// public static Binder DefaultBinder {
		// get;
		// }
		
		/// <summary>
		///
		/// </summary>
		
		/// <summary>
		///
		/// </summary>
		/// <summary>
		///
		/// </summary>

		/// <summary>
		///    The full name of the type including its namespace
		/// </summary>
		public abstract string FullName {
			get;
		}

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

		public static Type GetTypeFromHandle (RuntimeTypeHandle handle)
		{ 
			return internal_from_handle (handle);
		}

		public abstract RuntimeTypeHandle TypeHandle { get; }
		
		public bool IsValueType {
			get {
				return (Attributes & TypeAttributes.ClassSemanticsMask) == TypeAttributes.ValueType;
			}
		}

		public bool IsClass {
			get {
				return (Attributes & TypeAttributes.ClassSemanticsMask) == TypeAttributes.Class;
			}
		}

		public bool IsInterface {
			get {
				return (Attributes & TypeAttributes.ClassSemanticsMask) == TypeAttributes.Interface;
			}
		}

		public bool IsArray {
			get {
				return type_is_subtype_of (this, typeof (System.Array));
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern bool type_is_subtype_of (Type a, Type b);
		
		public bool IsSubclassOf (Type c)
		{
			return type_is_subtype_of (this, c);
		}

		public virtual Type[] FindInterfaces (TypeFilter filter, object filterCriteria)
		{
			// FIXME
			throw new NotImplementedException ();
		}

		public abstract Type[] GetInterfaces ();
		
		public virtual bool IsAssignableFrom (Type c)
		{
			// FIXME
			throw new NotImplementedException ();
		}

		public virtual int GetArrayRank ()
		{
			// FIXME
			throw new NotImplementedException ();
		}

		public abstract Type GetElementType ();

		public bool IsSealed {
			get {
				return (Attributes & TypeAttributes.Sealed) != 0;
			}
		}

		public bool IsAbstract {
			get {
				return (Attributes & TypeAttributes.Abstract) != 0;
			}
		}

		public bool IsContextful {
			get {
				return typeof (ContextBoundObject).IsAssignableFrom (this);
			}
		}

		public bool IsNotPublic {
			get {
				return !IsPublic;
			}
		}

		public bool IsPublic {
			get {
				// FIXME: handle nestedpublic, too?
				return (Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.Public;
			}
		}

		public abstract Module Module {get;}
		public abstract string Namespace {get;}

		public override int GetHashCode() {
			return (int)_impl.Value;
		}

		public FieldInfo[] GetFields ()
		{
			return GetFields (BindingFlags.Public);
		}

		public FieldInfo[] GetFields (BindingFlags bindingAttr)
		{
			MemberInfo[] m = FindMembers (MemberTypes.Field, bindingAttr, null, null);
			FieldInfo[] res = new FieldInfo [m.Length];
			int i;
			for (i = 0; i < m.Length; ++i)
				res [i] = (FieldInfo) m [i];
			return res;
		}

		public MethodInfo[] GetMethods ()
		{
			return GetMethods (BindingFlags.Public);
		}

		public MethodInfo[] GetMethods (BindingFlags bindingAttr)
		{
			MemberInfo[] m = FindMembers (MemberTypes.Method, bindingAttr, null, null);
			MethodInfo[] res = new MethodInfo [m.Length];
			int i;
			for (i = 0; i < m.Length; ++i)
				res [i] = (MethodInfo) m [i];
			return res;
		}

		public PropertyInfo GetProperty (string name, Type[] types)
		{
			// FIXME
			throw new NotImplementedException ();
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern ConstructorInfo get_constructor (Type type, Type[] types);
		
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

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern MethodInfo get_method (Type type, string name, Type[] types);
		
		public MethodInfo GetMethod (string name, Type[] types)
		{
			return get_method (this, name, types);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public virtual extern MemberInfo[] FindMembers( MemberTypes memberType, BindingFlags bindingAttr,
							 MemberFilter filter, object filterCriteria);

		public static TypeCode GetTypeCode( Type type)
		{
			return TypeCode.Empty;
		}

		public override string ToString()
		{
			return FullName;
		}

	}
}
