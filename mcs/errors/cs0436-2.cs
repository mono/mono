// CS0436: The type `System.Runtime.CompilerServices.RuntimeHelpers' conflicts with the imported type of same name'. Ignoring the imported type definition
// Line: 20
// Compiler options: -warn:2 -warnaserror

using System.Runtime.CompilerServices;

namespace System.Runtime.CompilerServices
{
	public class RuntimeHelpers
	{
		public static void SomeMethod ()
		{
		}
	}
}


class C
{
	public static void Main ()
	{
		RuntimeHelpers.SomeMethod (null);
	}
}
