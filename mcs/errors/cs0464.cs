// CS0464: The result of comparing type `int?' with null is always `false'
// Line: 9
// Compiler options: -warnaserror -warn:2

public class X
{
	public static bool Compute (int x)
	{
		return x < null;
	}
}
