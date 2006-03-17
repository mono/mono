public class A<T1>
{
	public T1 a;

	public class B<T2> : A<T2>
	{
		public T1 b;

		public class C<T3> : B<T3>
		{
			public T1 c;
		}
	}
}

class PopQuiz
{
	static void Main()
	{
		A<int>.B<char>.C<bool> o = new A<int>.B<char>.C<bool>();
		System.Console.WriteLine(o.a.GetType().FullName);
		System.Console.WriteLine(o.b.GetType().FullName);
		System.Console.WriteLine(o.c.GetType().FullName);
	}
}
