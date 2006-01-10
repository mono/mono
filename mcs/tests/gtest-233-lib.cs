// Compiler options: /t:library
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

public class MyClass<TItem>
{
	public MyClass() { }

	public event ListChangedEventHandler ListChanged;
	public void AddListChangedEventHandler (ListChangedEventHandler handler)
	{
		ListChanged += handler;
	}

	protected void OnListChanged (ListChangedEventArgs e)  {}
}
