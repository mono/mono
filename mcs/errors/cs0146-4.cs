// cs0146-4.cs: Circular base class dependency involving `Foo.Bar' and `Foo'
// Line: 5

class Foo : Baz {
	public class Bar {}
}
class Baz : Foo.Bar {}
