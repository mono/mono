using System;

public class GetElementTypeTest {
	public static int Main (string[] args) {
		GetElementTypeTest me = new GetElementTypeTest ();
		Type t = me.GetType ();
		Type elementt = t.GetElementType ();

		if (elementt != null)
			return 1;
		return 0;
	}
}

