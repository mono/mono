// gcs0413-2.cs: The as operator requires that the `T' type parameter be constrained by a class
// Line: 8

public class SomeClass {
}

public class Foo<T> where T : struct {
	public T Do (object o) { return o as T; }
}

class Driver {
	static void Main ()
	{
		Foo<SomeClass> f = new Foo<SomeClass> ();
		f.Do ("something");
	}
}


