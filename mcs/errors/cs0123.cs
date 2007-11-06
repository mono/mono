// CS0123: A method or delegate `MainClass.Delegate()' parameters do not match delegate `TestDelegate(bool)' parameters
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

