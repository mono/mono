// CS1725: Friend assembly reference `main, Culture=neutral' is invalid. InternalsVisibleTo declarations cannot have a version, culture or processor architecture specified
// Line: 6

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo ("main, Culture=neutral")]

class A
{
	public static void Main ()
	{
	}
}
