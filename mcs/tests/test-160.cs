class B {
        public S s;
}
class S {
        public int a;
}
class T {
	static B foo;

        static int blah (object arg) {
                B look = (B)arg;
		foo.s.a = 9;
		look.s.a = foo.s.a;
                return look.s.a;
        }

        public static int Main() {
		// Compilation only test;
		return 0;
	}
}
