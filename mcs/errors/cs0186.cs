// cs0186.cs: Use of null is not valid in this context
// Line: 8

using System;

class ClassMain {
        public static void Main() {
            Exception e = (object)null as Exception;
        }
}

