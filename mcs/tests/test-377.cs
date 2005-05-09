// Compiler options: -t:library -r:test-377-PROFILE-il.dll

using System;
using System.Reflection;

public class Tests {

	public void test () {
		Foo f = null;
		f.foo (5);
	}
}
