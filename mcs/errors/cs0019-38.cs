// CS0019: Operator `+' cannot be applied to operands of type `string' and `method group'
// Line: 11

using System;

class MainClass
{
	public static void Main (string[] args)
	{
		foreach (object at in args) {
			Console.WriteLine ("Tipo attributo: " + at.GetType);
		}
	}
}
