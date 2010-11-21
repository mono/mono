// Compiler options: -addmodule:test-418-2-mod.netmodule -addmodule:test-418-3-mod.netmodule

using System;

public class M3 : M1 {

	public M3 () : base ("FOO") {
	}

	public static int Main () {
		if (new M3 ().Foo != "FOO")
			return 1;
			
		/* Test that the EXPORTEDTYPES table is correctly set up */
		// It should be 3 but we add 2 compiler generated <$AssemblyAttributes$....>
		if (typeof (M3).Assembly.GetTypes ().Length != 3 + 2)
			return 2;

		if (typeof (M3).Assembly.GetType ("M2") == null)
			return 3;

		if (typeof (M3).Assembly.GetType ("M2") != typeof (M2))
			return 4;

		return 0;
	}
}
