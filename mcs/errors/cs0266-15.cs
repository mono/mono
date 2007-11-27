// CS0266: Cannot implicitly convert type `object' to `System.Collections.Hashtable'. An explicit conversion exists (are you missing a cast?)
// Line: 17

// This case actually tests that the compiler doesn't crash after reporting the error

using System.Collections;

class X {
        static void Main (string [] install)
        {
                ArrayList order = new ArrayList ();
                Hashtable states = new Hashtable ();

                try {
                        if (install != null){
                                foreach (string inst in order){
                                        Hashtable state = states [inst];
                                }
                        }
                } catch {
                }
        }
}
