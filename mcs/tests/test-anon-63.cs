using System;
using System.Collections;
 
class X {
        delegate int A ();
 
        static IEnumerator GetIt (int [] args)
        {
                foreach (int arg in args) {
                        Console.WriteLine ("OUT: {0}", arg);
                        A a = delegate {
                                Console.WriteLine ("arg: {0}", arg);
                                Console.WriteLine ("args: {0}", args);
				return arg;
                        };
                        yield return a ();
                }
        }
 
        public static int Main ()
        {
                IEnumerator enumerator = GetIt (new int [] { 4, 8, 9});
		enumerator.MoveNext ();
                return 0;
        }
}
