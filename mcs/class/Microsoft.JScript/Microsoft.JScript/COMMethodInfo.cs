//
// COMMethodInfo.cs:
//
// Author:
//	 Cesar Lopez Nataren (cnataren@novell.com)
//
// Copyright (C) 2005 Novell, Inc (http://novell.com)
//

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
using System.Reflection;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Microsoft.JScript {

	[GuidAttribute ("C7B9C313-2FD4-4384-8571-7ABC08BD17E5")]
	[ComVisibleAttribute (true)]
	public class COMMethodInfo : JSMethod, MemberInfoInitializer {

		public COMMemberInfo GetCOMMemberInfo ()
		{
			throw new NotImplementedException ();
		}

		public void Initialize (string name, COMMemberInfo dispatch)
		{
			throw new NotImplementedException ();
		}

		public override Type ReturnType { 
			get { throw new NotImplementedException (); }
		}

		public override ICustomAttributeProvider ReturnTypeCustomAttributes { 
			get { throw new NotImplementedException (); } 
		}

		public override MethodInfo GetBaseDefinition ()
		{
			throw new NotImplementedException ();
		}

		public override MethodImplAttributes GetMethodImplementationFlags ()
		{
			throw new NotImplementedException ();
		}

		public override ParameterInfo [] GetParameters ()
		{
			throw new NotImplementedException ();
		}

		public override Object Invoke (object obj, BindingFlags invokeAttr, 
		       System.Reflection.Binder binder, object [] parameters, CultureInfo culture)
		{
			throw new NotImplementedException ();
		}

		public override RuntimeMethodHandle MethodHandle { 
			get { throw new NotImplementedException (); }
		}

		public override MethodAttributes Attributes { 
			get { throw new NotImplementedException (); }
		}

		public override Type DeclaringType {
			get { throw new NotImplementedException (); }
		}

		public override MemberTypes MemberType {
			get { throw new NotImplementedException (); }
		}

		public override string Name {
			get { throw new NotImplementedException (); }
		}

		public override Type ReflectedType {
			get { throw new NotImplementedException (); }
		}

		public override string ToString ()
		{
			throw new NotImplementedException ();
		}

		protected COMMemberInfo _comObject;
		protected string _name;
		protected static readonly ParameterInfo [] EmptyParams;

#if NET_2_0
		public sealed override Type [] GetGenericArguments ()
		{
			throw new NotImplementedException ();
		}
#endif
	}
}
