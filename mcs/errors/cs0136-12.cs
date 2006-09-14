// CS0136: A local variable named `t' cannot be declared in this scope because it would give a different meaning to `t', which is already used in a `child' scope to denote something else
// Line: 20
using System;

public delegate void Hello (Test test);

public class Test
{
	public void Whatever ()
	{ }

	static void Main ()
	{
		Hello hello = delegate (Test test) {
			Hello hello2 = delegate (Test t) {
				t.Whatever ();
			};
			hello2 (test);
		};
		Test t = new Test ();
		hello (t);
	}
}
