// CS0197: Passing `T.bar' as ref or out or taking its address may cause a runtime exception because it is a field of a marshal-by-reference class
// Line: 15
// Compiler options: -warnaserror -warn:1

using System;
class T : MarshalByRefObject {
	int bar;

	static void Foo (ref int i)
	{
	}

	static void Main()
	{
		T t = new T ();
		t.bar = 12;
		Foo (ref t.bar);
	}
}






