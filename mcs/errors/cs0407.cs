// cs0407.cs: 'int MainClass.Delegate()' has the wrong return type
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
