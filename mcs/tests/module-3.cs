// Compiler options: -addmodule:module-1.netmodule -addmodule:module-2.netmodule

using System;

public class M3 : M1 {

	public M3 () : base ("FOO") {
	}

	public static int Main () {
		if (new M3 ().Foo != "FOO")
			return 1;
		/* Test that the EXPORTEDTYPES table is correctly set up */
		if (typeof (M3).Assembly.GetTypes ().Length != 3)
			return 2;
		if (typeof (M3).Assembly.GetType ("M2") == null)
			return 3;
		return 0;
	}
}
