using System;

interface IA
{
	event EventHandler e_a, e_b;
}

class C : IA
{
	event EventHandler IA.e_a { add {} remove {} }
	event EventHandler IA.e_b { add {} remove {} }
	
	public static void Main ()
	{
	}
}