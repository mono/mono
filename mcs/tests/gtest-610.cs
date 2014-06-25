using System;

class G1<T1, T2>
	where T1 : B
	where T2 : T1
{
	public static T2 Test1 (B b)
	{
		return (T2)b;
	}

	public static T2 Test2 (A a)
	{
		return (T2)a;
	}

	public static T2 Test3 (dynamic a)
	{
		return (T2)a;
	}
}

class B : A
{
}

class A
{
	static void Main ()
	{
		G1<B, B>.Test1 (new B ());
		G1<B, B>.Test2 (new B ());
		G1<B, B>.Test3 (null);
	}
}