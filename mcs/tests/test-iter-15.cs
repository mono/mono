using System;
using System.Collections;

public class Test
{
	public IEnumerator GetEnumerator ()
	{
		yield return "TEST";
		try {
			int.Parse (arg);
		} catch {
			yield break;
		}
		yield return "TEST2";
	}

	public static void Main ()
	{
		new Test ().Run ();
	}

	string arg;

	void Run ()
	{
		int i = 0;
		foreach (string s in this)
			i++;
		if (i != 1)
			throw new Exception ();

		arg = "1";
		i = 0;
		foreach (string s in this)
			i++;
		if (i != 2)
			throw new Exception ();
	}
}


