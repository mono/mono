// Compiler options: -t:library

public struct S
{
}

public enum E
{
	Value = 3
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

	public static ulong TestNew (ulong s = new ulong ())
	{
		return s;
	}

	public static decimal TestDecimal (int i, decimal d = decimal.MinValue)
	{
		return d;
	}
	
	public static E TestEnum (E e = E.Value)
	{
		return e;
	}
	
	byte ch;
	public byte this [int id, byte v = 1+5] {
		get { return v; }
		set { ch = value; } 
	}
}
