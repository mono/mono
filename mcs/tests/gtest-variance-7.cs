delegate T Covariant<out T> ();
delegate void Contra<in T> (T t);
delegate TR CoContra<out TR, in T> (T t);
delegate void None<T> (T t);

delegate Covariant<Contra<Contra<Covariant<Covariant<Covariant<Covariant<Covariant<U>>>>>>>> Test<out U> ();
delegate Contra<Covariant<Contra<Contra<Covariant<Covariant<Covariant<Covariant<Covariant<U>>>>>>>>> Test2<in U> ();
delegate Contra<Contra<Covariant<Covariant<Covariant<Covariant<Contra<Contra<U>>>>>>>> Test3<out U> ();
delegate Contra<Contra<Covariant<Covariant<Contra<Contra<Contra<Contra<U>>>>>>>> Test4<out U> ();
delegate Contra<Contra<Covariant<Covariant<Contra<Contra<Contra<U>>>>>>> Test5<in U> ();
delegate void Test6<in U> (Covariant<Contra<Contra<Covariant<Covariant<Covariant<Covariant<Covariant<U>>>>>>>> t);

delegate void Both<in U, out V> (CoContra<U, V> p);
delegate void Both2<in U, out V> (CoContra<U, Contra<U>> p);
delegate void Both3<in U, out V> (CoContra<U, Contra<int>> p);
delegate void Both4<in U, out V> (Both<V, U> b);
delegate void Both5<in U, out V> (Both<V, int> b);

class C
{
	public static void Main ()
	{
	}
}