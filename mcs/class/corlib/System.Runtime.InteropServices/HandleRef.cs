//
// System.Runtime.InteropServices.HandleRef
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2003 Tim Coleman

using System;

namespace System.Runtime.InteropServices {
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
	}
}
