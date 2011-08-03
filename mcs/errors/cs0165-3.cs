// CS0165: Use of unassigned local variable `s'
// Line: 9

public class Test
{
        public static string Foo {
                get {
                        string s;
                        if (0 == 1 && (s = "") == "a" || s == "")
                                return s;
                        return " ";
                }
        }
}