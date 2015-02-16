using static System.String;
using static S;

struct S
{
	internal static int Foo ()
	{
		return 5;
	}
}

class Test
{
    public static int Main ()
    {
		string res = Concat ("a", "b", "c");
		if (res != "abc")
			return 1;

		if (Foo () != 5)
			return 2;

		return 0;
    }
}