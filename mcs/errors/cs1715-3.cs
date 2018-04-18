// CS1715: `B.Foo': type must be `int' to match overridden member `A.Foo'
// Line: 11

public abstract class A
{
	public abstract ref int Foo { get; }
}

public class B : A
{
	public override ref long Foo {
		get {
			throw null;
		}
	}
}