//
// System.Runtime.InteropServices/GCHandle.cs
//
// Authors:
//   Ajay kumar Dwivedi (adwiv@yahoo.com) ??
//   Paolo Molaro (lupus@ximian.com)
//

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Runtime.InteropServices
{
	/// <summary>
	/// Summary description for GCHandle.
	/// </summary>
	public struct GCHandle 
	{
		// fields
		private int handle;

		private GCHandle(IntPtr h)
		{
			handle = (int)h;
		}
		
		// Constructors
		private GCHandle(object obj)
			: this(obj, GCHandleType.Normal)
		{}

		private GCHandle(object value, GCHandleType type)
		{
			handle = GetTargetHandle (value, 0, type);
		}

		// Properties

		public bool IsAllocated 
		{ 
			get
			{
				return (handle != 0);
			}
		}

		public object Target
		{ 
			get
			{
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
			if (res == IntPtr.Zero)
				throw new InvalidOperationException("The handle is not of Pinned type");
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
			FreeHandle(handle);
			handle = 0;
		}
		
		public static explicit operator IntPtr (GCHandle value)
		{
			return (IntPtr) value.handle;
		}
		
		public static explicit operator GCHandle(IntPtr value)
		{
			return new GCHandle (value);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static object GetTarget(int handle);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static int GetTargetHandle(object obj, int handle, GCHandleType type);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void FreeHandle(int handle);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static IntPtr GetAddrOfPinnedObject(int handle);
	} 
}

