// Compiler options: -unsafe

class T
{
        unsafe private byte *ptr;
        unsafe internal byte * Ptr {
                get { return ptr; }
                set { ptr = value; }
        }
	
	public static void Main () {}
}
