// CS0158: The label `Foo' shadows another label by the same name in a contained scope
// Line: 17
using System;

public delegate void Hello (Test test);

public class Test
{
	public void Whatever ()
	{ }

	static void RunIt (Test t)
	{
	Foo:
		Hello hello = delegate (Test test) {
			Hello hello2 = delegate (Test test2) {
				Foo:
				test2.Whatever ();
			};
			hello2 (test);
		};
		hello (t);
	}

	static void Main ()
	{
		Test t = new Test ();
		RunIt (t);
	}
}
