using System.Linq;
using System.Collections.Generic;

class MM
{
	public IEnumerable<int> myEnumerable { get; set; }
}

class Test
{
	public static void Main ()
	{
		MM myobject = null;
		(myobject?.myEnumerable?.Any ()).GetValueOrDefault (false);      
	}
}
