using System;
using System.Collections.Generic;

class MyDisposable : IDisposable
{
	static int next_id;
	int id = ++next_id;

	public void Dispose ()
	{ }

	public int ID {
		get { return id; }
	}

	public override string ToString ()
	{
		return String.Format ("{0} ({1})", GetType (), id);
	}
}

class X
{
	public static IEnumerable<int> Test (int a)
	{
		MyDisposable d;
		using (d = new MyDisposable ()) {
			yield return a;
			yield return d.ID;
		}
	}

	static void Main ()
	{
		foreach (int a in Test (5))
			Console.WriteLine (a);
	}
}
