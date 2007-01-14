// CS0266: Cannot implicitly convert type `System.IntPtr' to `byte*'. An explicit conversion exists (are you missing a cast?)
// Line: 23
// Compiler options: -unsafe

using System;

public class Pixbuf {
        static void Main (string [] args)
	{
		Bug ();
	}

	public IntPtr Pixels {
		get {
			return IntPtr.Zero;
		}
	}
	public static unsafe void Bug ()
	{
		Pixbuf pixbuf = null;
		//should be:
		//byte *pix = (byte *)pixbuf.Pixels;
		byte *pix = pixbuf.Pixels;
	}
}



