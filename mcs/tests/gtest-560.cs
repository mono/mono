struct G<T>
{
	public static G<T> s;

	private G (int i)
	{
	}
}

struct S
{
	private G<string> value;

	public static void Main ()
	{
	}
}
