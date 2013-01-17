using System.Collections;
using System.Collections.Generic;

class C
{
	public static void Main ()
	{
		
	}
	
	IEnumerable<int> Iter_1()
	{
		yield return 1;
	}
	
	IEnumerable Iter_2()
	{
		yield break;
	}
}