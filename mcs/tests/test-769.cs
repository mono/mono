using System;
using System.Reflection;

public interface I
{
	void Clear ();
}

public class C : I
{
	public void Clear () { }
	void I.Clear () { }

	public static int Main ()
	{
		var m1 = typeof (C).GetMethod ("Clear");
		Console.WriteLine (m1.Attributes);
		if (m1.Attributes != (MethodAttributes.Public | MethodAttributes.HideBySig))
			return 1;

		var m2 = typeof (C).GetMethod ("I.Clear", BindingFlags.NonPublic | BindingFlags.Instance);
		Console.WriteLine (m2.Attributes);
		if (m2.Attributes != (MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.VtableLayoutMask))
			return 2;

		return 0;
	}
}
