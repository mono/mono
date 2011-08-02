// CS0619-50: `A.B' is obsolete: `yes'
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