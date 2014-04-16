// CS0165: Use of unassigned local variable `s'
// Line: 9

class Program
{
	static void Main ()
	{
		S s;
		s.Test ();
	}
}

struct S
{
	public string pp;
	
	public void Test ()
	{
	}
}