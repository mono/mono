using System;
using System.Reflection;

public class Test
{
	public string Property1 { get; }

	public int Property2 { get; }

	public static int Main ()
	{
		var t = new Test ();
		if (t.Property1 != null)
			return 1;

		if (t.Property2 != 0)
			return 2;

		var fields = typeof (Test).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
		if (fields.Length != 2)
			return 3;

		foreach (var fi in fields) {
			if ((fi.Attributes & FieldAttributes.InitOnly) == 0)
				return 4;
		}

		return 0;
	}	
}
