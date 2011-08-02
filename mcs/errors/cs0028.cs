// CS0028: `T.Main(int)' has the wrong signature to be an entry point
// Line: 8
// Compiler options: -warnaserror -warn:4

class T {
        public static int Main ()
        {
        }
        public static int Main (int foo)
        {
        }
}

