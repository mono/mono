// CS0540: `Foo.ISomeProp.SomeProperty': containing type does not implement interface `ISomeProp'
// Line: 18

public class SomeProperty
{
}

public abstract class SomeAbstract : ISomeProp
{
	public abstract SomeProperty SomeProperty { get; }
}

interface ISomeProp
{
	SomeProperty SomeProperty { get; }
}

public class Foo : SomeAbstract
{
	SomeProperty ISomeProp.SomeProperty { get { return null; } }

	public override SomeProperty SomeProperty { get { return null; } }

	public static void Main ()
	{
	}
}
