// CS1725: Friend assembly reference `main, processorArchitecture=MSIL' is invalid. InternalsVisibleTo declarations cannot have a version, culture or processor architecture specified
// Line: 6

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo ("main, processorArchitecture=MSIL")]

class A
{
	public static void Main ()
	{
	}
}
