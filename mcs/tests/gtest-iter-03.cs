using System;
using System.Collections.Generic;

public class Test {

	List<object> annotations = new List<object> ();

	public IEnumerable<T> Annotations<T> () where T : class
	{
		foreach (T o in Annotations (typeof (T)))
			yield return o;
	}

	public IEnumerable<object> Annotations (Type type)
	{
		if (annotations == null)
			yield break;
		foreach (object o in annotations)
			if (o.GetType () == type)
				yield return o;
	}
	
	public static void Main ()
	{
		var test = new Test ();
		test.Annotations<Test> ();
	}
}
