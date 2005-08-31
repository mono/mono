using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

public class Dict : Dictionary <object, object>
{
}

public class Foo
{
	public static int Main ()
	{
		IDictionary<object, object> dict = new Dict ();

		dict.Add (new Object (), new Object ());
		foreach (object kv in dict) {
			Type t = kv.GetType ();
			if (t.IsGenericType)
				return 0;
			else
				return 1;
		}
		return 2;
	}
}
