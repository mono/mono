// CS0123: A method or delegate `C.Button1_Click(params object[])' parameters do not match delegate `EventHandler(params int[])' parameters
// Line: 20

using System;

public delegate void EventHandler (params int[] args);

public class C
{
	public void Connect ()
	{
		EventHandler Click = new EventHandler (Button1_Click);
	}

	public void Button1_Click (params object[] i)
	{
	}
}
