public class Test
{
        public static void Main ()
        {
                new Test (delegate () {});
        }

        public Test (TestMethod test) {}
        public Test (TestMethod2 test2) {}
}

public delegate void TestMethod ();
public delegate void TestMethod2 (object o);