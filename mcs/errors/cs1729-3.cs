// CS1729: The type `X' does not contain a constructor that takes `2' arguments
// Line: 8

public struct X {
        public X(int i) { }
        
        public static void Main() {
                X x = new X("foo", "bar");
        }
}
