// CS0214: Pointers and fixed size buffers may only be used in an unsafe context
// Line: 16
// Compiler options: -unsafe

using System;

public class Driver 
{
	public static unsafe byte* Frob 
	{
		get { return (byte *) 0; }
	}
  
	public static void Main () 
	{
		IntPtr q = (IntPtr) Frob;
	}
}
