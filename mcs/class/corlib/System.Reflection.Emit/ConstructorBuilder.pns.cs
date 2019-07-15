//
// ConstructorBuilder.pns.cs
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

using System;
using System.Collections;
using System.Collections.Generic;

#if !MONO_FEATURE_SRE

namespace System.Reflection.Emit
{
	public class ConstructorBuilder : ConstructorInfo
	{
		internal ConstructorBuilder () {}

		public bool InitLocals { get; set; }

		public override MethodAttributes Attributes { 
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public override CallingConventions CallingConvention {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public override Type DeclaringType {
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

		[Obsolete]
		public Type ReturnType {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public ParameterBuilder DefineParameter (int iSequence, ParameterAttributes attributes, string strParamName)
		{
			throw new PlatformNotSupportedException ();
		}

		public ILGenerator GetILGenerator ()
		{
			throw new PlatformNotSupportedException ();
		}

		public ILGenerator GetILGenerator (int streamSize)
		{
			throw new PlatformNotSupportedException ();
		}

		public override ParameterInfo[] GetParameters ()
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

		public string Signature => throw new PlatformNotSupportedException ();
		public Module GetModule () => throw new PlatformNotSupportedException ();
		public MethodToken GetToken () => throw new PlatformNotSupportedException ();
		public void SetMethodBody (byte[] il, int maxStack, byte[] localSignature,
			IEnumerable<ExceptionHandler> exceptionHandlers, IEnumerable<int> tokenFixups) => 
				throw new PlatformNotSupportedException ();

		public void AddDeclarativeSecurity (System.Security.Permissions.SecurityAction action, System.Security.PermissionSet pset) { throw new PlatformNotSupportedException (); }
		public override System.Reflection.MethodImplAttributes GetMethodImplementationFlags() { throw new PlatformNotSupportedException (); }
		public override System.RuntimeMethodHandle MethodHandle { get { throw new PlatformNotSupportedException (); } }
		public override object Invoke(System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, object[] parameters, System.Globalization.CultureInfo culture) { throw new PlatformNotSupportedException (); }
		public override bool IsDefined(System.Type attributeType, bool inherit) { throw new PlatformNotSupportedException (); }
		public override object[] GetCustomAttributes(bool inherit) { throw new PlatformNotSupportedException (); }
		public override object[] GetCustomAttributes(System.Type attributeType, bool inherit) { throw new PlatformNotSupportedException (); }
		public override System.Type ReflectedType { get { throw new PlatformNotSupportedException (); } }
		public override object Invoke(object obj, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, object[] parameters, System.Globalization.CultureInfo culture) { throw new PlatformNotSupportedException (); }
		public void SetSymCustomAttribute (string name, byte[] data) { throw new PlatformNotSupportedException (); }
		public override string ToString () { throw new PlatformNotSupportedException (); }
	}
}

#endif
