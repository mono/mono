// CS1935: An implementation of `Select' query expression pattern could not be found. Are you missing `System.Linq' using directive or `System.Core.dll' assembly reference?
// Line: 10


public class Test
{
	static void Main ()
	{
		var v = new int[0];
		var foo = from a in v select a;
	}
}
