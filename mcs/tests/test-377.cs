// Compiler options: -r:test-377-lib.dll

using System;
using System.Reflection;

public class Tests {

	public void test () {
		Foo f = null;
		f.foo (5);
	}
	
	public static void Main () {}
}
