// cs3022.cs: CLSCompliant attribute has no meaning when applied to parameters. Try putting it on the method instead
// Line: 8

using System;
[assembly: CLSCompliant (true)]

public class Class {
	public void Test ([CLSCompliant(false)] uint u) {
	}
}
