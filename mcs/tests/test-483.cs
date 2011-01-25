// Compiler options: -r:test-483-lib.dll

using System;

public class Tests
{
	public static void Main ()
	{
		Bar bar = null;
		try { bar.clone (); } catch (NullReferenceException) {}
	}
}

class B : Bar
{
	public override object clone ()
	{
		return null;
	}
}
