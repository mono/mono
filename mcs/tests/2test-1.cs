using System;
using System.Collections;

class X {
	static IEnumerator GetIt ()
	{
		Console.WriteLine ("test");
#if __V2__
		yield 1;
		yield 2;
		yield 3;
#else
		return null;
#endif
	}
	
	static void Main ()
	{
		IEnumerator e = GetIt ();
		while (e.MoveNext ()){
			Console.WriteLine ("Value=" + e.Current);
		}
		
	}
}
