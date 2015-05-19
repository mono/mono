struct S
{
	public static bool operator true (S? S)
	{
		return true;
	}

	public static bool operator false (S? S)
	{
		return true;
	}
}

class P
{
	static void Main ()
	{
		if (new S? ()) {
		}
	}
}