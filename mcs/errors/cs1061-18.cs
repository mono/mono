// CS1061: No overload for method `Call' takes `0' arguments
// Line: 11

using System;
 
class Program
{
	static void Main ()
	{
		Action<dynamic, object> action = delegate { };
		Foo (action).NoDynamicBinding ();
	}
 
	static T Foo<T>(Action<T, T> x)
	{
		throw new NotImplementedException ();
	}
}