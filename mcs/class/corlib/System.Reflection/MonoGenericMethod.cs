//
// System.Reflection.MonoGenericMethod
//
// Martin Baulig (martin@ximian.com)
//
// (C) 2004 Novell, Inc.
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace System.Reflection
{
	internal class MonoGenericMethod : MonoMethod
	{
		internal MonoGenericMethod ()
		{
			// this should not be used
			throw new InvalidOperationException ();
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern Type get_reflected_type (IntPtr handle);

		public override Type ReflectedType {
			get {
				return get_reflected_type (mhandle);
			}
		}
	}

	internal class MonoGenericCMethod : MonoCMethod
	{
		internal MonoGenericCMethod ()
		{
			// this should not be used
			throw new InvalidOperationException ();
		}

		public override Type ReflectedType {
			get {
				return MonoGenericMethod.get_reflected_type (mhandle);
			}
		}
	}
}
