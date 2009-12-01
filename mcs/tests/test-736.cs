// Compiler options: -warn:4 -warnaserror

using System;

// Nothing wrong with this, gmcs says otherwise
// Class is generic
public class TestGeneric<T>
{
	public event EventHandler Event;

	public void Raise ()
	{
		Event (this, EventArgs.Empty);
	}
}

// Nothing wrong with this, gmcs concurs
// Note that T is used in the delegate signature for Event
public class TestGeneric2<T>
{
	public delegate void GenericHandler (T t);
	public event GenericHandler Event;

	public void Raise ()
	{
		Event (default (T));
	}
}

// Nothing wrong with this, gmcs concurs
// Class is not generic
public class Test
{
	public event EventHandler Event;

	public void Raise ()
	{
		Event (this, EventArgs.Empty);
	}

	public static void Main ()
	{
	}
}

