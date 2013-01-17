

// Test case for bug: #349034

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

class Test {
        private static void TestNaturalSort()
        {
            Comparison<string> naturalSortComparer = (left, right) => {
                return Regex.Replace(left ?? "", @"([\d]+)|([^\d]+)", m =>
(m.Value.Length > 0 && char.IsDigit(m.Value[0])) ?
m.Value.PadLeft(Math.Max((left ?? "").Length, (right ?? "").Length)) :
m.Value).CompareTo(Regex.Replace(right ?? "", @"([\d]+)|([^\d]+)", m =>
(m.Value.Length > 0 && char.IsDigit(m.Value[0])) ?
m.Value.PadLeft(Math.Max((left ?? "").Length, (right ?? "").Length)) :
m.Value));
            };
        }

        public static void Main ()
        {
                TestNaturalSort ();
        }
}
