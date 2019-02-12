namespace System.Runtime.InteropServices
{
	partial struct GCHandle
	{
		static IntPtr InternalAlloc (object value, GCHandleType type) => throw new NotImplementedException ();

		static void InternalFree (IntPtr handle) => throw new NotImplementedException ();

		static object InternalGet (IntPtr handle) => throw new NotImplementedException ();

		static void InternalSet (IntPtr handle, object value) => throw new NotImplementedException ();
	}
}