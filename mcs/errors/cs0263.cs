// CS0263: Partial declarations of `Foo' must not specify different base classes
// Line: 12
public class Base
{ }

public class OtherBase
{ }

public partial class Foo : Base
{ }

public partial class Foo : OtherBase
{ }

class X
{
	static void Main ()
	{ }
}
