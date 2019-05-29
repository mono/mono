//
// TypeBuilder.pns.cs
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

using System.Runtime.InteropServices;

namespace System.Reflection.Emit
{
	public sealed partial class TypeBuilder : System.Reflection.TypeInfo
	{
		internal TypeBuilder() { throw new PlatformNotSupportedException (); } 
		public const int UnspecifiedTypeSize = 0;
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
		public override bool IsConstructedGenericType { get { throw new PlatformNotSupportedException (); } }
		public override bool IsGenericParameter { get { throw new PlatformNotSupportedException (); } }
		public override bool IsGenericType { get { throw new PlatformNotSupportedException (); } }
		public override bool IsGenericTypeDefinition { get { throw new PlatformNotSupportedException (); } }
		public override bool IsTypeDefinition { get { throw new PlatformNotSupportedException (); } }
		public override System.Reflection.Module Module { get { throw new PlatformNotSupportedException (); } }
		public override string Name { get { throw new PlatformNotSupportedException (); } }
		public override string Namespace { get { throw new PlatformNotSupportedException (); } }
		public System.Reflection.Emit.PackingSize PackingSize { get { throw new PlatformNotSupportedException (); } }
		public override System.Type ReflectedType { get { throw new PlatformNotSupportedException (); } }
		public int Size { get { throw new PlatformNotSupportedException (); } }
		public override System.RuntimeTypeHandle TypeHandle { get { throw new PlatformNotSupportedException (); } }
		public System.Reflection.Emit.TypeToken TypeToken { get { throw new PlatformNotSupportedException (); } }
		public override System.Type UnderlyingSystemType { get { throw new PlatformNotSupportedException (); } }

