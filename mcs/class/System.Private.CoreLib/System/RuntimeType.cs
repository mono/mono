using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System
{
	// Contains information about the type which is expensive to compute
	[StructLayout (LayoutKind.Sequential)]
	internal class MonoTypeInfo {
		// this is the displayed form: special characters
		// ,+*&*[]\ in the identifier portions of the names
		// have been escaped with a leading backslash (\)
		public string full_name;
		public /*MonoCMethod*/ object default_ctor;
	}


	internal sealed class RuntimeType : TypeInfo
	{
		[NonSerialized]
		internal MonoTypeInfo type_info;
		internal Object GenericCache;


		public override string ToString()
		{
			return getFullName (false, false);
		}
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern string getFullName(bool full_name, bool assembly_qualified);

		public override string FullName {
			get {
				// https://bugzilla.xamarin.com/show_bug.cgi?id=57938
				if (IsGenericType && ContainsGenericParameters && !IsGenericTypeDefinition)
					return null;

				string fullName;
				// This doesn't need locking
				if (type_info == null)
					type_info = new MonoTypeInfo ();
				if ((fullName = type_info.full_name) == null)
					fullName = type_info.full_name = getFullName (true, false);

				return fullName;
			}
		}
		public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
		{
			throw new NotImplementedException (); /* TODO */
		}

		public override FieldInfo[] GetFields(BindingFlags bindingAttr)
		{
			throw new NotImplementedException (); /* TODO */
		}

		protected override bool IsPrimitiveImpl()
		{
			throw new NotImplementedException (); /* TODO */
		}

		public override object[] GetCustomAttributes(bool inherit)
		{
			throw new NotImplementedException (); /* TODO */
		}
			
		public override object[] GetCustomAttributes(Type attributeType, bool inherit)
		{
			throw new NotImplementedException (); /* TODO */
		}

		public override Type GetInterface(string name, bool ignoreCase)
		{
			throw new NotImplementedException (); /* TODO */
		}
		public override Type[] GetInterfaces()
		{
			throw new NotImplementedException (); /* TODO */
		}
		
		public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
		{
			throw new NotImplementedException (); /* TODO */
		}
			
		public override bool IsDefined(Type attributeType, bool inherit)
		{
			throw new NotImplementedException (); /* TODO */
		}

		public override Type GetNestedType(string name, BindingFlags bindingAttr)
		{
			throw new NotImplementedException (); /* TODO */
		}

		protected override bool IsPointerImpl()
		{
			throw new NotImplementedException (); /* TODO */
		}

		public override string Namespace {
			get {
				throw new NotImplementedException (); /* TODO */
			}
		}

		public override string AssemblyQualifiedName {
			get {
				throw new NotImplementedException (); /* TODO */
			}
		}
		
		protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
		{
			throw new NotImplementedException (); /* TODO */
		}

		protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
		{
			throw new NotImplementedException (); /* TODO */
		}

		public override Guid GUID {
			get {
				throw new NotImplementedException (); /* TODO */
			}
		}

		protected override bool IsCOMObjectImpl()
		{
			throw new NotImplementedException (); /* TODO */
		}

		public override Type[] GetNestedTypes(BindingFlags bindingAttr)
		{
			throw new NotImplementedException (); /* TODO */
		}

		public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
		{
			throw new NotImplementedException (); /* TODO */
		}

		public override EventInfo[] GetEvents(BindingFlags bindingAttr)
		{
			throw new NotImplementedException (); /* TODO */
		}

		public override FieldInfo GetField(string name, BindingFlags bindingAttr)
		{
			throw new NotImplementedException (); /* TODO */
		}

		public override Type BaseType {
			get {
				throw new NotImplementedException (); /* TODO */
			}
		}

		public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
		{
			throw new NotImplementedException (); /* TODO */
		}

		public override Assembly Assembly {
			get {
				throw new NotImplementedException (); /* TODO */
			}
		}

		public override Module Module {
			get {
				throw new NotImplementedException (); /* TODO */
			}
		}

		public override Type GetElementType()
		{
			throw new NotImplementedException (); /* TODO */
		}

		public override string Name {
			get {
				throw new NotImplementedException (); /* TODO */
			}
		}

		protected override bool IsArrayImpl()
		{
			throw new NotImplementedException (); /* TODO */
		}

		protected override bool HasElementTypeImpl()
		{
			throw new NotImplementedException (); /* TODO */
		}

		protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
		{
			throw new NotImplementedException (); /* TODO */
		}

		protected override bool IsByRefImpl()
		{
			throw new NotImplementedException (); /* TODO */
		}

		public override Type UnderlyingSystemType {
			get {
				throw new NotImplementedException (); /* TODO */
			}
		}

		protected override TypeAttributes GetAttributeFlagsImpl()
		{
			throw new NotImplementedException (); /* TODO */
		}

		public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
		{
			throw new NotImplementedException (); /* TODO */
		}

		public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
		{
			throw new NotImplementedException (); /* TODO */
		}


	}


}
