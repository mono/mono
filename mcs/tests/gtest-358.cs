// Tests broken equality and inequality operators

using System;

struct Foo
{
	public static bool operator == (Foo d1, Foo d2)
	{
		throw new ApplicationException ();
	}
		
	public static bool operator != (Foo d1, Foo d2)
	{
		throw new ApplicationException ();	
	}
}

struct S2
{
	public static bool operator == (S2 d1, S2? d2)
	{
		throw new ApplicationException ();
	}
		
	public static bool operator != (S2 d1, S2? d2)
	{
		throw new ApplicationException ();	
	}
}

public struct S3
{
	public static decimal operator != (S3 a, object b)
	{
		return -1;
	}
	
	public static decimal operator == (S3 a, object b)
	{
		return 1;
	}
}


public class Test
{
	static Foo ctx;
	static S2? s2;
	static S3? s3;

	public static int Main ()
	{
		if (ctx == null)
			return 1;
		
		bool b = ctx != null;
		if (!b)
			return 2;
		
		if (s2 != null)
			return 3;
		
		s3 = new S3 ();
		decimal d = s3.Value == null;
		if (d != 1)
			return 4;
		
		Console.WriteLine ("ok");
		return 0;
	}
}
