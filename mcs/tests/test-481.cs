using System;
public delegate void TestDelegate (out int a);

public static class TestClass {
        public static int Main() {
                TestDelegate out_delegate = delegate (out int a) {
                        a = 0;
                };

		int x = 5;
		out_delegate (out x);
		return x;
        }
}
