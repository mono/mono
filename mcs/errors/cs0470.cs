// CS0470: Method `C.get_P()' cannot implement interface accessor `I.P.get'
// Line: 11

interface I
{
	int P { get; }
}

class C : I
{
	public int get_P ()
	{
		return 1;
	}
}