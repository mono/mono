#define C2

using System;
using System.Diagnostics;

class TestClass {
    
        [Conditional("C1"), Conditional("C2")]    
        public static void ConditionalMethod()
        {
            Console.WriteLine ("Succeeded");
            Environment.Exit (0);
        }
    
        static int Main()
        {
            ConditionalMethod ();
            return 1;
        }
}
