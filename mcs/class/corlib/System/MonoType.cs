// System.MonoType
//
// Sean MacIsaac (macisaac@ximian.com)
// Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.

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
		public Type[] interfaces;
		public Assembly assembly;
		public TypeAttributes attrs;
		public int rank;
	}

	internal class MonoType : Type
	{

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern void type_from_obj (MonoType type, Object obj);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern void get_type_info (RuntimeTypeHandle type, out MonoTypeInfo info);

		internal MonoType (Object obj) {
			// this should not be used - lupus
			type_from_obj (this, obj);
			throw new NotImplementedException ();
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern TypeAttributes get_attributes (Type type);
	
		protected override TypeAttributes GetAttributeFlagsImpl () {
			return get_attributes (this);
		}

		protected override ConstructorInfo GetConstructorImpl (BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers) {
			throw new NotImplementedException ();
		}

		public override ConstructorInfo[] GetConstructors (BindingFlags bindingAttr) {
			throw new NotImplementedException ();
		}

		public override EventInfo GetEvent (string name, BindingFlags bindingAttr) {
			throw new NotImplementedException ();
		}

		public override EventInfo[] GetEvents (BindingFlags bindingAttr) {
			throw new NotImplementedException ();
		}

		public override FieldInfo GetField( string name, BindingFlags bindingAttr) {
			//FIXME
			return null;
		}

		public override FieldInfo[] GetFields (BindingFlags bindingAttr) {
			//FIXME
			return null;
		}

		public override Type GetInterface (string name, bool ignoreCase) {
			throw new NotImplementedException ();
		}

		public override Type[] GetInterfaces()
		{
			MonoTypeInfo info;
			get_type_info (_impl, out info);
			return info.interfaces;
		}
		
		public override MemberInfo[] GetMembers( BindingFlags bindingAttr) {
			// FIXME
			throw new NotImplementedException ();
		}

		public override MethodInfo[] GetMethods (BindingFlags bindingAttr) {
			MemberInfo[] m = FindMembers (MemberTypes.Method, bindingAttr, null, null);
			MethodInfo[] res = new MethodInfo [m.Length];
			int i;
			for (i = 0; i < m.Length; ++i)
				res [i] = (MethodInfo) m [i];
			return res;
		}

		protected override MethodInfo GetMethodImpl( string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers) {
			// FIXME
			return null;
		}
		
		public override Type GetNestedType( string name, BindingFlags bindingAttr) {
			// FIXME
			return null;
		}

		public override Type[] GetNestedTypes (BindingFlags bindingAttr) {
			// FIXME
			return null;
		}

		public override PropertyInfo[] GetProperties( BindingFlags bindingAttr) {
			// FIXME
			return null;
		}
		
		protected override PropertyInfo GetPropertyImpl( string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers) {
			// FIXME
			return null;
		}

		protected override bool HasElementTypeImpl () {
			return IsArrayImpl() || IsByRefImpl() || IsPointerImpl ();
		}

		protected override bool IsArrayImpl () {
			return type_is_subtype_of (this, typeof (System.Array), false);
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
			return type_is_subtype_of (this, typeof (System.ValueType), false) &&
				this != typeof (System.ValueType) &&
				this != typeof (System.Enum);
		}
		
		public override object InvokeMember( string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters) {
			// FIXME
			return null;
		}

		public override Type GetElementType()
		{
			MonoTypeInfo info;
			get_type_info (_impl, out info);
			return info.etype;
		}

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
				return assQualifiedName ();
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern string assQualifiedName();

		public override Type BaseType {
			get {
				MonoTypeInfo info;
				get_type_info (_impl, out info);
				return info.parent;
			}
		}

		public override string FullName {
			get {
				string str = assQualifiedName ();
				return str.Split(',')[0];
			}
		}

		public override Guid GUID {
			get {return Guid.Empty;}
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
				return MemberTypes.All;
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

		public override int GetArrayRank () {
			MonoTypeInfo info;
			get_type_info (_impl, out info);
			return info.rank;
		}
	}
}
