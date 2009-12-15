// CS0172: Type of conditional expression cannot be determined as `byte' and `int' convert implicitly to each other
// Line: 9

public class Tester
{
	public static void Main ()
	{
		byte x = 4;
		var a = true ? x : 0;
	}
}
