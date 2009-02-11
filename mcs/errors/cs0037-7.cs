// CS0037: Cannot convert null to `char' because it is a value type
// Line: 12

class C
{
	static void Test ()
	{
		char c = 'c';
		switch (c)
		{
			case 'a': 
				goto case null;
		}
	}
}
