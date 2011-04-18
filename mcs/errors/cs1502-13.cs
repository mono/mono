// CS1502: The best overloaded method match for `T.Blah(out int)' has some invalid arguments
// Line: 11

using System;

class T {
        static void Blah (out int g) { g = 0; }

        static int Main (string [] args) {
                IntPtr g;
                Blah (out g);
		return (int) g;
        }
}
