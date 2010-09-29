// CS0122: `Bar.Baz' is inaccessible due to its protection level
// Line: 7

public class Foo<K> {
}

public class Bar : Foo<Bar.Baz> {
	private class Baz {
	}
}