		public void AddDeclarativeSecurity (System.Security.Permissions.SecurityAction action, System.Security.PermissionSet pset) { throw new PlatformNotSupportedException (); }
		public void AddInterfaceImplementation(System.Type interfaceType) { throw new PlatformNotSupportedException (); } 
		public System.Type CreateType() { throw new PlatformNotSupportedException (); }
		public System.Reflection.TypeInfo CreateTypeInfo() { throw new PlatformNotSupportedException (); }
		public System.Reflection.Emit.ConstructorBuilder DefineConstructor(System.Reflection.MethodAttributes attributes, System.Reflection.CallingConventions callingConvention, System.Type[] parameterTypes) { throw new PlatformNotSupportedException (); }
		public System.Reflection.Emit.ConstructorBuilder DefineConstructor(System.Reflection.MethodAttributes attributes, System.Reflection.CallingConventions callingConvention, System.Type[] parameterTypes, System.Type[][] requiredCustomModifiers, System.Type[][] optionalCustomModifiers) { throw new PlatformNotSupportedException (); }
		public System.Reflection.Emit.ConstructorBuilder DefineDefaultConstructor(System.Reflection.MethodAttributes attributes) { throw new PlatformNotSupportedException (); }
		public System.Reflection.Emit.EventBuilder DefineEvent(string name, System.Reflection.EventAttributes attributes, System.Type eventtype) { throw new PlatformNotSupportedException (); }
		public System.Reflection.Emit.FieldBuilder DefineField(string fieldName, System.Type type, System.Reflection.FieldAttributes attributes) { throw new PlatformNotSupportedException (); }
		public System.Reflection.Emit.FieldBuilder DefineField(string fieldName, System.Type type, System.Type[] requiredCustomModifiers, System.Type[] optionalCustomModifiers, System.Reflection.FieldAttributes attributes) { throw new PlatformNotSupportedException (); }
		public System.Reflection.Emit.GenericTypeParameterBuilder[] DefineGenericParameters(params string[] names) { throw new PlatformNotSupportedException (); }
		public System.Reflection.Emit.FieldBuilder DefineInitializedData(string name, byte[] data, System.Reflection.FieldAttributes attributes) { throw new PlatformNotSupportedException (); }
		public System.Reflection.Emit.MethodBuilder DefineMethod(string name, System.Reflection.MethodAttributes attributes) { throw new PlatformNotSupportedException (); }
		public System.Reflection.Emit.MethodBuilder DefineMethod(string name, System.Reflection.MethodAttributes attributes, System.Reflection.CallingConventions callingConvention) { throw new PlatformNotSupportedException (); }
		public System.Reflection.Emit.MethodBuilder DefineMethod(string name, System.Reflection.MethodAttributes attributes, System.Reflection.CallingConventions callingConvention, System.Type returnType, System.Type[] parameterTypes) { throw new PlatformNotSupportedException (); }
		public System.Reflection.Emit.MethodBuilder DefineMethod(string name, System.Reflection.MethodAttributes attributes, System.Reflection.CallingConventions callingConvention, System.Type returnType, System.Type[] returnTypeRequiredCustomModifiers, System.Type[] returnTypeOptionalCustomModifiers, System.Type[] parameterTypes, System.Type[][] parameterTypeRequiredCustomModifiers, System.Type[][] parameterTypeOptionalCustomModifiers) { throw new PlatformNotSupportedException (); }
		public System.Reflection.Emit.MethodBuilder DefineMethod(string name, System.Reflection.MethodAttributes attributes, System.Type returnType, System.Type[] parameterTypes) { throw new PlatformNotSupportedException (); }
		public void DefineMethodOverride(System.Reflection.MethodInfo methodInfoBody, System.Reflection.MethodInfo methodInfoDeclaration) { throw new PlatformNotSupportedException (); } 
		public System.Reflection.Emit.TypeBuilder DefineNestedType(string name) { throw new PlatformNotSupportedException (); }
		public System.Reflection.Emit.TypeBuilder DefineNestedType(string name, System.Reflection.TypeAttributes attr) { throw new PlatformNotSupportedException (); }
		public System.Reflection.Emit.TypeBuilder DefineNestedType(string name, System.Reflection.TypeAttributes attr, System.Type parent) { throw new PlatformNotSupportedException (); }
		public System.Reflection.Emit.TypeBuilder DefineNestedType(string name, System.Reflection.TypeAttributes attr, System.Type parent, int typeSize) { throw new PlatformNotSupportedException (); }
		public System.Reflection.Emit.TypeBuilder DefineNestedType(string name, System.Reflection.TypeAttributes attr, System.Type parent, System.Reflection.Emit.PackingSize packSize) { throw new PlatformNotSupportedException (); }
		public System.Reflection.Emit.TypeBuilder DefineNestedType(string name, System.Reflection.TypeAttributes attr, System.Type parent, System.Reflection.Emit.PackingSize packSize, int typeSize) { throw new PlatformNotSupportedException (); }
		public System.Reflection.Emit.TypeBuilder DefineNestedType(string name, System.Reflection.TypeAttributes attr, System.Type parent, System.Type[] interfaces) { throw new PlatformNotSupportedException (); }
		public System.Reflection.Emit.MethodBuilder DefinePInvokeMethod(string name, string dllName, System.Reflection.MethodAttributes attributes, System.Reflection.CallingConventions callingConvention, System.Type returnType, System.Type[] parameterTypes, System.Runtime.InteropServices.CallingConvention nativeCallConv, System.Runtime.InteropServices.CharSet nativeCharSet) { throw new PlatformNotSupportedException (); }
		public System.Reflection.Emit.MethodBuilder DefinePInvokeMethod(string name, string dllName, string entryName, System.Reflection.MethodAttributes attributes, System.Reflection.CallingConventions callingConvention, System.Type returnType, System.Type[] parameterTypes, System.Runtime.InteropServices.CallingConvention nativeCallConv, System.Runtime.InteropServices.CharSet nativeCharSet) { throw new PlatformNotSupportedException (); }
		public System.Reflection.Emit.MethodBuilder DefinePInvokeMethod(string name, string dllName, string entryName, System.Reflection.MethodAttributes attributes, System.Reflection.CallingConventions callingConvention, System.Type returnType, System.Type[] returnTypeRequiredCustomModifiers, System.Type[] returnTypeOptionalCustomModifiers, System.Type[] parameterTypes, System.Type[][] parameterTypeRequiredCustomModifiers, System.Type[][] parameterTypeOptionalCustomModifiers, System.Runtime.InteropServices.CallingConvention nativeCallConv, System.Runtime.InteropServices.CharSet nativeCharSet) { throw new PlatformNotSupportedException (); }
		public System.Reflection.Emit.PropertyBuilder DefineProperty(string name, System.Reflection.PropertyAttributes attributes, System.Reflection.CallingConventions callingConvention, System.Type returnType, System.Type[] parameterTypes) { throw new PlatformNotSupportedException (); }
		public System.Reflection.Emit.PropertyBuilder DefineProperty(string name, System.Reflection.PropertyAttributes attributes, System.Reflection.CallingConventions callingConvention, System.Type returnType, System.Type[] returnTypeRequiredCustomModifiers, System.Type[] returnTypeOptionalCustomModifiers, System.Type[] parameterTypes, System.Type[][] parameterTypeRequiredCustomModifiers, System.Type[][] parameterTypeOptionalCustomModifiers) { throw new PlatformNotSupportedException (); }
		public System.Reflection.Emit.PropertyBuilder DefineProperty(string name, System.Reflection.PropertyAttributes attributes, System.Type returnType, System.Type[] parameterTypes) { throw new PlatformNotSupportedException (); }
		public System.Reflection.Emit.PropertyBuilder DefineProperty(string name, System.Reflection.PropertyAttributes attributes, System.Type returnType, System.Type[] returnTypeRequiredCustomModifiers, System.Type[] returnTypeOptionalCustomModifiers, System.Type[] parameterTypes, System.Type[][] parameterTypeRequiredCustomModifiers, System.Type[][] parameterTypeOptionalCustomModifiers) { throw new PlatformNotSupportedException (); }
		public System.Reflection.Emit.ConstructorBuilder DefineTypeInitializer() { throw new PlatformNotSupportedException (); }
		public System.Reflection.Emit.FieldBuilder DefineUninitializedData(string name, int size, System.Reflection.FieldAttributes attributes) { throw new PlatformNotSupportedException (); }
		protected override System.Reflection.TypeAttributes GetAttributeFlagsImpl() { throw new PlatformNotSupportedException (); }
		public static System.Reflection.ConstructorInfo GetConstructor(System.Type type, System.Reflection.ConstructorInfo constructor) { throw new PlatformNotSupportedException (); }
		protected override System.Reflection.ConstructorInfo GetConstructorImpl(System.Reflection.BindingFlags bindingAttr, System.Reflection.Binder binder, System.Reflection.CallingConventions callConvention, System.Type[] types, System.Reflection.ParameterModifier[] modifiers) { throw new PlatformNotSupportedException (); }
		public override System.Reflection.ConstructorInfo[] GetConstructors(System.Reflection.BindingFlags bindingAttr) { throw new PlatformNotSupportedException (); }
		public override object[] GetCustomAttributes(bool inherit) { throw new PlatformNotSupportedException (); }
		public override object[] GetCustomAttributes(System.Type attributeType, bool inherit) { throw new PlatformNotSupportedException (); }
		public override System.Type GetElementType() { throw new PlatformNotSupportedException (); }
		public override System.Reflection.EventInfo GetEvent(string name, System.Reflection.BindingFlags bindingAttr) { throw new PlatformNotSupportedException (); }
		public override System.Reflection.EventInfo[] GetEvents() { throw new PlatformNotSupportedException (); }
		public override System.Reflection.EventInfo[] GetEvents(System.Reflection.BindingFlags bindingAttr) { throw new PlatformNotSupportedException (); }
		public override System.Reflection.FieldInfo GetField(string name, System.Reflection.BindingFlags bindingAttr) { throw new PlatformNotSupportedException (); }
		public static System.Reflection.FieldInfo GetField(System.Type type, System.Reflection.FieldInfo field) { throw new PlatformNotSupportedException (); }
		public override System.Reflection.FieldInfo[] GetFields(System.Reflection.BindingFlags bindingAttr) { throw new PlatformNotSupportedException (); }
		public override System.Type[] GetGenericArguments() { throw new PlatformNotSupportedException (); }
		public override System.Type GetGenericTypeDefinition() { throw new PlatformNotSupportedException (); }
		public override System.Type GetInterface(string name, bool ignoreCase) { throw new PlatformNotSupportedException (); }
		public override System.Reflection.InterfaceMapping GetInterfaceMap(System.Type interfaceType) { throw new PlatformNotSupportedException (); }
		public override System.Type[] GetInterfaces() { throw new PlatformNotSupportedException (); }
		public override System.Reflection.MemberInfo[] GetMember(string name, System.Reflection.MemberTypes type, System.Reflection.BindingFlags bindingAttr) { throw new PlatformNotSupportedException (); }
		public override System.Reflection.MemberInfo[] GetMembers(System.Reflection.BindingFlags bindingAttr) { throw new PlatformNotSupportedException (); }
		public static System.Reflection.MethodInfo GetMethod(System.Type type, System.Reflection.MethodInfo method) { throw new PlatformNotSupportedException (); }
		protected override System.Reflection.MethodInfo GetMethodImpl(string name, System.Reflection.BindingFlags bindingAttr, System.Reflection.Binder binder, System.Reflection.CallingConventions callConvention, System.Type[] types, System.Reflection.ParameterModifier[] modifiers) { throw new PlatformNotSupportedException (); }
		public override System.Reflection.MethodInfo[] GetMethods(System.Reflection.BindingFlags bindingAttr) { throw new PlatformNotSupportedException (); }
		public override System.Type GetNestedType(string name, System.Reflection.BindingFlags bindingAttr) { throw new PlatformNotSupportedException (); }
		public override System.Type[] GetNestedTypes(System.Reflection.BindingFlags bindingAttr) { throw new PlatformNotSupportedException (); }
		public override System.Reflection.PropertyInfo[] GetProperties(System.Reflection.BindingFlags bindingAttr) { throw new PlatformNotSupportedException (); }
		protected override System.Reflection.PropertyInfo GetPropertyImpl(string name, System.Reflection.BindingFlags bindingAttr, System.Reflection.Binder binder, System.Type returnType, System.Type[] types, System.Reflection.ParameterModifier[] modifiers) { throw new PlatformNotSupportedException (); }
		protected override bool HasElementTypeImpl() { throw new PlatformNotSupportedException (); }
		public override object InvokeMember(string name, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, object target, object[] args, System.Reflection.ParameterModifier[] modifiers, System.Globalization.CultureInfo culture, string[] namedParameters) { throw new PlatformNotSupportedException (); }
		protected override bool IsArrayImpl() { throw new PlatformNotSupportedException (); }
		public override bool IsAssignableFrom (System.Type c) { throw new PlatformNotSupportedException (); }
		public override bool IsAssignableFrom (TypeInfo typeInfo) { throw new PlatformNotSupportedException (); }
		protected override bool IsByRefImpl() { throw new PlatformNotSupportedException (); }
		protected override bool IsCOMObjectImpl() { throw new PlatformNotSupportedException (); }
		public bool IsCreated() { throw new PlatformNotSupportedException (); }
		public override bool IsDefined(System.Type attributeType, bool inherit) { throw new PlatformNotSupportedException (); }
		protected override bool IsPointerImpl() { throw new PlatformNotSupportedException (); }
		protected override bool IsPrimitiveImpl() { throw new PlatformNotSupportedException (); }
		public override bool IsSubclassOf(System.Type c) { throw new PlatformNotSupportedException (); }
		protected override bool IsValueTypeImpl () { throw new PlatformNotSupportedException (); }
		public override System.Type MakeArrayType() { throw new PlatformNotSupportedException (); }
		public override System.Type MakeArrayType(int rank) { throw new PlatformNotSupportedException (); }
		public override System.Type MakeByRefType() { throw new PlatformNotSupportedException (); }
		public override System.Type MakeGenericType(params System.Type[] typeArguments) { throw new PlatformNotSupportedException (); }
		public override System.Type MakePointerType() { throw new PlatformNotSupportedException (); }
		public void SetCustomAttribute(System.Reflection.ConstructorInfo con, byte[] binaryAttribute) { throw new PlatformNotSupportedException (); } 
		public void SetCustomAttribute(System.Reflection.Emit.CustomAttributeBuilder customBuilder) { throw new PlatformNotSupportedException (); } 
		public void SetParent(System.Type parent) { throw new PlatformNotSupportedException (); } 
		public override string ToString() { throw new PlatformNotSupportedException (); }
	}
}

#endif
