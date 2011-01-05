// CS0065: `EventClass<T>.handler': event property must have both add and remove accessors
// Line: 10

using System;

public delegate void EventHandler (object sender, EventArgs e);

public class EventClass<T>
{
	event EventHandler handler { add {} }
}

