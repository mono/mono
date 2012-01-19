using System;
using System.Linq;

class C
{
	public static int Main ()
	{
		var r = from m in "ab"
				let n = from n in "xyz" select n
				select n;
		
		int counter = 0;
		foreach (var a in r) {
			foreach (var b in a) {
				Console.WriteLine (b);
				counter++;
			}
		}
		
		if (counter != 6)
			return 1;
		
		return 0;
	}
}