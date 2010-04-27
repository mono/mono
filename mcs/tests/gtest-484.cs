using System;

class MainClass
{
	static void Foo (params Action<MainClass>[][] funcs) { }

	static Action<MainClass>[] Set (params Action<MainClass>[] arr)
	{
		return arr;
	}

	static void Bar (MainClass mc) { }

	public static void Main (string[] args)
	{
		Foo (Set (Bar, Bar), Set (Bar, Bar));
	}
}
