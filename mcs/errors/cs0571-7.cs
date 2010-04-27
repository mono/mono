// CS0571: `C.Prop.set': cannot explicitly call operator or accessor
// Line: 14

class C
{
	delegate void D (int i);

	static int Prop {
		set {}
	}
	
	public static void Main ()
	{
		D d = set_Prop;
	}
}
