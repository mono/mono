// Test for bug #60459

using System;
using System.Reflection;

public class EventTestClass {
        public event EventHandler Elapsed;
}

public interface IEventTest {
        event EventHandler Elapsed;
}


public class EntryPoint
{
	static bool test (Type type) { return type.GetEvent ("Elapsed").IsSpecialName; }	
        public static int Main ()
        {
                return (test (typeof (EventTestClass)) 
			|| test (typeof (IEventTest))) ? 1 : 0;
        }
}
