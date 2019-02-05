//
// ModuleBuilder.pns.cs
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

#if FULL_AOT_RUNTIME
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Resources;
using System.Runtime.InteropServices;

namespace System.Reflection.Emit
{
	public abstract class ModuleBuilder : Module
	{
		public override Assembly Assembly {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public override string FullyQualifiedName {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public override int MetadataToken {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public override Guid ModuleVersionId {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public override string Name {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public override string ScopeName {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public void CreateGlobalFunctions ()
		{
			throw new PlatformNotSupportedException ();
		}

		public ISymbolDocumentWriter DefineDocument (string url, Guid language, Guid languageVendor, Guid documentType)
		{
			throw new PlatformNotSupportedException ();
		}

		public EnumBuilder DefineEnum (string name, TypeAttributes visibility, Type underlyingType)
		{
			throw new PlatformNotSupportedException ();
		}

		public MethodBuilder DefineGlobalMethod (string name, MethodAttributes attributes, Type returnType, Type[] parameterTypes)
		{
			throw new PlatformNotSupportedException ();
		}

		public MethodBuilder DefineGlobalMethod (string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes)
		{
			throw new PlatformNotSupportedException ();
		}

		public MethodBuilder DefineGlobalMethod (string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] requiredReturnTypeCustomModifiers, Type[] optionalReturnTypeCustomModifiers, Type[] parameterTypes, Type[][] requiredParameterTypeCustomModifiers, Type[][] optionalParameterTypeCustomModifiers)
		{
			throw new PlatformNotSupportedException ();
		}

		public FieldBuilder DefineInitializedData (string name, byte[] data, FieldAttributes attributes)
		{
			throw new PlatformNotSupportedException ();
		}

		public void DefineManifestResource (string name, Stream stream, ResourceAttributes attribute)
		{
			throw new PlatformNotSupportedException ();
		}

		public MethodBuilder DefinePInvokeMethod (string name, string dllName, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, CallingConvention nativeCallConv, CharSet nativeCharSet)
		{
			throw new PlatformNotSupportedException ();
		}

		public MethodBuilder DefinePInvokeMethod (string name, string dllName, string entryName, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, CallingConvention nativeCallConv, CharSet nativeCharSet)
		{
			throw new PlatformNotSupportedException ();
		}

		public IResourceWriter DefineResource (string name, string description)
		{
			throw new PlatformNotSupportedException ();
		}

		public IResourceWriter DefineResource (string name, string description, ResourceAttributes attribute)
		{
			throw new PlatformNotSupportedException ();
		}

		public TypeBuilder DefineType (string name)
		{
			throw new PlatformNotSupportedException ();
		}

		public TypeBuilder DefineType (string name, TypeAttributes attr)
		{
			throw new PlatformNotSupportedException ();
		}

		public TypeBuilder DefineType (string name, TypeAttributes attr, Type parent)
		{
			throw new PlatformNotSupportedException ();
		}

		public TypeBuilder DefineType (string name, TypeAttributes attr, Type parent, int typesize)
		{
			throw new PlatformNotSupportedException ();
		}

		public TypeBuilder DefineType (string name, TypeAttributes attr, Type parent, PackingSize packsize)
		{
			throw new PlatformNotSupportedException ();
		}

		public TypeBuilder DefineType (string name, TypeAttributes attr, Type parent, Type[] interfaces)
		{
			throw new PlatformNotSupportedException ();
		}

		public TypeBuilder DefineType (string name, TypeAttributes attr, Type parent, PackingSize packingSize, int typesize)
		{
			throw new PlatformNotSupportedException ();
		}

		public FieldBuilder DefineUninitializedData (string name, int size, FieldAttributes attributes)
		{
			throw new PlatformNotSupportedException ();
		}

		public void DefineUnmanagedResource (byte[] resource)
		{
			throw new PlatformNotSupportedException ();
		}

		public void DefineUnmanagedResource (string resourceFileName)
		{
			throw new PlatformNotSupportedException ();
		}

		public MethodInfo GetArrayMethod (Type arrayClass, string methodName, CallingConventions callingConvention, Type returnType, Type[] parameterTypes)
		{
			throw new PlatformNotSupportedException ();
		}

		public MethodToken GetArrayMethodToken (Type arrayClass, string methodName, CallingConventions callingConvention, Type returnType, Type[] parameterTypes)
		{
			throw new PlatformNotSupportedException ();
		}

		public MethodToken GetConstructorToken (ConstructorInfo con)
		{
			throw new PlatformNotSupportedException ();
		}

		public MethodToken GetConstructorToken (ConstructorInfo constructor, IEnumerable<Type> optionalParameterTypes)
		{
			throw new PlatformNotSupportedException ();
		}

		public FieldToken GetFieldToken (FieldInfo field)
		{
			throw new PlatformNotSupportedException ();
		}

		public MethodToken GetMethodToken (MethodInfo method)
		{
			throw new PlatformNotSupportedException ();
		}

		public MethodToken GetMethodToken (MethodInfo method, IEnumerable<Type> optionalParameterTypes)
		{
			throw new PlatformNotSupportedException ();
		}

		public SignatureToken GetSignatureToken (SignatureHelper sigHelper)
		{
			throw new PlatformNotSupportedException ();
		}

		public SignatureToken GetSignatureToken (byte[] sigBytes, int sigLength)
		{
			throw new PlatformNotSupportedException ();
		}

		public StringToken GetStringConstant (string str)
		{
			throw new PlatformNotSupportedException ();
		}

		public ISymbolWriter GetSymWriter ()
		{
			throw new PlatformNotSupportedException ();
		}

		public TypeToken GetTypeToken (string name)
		{
			throw new PlatformNotSupportedException ();
		}

		public TypeToken GetTypeToken (Type type)
		{
			throw new PlatformNotSupportedException ();
		}

		public bool IsTransient ()
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

		public void SetSymCustomAttribute (string name, byte[] data)
		{
			throw new PlatformNotSupportedException ();
		}

		public void SetUserEntryPoint (MethodInfo entryPoint)
		{
			throw new PlatformNotSupportedException ();
		}
	}
}

#endif