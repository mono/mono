// CS0315: The type `int' cannot be used as type parameter `U' in the generic type or method `A<int?>.Test<U>()'. There is no boxing conversion from `int' to `int?'
// Line: 19

class A<T>
{
	public static void Test<U> () where U : T
	{
	}
}

class B : A<int?>
{
}

class Program
{
	public static void Main ()
	{
		B.Test<int> ();
	}
}
