﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace WebAssembly {

	public abstract class AnyRef : SafeHandleMinusOneIsInvalid {

		public int JSHandle {
			get => (int)handle;
		}

		private AnyRef () : base (true) { }

		internal GCHandle AnyRefHandle;

		internal AnyRef (int js_handle, bool ownsHandle) : base(ownsHandle)
		{
			SetHandle ((IntPtr)js_handle);
			this.AnyRefHandle = GCHandle.Alloc (this, ownsHandle ? GCHandleType.Weak : GCHandleType.Normal);
		}

		internal AnyRef (IntPtr js_handle, bool ownsHandle) : base(ownsHandle)
		{
			SetHandle (js_handle);
			this.AnyRefHandle = GCHandle.Alloc (this, ownsHandle ? GCHandleType.Weak : GCHandleType.Normal);
		}

		// We should not provide a finalizer - SafeHandle's critical finalizer will call ReleaseHandle inside a CER for us.
		override protected bool ReleaseHandle () => throw new NotImplementedException ();

#if DEBUG_HANDLE

		private int _refCount = 0;

		public void AddRef ()
		{
			Interlocked.Increment (ref _refCount);
		}

		public void Release ()
		{
			Interlocked.MemoryBarrier ();
			Debug.Assert (_refCount > 0, "AnyRefSafeHandle: Release() called more times than AddRef");
			Interlocked.Decrement (ref _refCount);
		}

		public int RefCount => _refCount;

#endif


	}
}
