// Compiler options: -warnaserror

using System;
[assembly:CLSCompliant (true)]

[CLSCompliant (false)]
public interface I {
        [CLSCompliant (false)]
        void Error (ulong arg);
}

[CLSCompliant (false)]
public interface I2 {
#pragma warning disable 3018	
        [CLSCompliant (true)]
        void Error (long arg);
#pragma warning disable 3018        
}


public class MainClass {
        public static void Main () {
        }
}
