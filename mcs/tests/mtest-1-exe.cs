// Compiler options: -r:mtest-1-dll.dll

using System;
using Q;

public class B {
	public static int Main() {
		return (A.ToString() == "Hello world!") ? 0 : 1;
	}
}
