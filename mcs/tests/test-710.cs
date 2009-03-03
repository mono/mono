// Compiler options: -warnaserror -warnaserror-:612,219

using System;

[Obsolete]
class Z
{
}

class C
{
	public static void Main ()
	{
		Z z = new Z ();
	}
}
