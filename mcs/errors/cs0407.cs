// CS0407: A method or delegate `int MainClass.Delegate()' return type does not match delegate `void TestDelegate()' return type
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
