// CS0146: Circular base class dependency involving `Foo' and `Foo.Bar'
// Line: 5

class Foo : Foo.Bar {
	public class Bar {}
}
