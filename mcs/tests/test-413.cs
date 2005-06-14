// Compiler options: -r:test-413-lib.dll

using Inner = Foo.Bar.Baz.Inner;
public class Driver {
        public static void Main () {
                Inner.Frob();
        }
}
