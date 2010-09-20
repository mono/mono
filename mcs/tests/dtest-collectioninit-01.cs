using System;
using System.Collections;
using System.Collections.Generic;

public class Test
{
	class Wrap
	{
		List<short> numbers = new List<short> ();
		
		public dynamic Numbers { 
			get { 
				return numbers;
			}
		}
	}
	
	static int Main ()
	{
		var a = new Wrap () {
			Numbers =  { 3, 9 }
		};
		
		if (a.Numbers [1] != 9)
			return 1;
		
		Console.WriteLine ("OK");
		return 0;
	}
}

