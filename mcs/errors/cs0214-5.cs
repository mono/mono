// cs0214: Pointer can only be used in unsafe context
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
