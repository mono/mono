// Compiler options: -r:System
using System;
using System.Collections.Generic;

public class X {
	public static void Main(string[] args)
	{
		Collection<int> list = new Collection<int>();
		list.Add (3);
		foreach (int i in list) {
			Console.WriteLine(i);
		}
	}
}
