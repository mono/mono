using System;

public class CLSClass {
        [CLSCompliant (false)]
        static public implicit operator CLSClass(byte value) {
               return new CLSClass();
        }
        
        [CLSCompliant (true)]
        private void Error (bool arg) {
        }
}

public class MainClass {
        public static void Main () {
        }
}