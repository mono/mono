using System;

interface IA
{
	int Foo { get; }
}

interface IB_1 : IA
{
	new string Foo { get; }
}

interface IB_2 : IA
{
	new char Foo { get; }
}

interface IC : IB_2, IB_1
{
	new byte Foo { get; }
}

class A : IA
{
	public int Foo { get { return 3; } }
}

class B : A, IB_1
{
	public new string Foo { get { return "1"; } }
}

class C : B, IC
{
	char IB_2.Foo { get { return 'a'; } }
	
	public new byte Foo { get { return 2; } }
	
	public static void Main ()
	{
		new C ();
	}
}
