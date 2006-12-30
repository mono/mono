// CS0571: `C2.this[int].set': cannot explicitly call operator or accessor
// Line: 8
// Compiler options: -r:CS0571-5-lib.dll

class MainClass {
        public static void Main() {
                C2 c = new C2 ();
                c.set_Item(1, 2);
        }
}
