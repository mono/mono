//
// Tests that a nested class has full access to its container members
//
// A compile-only test.
//

class A {
        private static int X = 0;

        class B {
                void Foo ()
                {
                        ++ X;
                }
        }

        public static int Main ()
        {
		return 0;
        }
}
