public interface IFoo<T> where T : IFoo<T> { }
public interface IBaz<T> where T : IFoo<T> { }

class Foo : IFoo<Foo>
{ }

class X
{
	public static void Main ()
	{ }
}
