//
// System.MonoType
//
// Sean MacIsaac (macisaac@ximian.com)
// Paolo Molaro (lupus@ximian.com)
// Patrik Torstensson (patrik.torstensson@labs2.com)
//
// (C) 2001 Ximian, Inc.
//

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Globalization;

namespace System
{
	internal struct MonoTypeInfo {
		public string name;
		public string name_space;
		public Type parent;
		public Type etype;
		public Assembly assembly;
		public TypeAttributes attrs;
		public int rank;
		public bool isbyref;
		public bool ispointer;
		public bool isprimitive;
	}

	internal class MonoType : Type
	{

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern void type_from_obj (MonoType type, Object obj);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern void get_type_info (RuntimeTypeHandle type, out MonoTypeInfo info);

		[MonoTODO]
		internal MonoType (Object obj)
		{
			// this should not be used - lupus
			type_from_obj (this, obj);
			
			throw new NotImplementedException ();
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern TypeAttributes get_attributes (Type type);
	
		protected override TypeAttributes GetAttributeFlagsImpl ()
		{
			return get_attributes (this);
		}

		[MonoTODO]
		protected override ConstructorInfo GetConstructorImpl (BindingFlags bindingAttr,
								       Binder binder,
								       CallingConventions callConvention,
								       Type[] types,
								       ParameterModifier[] modifiers)
		{
			// FIXME
			throw new NotImplementedException ();
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override ConstructorInfo[] GetConstructors (BindingFlags bindingAttr);

		[MonoTODO]
		public override EventInfo GetEvent (string name, BindingFlags bindingAttr)
		{
			// FIXME
			throw new NotImplementedException ();
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override EventInfo[] GetEvents (BindingFlags bindingAttr);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override FieldInfo GetField (string name, BindingFlags bindingAttr);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override FieldInfo[] GetFields (BindingFlags bindingAttr);

		public override Type GetInterface (string name, bool ignoreCase)
		{
			if (name == null)
				throw new ArgumentNullException ();

			Type[] interfaces = GetInterfaces();

			foreach (Type type in interfaces)
				if (String.Compare (type.Name, name, ignoreCase) == 0)
					return type;

			return null;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		[MonoTODO]
		public extern override Type[] GetInterfaces();
		
		public override MemberInfo[] GetMembers( BindingFlags bindingAttr)
		{
			// FIXME
			throw new NotImplementedException ();
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override MethodInfo[] GetMethods (BindingFlags bindingAttr);

		protected override MethodInfo GetMethodImpl (string name, BindingFlags bindingAttr,
							     Binder binder,
							     CallingConventions callConvention,
							     Type[] types, ParameterModifier[] modifiers)
		{
			// FIXME
			throw new NotImplementedException ();
		}
		
		public override Type GetNestedType( string name, BindingFlags bindingAttr)
		{
			// FIXME
			throw new NotImplementedException ();
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override Type[] GetNestedTypes (BindingFlags bindingAttr);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override PropertyInfo[] GetProperties( BindingFlags bindingAttr);

		[MonoTODO]
		protected override PropertyInfo GetPropertyImpl (string name, BindingFlags bindingAttr,
								 Binder binder, Type returnType,
								 Type[] types,
								 ParameterModifier[] modifiers)
		{
			// fixme: needs to use the binder, and send the modifiers to that binder
			if (null == name || types == null)
				throw new ArgumentNullException ();
			
			PropertyInfo ret = null;
			PropertyInfo [] props = GetProperties(bindingAttr);

			foreach (PropertyInfo info in props) {
					if (info.Name != name) 
						continue;

					if (returnType != null)
						if (info.GetGetMethod().ReturnType != returnType)
							continue;

					if (types.Length > 0) {
						if (info.GetIndexParameters().Length != types.Length)
							continue;
	
						// fixme: compare parameters
					}

					if (null != ret)
						throw new AmbiguousMatchException();

					ret = info;
			}

			return ret;
		}

		protected override bool HasElementTypeImpl ()
		{
			return IsArrayImpl() || IsByRefImpl() || IsPointerImpl ();
		}

		protected override bool IsArrayImpl ()
		{
			return type_is_subtype_of (this, typeof (System.Array), false);
		}

		protected override bool IsByRefImpl ()
		{
			MonoTypeInfo info;

			get_type_info (_impl, out info);
			return info.isbyref;
		}

		protected override bool IsCOMObjectImpl ()
		{
			return false;
		}

		protected override bool IsPointerImpl ()
		{
			MonoTypeInfo info;

			get_type_info (_impl, out info);
			return info.ispointer;
		}

		protected override bool IsPrimitiveImpl ()
		{
			MonoTypeInfo info;

			get_type_info (_impl, out info);
			return info.isprimitive;
		}

		protected override bool IsValueTypeImpl ()
		{
			return type_is_subtype_of (this, typeof (System.ValueType), false) &&
				this != typeof (System.ValueType) &&
				this != typeof (System.Enum);
		}
		
		public override object InvokeMember (string name, BindingFlags invokeAttr,
						     Binder binder, object target, object[] args,
						     ParameterModifier[] modifiers,
						     CultureInfo culture, string[] namedParameters)
		{
			// FIXME
			throw new NotImplementedException ();
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override Type GetElementType ();

		public override Type UnderlyingSystemType {
			get {
				MonoTypeInfo info;
				get_type_info (_impl, out info);
				return info.etype;
			}
		}

		public override Assembly Assembly {
			get {
				MonoTypeInfo info;
				get_type_info (_impl, out info);
				return info.assembly;
			}
		}

		public override string AssemblyQualifiedName {
			get {
				return getFullName () + "," + Assembly.ToString ();
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern string getFullName();

		public override Type BaseType {
			get {
				MonoTypeInfo info;
				get_type_info (_impl, out info);
				return info.parent;
			}
		}

		public override string FullName {
			get {
				return getFullName ();
			}
		}

		public override Guid GUID {
			get {
				return Guid.Empty;
			}
		}

		public override bool IsDefined (Type attributeType, bool inherit)
		{
			return MonoCustomAttrs.IsDefined (this, attributeType, inherit);
		}

		public override object[] GetCustomAttributes (bool inherit)
		{
			return MonoCustomAttrs.GetCustomAttributes (this, inherit);
		}

		public override object[] GetCustomAttributes (Type attributeType, bool inherit)
		{
			return MonoCustomAttrs.GetCustomAttributes (this, attributeType, inherit);
		}

		public override MemberTypes MemberType {
			get {
				return MemberTypes.TypeInfo;
			}
		}

		public override string Name {
			get {
				MonoTypeInfo info;
				get_type_info (_impl, out info);
				return info.name;
			}
		}

		public override string Namespace {
			get {
				MonoTypeInfo info;
				get_type_info (_impl, out info);
				return info.name_space;
			}
		}

		public override Module Module {
			get {
				return null;
			}
		}

		public override Type ReflectedType {
			get {
				return null;
			}
		}

		public override RuntimeTypeHandle TypeHandle {
			get {
				return _impl;
			}
		}

		public override int GetArrayRank ()
		{
			MonoTypeInfo info;
			
			get_type_info (_impl, out info);
			return info.rank;
		}
	}
}
