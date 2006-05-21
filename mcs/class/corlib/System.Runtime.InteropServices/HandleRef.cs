//
// System.Runtime.InteropServices.HandleRef
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2003 Tim Coleman

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

using System;

namespace System.Runtime.InteropServices {
#if NET_2_0
	[ComVisible (true)]
#endif
	public struct HandleRef {

		#region Fields

		object wrapper;
		IntPtr handle;

		#endregion // Fields

		#region Constructors

		public HandleRef (object wrapper, IntPtr handle)
		{
			this.wrapper = wrapper;
			this.handle = handle;
		}

		#endregion // Constructors

		#region Properties

		public IntPtr Handle {
			get { return handle; }
		}

		public object Wrapper {
			get { return wrapper; }
		}

		#endregion // Properties

		#region Type Conversions

		public static explicit operator IntPtr (HandleRef value)
		{
			return value.Handle;
		}
		
		#endregion // Type Conversions
#if NET_2_0
		public static IntPtr ToIntPtr(HandleRef value)
		{
			return value.Handle; 
			// Why did MS add a function for this?
		}
#endif
	}
}
