namespace System.Runtime.CompilerServices
{
	internal struct Ephemeron
	{
		internal object key;
		internal object value;
	}

	//
	// Instead of dependent handles, mono uses arrays of Ephemeron objects.
	//
	readonly struct DependentHandle
	{
		Ephemeron[] data;

		public DependentHandle (object primary, object secondary)
		{
			handle = IntPtr.Zero;

			data = new Ephemeron [1];
			data [0].key = primary;
			data [0].value = secondary;
			GC.register_ephemeron_array (data);
		}

		public bool IsAllocated => data != null;

		// Getting the secondary object is more expensive than getting the first so
		// we provide a separate primary-only accessor for those times we only want the
		// primary.
		public object GetPrimary () {
			if (!IsAllocated)
				throw new NotSupportedException ();
			if (data [0].key == GC.EPHEMERON_TOMBSTONE)
				return null;
			return data [0].key;
		}

		public object GetPrimaryAndSecondary (out object secondary) {
			if (!IsAllocated)
				throw new NotSupportedException ();
			if (data [0].key == GC.EPHEMERON_TOMBSTONE) {
				secondary = null;
				return null;
			}
			secondary = data [0].value;
			return data [0].key;
		}

		public void SetPrimary (object primary) {
			if (!IsAllocated)
				throw new NotSupportedException ();
			data [0].key = primary;
		}

		public void SetSecondary (object secondary) {
			if (!IsAllocated)
				throw new NotSupportedException ();
			data [0].value = secondary;
		}

		public void Free () {
			data = null;
		}
	}
}