// Compiler options: -warnaserror -warn:4

using System;
using System.Reflection;

public class EventTestClass : IEventTest {
        public event EventHandler Elapsed; // No warning is reported
}

public interface IEventTest {
        event EventHandler Elapsed;
}


public class EntryPoint
{
	public static bool test (Type type) { return type.GetEvent ("Elapsed").IsSpecialName; }	
        public static int Main ()
        {
                return (test (typeof (EventTestClass)) 
			|| test (typeof (IEventTest))) ? 1 : 0;
        }
}
