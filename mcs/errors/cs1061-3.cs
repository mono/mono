// CS1061: Type `object' does not contain a definition for `Test' and no extension method `Test' of type `object' could be found (are you missing a using directive or an assembly reference?)
// Line: 17

public class S
{
	public static void Test()
	{
	}
}

public class M
{
	public object S { get; set; }

	public void Main ()
	{
		S.Test ();
	}
}
