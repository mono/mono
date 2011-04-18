// CS0065: `EventClass.handler': event property must have both add and remove accessors
// Line : 9

using System;

public delegate void EventHandler (object sender, EventArgs e);

public class EventClass {
        event EventHandler handler { add; }
}

public class MainClass {
	public static void Main () {}
}
