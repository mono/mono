using System;
using System.Collections.Generic;

class C
{
	string Name;
	int value;
	
	public static void Main ()
	{
	}
	
	void Test_1 ()
	{
		var o = new Dictionary<string, int> ()
		{
			{
				"Foo", 3
			},
			{
				"Bar", 1
			},
		};
	}
	
	void Test_2 ()
	{
		var user = new C()
		{
			Name = "nn",
			value = 8
		};
	}
}
