//
// MethodBuilder.pns.cs
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

using System;
using System.Collections.Generic;

namespace System.Reflection.Emit
{
	public sealed partial class MethodBuilder : System.Reflection.MethodInfo
	{
		internal MethodBuilder() { throw new PlatformNotSupportedException (); } 
		public override System.Reflection.MethodAttributes Attributes { get { throw new PlatformNotSupportedException (); } }
		public override System.Reflection.CallingConventions CallingConvention { get { throw new PlatformNotSupportedException (); } }
		public override bool ContainsGenericParameters { get { throw new PlatformNotSupportedException (); } }
		public override System.Type DeclaringType { get { throw new PlatformNotSupportedException (); } }
		public bool InitLocals { get { throw new PlatformNotSupportedException (); } set { throw new PlatformNotSupportedException (); }  }
		public override bool IsGenericMethod { get { throw new PlatformNotSupportedException (); } }
		public override bool IsGenericMethodDefinition { get { throw new PlatformNotSupportedException (); } }
		public override System.RuntimeMethodHandle MethodHandle { get { throw new PlatformNotSupportedException (); } }
		public override System.Reflection.Module Module { get { throw new PlatformNotSupportedException (); } }
		public override string Name { get { throw new PlatformNotSupportedException (); } }
		public override System.Type ReflectedType { get { throw new PlatformNotSupportedException (); } }
		public override System.Reflection.ParameterInfo ReturnParameter { get { throw new PlatformNotSupportedException (); } }
		public override System.Type ReturnType { get { throw new PlatformNotSupportedException (); } }
		public override System.Reflection.ICustomAttributeProvider ReturnTypeCustomAttributes { get { throw new PlatformNotSupportedException (); } }
		public string Signature { get { throw new PlatformNotSupportedException (); } }

		public void AddDeclarativeSecurity (System.Security.Permissions.SecurityAction action, System.Security.PermissionSet pset) { throw new PlatformNotSupportedException (); } 
		public void CreateMethodBody(byte[] il, int count) { throw new PlatformNotSupportedException (); } 
		public System.Reflection.Emit.GenericTypeParameterBuilder[] DefineGenericParameters(params string[] names) { throw new PlatformNotSupportedException (); }
		public System.Reflection.Emit.ParameterBuilder DefineParameter(int position, System.Reflection.ParameterAttributes attributes, string strParamName) { throw new PlatformNotSupportedException (); }
		public override bool Equals(object obj) { throw new PlatformNotSupportedException (); }
		public override System.Reflection.MethodInfo GetBaseDefinition() { throw new PlatformNotSupportedException (); }
		public override object[] GetCustomAttributes(bool inherit) { throw new PlatformNotSupportedException (); }
		public override object[] GetCustomAttributes(System.Type attributeType, bool inherit) { throw new PlatformNotSupportedException (); }
		public override System.Type[] GetGenericArguments() { throw new PlatformNotSupportedException (); }
		public override System.Reflection.MethodInfo GetGenericMethodDefinition() { throw new PlatformNotSupportedException (); }
		public override int GetHashCode() { throw new PlatformNotSupportedException (); }
		public System.Reflection.Emit.ILGenerator GetILGenerator() { throw new PlatformNotSupportedException (); }
		public System.Reflection.Emit.ILGenerator GetILGenerator(int size) { throw new PlatformNotSupportedException (); }
		public override System.Reflection.MethodImplAttributes GetMethodImplementationFlags() { throw new PlatformNotSupportedException (); }
		public System.Reflection.Module GetModule() { throw new PlatformNotSupportedException (); }
		public override System.Reflection.ParameterInfo[] GetParameters() { throw new PlatformNotSupportedException (); }
		public System.Reflection.Emit.MethodToken GetToken() { throw new PlatformNotSupportedException (); }
		public override object Invoke(object obj, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, object[] parameters, System.Globalization.CultureInfo culture) { throw new PlatformNotSupportedException (); }
		public override bool IsDefined(System.Type attributeType, bool inherit) { throw new PlatformNotSupportedException (); }
		public override System.Reflection.MethodInfo MakeGenericMethod(params System.Type[] typeArguments) { throw new PlatformNotSupportedException (); }
		public void SetCustomAttribute(System.Reflection.ConstructorInfo con, byte[] binaryAttribute) { throw new PlatformNotSupportedException (); } 
		public void SetCustomAttribute(System.Reflection.Emit.CustomAttributeBuilder customBuilder) { throw new PlatformNotSupportedException (); } 
		public void SetImplementationFlags(System.Reflection.MethodImplAttributes attributes) { throw new PlatformNotSupportedException (); } 
		[Obsolete ("An alternate API is available: Emit the MarshalAs custom attribute instead.")]
		public void SetMarshal (UnmanagedMarshal unmanagedMarshal) { throw new PlatformNotSupportedException (); }
		public void SetMethodBody(byte[] il, int maxStack, byte[] localSignature, System.Collections.Generic.IEnumerable<System.Reflection.Emit.ExceptionHandler> exceptionHandlers, System.Collections.Generic.IEnumerable<int> tokenFixups) { throw new PlatformNotSupportedException (); } 
		public void SetParameters(params System.Type[] parameterTypes) { throw new PlatformNotSupportedException (); } 
		public void SetReturnType(System.Type returnType) { throw new PlatformNotSupportedException (); } 
		public void SetSignature(System.Type returnType, System.Type[] returnTypeRequiredCustomModifiers, System.Type[] returnTypeOptionalCustomModifiers, System.Type[] parameterTypes, System.Type[][] parameterTypeRequiredCustomModifiers, System.Type[][] parameterTypeOptionalCustomModifiers) { throw new PlatformNotSupportedException (); } 
		public void SetSymCustomAttribute (string name, byte[] data) { throw new PlatformNotSupportedException (); }
		public override string ToString() { throw new PlatformNotSupportedException (); }
	}
}

#endif
