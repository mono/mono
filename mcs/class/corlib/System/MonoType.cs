// System.MonoType
//
// Sean MacIsaac (macisaac@ximian.com)
//
// (C) 2001 Ximian, Inc.

using System.Reflection;
using System.Runtime.CompilerServices;

namespace System
{
	internal struct MonoTypeInfo {
		public string name;
		public string name_space;
		public Type parent;
		public Type etype;
		public Type[] interfaces;
		public Assembly assembly;
	}

	internal class MonoType : Type
	{

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern void type_from_obj (MonoType type, Object obj);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern void get_type_info (RuntimeTypeHandle type, out MonoTypeInfo info);

		internal MonoType (Object obj) {
			type_from_obj (this, obj);
		}

		public override Type[] GetInterfaces()
		{
			MonoTypeInfo info;
			get_type_info (_impl, out info);
			return info.interfaces;
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

		public override bool IsDefined (Type attributeType, bool inherit)
		{
			return false;
		}

		public override object[] GetCustomAttributes (bool inherit)
		{
			return null;
		}

		public override object[] GetCustomAttributes (Type attributeType, bool inherit)
		{
			return null;
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
	}
}
