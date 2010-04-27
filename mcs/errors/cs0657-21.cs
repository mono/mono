// cs0657-21.cs: `return' is not a valid attribute location for this declaration. Valid attribute locations for this declaration are `method'
// Line : 9

using System;

public class C
{
	[return: CLSCompliant (false)]
	public C ()
	{
	}
}
