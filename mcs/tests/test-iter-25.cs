using System;
using System.Collections.Generic;

public class D : IDisposable
{
	public D (string bar)
	{
	}

	public void Dispose ()
	{
	}
}

public class UploadAction
{
	public static void RunOnThread (Action a)
	{
		a.Invoke ();
	}

	public static IEnumerable<object> TagsError ()
	{
		string tags;
		tags = "";

		RunOnThread (() => {
			using (D u = new D (tags)) {
				Console.WriteLine ("No Op");
			}
		});

		yield break;
	}

	static void Main ()
	{
		foreach (object bar in TagsError ()) {
			Console.WriteLine ("No op {0}", bar);
		}
	}
}
