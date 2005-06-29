// cs0123.cs: Method `int MainClass.Delegate()' does not match delegate `int TestDelegate(bool)'
// Line: 12

delegate int TestDelegate(bool b);

public class MainClass {
        public static int Delegate() {
                return 0;
        }

        public static void Main() {
                TestDelegate delegateInstance = new TestDelegate (Delegate);
       }
}

