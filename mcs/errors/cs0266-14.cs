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



