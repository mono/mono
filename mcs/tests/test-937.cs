// Compiler options: /noconfig /nostdlib -r:$REF_DIR/Facades/System.Runtime.dll -r:$REF_DIR/mscorlib.dll

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
