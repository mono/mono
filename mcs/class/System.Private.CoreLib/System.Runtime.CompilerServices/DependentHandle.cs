namespace System.Runtime.CompilerServices
{
	//=========================================================================================
	// This struct collects all operations on native DependentHandles. The DependentHandle
	// merely wraps an IntPtr so this struct serves mainly as a "managed typedef."
	//
	// DependentHandles exist in one of two states:
	//
	//    IsAllocated == false
	//        No actual handle is allocated underneath. Illegal to call GetPrimary
	//        or GetPrimaryAndSecondary(). Ok to call Free().
	//
	//        Initializing a DependentHandle using the nullary ctor creates a DependentHandle
	//        that's in the !IsAllocated state.
	//        (! Right now, we get this guarantee for free because (IntPtr)0 == NULL unmanaged handle.
	//         ! If that assertion ever becomes false, we'll have to add an _isAllocated field
	//         ! to compensate.)
	//        
	//
	//    IsAllocated == true
	//        There's a handle allocated underneath. You must call Free() on this eventually
	//        or you cause a native handle table leak.
	//
	// This struct intentionally does no self-synchronization. It's up to the caller to
	// to use DependentHandles in a thread-safe way.
	//=========================================================================================
	struct DependentHandle
	{
		IntPtr handle;

		public DependentHandle (object primary, object secondary)
		{
			handle = IntPtr.Zero;
		}

		public bool IsAllocated => handle != IntPtr.Zero;

		// Getting the secondary object is more expensive than getting the first so
		// we provide a separate primary-only accessor for those times we only want the
		// primary.
		public object GetPrimary () => throw new NotImplementedException ();

		public object GetPrimaryAndSecondary (out object secondary) => throw new NotImplementedException ();

		public void SetPrimary (object primary) => throw new NotImplementedException ();

		public void SetSecondary (object secondary) => throw new NotImplementedException ();

		public void Free()
		{
			if (handle != IntPtr.Zero)
			{
				IntPtr _handle = handle;
				handle = IntPtr.Zero;
				FreeHandle (_handle);
			}
		}

		static void FreeHandle (IntPtr handle)
		{
		}
	}
}