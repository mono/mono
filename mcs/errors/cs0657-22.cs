// CS0657: `field' is not a valid attribute location for this declaration. Valid attribute locations for this declaration are `event, method'. All attributes in this section will be ignored
// Line: 9
// Compiler options: -warnaserror

using System;

abstract class X
{
	[field:NonSerialized]
	public abstract event EventHandler XEvent;
}
