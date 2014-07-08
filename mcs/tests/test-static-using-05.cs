using System.Console;
using System.Linq.Enumerable;

class Test
{
	static void Main() 
	{
		var range = Range (5, 17);
		WriteLine (range.Where (i => i % 2 == 0).Count ());
	}
}