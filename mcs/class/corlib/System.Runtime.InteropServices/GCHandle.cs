//
// System.Runtime.InteropServices/GCHandle.cs
//
// Authors:
//   Ajay kumar Dwivedi (adwiv@yahoo.com) ??
//   Paolo Molaro (lupus@ximian.com)
//

//
// Copyright (C) 2004, 2009 Novell, Inc (http://www.novell.com)
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
using System.Threading;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Runtime.InteropServices
{

	[ComVisible(true)]
	// TODO Should be [StructLayout(LayoutKind.Sequential)] but will need to be reordered for that
	public struct GCHandle 
	{
		// fields
		private IntPtr handle;

		private GCHandle(IntPtr h)
		{
			handle = h;
		}
		
		// Constructors
		private GCHandle(object obj)
			: this(obj, GCHandleType.Normal)
		{}

		internal GCHandle(object value, GCHandleType type)
		{
			// MS does not crash/throw on (most) invalid GCHandleType values (except -1)
			if ((type < GCHandleType.Weak) || (type > GCHandleType.Pinned))
				type = GCHandleType.Normal;
			handle = GetTargetHandle (value, IntPtr.Zero, type);
		}

		// Properties

		public bool IsAllocated 
		{ 
			get
			{
				return (handle != IntPtr.Zero);
			}
		}

		public object Target
		{ 
			get
			{
				if (!IsAllocated)
					throw new InvalidOperationException ("Handle is not allocated");
				return GetTarget (handle);
			} 
			set
			{
				handle = GetTargetHandle (value, handle, (GCHandleType)(-1));
			} 
		}

		// Methods
		public IntPtr AddrOfPinnedObject()
		{
			IntPtr res = GetAddrOfPinnedObject(handle);
			if (res == (IntPtr)(-1))
				throw new ArgumentException ("Object contains non-primitive or non-blittable data.");
			if (res == (IntPtr)(-2))
				throw new InvalidOperationException("Handle is not pinned.");
			return res;
		}

		public static System.Runtime.InteropServices.GCHandle Alloc(object value)
		{
			return new GCHandle (value);
		}

		public static System.Runtime.InteropServices.GCHandle Alloc(object value, GCHandleType type)
		{
			return new GCHandle (value,type);
		}

		public void Free()
		{
			// Copy the handle instance member to a local variable. This is required to prevent
			// race conditions releasing the handle.
			IntPtr local_handle = handle;

			// Free the handle if it hasn't already been freed.
			if (local_handle != IntPtr.Zero && Interlocked.CompareExchange (ref handle, IntPtr.Zero, local_handle) == local_handle) {
				FreeHandle (local_handle);
			}
			else {
				throw new InvalidOperationException ("Handle is not initialized.");
			}
		}
		
		public static explicit operator IntPtr (GCHandle value)
		{
			return (IntPtr) value.handle;
		}
		
		public static explicit operator GCHandle(IntPtr value)
		{
			if (value == IntPtr.Zero)
				throw new InvalidOperationException ("GCHandle value cannot be zero");
			if (!CheckCurrentDomain (value))
				throw new ArgumentException ("GCHandle value belongs to a different domain");
			return new GCHandle (value);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static bool CheckCurrentDomain (IntPtr handle);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static object GetTarget(IntPtr handle);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static IntPtr GetTargetHandle(object obj, IntPtr handle, GCHandleType type);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void FreeHandle(IntPtr handle);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static IntPtr GetAddrOfPinnedObject(IntPtr handle);

		public static bool operator ==(GCHandle a, GCHandle b)
		{
			return a.handle == b.handle;
		}

		public static bool operator !=(GCHandle a, GCHandle b)
		{
			return !(a == b);
		}
		
		public override bool Equals(object o)
		{
			return o is GCHandle ? this == (GCHandle)o : false;
		}

		public override int GetHashCode()
		{
			return handle.GetHashCode ();
		}

		public static GCHandle FromIntPtr (IntPtr value)
		{
			return (GCHandle)value;
		}

		public static IntPtr ToIntPtr (GCHandle value)
		{
			return (IntPtr)value;
		}
	} 
}

