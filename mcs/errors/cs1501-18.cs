// CS1501: No overload for method `Bar' takes `2' arguments
// Line: 19

using System;

class T
{
	void Foo (int arg, Action a)
	{
	}

	void Foo (string title, Action a)
	{
	}

	void Bar ()
	{
		Foo (arg: 1, a: () => {
			Bar ("a", "b");
		});
	}
}