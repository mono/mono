// CS0202: foreach statement requires that the return type `System.Collections.Generic.IEnumerable<string>' of `Test.GetEnumerator()' must have a suitable public MoveNext method and public Current property
// Line: 10
using System;
using System.Collections.Generic;

class Test {
        static void Main ()
        {
                Test obj = new Test ();
                foreach (string s in obj) {
                }
        }

        public IEnumerable<string> GetEnumerator ()
        {
		return null;
        }
}
