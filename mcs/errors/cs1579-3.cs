// CS1579: foreach statement cannot operate on variables of type `C' because it does not contain a definition for `GetEnumerator' or is inaccessible
// Line: 37

using System;

public class Enumerator
{
	public bool MoveNext ()
	{
		return false;
	}

	public int Current { get; set; }
}


public class Base
{
	public Enumerator GetEnumerator ()
	{
		return new Enumerator ();
	}
}

public class C : Base
{
	new internal Enumerator GetEnumerator ()
	{
		return new Enumerator ();
	}
}

class Test
{
	public static void Main ()
	{
		foreach (var e in new C ())
			Console.WriteLine (e);
	}
}
