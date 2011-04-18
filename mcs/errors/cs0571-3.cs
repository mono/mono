// CS0571: `Test.C0.foo.get': cannot explicitly call operator or accessor
// Line: 12
// Compiler options: -r:CS0571-3-lib.dll

// Testcase for bug #59980

using Test;

public class EntryPoint {
        public static int Main () {
		C1 foo = new C2 ();
                return foo.get_foo ();
        }
}

