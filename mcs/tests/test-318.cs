// This code must be compilable without any warning
// Compiler options: -warnaserror -warn:4 -unsafe
// Test of wrong warnings

using System;
using System.ComponentModel;

public class Ev
{
        object disposedEvent = new object ();
        EventHandlerList Events = new EventHandlerList();
                
        public event EventHandler Disposed
        {
                add { Events.AddHandler (disposedEvent, value); }
                remove { Events.RemoveHandler (disposedEvent, value); }
        }

        public void OnClick(EventArgs e)
        {
            EventHandler clickEventDelegate = (EventHandler)Events[disposedEvent];
            if (clickEventDelegate != null) {
                clickEventDelegate(this, e);
            }
        }
}

public interface EventInterface {
	event EventHandler Event;
}

class Foo : EventInterface {
	event EventHandler EventInterface.Event
	{
			add { }
			remove { }
	}
}

public class C {
    
	public static void my_from_fixed(out int val)
	{
		val = 3;
	}
        
	public static void month_from_fixed(int date)
	{
		int year;
		my_from_fixed(out year);
	}
        
	internal static int CreateFromString (int arg)
	{
		int major = 0;
		int number = 5;

		major = number;
		number = -1;
                    
		return major;
	}   
        
        public unsafe double* GetValue (double value)
	{
		double d = value;
		return &d;
	}        
        
	public static void Main () {
	}
}
