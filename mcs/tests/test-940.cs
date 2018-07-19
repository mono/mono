// Compiler options: -unsafe

using System;

struct nint
{
	public static nint operator * (nint a, nint b)
	{
		return a;
	}

	public static implicit operator long (nint v)
	{
		return 0;
	}
}

class X
{
	public static void Main ()
	{
		nint width;
		nint bytesPerRow;

		unsafe {
			var da = (uint*)0;
			var dp1 = da + width * bytesPerRow;
			var dp2 = width * bytesPerRow + da;
		}
	}
}