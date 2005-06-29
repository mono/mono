// cs1501-3.cs: No overload for method `X' takes `2' arguments
public struct X {
        public X(int i) { }
        
        public static void Main() {
                X x = new X("foo", "bar");
        }
}
