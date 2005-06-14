using System;
[assembly:CLSCompliant (true)]

[CLSCompliant (true)]
public abstract class CLSClass {
        [CLSCompliant (true)]
        public abstract void Test (IComparable arg);
}

public abstract class CLSCLass_2 {
        public abstract void Test ();
}

public class MainClass {
        public static void Main () {
        }
}