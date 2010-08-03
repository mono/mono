using System;

class G<T>
{
}

class DynamicAssignments
{
	static int Main ()
	{
		dynamic d1 = null;
		dynamic d2 = null;
		d1 = d2;
		d2 = d1;

		G<object> g1 = null;
		G<dynamic> g2 = null;
		g1 = g2;
		g2 = g1;

		G<G<object>> g_n1 = null;
		G<G<dynamic>> g_n2 = null;
		g_n1 = g_n2;
		g_n2 = g_n1;

		return 0;
	}
}
