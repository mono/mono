// Compiler options: -t:library

public class A
{
	public override int GetHashCode ()
	{
		return 1;
	}
	
	public override bool Equals (object o)
	{
		return true;
	}
	
	public static bool operator == (A u1, A u2)
	{
		return true;
	}

	public static bool operator != (A u1, A u2)
	{
		return false;
	}
}
