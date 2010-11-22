// Compiler options: -unsafe -r:gtest-fixedbuffer-01-lib.dll

// Fixed buffers tests

using System;
using System.Runtime.InteropServices;

[module: DefaultCharSet (CharSet.Ansi)]

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public unsafe struct TestNew {
	private fixed char test_1 [128];
	public fixed bool test2 [4];
	
	public fixed int T [2];
	public fixed bool test20 [4], test21 [40];
	
	private int foo, foo2;
	public void SetTest () {
		fixed (char* c = test_1) {
			*c = 'g';
		}
	}
}

struct Struct2 {
	public unsafe fixed byte Pad[64];
}

public class C {
	unsafe static int Test () {
		TestNew tt = new TestNew ();
		tt.SetTest ();
		tt.test2 [2] = false;
		tt.T [1] = 5544;
		if (tt.T [1] != 5544)
			return 2;
	
		ExternalStruct es = new ExternalStruct ();
		es.double_buffer [1] = 999999.8888;
		es.double_buffer [0] = es.double_buffer [1];

		// Attributes test
		if (Attribute.GetCustomAttribute (typeof (TestNew).GetField ("test2"), typeof (System.Runtime.CompilerServices.FixedBufferAttribute)) == null)
			return 3;

		
		if (typeof (TestNew).GetNestedTypes ().Length != 5)
			return 5;

		foreach (Type t in typeof (TestNew).GetNestedTypes ()) {
			if (Attribute.GetCustomAttribute (t, typeof (System.Runtime.CompilerServices.CompilerGeneratedAttribute)) == null)
				return 40;
				
			if (Attribute.GetCustomAttribute (t, typeof (System.Runtime.CompilerServices.UnsafeValueTypeAttribute)) == null)
				return 41;
				
			if (!t.IsUnicodeClass)
				return 42;
		}

		Console.WriteLine ("OK");
		return 0;
	}
    
	public static int Main () {
		return Test ();
	}
}
