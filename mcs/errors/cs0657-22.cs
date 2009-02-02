// CS0657: `field' is not a valid attribute location for this declaration. Valid attribute locations for this declaration are `event, method'
// Line: 8

using System;

abstract class X
{
	[field:NonSerialized]
	public abstract event EventHandler XEvent;
}
