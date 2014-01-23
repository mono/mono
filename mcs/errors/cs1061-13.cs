// CS1061: Type `string' does not contain a definition for `Where' and no extension method `Where' of type `string' could be found. Are you missing `System.Linq' using directive?
// Line: 8

public class M
{
	public static void Main ()
	{
		var a = "ababab".Where (l => l == 'b');
	}
}