// cs0657-20.cs: `return' is not a valid attribute location for this declaration. Valid attribute locations for this declaration are `method'
// Line : 7

using System;

public class C
{
	[return: CLSCompliant (false)]
	~C ()
	{
	}
}
