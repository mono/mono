using System;

struct S1
{
	public static implicit operator int (S1? s)
	{
		return 1;
	}
}

struct S2
{
	public static implicit operator int? (S2? s)
	{
		return null;
	}
}

struct S3
{
	public static implicit operator int? (S3? s)
	{
		return 2;
	}
}

struct S4
{
	public static implicit operator int? (S4 s)
	{
		return 3;
	}
}

class C
{
	public static int Main ()
	{
		S1? s1 = new S1 ();
		switch (s1) {
		case 1:
			break;
		default:
			return 1;
		}

		S2? s2 = new S2 ();
		switch (s2) {
		case null:
			break;
		default:
			return 2;
		}

		S3? s3 = new S3 ();
		switch (s3) {
		case 2:
			break;
		default:
			return 3;
		}

		S4 s4 = new S4 ();
		switch (s4) {
		case 3:
			break;
		default:
			return 4;
		}

		return 0;
	}
}