// Compiler options: -r:ns0.dll

using Inner = Foo.Bar.Baz.Inner;
public class Driver {
        public static void Main () {
                Inner.Frob();
        }
}
