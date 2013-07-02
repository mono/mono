// Bug #81019
public class Foo<K>
{ }

partial class B
{ }

partial class B : Foo<B.C>
{
	public class C
	{ }
  
}

class X
{
	public static void Main ()
	{ }
}
