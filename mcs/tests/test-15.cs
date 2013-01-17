using System;

interface Iface {
	int A ();
}

class Implementor : Iface {
	public int A () {
		return 1;
	}
}

struct StructImplementor : Iface {
	public int A () {
		return 2;
	}
}
class Run {

	public static int Main ()
	{
		Iface iface;
		Implementor i = new Implementor ();

		iface = i;
		if (iface.A () != 1)
			return 1;

		StructImplementor s = new StructImplementor ();
		Iface xiface = (Iface) s;
		if (xiface.A () != 2)
			return 2;
		
		return 0;
	}
}
