using System;

public class C<T1> where T1 : B<T1>, new ()
{
	public void Test ()
	{
		using (var a = new T1 ()) {
		}
	}
}

public class B<T2> : IDisposable
{
	void IDisposable.Dispose ()
	{
	}
}

public class Test : B<Test>
{
	public static void Main ()
	{
		new C<Test> ().Test ();
	}
}
