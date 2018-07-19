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

#if FULL_AOT_RUNTIME

namespace System.Reflection.Emit
{
	public abstract class MethodBuilder : MethodInfo
	{
		public bool InitLocals { get; set; }

		public override MethodAttributes Attributes {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public override Type DeclaringType {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public override string Name {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public GenericTypeParameterBuilder[] DefineGenericParameters (string[] names)
		{
			throw new PlatformNotSupportedException ();
		}

		public ParameterBuilder DefineParameter (int position, ParameterAttributes attributes, string strParamName)
		{
			throw new PlatformNotSupportedException ();
		}

		public ILGenerator GetILGenerator ()
		{
			throw new PlatformNotSupportedException ();
		}

		public ILGenerator GetILGenerator (int size)
		{
			throw new PlatformNotSupportedException ();
		}

		public override System.Reflection.ParameterInfo[] GetParameters ()
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

		public void SetImplementationFlags (MethodImplAttributes attributes)
		{
			throw new PlatformNotSupportedException ();
		}

		public void SetParameters (Type[] parameterTypes)
		{
			throw new PlatformNotSupportedException ();
		}

		public void SetReturnType (Type returnType)
		{
			throw new PlatformNotSupportedException ();
		}

		public void SetSignature (Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers)
		{
			throw new PlatformNotSupportedException ();
		}
	}
}

#endif