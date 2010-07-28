class A<U>
{
	protected int foo;
}

class B<T> : A<T>
{
	protected class N
	{
		public void Test (C b)
		{
			var v = b.foo;
		}
	}
}

class C : B<int>
{
	public static void Main ()
	{
		new C.N ().Test (new C ());
	}
}
