class A<T>
{
	public interface IB { }
}

class E : A<int>.IB, A<string>.IB
{
	public static void Main ()
	{
		new E ();
	}
}
