// CS0472: The result of comparing value type `int' with null is `false'
// Line: 8
// Compiler options: -warnaserror -warn:2

public class X {
	public static bool Compute (int x)
	{
		return x == null;
	}
}
