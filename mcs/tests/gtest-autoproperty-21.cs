// Compiler options: -r:gtest-autoproperty-21-lib.dll

public class Subclass : Base
{
	public override string Value { get; }

	public Subclass ()
	{
		Value = "test";
	}

	public static void Main ()
	{
		new Subclass ();
	}
}