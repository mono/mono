//cs0197.cs: Can not pass fields of a MarshalByRefObject by ref or out
// Line: 15
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






