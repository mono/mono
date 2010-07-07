using System;

public class A
{
	public static int Counter;
	public static implicit operator string (A c)
	{
		++Counter;
		return "A-class";
	}
		
	public static implicit operator Delegate (A c)
	{
		return null;
	}
}
	
public struct B
{
	public static int Counter;
	public static implicit operator string (B c)
	{
		++Counter;
		return "B-struct";
	}
}

public struct D
{
	public static int Counter;
	public static implicit operator Delegate (D d)
	{
		++Counter;
		return null;
	}
}

public struct E
{
	public static int Counter;
	public static implicit operator bool (E d)
	{
		++Counter;
		return true;
	}
}

class Program
{	
	public static int Main ()
	{
		if (new B () != new B () || B.Counter != 2)
			return 1;
		
		if (new B () != "B-struct" || B.Counter != 3)
			return 2;
		
		if (new B () == null || B.Counter != 4) {
			// FIXME: Incorrect null lifting
			//return 3;
		}

		if (new D () != new D () || D.Counter != 2)
			return 10;

		if (new D () != null || D.Counter != 3) {
			// FIXME: Incorrect null lifting
			//return 11;
		}
		
		if (new A () != "A-class" || A.Counter != 1)
			return 20;
		
		if (new A () == null  || A.Counter != 1)
			return 21;

		if (new E () != new E ()  || E.Counter != 2)
			return 31;
		
		Console.WriteLine ("ok");
		return 0;
	}
}
