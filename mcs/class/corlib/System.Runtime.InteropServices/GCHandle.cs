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
		private IntPtr handle;
		private GCHandleType handleType;

		// Constructors
		private GCHandle(object obj)
			: this(obj, GCHandleType.Normal)
		{}

		private GCHandle(object value, GCHandleType type)
		{
			handle = IntPtr.Zero;
			handleType = type;
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
				return GetTarget(handle);
			} 
			set
			{
				SetTarget(handle,value);
			} 
		}

		// Methods
		public IntPtr AddrOfPinnedObject()
		{
			if(this.handleType == System.Runtime.InteropServices.GCHandleType.Pinned)
			{
				throw new InvalidOperationException("The handle is not of Pinned type");
			}
			return GetAddrOfPinnedObject();
		}

		public static System.Runtime.InteropServices.GCHandle Alloc(object value)
		{
			return new GCHandle(value);
		}

		public static System.Runtime.InteropServices.GCHandle Alloc(object value, GCHandleType type)
		{
			return new GCHandle(value,type);
		}

		public void Free()
		{
			FreeHandle(handle);
			handle = IntPtr.Zero;
		}
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static explicit operator IntPtr(GCHandle value);
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static explicit operator GCHandle(IntPtr value);

		//TODO: Private Native Functions
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern object GetTarget(IntPtr pointer);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern void SetTarget(IntPtr pointer,object obj);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern void FreeHandle(IntPtr pointer);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern IntPtr GetAddrOfPinnedObject();
	} 
}