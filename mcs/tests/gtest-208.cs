public class SomeClass {
}

public class Foo<T> where T : class {
	public T Do (object o) { return o as T; }
}

class Driver {
	public static void Main ()
	{
		Foo<SomeClass> f = new Foo<SomeClass> ();
		f.Do ("something");
	}
}


