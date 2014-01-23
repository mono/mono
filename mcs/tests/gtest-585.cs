using System;

struct S
{
	public static implicit operator int (S arg)
	{
		throw new ApplicationException ();
	}
}

struct S2
{
	public static implicit operator int?(S2 arg)
	{
		return 10000;
	}

	public static implicit operator uint?(S2 arg)
	{
		throw new ApplicationException ();
	}
}

public struct S3
{
	public static int counter;
	
	public static implicit operator string (S3 s3)
	{
		counter++;
		return "";
	}
}

class C
{
	public static int Main ()
	{
		S? s = null;
		bool res = s > 1;
		if (res)
			return 1;

		S2 s2 = new S2 ();

		var b = s2 >> 3;
		if (b != 1250)
			return 2;

		var b2 = s2 >> s2;
		if (b2 != 0)
			return 3;

		var b3 = s2 + 1;
		if (b3 != 10001)
			return 4;

		var s3 = new S3 ();
		if ((s3 == null) != false)
			return 5;

		if ((s3 != null) != true)
			return 6;
		
		if (S3.counter != 2)
			return 7;

		Console.WriteLine ("ok");
		return 0;
	}
}