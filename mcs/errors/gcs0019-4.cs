// CS0019: Operator `+=' cannot be applied to operands of type `EventHandler' and `T'
// Line: 10

using System;

public delegate void EventHandler (int i, int j);

public class Button {

	public event EventHandler Click;

	public void Connect<T> () where T : class
	{
		Click += default (T);
	}
}
