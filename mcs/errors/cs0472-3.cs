// CS0472: The result of comparing value type `int' with null is always `true'
// Line: 9
// Compiler options: -warnaserror -warn:2

public class X
{
	public static void Compute (int x)
	{
		if (true && x != null)
			return;
	}
}
