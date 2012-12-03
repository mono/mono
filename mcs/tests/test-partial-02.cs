// Compiler options: -langversion:default

namespace Test1
{
	public class Base
	{ }

	public partial class Foo : Base
	{ }

	public partial class Foo : Base
	{ }
}

namespace Test2
{
	public interface Base
	{ }

	public partial class Foo : Base
	{ }

	public partial class Foo : Base
	{ }
}

public partial class ReflectedType { }
partial class ReflectedType { }

partial class D { }
public partial class D { }
partial class D { }

class X
{
	public static void Main ()
	{ }
}
