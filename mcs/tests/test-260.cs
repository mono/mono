using x;
using y;
using Test = x.Test;

namespace x
{
	public class Test
	{ }
}

namespace y
{
	public class Test
	{ }
}

namespace b
{
	public class a
	{
		public static void Main()
		{
			// Test should be an alias to x.Test
			Test test = new Test();
		}
	}
}
