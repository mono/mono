// CS0165: Use of unassigned local variable `x'
// Line: 19

struct S
{
	public object O;
}

class X
{
	public S s;
}

class C
{
	public static void Main ()
	{
		X x;
		x.s.O = 2;
	}
}