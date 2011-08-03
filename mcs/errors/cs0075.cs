// CS0075: To cast a negative value, you must enclose the value in parentheses
// Line: 20
class X
{
	public readonly int i;

	public X (int i)
	{
		this.i = i;
	}

	public static implicit operator X (int value)
	{
		return new X (value);
	}

	public static void Main ()
	{
		int a = 4, b = 5;
		X x = (X) -a;
		System.Console.WriteLine (x.i);
	}
}
