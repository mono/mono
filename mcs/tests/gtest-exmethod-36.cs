using System;

class S
{
	public string Prop { get { return "5"; } }
}

class S2
{
	public bool Prop;
}

static class E
{
	public static int Prop (this S s)
	{
		return 8;
	}
	
	public static int Prop (this S2 s)
	{
		return 18;
	}
}

class C
{
	public static void Main ()
	{
		S s = new S ();
		int b = s.Prop ();
		string bb = s.Prop;
		
		S2 s2 = new S2 ();
		int b2 = s2.Prop ();
		bool bb2 = s2.Prop;
	}
}
