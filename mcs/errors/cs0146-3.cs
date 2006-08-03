// cs0146-3.cs: Circular base class dependency involving `Foo.Bar' and `Foo'
// Line: 5

class Foo : Foo.Bar {
	public class Bar {}
}
