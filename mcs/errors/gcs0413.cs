//gcs0413.cs: The as operator requires that the `T' type parameter be constrained by a class
// Line: 7

public class SomeClass {}

public class Foo<T>  {
	public T Do (object o) { return o as T; }
}

class Driver {
	static void Main ()
	{
		Foo<SomeClass> f = new Foo<SomeClass> ();
		f.Do ("something");
	}
}


