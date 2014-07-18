// CS8031: Async lambda expression or anonymous method converted to a `Task' cannot return a value. Consider returning `Task<T>'
// Line: 12

using System;
using System.Threading.Tasks;

class Test
{
	public static void Main()
	{
		Func<Task> t = async delegate {
			return null;
		};

		return;
	}
}
