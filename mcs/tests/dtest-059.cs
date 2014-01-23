using System;

class X
{
	public static void Main ()
	{
		new C<int> ().Test ();
	}
}

class C<T>
{
	public void Test ()
	{
		dynamic d = null;

		int v;
		int.TryParse (d, out v);

		int.TryParse (d, out v);
	}
}