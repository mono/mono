// Compiler options: -r:mtest-4-dll.dll

using Inner = Foo.Bar.Baz.Inner;
public class Driver {
        public static void Main () {
                Inner.Frob();
        }
}
