//
// This was a bug which was triggered because I removed a routine
// inadvertently.   The routine was restored, and now the scopes
// are initialized
//
using System;
using System.Collections;
using System.Reflection;

public class CustomDict {
	ArrayList data;

	public CustomDict() { 
		foreach (object o in this)
			Console.WriteLine (o);
	}

	public IEnumerator GetEnumerator() {
		if (data != null)
			yield return 1;
	}
}

public class Tests
{

	public static void Main () {
		new CustomDict ();
	}
}
