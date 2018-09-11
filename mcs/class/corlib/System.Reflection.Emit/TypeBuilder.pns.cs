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

namespace System.Reflection.Emit
{
	public abstract class TypeBuilder : TypeInfo
	{
		public const int UnspecifiedTypeSize = 0;

		public PackingSize PackingSize {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public int Size {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public override Assembly Assembly {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public override string AssemblyQualifiedName {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public override Type BaseType {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public override string FullName {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public override Guid GUID {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public override Module Module {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public override string Name {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public override string Namespace {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public void AddInterfaceImplementation (Type interfaceType)
		{
			throw new PlatformNotSupportedException ();
		}

		public TypeInfo CreateTypeInfo ()
		{
			throw new PlatformNotSupportedException ();
		}

		public ConstructorBuilder DefineConstructor (MethodAttributes attributes, CallingConventions callingConvention, Type[] parameterTypes)
		{
			throw new PlatformNotSupportedException ();
		}

		public ConstructorBuilder DefineConstructor (MethodAttributes attributes, CallingConventions callingConvention, Type[] parameterTypes, Type[][] requiredCustomModifiers, Type[][] optionalCustomModifiers)
		{
			throw new PlatformNotSupportedException ();
		}

		public ConstructorBuilder DefineDefaultConstructor (MethodAttributes attributes)
		{
			throw new PlatformNotSupportedException ();
		}

		public EventBuilder DefineEvent (string name, EventAttributes attributes, Type eventtype)
		{
			throw new PlatformNotSupportedException ();
		}

		public FieldBuilder DefineField (string fieldName, Type type, FieldAttributes attributes)
		{
			throw new PlatformNotSupportedException ();
		}

		public FieldBuilder DefineField (string fieldName, Type type, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers, FieldAttributes attributes)
		{
			throw new PlatformNotSupportedException ();
		}

		public GenericTypeParameterBuilder[] DefineGenericParameters (string[] names)
		{
			throw new PlatformNotSupportedException ();
		}

		public FieldBuilder DefineInitializedData (string name, byte[] data, FieldAttributes attributes)
		{
			throw new PlatformNotSupportedException ();
		}

		public MethodBuilder DefineMethod (string name, MethodAttributes attributes)
		{
			throw new PlatformNotSupportedException ();
		}

		public MethodBuilder DefineMethod (string name, MethodAttributes attributes, CallingConventions callingConvention)
		{
			throw new PlatformNotSupportedException ();
		}

		public MethodBuilder DefineMethod (string name, MethodAttributes attributes, Type returnType, Type[] parameterTypes)
		{
			throw new PlatformNotSupportedException ();
		}

		public MethodBuilder DefineMethod (string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes)
		{
			throw new PlatformNotSupportedException ();
		}

		public MethodBuilder DefineMethod (string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers)
		{
			throw new PlatformNotSupportedException ();
		}

		public void DefineMethodOverride (MethodInfo methodInfoBody, MethodInfo methodInfoDeclaration)
		{
			throw new PlatformNotSupportedException ();
		}

		public TypeBuilder DefineNestedType (string name)
		{
			throw new PlatformNotSupportedException ();
		}

		public TypeBuilder DefineNestedType (string name, TypeAttributes attr)
		{
			throw new PlatformNotSupportedException ();
		}

		public TypeBuilder DefineNestedType (string name, TypeAttributes attr, Type parent)
		{
			throw new PlatformNotSupportedException ();
		}

		public TypeBuilder DefineNestedType (string name, TypeAttributes attr, Type parent, int typeSize)
		{
			throw new PlatformNotSupportedException ();
		}

		public TypeBuilder DefineNestedType (string name, TypeAttributes attr, Type parent, PackingSize packSize)
		{
			throw new PlatformNotSupportedException ();
		}

		public TypeBuilder DefineNestedType (string name, TypeAttributes attr, Type parent, Type[] interfaces)
		{
			throw new PlatformNotSupportedException ();
		}

		public TypeBuilder DefineNestedType (string name, TypeAttributes attr, Type parent, PackingSize packSize, int typeSize)
		{
			throw new PlatformNotSupportedException ();
		}

		public PropertyBuilder DefineProperty (string name, PropertyAttributes attributes, Type returnType, Type[] parameterTypes)
		{
			throw new PlatformNotSupportedException ();
		}

		public PropertyBuilder DefineProperty (string name, PropertyAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes)
		{
			throw new PlatformNotSupportedException ();
		}

		public PropertyBuilder DefineProperty (string name, PropertyAttributes attributes, Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers)
		{
			throw new PlatformNotSupportedException ();
		}

		public PropertyBuilder DefineProperty (string name, PropertyAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers)
		{
			throw new PlatformNotSupportedException ();
		}

		public ConstructorBuilder DefineTypeInitializer ()
		{
			throw new PlatformNotSupportedException ();
		}

		public FieldBuilder DefineUninitializedData (string name, int size, FieldAttributes attributes)
		{
			throw new PlatformNotSupportedException ();
		}

		public static ConstructorInfo GetConstructor (Type type, ConstructorInfo constructor)
		{
			throw new PlatformNotSupportedException ();
		}

		public static FieldInfo GetField (Type type, FieldInfo field)
		{
			throw new PlatformNotSupportedException ();
		}

		public static MethodInfo GetMethod (Type type, MethodInfo method)
		{
			throw new PlatformNotSupportedException ();
		}

		public bool IsCreated ()
		{
			throw new PlatformNotSupportedException ();
		}

		public void SetCustomAttribute (CustomAttributeBuilder customBuilder)
		{
			throw new PlatformNotSupportedException ();
		}

		public void SetCustomAttribute (ConstructorInfo con, byte[] binaryAttribute)
		{
			throw new PlatformNotSupportedException ();
		}

		public void SetParent (Type parent)
		{
			throw new PlatformNotSupportedException ();
		}

		public override Type GetElementType ()
		{
			throw new PlatformNotSupportedException ();
		}
	}
}

#endif
