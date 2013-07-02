// CS0534: `C' does not implement inherited abstract member `B.Foo(string)'
// Line: 13

public abstract class A
{
	public abstract int Foo (string s);
}

public abstract class B : A
{
	public abstract override int Foo (string s);
}

public class C : B
{
}
