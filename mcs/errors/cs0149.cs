// cs0149.cs: Method 'MainClass.Delegate()' does not match delegate 'void TestDelegate()'
// Line: 8

delegate void TestDelegate();

public class MainClass {
        public static void Main() {
                TestDelegate delegateInstance = new TestDelegate (0);
       }
}
