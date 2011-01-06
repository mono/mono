// CS1935: An implementation of `Cast' query expression pattern could not be found. Are you missing `System.Linq' using directive or `System.Core.dll' assembly reference?
// Line: 12


public class Test
{
	class Enumerable {}
		
	static void Main ()
	{
		var v = new int[0];
		var foo = from int a in v where a > 0 select a;
	}
}
