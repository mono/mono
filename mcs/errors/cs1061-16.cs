// CS1061: Type `A' does not contain a definition for `Extension' and no extension method `Extension' of type `A' could be found. Are you missing an assembly reference?
// Line: 21

public class A
{
}

static class X
{
	public static string Extension (this int a)
	{
		return null;
	}
}

public static class Test
{
	public static void Main ()
	{
		A a = null;
		var x = nameof (a.Extension);
	}
}
