using System;
using X = N;
namespace N { class A {} }
class B<T> { }
class Test {
	static public B<X::A> q;
	public static void Main ()
	{
		q = new B<N.A> ();
		if (typeof (B<X::A>) != typeof (B<X.A>))
			throw new Exception ("huh");
	}
}
