// Compiler options: -r:test-410-lib.dll

using System;
using Q;

public class B {
	public static int Main() {
		return (A.ToString() == "Hello world!") ? 0 : 1;
	}
}
