//
// System.ModuleHandle.cs
//
// Author:
//   Zoltan Varga (vargaz@gmail.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
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
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;

#if NET_2_0

namespace System
{
	public struct ModuleHandle
	{
		IntPtr value;

		public static readonly ModuleHandle EmptyHandle = new ModuleHandle (IntPtr.Zero);

		internal ModuleHandle (IntPtr v)
		{
			value = v;
		}

		public IntPtr Value {
			get {
				return value;
			}
		}

		public void GetPEKind (out PortableExecutableKind peKind, out ImageFileMachine machine)
		{
			if (value == IntPtr.Zero)
				throw new ArgumentNullException (String.Empty, "Invalid handle");
			Module.GetPEKind (value, out peKind, out machine);
		}

		public RuntimeFieldHandle ResolveFieldHandle (int fieldToken)
		{
			ResolveTokenError error;
			if (value == IntPtr.Zero)
				throw new ArgumentNullException (String.Empty, "Invalid handle");
			IntPtr res = Module.ResolveFieldToken (value, fieldToken, out error);
			if (res == IntPtr.Zero)
				throw new Exception (String.Format ("Could not load field '0x{0:x}' from assembly '0x{1:x}'", fieldToken, value.ToInt64 ()));
			else
				return new RuntimeFieldHandle (res);
		}

		public RuntimeMethodHandle ResolveMethodHandle (int methodToken)
		{
			ResolveTokenError error;
			if (value == IntPtr.Zero)
				throw new ArgumentNullException (String.Empty, "Invalid handle");
			IntPtr res = Module.ResolveMethodToken (value, methodToken, out error);
			if (res == IntPtr.Zero)
				throw new Exception (String.Format ("Could not load method '0x{0:x}' from assembly '0x{1:x}'", methodToken, value.ToInt64 ()));
			else
				return new RuntimeMethodHandle (res);
		}

		public RuntimeTypeHandle ResolveTypeHandle (int typeToken)
		{
			ResolveTokenError error;
			if (value == IntPtr.Zero)
				throw new ArgumentNullException (String.Empty, "Invalid handle");
			IntPtr res = Module.ResolveTypeToken (value, typeToken, out error);
			if (res == IntPtr.Zero)
				throw new TypeLoadException (String.Format ("Could not load type '0x{0:x}' from assembly '0x{1:x}'", typeToken, value.ToInt64 ()));
			else
				return new RuntimeTypeHandle (res);
		}
	}
}

#endif
