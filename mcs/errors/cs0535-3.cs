// CS0535: `Test' does not implement interface member `X.Hola(ref string)'
// Line: 9

using System;
interface X {
	void Hola (ref string name);
}

class Test : X {
	static void Main ()
	{
	}

	public void Hola (out string name)
	{
		name = null;
	}
}

