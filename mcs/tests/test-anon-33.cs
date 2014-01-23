//
// This test showed that there were cases where there was no
// shared "ScopeInfo", and that we could not root and keep a "topmost"
// variable in the compiler for a CaptureContext.
//
// This illustrates two roots of captured scopes, independent of
// each other
//

using System;

delegate void Do ();

class T {
        static void doit (int v) {
                Console.WriteLine (v);
        }
        public static void Main () {
                Do[] arr = new Do [5];
                for (int i = 0; i < 5; ++i) {
                        arr [i] = delegate {doit (i);};
                }
                for (int i = 0; i < 5; ++i) {
                        arr [i] ();
                }
{
                for (int j = 0; j < 5; ++j) {
                        arr [j] = delegate {doit (j);};
                }
}
        }

}


