
//
// System.Reflection.Emit/EnumBuilder.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace System.Reflection.Emit {
	public sealed class EnumBuilder : Type {
		CustomAttributeBuilder[] cattrs;

		public override Assembly Assembly {
			get { return null; }
		}
		public override string AssemblyQualifiedName {
			get { return null; }
		}
		public override Type BaseType {
			get { return null; }
		}
		public override Type DeclaringType {
			get { return null; }
		}
		public override string FullName {
			get { return null; }
		}
		public override Guid GUID {
			get { return Guid.Empty; }
		}
		public override Module Module {
			get { return null; }
		}
		public override string Name {
			get { return null; }
		}
		public override string Namespace {
			get { return null; }
		}
		public override Type ReflectedType {
			get { return null; }
		}
		public override RuntimeTypeHandle TypeHandle {
			get { return new RuntimeTypeHandle (); }
		}
		public TypeToken TypeToken {
			get { return new TypeToken (); }
		}
		public FieldBuilder UnderlyingField {
			get { return null; }
		}
		public override Type UnderlyingSystemType {
			get { return null; }
		}
/* no need to override
		public override MemberTypes MemberType {
			get {return MemberTypes.TypeInfo;}
		}
*/

		internal EnumBuilder (ModuleBuilder mb, string name, TypeAttributes visibility, Type underlyingType)
		{
		}

		public Type CreateType ()
		{
			return null;
		}

		public FieldBuilder DefineLiteral (string literalName, object literalValue)
		{
			return null;
		}
		protected override TypeAttributes GetAttributeFlagsImpl ()
		{
			return (TypeAttributes)0;
		}

		protected override ConstructorInfo GetConstructorImpl (
			BindingFlags bindingAttr, Binder binder, CallingConventions cc,
			Type[] types, ParameterModifier[] modifiers)
		{
			return null;
		}

		public override ConstructorInfo[] GetConstructors( BindingFlags bindingAttr)
		{
			return null;
		}

		public override object[] GetCustomAttributes(bool inherit)
		{
			return null;
		}

		public override object[] GetCustomAttributes(Type attributeType, bool inherit)
		{
			return null;
		}

		public override Type GetElementType()
		{
			throw new NotSupportedException ();
		}

		public override EventInfo GetEvent( string name, BindingFlags bindingAttr)
		{
			return null;
		}

		public override EventInfo[] GetEvents()
		{
			return null;
		}

		public override EventInfo[] GetEvents( BindingFlags bindingAttr)
		{
			return null;
		}

		public override FieldInfo GetField( string name, BindingFlags bindingAttr)
		{
			return null;
		}

		public override FieldInfo[] GetFields( BindingFlags bindingAttr)
		{
			return null;
		}

		public override Type GetInterface( string name, bool ignoreCase)
		{
			return null;
		}

		public override InterfaceMapping GetInterfaceMap( Type interfaceType)
		{
			throw new NotImplementedException ();
		}

		public override Type[] GetInterfaces()
		{
			return null;
		}

		public override MemberInfo[] GetMember( string name, MemberTypes type, BindingFlags bindingAttr)
		{
			return null;
		}

		public override MemberInfo[] GetMembers( BindingFlags bindingAttr)
		{
			return null;
		}

		protected override MethodInfo GetMethodImpl (
			string name, BindingFlags bindingAttr, Binder binder,
			CallingConventions callConvention, Type[] types,
			ParameterModifier[] modifiers)
		{
			// FIXME
			return null;
		}
		
		public override MethodInfo[] GetMethods( BindingFlags bindingAttr)
		{
			return null;
		}

		public override Type GetNestedType( string name, BindingFlags bindingAttr)
		{
			return null;
		}

		public override Type[] GetNestedTypes( BindingFlags bindingAttr)
		{
			return null;
		}

		public override PropertyInfo[] GetProperties( BindingFlags bindingAttr)
			{
			return null;
		}

		protected override PropertyInfo GetPropertyImpl (
			string name, BindingFlags bindingAttr, Binder binder,
			Type returnType, Type[] types,
			ParameterModifier[] modifiers)
		{
			return null;
		}

		protected override bool HasElementTypeImpl()
			{
			throw new NotSupportedException ();
		}

		public override object InvokeMember (
			string name, BindingFlags invokeAttr, Binder binder,
			object target, object[] args,
			ParameterModifier[] modifiers, CultureInfo culture,
			string[] namedParameters)
		{
			return null;
		}

		protected override bool IsArrayImpl()
		{
			return false;
		}

		protected override bool IsByRefImpl()
		{
			return false;
		}

		protected override bool IsCOMObjectImpl()
		{
			return false;
		}

		protected override bool IsPointerImpl()
		{
			return false;
		}

		protected override bool IsPrimitiveImpl()
		{
			return false;
		}
		
		protected override bool IsValueTypeImpl()
		{
			return true;
		}

		public override bool IsDefined( Type attributeType, bool inherit)
		{
			return false;
		}
		
		public void SetCustomAttribute( CustomAttributeBuilder customBuilder)
		{
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

		public void SetCustomAttribute( ConstructorInfo con, byte[] binaryAttribute)
		{
			SetCustomAttribute (new CustomAttributeBuilder (con, binaryAttribute));
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
