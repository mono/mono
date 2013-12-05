// CS0165: Use of unassigned local variable `j'
// Line: 10

class Test
{
	static void Main ()
	{
		int? i;
		int? j;
		int? x = (i = 7) ?? j;
    }
}