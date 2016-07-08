// Compiler options: /noconfig /nostdlib -r:../class/lib/net_4_x/Facades/System.Runtime.dll -r:../class/lib/net_4_x/mscorlib.dll

using System;

class TypeForwarderOfSystemObject
{
	void TestAttributeReadDoesNotCrash ()
	{
		System.Runtime.InteropServices.Marshal.ReadByte (IntPtr.Zero, 0);		
	}

	static void Main ()
	{
	}
}
