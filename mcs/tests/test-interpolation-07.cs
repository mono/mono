using System;

class CloningTests
{
	static Action a;

	static void Do (Action cb)
	{
		cb ();
	}

	static void SetupBAD ()
	{
		int number = 0;
		Do(() => {
			a = () => Console.WriteLine ($"Number: {++number}");
		});
	}

	static void Main ()
	{
		SetupBAD ();
		a ();
	}
}