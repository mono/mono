using System;

class Test {
        public static int Main() {
                Console.WriteLine("test");
                TestClass tst = new TestClass();
                tst.test("test");
                TestInterface ti = (TestInterface)tst;
                ti.test("test");
		return 0;
        }

        public interface TestInterface {
                string test(string name);
        }

        public class TestClass: TestInterface {
                public string test(string name) {
                    Console.WriteLine("test2");
                    return name + " testar";
                }
        }
}
