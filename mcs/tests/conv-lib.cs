// Compiler options: -t:library

public class A
{
	public static bool operator==(A a1, A a2)
	{
		return true;
	}

	public static bool operator!=(A a1, A a2)
	{
		return false;
	}

	public override bool Equals (object o)
	{
		return true;
	}

	public override int GetHashCode ()
	{
		return base.GetHashCode ();
	}

	public int KK () { return 1; }
}

public class B : A {
}

