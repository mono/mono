// Compiler options: -unsafe

using System;

class Test {
	public String GetString (byte[] bytes)
	{
                unsafe {
                    int* i;
                }
                
		unsafe {
			fixed (byte *ss = &bytes [0]) {
                                int* i;
				return new String ((sbyte*)ss, 0, bytes.Length);
			}
		}
	}
        
        public static void Main () {}
}
