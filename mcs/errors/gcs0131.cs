// CS0131: Left hand of an assignment must be a variable, a property or an indexer
// Line: 13
using System;
 
public class Foo<T>
{
}
 
class X
{
	static void Main ()
	{
		Foo<X> = new Foo<X> ();
	}
}
