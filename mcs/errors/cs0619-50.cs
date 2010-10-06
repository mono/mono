// cs0619-50.cs: `A.B' is obsolete: `yes'
// Line: 12

using Z = A.B;

class A
{
	[System.Obsolete("yes", true)]
	public class B
	{
	}

	static void Main ()
	{
		Z z;
	}
}