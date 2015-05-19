// Compiler options: -langversion:experimental
using System;

struct S1
{
	public readonly int Value;
	
	public S1 ()
	{
		Value = 17;
	}
}

struct S2
{
	public readonly int Value = 23;
}

struct S3
{
	public readonly int Value = 11;
	
	public S3 ()
	{
		Value = 5;
	}
}

struct S4
{
	public readonly int Value = 11;
	
	public S4 (int v)
	{
	}
}

struct S5
{
	public readonly int Value = 7;
	
	public S5 (int v)
		: this ()
	{
		this.Value += v;
	}
}

class C
{
	static int Main ()
	{
		var s = new S1 ();
		if (s.Value != 17)
			return 1;

		var s2 = new S2 ();
		if (s2.Value != 23)
			return 2;

		var s3 = new S3 ();
		if (s3.Value != 5)
			return 3;

		var s4 = new S4 (5);
		if (s4.Value != 11)
			return 4;

		var s5 = new S5 (2);
		if (s5.Value != 9)
			return 5;

		Console.WriteLine ("ok");
		return 0;
	}
}