// Compiler options: /r:gtest-233-lib.dll
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

class Program
{
	public static void Main (string[] args)
	{
		MyClass<int> list = new MyClass<int>();

		list.ListChanged += new ListChangedEventHandler (list_ListChanged);
	}

	static void list_ListChanged (object sender, ListChangedEventArgs e) { }
}
