// CS0067: The event `EventTestClass.Elapsed' is never used
// Line: 8
// Compiler options: -warnaserror

using System;

public class EventTestClass : IEventTest
{
	public event EventHandler Elapsed;
}

public interface IEventTest 
{
	event EventHandler Elapsed;
}
