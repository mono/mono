using System;

class B<U>
{
}

class C<T> : B<T>
{
}

interface I<T> : IA<T>
{
}

interface IA<U>
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

		B<object> g1 = null;
		B<dynamic> g2 = null;
		g1 = g2;
		g2 = g1;

		B<B<object>> g_n1 = null;
		B<B<dynamic>> g_n2 = null;
		g_n1 = g_n2;
		g_n2 = g_n1;
		
		object[] o = null;
		dynamic[] d = o;

		C<object> a = null;
		B<dynamic> b = a;
		a = (C<object>)b;
		a = (C<dynamic>)b;

		I<object> io = null;
		IA<dynamic> id = io;

		return 0;
	}
}
