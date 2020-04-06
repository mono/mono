using System;
using System.Threading.Tasks;
namespace DebuggerTests
{
	public class EvaluateTestsClass
	{
        public class TestEvaluate {
            public int a;
            public int b;
            public int c;
            public void run(int g, int h, string valString) {
                int d = g + 1;
                int e = g + 2;
                int f = g + 3;
                int i = d + e + f;
                a = 1;
                b = 2;
                c = 3;
                a = a + 1;
                b = b + 1;
                c = c + 1;
            }
        }    

	public static void EvaluateLocals ()
	{
		TestEvaluate f = new TestEvaluate();
		f.run(100, 200, "test");

		Console.WriteLine ($"a: {f.a}, b: {f.b}, c: {f.c}");
	}
    }
}
