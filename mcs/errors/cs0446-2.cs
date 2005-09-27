// cs0446.cs: Foreach statement cannot operate on a `anonymous method'
// Line: 8

class C
{
	static void M ()
	{
		foreach (int i in delegate { } )
		{
		}
	}
}
