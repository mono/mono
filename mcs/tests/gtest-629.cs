// Compiler options: -unsafe

using System;
using System.Collections.Generic;

public class Program
{
	 public unsafe static void Main ()
	 {
		var list = new List<object> () { "" };
		fixed (char *c = (string)list[0]) {
			
		}

		var list2 = new List<object> () { null };
		fixed (byte* p = (byte[])list2[0]) {
		}
	
	 }
}