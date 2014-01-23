class MyString
{
	public static implicit operator string (MyString s)
	{
		return "ggtt";
	}
}

public class Test
{
	public static int Main ()
	{
		var v = new [] { new MyString (), "a" };
		if (v [0] != "ggtt")
			return 1;
		return 0;
	}
}
