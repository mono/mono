//
// System.Reflection.Emit.GenericTypeParameterBuilder
//
// Martin Baulig (martin@ximian.com)
//
// (C) 2004 Novell, Inc.
//

using System.Reflection;
using System.Reflection.Emit;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Globalization;
using System.Runtime.Serialization;

#if NET_1_2
namespace System.Reflection.Emit
{
	public sealed class GenericTypeParameterBuilder : Type
	{
	#region Sync with reflection.h
		private TypeBuilder tbuilder;
		private MethodBuilder mbuilder;
		private string name;
		private int index;
		private Type base_type;
		private Type[] iface_constraints;
	#endregion

		public void SetBaseTypeConstraint (Type base_type_constraint)
		{
			this.base_type = base_type_constraint;
		}

		public void SetInterfaceConstraints (Type[] iface_constraints)
		{
			this.iface_constraints = iface_constraints;
		}

		internal GenericTypeParameterBuilder (TypeBuilder tbuilder,
						      MethodBuilder mbuilder,
						      string name, int index)
		{
			this.tbuilder = tbuilder;
			this.mbuilder = mbuilder;
			this.name = name;
			this.index = index;

			initialize ();
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern void initialize ();

		public override bool IsSubclassOf (Type c)
		{
			if (BaseType == null)
				return false;
			else
				return BaseType == c || BaseType.IsSubclassOf (c);
		}

		protected override TypeAttributes GetAttributeFlagsImpl ()
		{
			return TypeAttributes.Class;
		}

		protected override ConstructorInfo GetConstructorImpl (BindingFlags bindingAttr,
								       Binder binder,
								       CallingConventions callConvention,
								       Type[] types,
								       ParameterModifier[] modifiers)
		{
			throw not_supported ();
		}

		public override ConstructorInfo[] GetConstructors (BindingFlags bindingAttr)
		{
			throw not_supported ();
		}

		public override EventInfo GetEvent (string name, BindingFlags bindingAttr)
		{
			throw not_supported ();
		}

		public override EventInfo[] GetEvents (BindingFlags bindingAttr)
		{
			throw not_supported ();
		}

		public override FieldInfo GetField (string name, BindingFlags bindingAttr)
		{
			throw not_supported ();
		}

		public override FieldInfo[] GetFields (BindingFlags bindingAttr)
		{
			throw not_supported ();
		}
		
		public override Type GetInterface (string name, bool ignoreCase)
		{
			throw not_supported ();
		}

		public override Type[] GetInterfaces ()
		{
			throw not_supported ();
		}
		
		public override MemberInfo[] GetMembers (BindingFlags bindingAttr)
		{
			throw not_supported ();
		}

		public override MethodInfo [] GetMethods (BindingFlags bindingAttr)
		{
			throw not_supported ();
		}

		protected override MethodInfo GetMethodImpl (string name, BindingFlags bindingAttr,
							     Binder binder,
							     CallingConventions callConvention,
							     Type[] types, ParameterModifier[] modifiers)
		{
			throw not_supported ();
		}

		public override Type GetNestedType (string name, BindingFlags bindingAttr)
		{
			throw not_supported ();
		}

		public override Type[] GetNestedTypes (BindingFlags bindingAttr)
		{
			throw not_supported ();
		}

		public override PropertyInfo [] GetProperties (BindingFlags bindingAttr)
		{
			throw not_supported ();
		}

		protected override PropertyInfo GetPropertyImpl (string name, BindingFlags bindingAttr,
								 Binder binder, Type returnType,
								 Type[] types,
								 ParameterModifier[] modifiers)
		{
			throw not_supported ();
		}

		protected override bool HasElementTypeImpl ()
		{
			return false;
		}

		protected override bool IsArrayImpl ()
		{
			return false;
		}

		protected override bool IsByRefImpl ()
		{
			return false;
		}

		protected override bool IsCOMObjectImpl ()
		{
			return false;
		}

		protected override bool IsPointerImpl ()
		{
			return false;
		}

		protected override bool IsPrimitiveImpl ()
		{
			return false;
		}

		protected override bool IsValueTypeImpl ()
		{
#warning "FIXME"
			return false;
			return base_type != null ? base_type.IsValueType : false;
		}
		
		public override object InvokeMember (string name, BindingFlags invokeAttr,
						     Binder binder, object target, object[] args,
						     ParameterModifier[] modifiers,
						     CultureInfo culture, string[] namedParameters)
		{
			throw not_supported ();
		}

		public override Type GetElementType ()
		{
			throw not_supported ();
		}

		public override Type UnderlyingSystemType {
			get {
				return null;
			}
		}

		public override Assembly Assembly {
			get { return tbuilder.Assembly; }
		}

		public override string AssemblyQualifiedName {
			get { return null; }
		}

		public override Type BaseType {
			get { return base_type; }
		}

		public override string FullName {
			get { return null; }
		}

		public override Guid GUID {
			get { return Guid.Empty; }
		}

		public override bool IsDefined (Type attributeType, bool inherit)
		{
			throw not_supported ();
		}

		public override object[] GetCustomAttributes (bool inherit)
		{
			throw not_supported ();
		}

		public override object[] GetCustomAttributes (Type attributeType, bool inherit)
		{
			throw not_supported ();
		}

		public override MemberTypes MemberType {
			get { return MemberTypes.TypeInfo; }
		}

		public override string Name {
			get { return name; }
		}

		public override string Namespace {
			get { return null; }
		}

		public override Module Module {
			get { return tbuilder.Module; }
		}

		public override Type DeclaringType {
			get { return mbuilder != null ? null : tbuilder; }
		}

		public override Type ReflectedType {
			get {
				return DeclaringType;
			}
		}

		public override RuntimeTypeHandle TypeHandle {
			get { throw not_supported (); }
		}

		public override int GetArrayRank ()
		{
			throw not_supported ();
		}

		public override Type GetGenericTypeDefinition ()
		{
			throw not_supported ();
		}

		public override bool HasGenericArguments {
			get { return false; }
		}

		public override bool ContainsGenericParameters {
			get { return false; }
		}

		public override bool IsGenericParameter {
			get { return true; }
		}

		public override int GenericParameterPosition {
			get { return index; }
		}

		public override MethodInfo DeclaringMethod {
			get { return mbuilder; }
		}

		private Exception not_supported ()
		{
			return new NotSupportedException ();
		}

		public override string ToString ()
		{
			return name;
		}
	}
}
#endif
