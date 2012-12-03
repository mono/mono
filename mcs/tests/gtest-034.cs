class Foo<T>
{ }

class Stack<T>
{ }

//
// We may use a constructed type `Stack<T>' instead of
// just a type parameter.
//

class Bar<T> : Foo<Stack<T>>
{ }

class X
{
	public static void Main ()
	{ }
}
