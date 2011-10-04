using System;

struct S2
{
	public float f1;
}

struct S
{
	public S2 s2;
	public float F;
}

class C
{
	static void Test (bool b, out S s)
	{
		if (b) {
			s.s2 = new S2 ();
			s.F = 1.0f;
		} else {
			s.s2.f1 = 2.1f;
			s.F = 1.0f;
		}
	}
	
	public static int Main ()
	{
		S s;
		Test (true, out s);
		Test (false, out s);
		return 0;
	}
}
