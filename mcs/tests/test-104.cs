//
// Test to ensure we do correct overload resolution
//
using System;

class Test {
        public static int Main(String[] args) {
                int iTest = 1;

                System.Threading.Interlocked.Increment(ref iTest);

		return 0;
        }
}
