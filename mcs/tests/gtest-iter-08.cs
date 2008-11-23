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
		
		if (TestType<string>().GetType () != typeof (string))
			return 2;
		
		return 0;
	}
	
    public static IEnumerable QueryEnumerable<T> ()
    {
		yield return "Type: " + typeof(T);
    }
    
	public static T TestType<T> ()
	{
		return (T) TestType (typeof(T));
	}

	public static object TestType (Type t)
	{
		return "1";
	}    
}
