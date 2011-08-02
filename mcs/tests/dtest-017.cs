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

struct S<T>
{
}

delegate dynamic D (dynamic d);

class DynamicAssignments
{
	static void Foo (IA<object> o)
	{
	}
	
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
		
		IA<object> ia_o = null;
		IA<dynamic> ia_d = ia_o;
		
		S<dynamic> s_d = new S<dynamic> ();
		S<object> s_o = s_d;
		S<object>? s_o_n = s_d;
		
		D del = delegate (object del_arg) {
			 return (object) null;
		};
		
		Action<IA<dynamic>> del2 = Foo;
		
		Action<object> del31 = null;
		Action<dynamic> del32 = del31;
		
		I<dynamic>[] a20 = null;
		I<object>[] b20 = a20;

		return 0;
	}
}
