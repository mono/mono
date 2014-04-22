// CS0534: `Foo' does not implement inherited abstract member `SomeAbstract.SomeProperty.get'
// Line: 13

public class SomeProperty
{
}

public abstract class SomeAbstract
{
	public abstract SomeProperty SomeProperty { get; }
}

public class Foo : SomeAbstract
{
	public static SomeProperty SomeProperty { get { return null; } }
}

