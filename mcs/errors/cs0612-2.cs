// CS0612: `Foo.Bar' is obsolete
// Line: 8
// Compiler options: -r:CS0612-2-lib.dll -warnaserror

public class Bar {
        public static int Main ()
        {
                Foo foo = new Foo ();
                return foo.Bar;
        }
}
