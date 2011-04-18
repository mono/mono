// CS0038: Cannot access a nonstatic member of outer type `X' via nested type `X.Y'
// Line: 15
using System;

class X
{
	int a = 5;

	class Y
	{
		public long b;

		public Y ()
		{
			Console.WriteLine (a);
		}
	}

	static void Main ()
	{
	}
}
