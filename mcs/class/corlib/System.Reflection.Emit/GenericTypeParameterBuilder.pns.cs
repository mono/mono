//
// GenericTypeParameterBuilder.pns.cs
//
// Authors:
//   Marek Safar (marek.safar@gmail.com)
//
// Copyright (C) 2016 Xamarin Inc (http://www.xamarin.com)
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

#if !MONO_FEATURE_SRE

namespace System.Reflection.Emit
{
	public sealed partial class GenericTypeParameterBuilder : System.Reflection.TypeInfo
	{
		internal GenericTypeParameterBuilder() { throw new PlatformNotSupportedException (); } 
		public override System.Reflection.Assembly Assembly { get { throw new PlatformNotSupportedException (); } }
		public override string AssemblyQualifiedName { get { throw new PlatformNotSupportedException (); } }
		public override System.Type BaseType { get { throw new PlatformNotSupportedException (); } }
		public override bool ContainsGenericParameters { get { throw new PlatformNotSupportedException (); } }
		public override System.Reflection.MethodBase DeclaringMethod { get { throw new PlatformNotSupportedException (); } }
		public override System.Type DeclaringType { get { throw new PlatformNotSupportedException (); } }
		public override string FullName { get { throw new PlatformNotSupportedException (); } }
		public override System.Reflection.GenericParameterAttributes GenericParameterAttributes { get { throw new PlatformNotSupportedException (); } }
		public override int GenericParameterPosition { get { throw new PlatformNotSupportedException (); } }
		public override System.Guid GUID { get { throw new PlatformNotSupportedException (); } }
		public override bool IsGenericParameter { get { throw new PlatformNotSupportedException (); } }
		public override bool IsGenericType { get { throw new PlatformNotSupportedException (); } }
		public override bool IsGenericTypeDefinition { get { throw new PlatformNotSupportedException (); } }
		public override System.Reflection.Module Module { get { throw new PlatformNotSupportedException (); } }
		public override string Name { get { throw new PlatformNotSupportedException (); } }
		public override string Namespace { get { throw new PlatformNotSupportedException (); } }
		public override System.Type ReflectedType { get { throw new PlatformNotSupportedException (); } }
		public override System.RuntimeTypeHandle TypeHandle { get { throw new PlatformNotSupportedException (); } }
		public override System.Type UnderlyingSystemType { get { throw new PlatformNotSupportedException (); } }
		public override bool Equals(object o) { throw new PlatformNotSupportedException (); }
		protected override System.Reflection.TypeAttributes GetAttributeFlagsImpl() { throw new PlatformNotSupportedException (); }
		protected override System.Reflection.ConstructorInfo GetConstructorImpl(System.Reflection.BindingFlags bindingAttr, System.Reflection.Binder binder, System.Reflection.CallingConventions callConvention, System.Type[] types, System.Reflection.ParameterModifier[] modifiers) { throw new PlatformNotSupportedException (); }
		public override System.Reflection.ConstructorInfo[] GetConstructors(System.Reflection.BindingFlags bindingAttr) { throw new PlatformNotSupportedException (); }
		public override object[] GetCustomAttributes(bool inherit) { throw new PlatformNotSupportedException (); }
		public override object[] GetCustomAttributes(System.Type attributeType, bool inherit) { throw new PlatformNotSupportedException (); }
		public override System.Type GetElementType() { throw new PlatformNotSupportedException (); }
		public override System.Reflection.EventInfo GetEvent(string name, System.Reflection.BindingFlags bindingAttr) { throw new PlatformNotSupportedException (); }
		public override System.Reflection.EventInfo[] GetEvents() { throw new PlatformNotSupportedException (); }
		public override System.Reflection.EventInfo[] GetEvents(System.Reflection.BindingFlags bindingAttr) { throw new PlatformNotSupportedException (); }
		public override System.Reflection.FieldInfo GetField(string name, System.Reflection.BindingFlags bindingAttr) { throw new PlatformNotSupportedException (); }
		public override System.Reflection.FieldInfo[] GetFields(System.Reflection.BindingFlags bindingAttr) { throw new PlatformNotSupportedException (); }
		public override System.Type[] GetGenericArguments() { throw new PlatformNotSupportedException (); }
		public override System.Type[] GetGenericParameterConstraints () { throw new PlatformNotSupportedException (); }
		public override System.Type GetGenericTypeDefinition() { throw new PlatformNotSupportedException (); }
		public override int GetHashCode() { throw new PlatformNotSupportedException (); }
		public override System.Type GetInterface(string name, bool ignoreCase) { throw new PlatformNotSupportedException (); }
		public override System.Reflection.InterfaceMapping GetInterfaceMap(System.Type interfaceType) { throw new PlatformNotSupportedException (); }
		public override System.Type[] GetInterfaces() { throw new PlatformNotSupportedException (); }
		public override System.Reflection.MemberInfo[] GetMember(string name, System.Reflection.MemberTypes type, System.Reflection.BindingFlags bindingAttr) { throw new PlatformNotSupportedException (); }
		public override System.Reflection.MemberInfo[] GetMembers(System.Reflection.BindingFlags bindingAttr) { throw new PlatformNotSupportedException (); }
		protected override System.Reflection.MethodInfo GetMethodImpl(string name, System.Reflection.BindingFlags bindingAttr, System.Reflection.Binder binder, System.Reflection.CallingConventions callConvention, System.Type[] types, System.Reflection.ParameterModifier[] modifiers) { throw new PlatformNotSupportedException (); }
		public override System.Reflection.MethodInfo[] GetMethods(System.Reflection.BindingFlags bindingAttr) { throw new PlatformNotSupportedException (); }
		public override System.Type GetNestedType(string name, System.Reflection.BindingFlags bindingAttr) { throw new PlatformNotSupportedException (); }
		public override System.Type[] GetNestedTypes(System.Reflection.BindingFlags bindingAttr) { throw new PlatformNotSupportedException (); }
		public override System.Reflection.PropertyInfo[] GetProperties(System.Reflection.BindingFlags bindingAttr) { throw new PlatformNotSupportedException (); }
		protected override System.Reflection.PropertyInfo GetPropertyImpl(string name, System.Reflection.BindingFlags bindingAttr, System.Reflection.Binder binder, System.Type returnType, System.Type[] types, System.Reflection.ParameterModifier[] modifiers) { throw new PlatformNotSupportedException (); }
		protected override bool HasElementTypeImpl() { throw new PlatformNotSupportedException (); }
		public override object InvokeMember(string name, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, object target, object[] args, System.Reflection.ParameterModifier[] modifiers, System.Globalization.CultureInfo culture, string[] namedParameters) { throw new PlatformNotSupportedException (); }
		protected override bool IsArrayImpl() { throw new PlatformNotSupportedException (); }
		public override bool IsAssignableFrom(System.Type c) { throw new PlatformNotSupportedException (); }
		public override bool IsAssignableFrom (TypeInfo typeInfo) { throw new PlatformNotSupportedException (); }
		protected override bool IsByRefImpl() { throw new PlatformNotSupportedException (); }
		protected override bool IsCOMObjectImpl() { throw new PlatformNotSupportedException (); }
		public override bool IsDefined(System.Type attributeType, bool inherit) { throw new PlatformNotSupportedException (); }
		public override bool IsInstanceOfType (object o) { throw new PlatformNotSupportedException (); }
		protected override bool IsPointerImpl() { throw new PlatformNotSupportedException (); }
		protected override bool IsPrimitiveImpl() { throw new PlatformNotSupportedException (); }
		public override bool IsSubclassOf(System.Type c) { throw new PlatformNotSupportedException (); }
		protected override bool IsValueTypeImpl() { throw new PlatformNotSupportedException (); }
		public override System.Type MakeArrayType() { throw new PlatformNotSupportedException (); }
		public override System.Type MakeArrayType(int rank) { throw new PlatformNotSupportedException (); }
		public override System.Type MakeByRefType() { throw new PlatformNotSupportedException (); }
		public override System.Type MakeGenericType(params System.Type[] typeArguments) { throw new PlatformNotSupportedException (); }
		public override System.Type MakePointerType() { throw new PlatformNotSupportedException (); }
		public void SetBaseTypeConstraint(System.Type baseTypeConstraint) { throw new PlatformNotSupportedException (); } 
		public void SetCustomAttribute(System.Reflection.ConstructorInfo con, byte[] binaryAttribute) { throw new PlatformNotSupportedException (); } 
		public void SetCustomAttribute(System.Reflection.Emit.CustomAttributeBuilder customBuilder) { throw new PlatformNotSupportedException (); } 
		public void SetGenericParameterAttributes(System.Reflection.GenericParameterAttributes genericParameterAttributes) { throw new PlatformNotSupportedException (); } 
		public void SetInterfaceConstraints(params System.Type[] interfaceConstraints) { throw new PlatformNotSupportedException (); } 
		public override string ToString() { throw new PlatformNotSupportedException (); }
	}
}

#endif
