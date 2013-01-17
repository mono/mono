#define C2

using System;
using System.Diagnostics;

class TestClass {
	static int return_code = 1;
    
        [Conditional("C1"), Conditional("C2")]    
        public static void ConditionalMethod()
        {
            Console.WriteLine ("Succeeded");
            return_code = 0;
        }
    
        public static int Main()
        {
            ConditionalMethod ();
            return return_code;
        }
}
