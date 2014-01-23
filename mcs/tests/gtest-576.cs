using System;

interface IParam<U> where U : IParam<U>
{
}

interface IA
{
	void f<T2> (IParam<T2> p) where T2 : A, IParam<T2>;
}

class A : IA
{
	void IA.f<T1> (IParam<T1> p)
	{
	}

	public static void Main ()
	{
		new A ();
	}
}