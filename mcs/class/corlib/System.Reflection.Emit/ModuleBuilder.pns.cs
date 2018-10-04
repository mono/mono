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

#if !MONO_FEATURE_SRE

namespace System.Reflection.Emit
{
	public abstract class ModuleBuilder : Module
	{
		public void CreateGlobalFunctions ()
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

		public MethodInfo GetArrayMethod (Type arrayClass, string methodName, CallingConventions callingConvention, Type returnType, Type[] parameterTypes)
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
	}
}

#endif
