// Compiler options: -unsafe

unsafe class X
{
	delegate void D ();
	
	public static int Main ()
	{
		byte* a = null;
		D d = delegate () {
			byte* x = &*a;
		};
		
		return 0;
	}
}
