// CS1501: No overload for method `Bar' takes `2' arguments
// Line: 25

using System;

class T
{
	void Foo (int arg, Action a)
	{
	}

	void Foo (string title, Action a)
	{
	}

	static void Mismatch (string s)
	{
	}

	public static void Main ()
	{
	}

	void Bar ()
	{
		Foo (arg: 1, a: () => {
			Bar ("a", "b");
		});
	}
}