// System.Reflection/TypeDelegator.cs
//
// Paolo Molaro (lupus@ximian.com)
//
// (C) 2002 Ximian, Inc.

using System;
using System.Reflection;
using System.Globalization;

namespace System.Reflection {

	[Serializable]
	public class TypeDelegator : Type {
		protected Type typeImpl;
	
		protected TypeDelegator () {
		}

		public TypeDelegator( Type delegatingType)
		{
			if (delegatingType == null)
				throw new ArgumentNullException ("delegatingType must be non-null");
			typeImpl = delegatingType;
		}

		public override Assembly Assembly {
			get { return typeImpl.Assembly; }
		}

		public override string AssemblyQualifiedName {
			get { return typeImpl.AssemblyQualifiedName; }
		}

		public override Type BaseType {
			get { return typeImpl.BaseType; }
		}

		public override string FullName {
			get { return typeImpl.FullName; }
		}

		public override Guid GUID {
			get { return typeImpl.GUID; }
		}

		public override Module Module {
			get { return typeImpl.Module; }
		}

		public override string Name {
			get { return typeImpl.Name; }
		}

		public override string Namespace {
			get { return typeImpl.Namespace; }
		}

		public override RuntimeTypeHandle TypeHandle {
			get { return typeImpl.TypeHandle; }
		}

		public override Type UnderlyingSystemType {
			get { return typeImpl.UnderlyingSystemType; }
		}

		protected override TypeAttributes GetAttributeFlagsImpl ()
		{
			throw new NotImplementedException ();
			//return typeImpl.GetAttributeFlagsImpl ();
		}
		
		protected override ConstructorInfo GetConstructorImpl (
			BindingFlags bindingAttr, Binder binder, CallingConventions cc,
			Type[] types, ParameterModifier[] modifiers)
		{
			throw new NotImplementedException ();
			//return typeImpl.GetConstructorImpl (bindingAttr, binder, callConvention, types, modifiers);
		}

		public override ConstructorInfo[] GetConstructors( BindingFlags bindingAttr)
		{
			return typeImpl.GetConstructors (bindingAttr);
		}

		public override object[] GetCustomAttributes (bool inherit)
		{
			return typeImpl.GetCustomAttributes (inherit);
		}

		public override object[] GetCustomAttributes (Type attributeType, bool inherit)
		{
			return typeImpl.GetCustomAttributes (attributeType, inherit);
		}

		public override Type GetElementType()
		{
			return typeImpl.GetElementType ();
		}

		public override EventInfo GetEvent( string name, BindingFlags bindingAttr)
		{
			return typeImpl.GetEvent (name, bindingAttr);
		}

		public override EventInfo[] GetEvents()
		{
			return GetEvents (BindingFlags.Public);
		}

		public override EventInfo[] GetEvents (BindingFlags bindingAttr)
		{
			return typeImpl.GetEvents (bindingAttr);
		}

		public override FieldInfo GetField (string name, BindingFlags bindingAttr)
		{
			return typeImpl.GetField (name, bindingAttr);
		}

		public override FieldInfo[] GetFields( BindingFlags bindingAttr)
		{
			return typeImpl.GetFields (bindingAttr);
		}

		public override Type GetInterface( string name, bool ignoreCase)
		{
			return typeImpl.GetInterface (name, ignoreCase);
		}

		public override InterfaceMapping GetInterfaceMap( Type interfaceType)
		{
			return typeImpl.GetInterfaceMap (interfaceType);
		}
		
		public override Type[] GetInterfaces ()
		{
			return typeImpl.GetInterfaces ();
		}

		public override MemberInfo[] GetMember( string name, MemberTypes type, BindingFlags bindingAttr)
		{
			return typeImpl.GetMember (name, type, bindingAttr);
		}

		public override MemberInfo[] GetMembers( BindingFlags bindingAttr)
		{
			return typeImpl.GetMembers (bindingAttr);
		}

		protected override MethodInfo GetMethodImpl( string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
		{
			throw new NotImplementedException ();
			//return typeImpl.GetMethodImpl (name, bindingAttr, binder, callConvention, types, modifiers);
		}

		public override MethodInfo[] GetMethods( BindingFlags bindingAttr)
		{
			return typeImpl.GetMethods (bindingAttr);
		}

		public override Type GetNestedType( string name, BindingFlags bindingAttr)
		{
			return typeImpl.GetNestedType (name, bindingAttr);
		}

		public override Type[] GetNestedTypes( BindingFlags bindingAttr)
		{
			return typeImpl.GetNestedTypes (bindingAttr);
		}

		public override PropertyInfo[] GetProperties( BindingFlags bindingAttr)
		{
			return typeImpl.GetProperties (bindingAttr);
		}

		protected override PropertyInfo GetPropertyImpl( string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
		{
			throw new NotImplementedException ();
			//return typeImpl.GetPropertyImpl (name, bindingAttr, bindingAttr, returnType, types, modifiers);
		}

		protected override bool HasElementTypeImpl()
		{
			throw new NotImplementedException ();
			//return typeImpl.HasElementTypeImpl ();
		}

		public override object InvokeMember( string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters) {
			return typeImpl.InvokeMember (name, invokeAttr, binder, target, args, modifiers, culture, namedParameters);
		}

		protected override bool IsArrayImpl()
		{
			throw new NotImplementedException ();
			//return typeImpl.IsArrayImpl ();
		}

		protected override bool IsByRefImpl()
		{
			throw new NotImplementedException ();
			//return typeImpl.IsByRefImpl ();
		}

		protected override bool IsCOMObjectImpl()
		{
			throw new NotImplementedException ();
			//return typeImpl.IsCOMObjectImpl ();
		}

		public override bool IsDefined( Type attributeType, bool inherit) {
			return typeImpl.IsDefined (attributeType, inherit);
		}

		protected override bool IsPointerImpl()
		{
			throw new NotImplementedException ();
			//return typeImpl.IsPointerImpl ();
		}

		protected override bool IsPrimitiveImpl()
		{
			throw new NotImplementedException ();
			//return typeImpl.IsPrimitiveImpl ();
		}

		protected override bool IsValueTypeImpl()
		{
			throw new NotImplementedException ();
			//return typeImpl.IsValueTypeImpl ();
		}
#if GENERICS
		public override bool HasGenericParameters {
			get {
				throw new NotImplementedException ();
			}
		}

		public override bool HasUnboundGenericParameters {
			get {
				throw new NotImplementedException ();
			}
		}

		public override bool IsUnboundGenericParameter {
			get {
				throw new NotImplementedException ();
			}
		}

		public override int GenericParameterPosition {
			get {
				throw new Exception ("Unimplemented");
			}
		}
#endif

	}
}
