using System;
using System.Collections;
 
class X {
        delegate void A ();
 
        static IEnumerable GetIt (int [] args)
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
 
        static int Main ()
        {
                int total = 0;
                foreach (int i in GetIt (new int [] { 1, 2, 3})){
                        Console.WriteLine ("Got: " + i);
                        total += i;
                }
 
                if (total != 6)
                        return 1;
 
                return 0;
        }
}
