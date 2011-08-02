// CS0657: `return' is not a valid attribute location for this declaration. Valid attribute locations for this declaration are `method'. All attributes in this section will be ignored
// Line: 9
// Compiler options: -warnaserror

using System;

public class C
{
	[return: CLSCompliant (false)]
	public C ()
	{
	}
}
