using System;

public class A {
        public void DoStuff ()
        {
                Console.WriteLine ("stuff");
        }
}

public struct B {
    public bool Val {
        get {
            return false;
        }
    }
}

public class T : MarshalByRefObject {
        internal static A a = new A ();
        public static B b;
}

public class Driver {

        public static void Main ()
        {
                T.a.DoStuff ();
                bool b = T.b.Val;
        }
}
