using System;
[assembly:CLSCompliant (true)]

[CLSCompliant (false)]
public interface I {
        [CLSCompliant (false)]
        void Error (ulong arg);
}

[CLSCompliant (false)]
public interface I2 {
        [CLSCompliant (true)]
        void Error (long arg);
}


public class MainClass {
        public static void Main () {
        }
}