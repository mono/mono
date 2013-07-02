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
	public static int Main()
	{
		A<int>.B<char>.C<bool> o = new A<int>.B<char>.C<bool>();
		string s = o.a.GetType().FullName;
		System.Console.WriteLine(s);
		if (s != "System.Boolean")
			return 1;

		s = o.b.GetType().FullName;
		System.Console.WriteLine(s);
		if (s != "System.Char")
			return 2;
		
		s = o.c.GetType().FullName;
		System.Console.WriteLine();
		if (s != "System.Int32")
			return 3;
		
		return 0;
	}
}
