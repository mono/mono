// CS8148: `B.Foo': must return by reference to match overridden member `A.Foo'
// Line: 11

public abstract class A
{
	public abstract ref int Foo { get; }
}

public class B : A
{
	public override long Foo {
		get {
			throw null;
		}
	}
}