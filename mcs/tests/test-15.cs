using System;

interface Iface {
	void A ();
}

class Implementor : Iface {
	public void A () {}
}

class Run {

	static int Main ()
	{
		Iface iface;
		Implementor i = new Implementor ();

		iface = i;
		
		return 0;
	}
}
