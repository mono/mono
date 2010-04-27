// CS0146: Circular base class dependency involving `Baz' and `Foo.Bar'
// Line: 5

class Foo : Baz {
	public class Bar {}
}
class Baz : Foo.Bar {}
