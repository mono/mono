// System.MonoType
//
// Sean MacIsaac (macisaac@ximian.com)
//
// (C) 2001 Ximian, Inc.

using System.Reflection;
using System.Runtime.CompilerServices;

namespace System
{
	internal class MonoType : Type
	{

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern void type_from_obj (MonoType type, Object obj);
		
		internal MonoType (Object obj) {
			type_from_obj (this, obj);
		}

		public override Type[] GetInterfaces()
		{
			return null;
		}

		public override Type GetElementType()
		{
			return null;
		}

		public override Assembly Assembly {
			get {
				return null;
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
				return null;
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
				return null;
			}
		}

		public override string Namespace {
			get {
				return null;
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
