namespace Martin
{
	public class Foo<T>
	{ }
}

namespace Baulig
{
	using M = Martin;

	class X
	{
		public static void Main ()
		{
			M.Foo<int> foo;
		}
	}
}
