struct S<T1, T2>
{
	public T1 First;
	public T2 Second;
}

class A
{
	public virtual S<U, object> Foo<U> (U u)
	{
		return new S<U, object> ();
	}
}

class B : A
{
	public override S<T, dynamic> Foo<T> (T t)
	{
		return new S<T, dynamic> () {
			First = t,
			Second = "second"
		};
	}
}

public class MainClass
{
	public static int Main ()
	{
		B b = new B ();
		var res = b.Foo<int> (5);
		int i;
		i = res.First;
		if (i != 5)
			return 1;
		
		i = res.Second.Length;
		if (i != 6)
			return 2;
		
		res = b.Foo (4);
		i = res.First;
		if (i != 4)
			return 3;
		
		i = res.Second.Length;
		if (i != 6)
			return 4;
		
		return 0;
	}
}