public class B
{
	public virtual T Get<T> ()
	{
		return default (T);
	}
}

public class A : B
{
	public override T Get<T>()
	{
		T resp = base.Get<T> ();
		System.Console.WriteLine("T: " + resp);
		return resp;
	}

	public static void Main ()
	{
		new A().Get<int> ();
	}
}
