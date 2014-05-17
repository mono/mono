// Compiler options: -warnaserror -warn:4

using System;
using System.Runtime.InteropServices;

[StructLayout (LayoutKind.Sequential)]
public class EventTestClass : IEventTest
{
	int i;
	public event EventHandler Elapsed;

	public static void Main ()
	{		
	}
}

public interface IEventTest 
{
	event EventHandler Elapsed;
}
