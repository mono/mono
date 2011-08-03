// CS0117: `System.Reflection.BindingFlags' does not contain a definition for `Private'
// Line: 8

using System;
using System.Reflection;

public class Test {
	const BindingFlags Def = BindingFlags.Private | BindingFlags.Static;
	const BindingFlags SetBindingFlags = Def | BindingFlags.SetProperty;

	static void Main ()
	{
	}
}
