// CS0029: Cannot implicitly convert type `T' to `EventHandler'
// Line: 14

using System;

public delegate void EventHandler (int i, int j);

public class Button {

	public event EventHandler Click;

	public void Connect<T> () where T : class
	{
		Click += default (T);
	}
}
