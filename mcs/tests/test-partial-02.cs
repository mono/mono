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

class X
{
	static void Main ()
	{ }
}
