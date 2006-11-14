//
// This is a test for bug 78786
// The issue was that the method call would trigger argument compatibility
// checks, and on success it would re-resolve the tree and not every
// expression copes with that gracefully
//

using System.Collections.Generic;

public class Test {
        public delegate int TestDel (int a);

        public static void Main (string[] args) {
                Dictionary<string, TestDel> dict = new Dictionary<string,
TestDel> ();

                dict["a"] = delegate (int b) {
                        return b;
                };

                System.Console.WriteLine (dict["a"] (2));
        }
}
