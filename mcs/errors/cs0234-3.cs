// error CS0234: The type or namespace name `Type' could not be found in namespace `MonoTests.System'
// Line: 12

using System;

namespace MonoTests.System
{
	public class Test
	{
		public static void Main ()
		{
			Console.WriteLine (System.Type.GetType ("System.String"));
		}
	}
}


