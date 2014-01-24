// Compiler options: -optimize

using System;

class C
{
	static void Main ()
	{
		AddEH<string> ();
	}

	static void AddEH<T>()
	{
		var e = new E<T> ();
		e.EEvent += EHandler;
	}

	static void EHandler ()
	{
	}

	class E<T>
	{
		public delegate void EMethod ();
		public event EMethod EEvent;
	}
}