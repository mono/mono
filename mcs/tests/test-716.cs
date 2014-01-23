using System;
using System.Reflection;

struct S
{
}

struct SS
{
	static int i = 9;
}

struct SSS
{
	static SSS Empty;
	
	static SSS ()
	{
		Empty = new SSS ();
	}
}

class C
{
}

class CC
{
	static int i = 9;
}

class CCC
{
	static CCC Empty;
	
	static CCC ()
	{
		Empty = new CCC ();
	}
}

class X
{
	public static int Main ()
	{
		Type t = typeof (S);
		if ((t.Attributes & TypeAttributes.BeforeFieldInit) == 0)
			return 1;

		t = typeof (SS);
		if ((t.Attributes & TypeAttributes.BeforeFieldInit) == 0)
			return 2;
		
		t = typeof (SSS);
		if ((t.Attributes & TypeAttributes.BeforeFieldInit) != 0)
			return 3;
		
		t = typeof (C);
		if ((t.Attributes & TypeAttributes.BeforeFieldInit) == 0)
			return 4;

		t = typeof (CC);
		if ((t.Attributes & TypeAttributes.BeforeFieldInit) == 0)
			return 5;
		
		t = typeof (CCC);
		if ((t.Attributes & TypeAttributes.BeforeFieldInit) != 0)
			return 6;
		
		Console.WriteLine ("OK");
		return 0;
	}
}
