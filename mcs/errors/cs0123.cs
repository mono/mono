// cs0123.cs: Method 'MainClass.Delegate()' does not match delegate 'void TestDelegate()'
// Line: 12

delegate void TestDelegate();

public class MainClass {
        public static int Delegate() {
                return 0;
        }

        public static void Main() {
                TestDelegate delegateInstance = new TestDelegate (Delegate);
       }
}
