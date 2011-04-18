// CS0577: Conditional not valid on `MainClass.~MainClass()' because it is a constructor, destructor, operator or explicit interface implementation
// Line: 5

class MainClass {
        [System.Diagnostics.Conditional("DEBUG")]
        ~MainClass () {}

        public static void Main () {}
}
