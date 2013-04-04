// CS1997: `System.Func<System.Threading.Tasks.Task>': A return keyword must not be followed by an expression when async delegate returns `Task'. Consider using `Task<T>' return type
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
