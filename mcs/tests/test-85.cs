//
// This test declares a field variable called `UnmanagedType' of type
// UnmanagedType.
//
// The test is used to test the cast of 0 to UnmanagedType, as before
// that would have been resolved to a variable instead of a type.
//
using System.Runtime.InteropServices;

class X {
	static UnmanagedType UnmanagedType;

	public static int Main ()
	{
		UnmanagedType = (UnmanagedType) 0;

		if (UnmanagedType != 0)
			return 1;

		return 0;
	}
}

