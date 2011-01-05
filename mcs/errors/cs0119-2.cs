// CS0119: Expression denotes a `type', where a `variable' or `value' was expected
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
