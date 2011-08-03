// CS0571: `MainClass.Value.get': cannot explicitly call operator or accessor
// Line: 12

class MainClass {
        static int Value {
                get {
                        return 1;
                }
        }
        
        public static void Main() {
                int value = get_Value();
        }
}
