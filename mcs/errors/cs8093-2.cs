// CS8093: An argument to nameof operator cannot be extension method group
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
