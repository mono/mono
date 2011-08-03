// CS3022: CLSCompliant attribute has no meaning when applied to parameters. Try putting it on the method instead
// Line: 8
// Compiler options: -warn:1 -warnaserror

using System;
[assembly: CLSCompliant (true)]

public class Class {
	public void Test ([CLSCompliant(false)] uint u) {
	}
}
