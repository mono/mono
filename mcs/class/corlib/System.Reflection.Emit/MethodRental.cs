//
// System.Reflection.Emit/MethodRental.cs
//
// Author:
//   Zoltan Varga (vargaz@freemail.hu)
//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

#if !FULL_AOT_RUNTIME
using System.Security.Permissions;
using System.Runtime.InteropServices;

namespace System.Reflection.Emit
{
	[ComVisible (true)]
	[ComDefaultInterface (typeof (_MethodRental))]
	[ClassInterface (ClassInterfaceType.None)]
	public sealed class MethodRental : _MethodRental {

		public const int JitImmediate = 1;
		public const int JitOnDemand = 0;

		private MethodRental() {
		}

		[MonoTODO]
		[SecurityPermission (SecurityAction.Demand, UnmanagedCode = true)]
		public static void SwapMethodBody (Type cls, int methodtoken, IntPtr rgIL, int methodSize, int flags)
		{
			if (methodSize <= 0 || methodSize >= 0x3f0000) {
				throw new ArgumentException ("Data size must be > 0 and < 0x3f0000", "methodSize");
			}

			if (cls == null)
				throw new ArgumentNullException ("cls");
			if ((cls is TypeBuilder) && (! ((TypeBuilder)cls).is_created))
				throw new NotSupportedException ("Type '" + cls + "' is not yet created.");

			throw new NotImplementedException ();
		}

		void _MethodRental.GetIDsOfNames ([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException ();
		}

		void _MethodRental.GetTypeInfo (uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException ();
		}

		void _MethodRental.GetTypeInfoCount (out uint pcTInfo)
		{
			throw new NotImplementedException ();
		}

		void _MethodRental.Invoke (uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException ();
		}

	}
}

#endif
