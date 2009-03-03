// Compiler options: -warnaserror

using System;
[assembly:CLSCompliant (true)]

[CLSCompliant (false)]
[CLSAttribute (new bool [] {true, false})]
public class CLSAttribute: Attribute {
       public CLSAttribute(bool[] array) {
       }
}

public class ClassMain {
        public static void Main () {}
}
