//
// SignatureHelper.pns.cs
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
	public class SignatureHelper
	{
		SignatureHelper ()
		{			
		}
		
		public void AddArgument (Type clsArgument)
		{
			throw new PlatformNotSupportedException ();
		}

		public void AddArgument (Type argument, bool pinned)
		{
			throw new PlatformNotSupportedException ();
		}

		public void AddArgument (Type argument, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers)
		{
			throw new PlatformNotSupportedException ();
		}

		public void AddArguments (Type[] arguments, Type[][] requiredCustomModifiers, Type[][] optionalCustomModifiers)
		{
			throw new PlatformNotSupportedException ();
		}

		public void AddSentinel ()
		{
			throw new PlatformNotSupportedException ();
		}

		public static SignatureHelper GetFieldSigHelper (Module mod)
		{
			throw new PlatformNotSupportedException ();
		}

		public static SignatureHelper GetLocalVarSigHelper ()
		{
			throw new PlatformNotSupportedException ();
		}

		public static SignatureHelper GetLocalVarSigHelper (Module mod)
		{
			throw new PlatformNotSupportedException ();
		}

		public static SignatureHelper GetMethodSigHelper (CallingConventions callingConvention, Type returnType)
		{
			throw new PlatformNotSupportedException ();
		}

		public static SignatureHelper GetMethodSigHelper (Module mod, CallingConventions callingConvention, Type returnType)
		{
			throw new PlatformNotSupportedException ();
		}

		public static SignatureHelper GetMethodSigHelper (Module mod, Type returnType, Type[] parameterTypes)
		{
			throw new PlatformNotSupportedException ();
		}

		public static SignatureHelper GetPropertySigHelper (Module mod, Type returnType, Type[] parameterTypes)
		{
			throw new PlatformNotSupportedException ();
		}

		public static SignatureHelper GetPropertySigHelper (Module mod, Type returnType, Type[] requiredReturnTypeCustomModifiers, Type[] optionalReturnTypeCustomModifiers, Type[] parameterTypes, Type[][] requiredParameterTypeCustomModifiers, Type[][] optionalParameterTypeCustomModifiers)
		{
			throw new PlatformNotSupportedException ();
		}

		public static SignatureHelper GetPropertySigHelper (Module mod, CallingConventions callingConvention, Type returnType, Type[] requiredReturnTypeCustomModifiers, Type[] optionalReturnTypeCustomModifiers, Type[] parameterTypes, Type[][] requiredParameterTypeCustomModifiers, Type[][] optionalParameterTypeCustomModifiers)
		{
			throw new PlatformNotSupportedException ();
		}

		public byte[] GetSignature ()
		{
			throw new PlatformNotSupportedException ();
		}
	}
}

#endif
