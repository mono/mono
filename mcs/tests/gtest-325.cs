public class SomeClass<T> where T : new() {
    public void Foo() {
        new T();
    }
}

class Foo {
	static void Main ()
	{
		SomeClass<object> x = new SomeClass<object> ();
		x.Foo ();
	}
}
