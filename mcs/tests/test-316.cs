using System;

interface IA
{
	int Add(int i);
}

interface IB
{
	int Add(int i);	
}

interface IC : IA, IB {}

interface IE : ICloneable, IDisposable {
	void doom ();
}

class D : IC, IB
{
	int IA.Add (int i) {
		return 5;
	}
	
	int IB.Add (int i) {
		return 6;
	}
}

class E: IE, IC {
	public E() {
	}
	public void doom () {
		return;
	}
	public Object Clone () {
		return null;
	}
	public void Dispose () {}
	int IA.Add (int i) {
		return 7;
	}
	
	int IB.Add (int i) {
		return 8;
	}
}

class C
{
	static int Test(IC n) {
		IA a = (IA) n;
		if (a.Add(0) != 5)
			return 1;

		if (((IA)n).Add(0) != 5)
			return 1;

		if (((IB)n).Add(0) != 6)
			return 1;


		return 0;
	}

	static void Test2(IE ie) {
		ie.doom ();
		ie.Clone();
		ie.Dispose ();
	}

	public static int Main()
	{
		D d = new D();
		E e = new E();
		Test (e);
		Test2 (e);
		
		return Test (d);
	}
}

