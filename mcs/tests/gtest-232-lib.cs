// Compiler options: -t:library

public class A<T>
{
	public class B<U>
	{
		public class C<V>
		{
			public T T;
			public U U;
		}
	}
	
	public class B2
	{
		public T T;

		public class C<V>
		{
			public T T2;
		}
	}	
}

public static class Factory
{
	public static A<int>.B<bool>.C<string> Create_1()
	{
		return new A<int>.B<bool>.C<string> ();
	}
	
	public static A<int>.B2.C<string> Create_2()
	{
		return new A<int>.B2.C<string> ();
	}
	
	public static A<int>.B2 Create_3()
	{
		return new A<int>.B2 ();
	}	
}
