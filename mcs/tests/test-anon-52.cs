using System;
using System.Collections;
 
class X {
        delegate void A ();
 
        static IEnumerator GetIt (int [] args)
        {
                foreach (int arg in args) {
                        Console.WriteLine ("OUT: {0}", arg);
                        A a = delegate {
                                Console.WriteLine ("arg: {0}", arg);
				return;
                        };
                        a ();
                        yield return arg;
                }
        }
 
        public static int Main ()
        {
                IEnumerator enumerator = GetIt (new int [] { 1, 2, 3});
		enumerator.MoveNext ();
                return 0;
        }
}
