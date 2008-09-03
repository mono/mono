using System;
using System.Collections;

class App
{
	public static int Main ()
	{
		foreach (object o in QueryEnumerable<string>()) {
			if ((string)o != "Type: System.String")
				return 1;
		}
		
		return 0;
	}
	
    public static IEnumerable QueryEnumerable<T> ()
    {
		Console.WriteLine ("Type: " + typeof(T));
		yield break;
    }
}
