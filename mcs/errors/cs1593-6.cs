// CS1593: Delegate `System.Action<System.Exception,object>' does not take `1' arguments
// Line: 16

using System;
using System.Threading.Tasks;

class MainClass
{
	public static void Run (Func<Task> func)
	{
	}

	public static void Main (string[] args)
	{
		Run(async () => {
			Function(async (handle) => {
			});
		});
	}

	public static void Function (Action<Exception, object> callback)
	{
	}
}
