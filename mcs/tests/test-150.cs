using System;
class T {
	//
	// Tests that the following compiles
	
	uint bar = (uint) int.MaxValue + 1;
	
        public static int Main() {
                if (Int32.MinValue == 0x80000000)
                        return 1;


                return 0;
        }
}
