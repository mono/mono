

using System;

public interface IA
{
	void Foo (IA self);
}

public static class C
{
	public static void Foo (this IA self)
	{
		self.Foo<int> ();
	}
	
	public static void Bar<U> (this IA self)
	{
		self.Foo<U> ();
	}
	
	public static void Foo<T> (this IA self)
	{
	}
	
	public static void Main ()
	{
	}
}
