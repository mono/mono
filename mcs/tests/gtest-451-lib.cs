// Compiler options: -t:library -langversion:future

public struct S
{
}

public class B
{
	public static string TestString (string s = "mono")
	{
		return s;
	}

	public static B TestB (B b = null)
	{
		return b;
	}

	public static T Test<T> (T t = default (T))
	{
		return t;
	}

	public static S TestNew (S s = new S ())
	{
		return s;
	}

	public static decimal TestDecimal (int i, decimal d = decimal.MinValue)
	{
		return d;
	}
	
	char ch;
	public char this [int id, char v = 'h'] {
		get { return v; }
		set { ch = value; } 
	}
}
