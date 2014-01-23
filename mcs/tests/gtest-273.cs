using System;

public class ThisBaseClass<A, B, C> 
	where A: ThisBaseClass<A, B, C>
	where B: ThatBaseClass<B, A, C>, new()
	where C: class
{ }

public class ThatBaseClass<B, A, C>
	where B: ThatBaseClass<B, A, C>, new()
	where A: ThisBaseClass<A, B, C>
	where C: class
{ }

public class ThisClass<A, B, C>: ThisBaseClass<A, B, C>
	where A: ThisClass<A, B, C>
	where B: ThatClass<B, A, C>, new()
	where C: class
{ }

public class ThatClass<B, A, C>: ThatBaseClass<B, A, C>
	where B: ThatClass<B, A, C>, new()
	where A: ThisClass<A, B, C>
	where C: class
{ }

public class ThisClass: ThisClass<ThisClass, ThatClass, object>
{ }

public class ThatClass: ThatClass<ThatClass, ThisClass, object>
{ }

public class Test
{
	public static void Main ()
	{ }
}
