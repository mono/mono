// Compiler options: -t:library

public class Foo<T>
{
	public class Bar
	{
		public class FooBar : System.IEquatable<FooBar>
		{
			public bool Equals(FooBar a)
			{
				return true;
			}
		}
	}
}