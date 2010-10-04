// CS0060: Inconsistent accessibility: base class `Foo<Bar.Baz>' is less accessible than class `Bar'
// Line: 7

public class Foo<K> {
}

public class Bar : Foo<Bar.Baz> {
	private class Baz {
	}
}
