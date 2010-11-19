// CS1725: Friend assembly reference `main, Version=1.1.1.1' is invalid. InternalsVisibleTo declarations cannot have a version, culture or processor architecture specified
// Line: 6

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo ("main, Version=1.1.1.1")]

class A
{
	public static void Main ()
	{
	}
}
