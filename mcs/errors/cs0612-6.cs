// CS0612: `E.GetEnumerator()' is obsolete
// Line: 22
// Compiler options: -warnaserror

using System.Collections;
using System;

class E : IEnumerable
{
	[Obsolete]
	public IEnumerator GetEnumerator ()
	{
		throw new System.NotImplementedException ();
	}
}

class C
{
	public static void Main ()
	{
		var e = new E ();
		foreach (var entry in e) {
		}
	}
}