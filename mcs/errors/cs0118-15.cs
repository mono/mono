// CS0118: `Foo<X>' is a `type' but a `variable' was expected
// Line: 12
 
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
