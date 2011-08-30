// Compiler options: -r:gtest-561-lib.dll

using System.Collections.Generic;

class C : A, I
{
	public void Foo<T> (List<T> arg) where T : A
	{
	}
	
	public static void Main ()
	{
		new C ().Foo(new List<A> ());
	}
}