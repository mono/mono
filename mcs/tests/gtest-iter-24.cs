using System;
using System.Collections.Generic;

public class B : IDisposable
{
	public void Dispose ()
	{
	}

	public void DoSomething ()
	{
	}
}

public class C
{
	public static IEnumerable<int> Test ()
	{
		using (var b = new B ()) {
			Action a = () => b.DoSomething ();
			a ();

			yield return 1;
		}
	}
	
	public static int Main ()
	{
		foreach (var e in Test ()) {
			Console.WriteLine (e);
		}
		
		return 0;
	}
}
