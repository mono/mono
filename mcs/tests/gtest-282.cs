class Foo : C<Foo.Bar> {
	public class Bar {}
}
class C<T> {}

class Test {
	static Foo f = new Foo ();
	static void Main () { System.Console.WriteLine (f.GetType ().BaseType); }
}
