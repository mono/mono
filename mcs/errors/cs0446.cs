// CS0446: Foreach statement cannot operate on a `method group'
// Line: 8

class C
{
	static void M ()
	{
		foreach (int i in Test)
		{
		}
	}

	static void Test () { }
}
