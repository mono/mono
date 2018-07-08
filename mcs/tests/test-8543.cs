using System;
using System.Reflection.Emit;

internal class Program
{
	public static int Main ()
	{
		var methodBuilder = new DynamicMethod ("test", typeof (void), new Type[0], typeof (Program).Module);
		Delegate.CreateDelegate (typeof (Action), methodBuilder);

		return 0;
	}
}
