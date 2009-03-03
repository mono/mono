// Compiler options: -warnaserror

using System;
[assembly:CLSCompliant (true)]

public class CLSClass {
        [CLSCompliant (false)]
        static public implicit operator CLSClass(byte value) {
               return new CLSClass();
        }

#pragma warning disable 3019, 169
        [CLSCompliant (true)]
        private void Error (bool arg) {
        }
#pragma warning restore 3019, 169
}

public class MainClass {
        public static void Main () {
        }
}
