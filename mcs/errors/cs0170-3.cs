// CS0170: Use of possibly unassigned field `p'
// Line: 21

using System;

struct S2
{
	public int p;
}

struct S
{
	public S2 s2;
}

class C
{
    static void Main()
    {
		S s;
		Console.WriteLine (s.s2.p);
    }
}
