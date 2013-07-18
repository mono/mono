partial class A<T>
{
	internal partial class B : A<int>
	{
		public void Test ()
		{
			Foo (3);
		}
	}
}

partial class A<T> : X<T>
{

}

class X<U>
{
	public void Foo (U arg)
	{
	}
}

class M
{
	public static void Main ()
	{
		new A<string>.B ().Test ();
	}
}