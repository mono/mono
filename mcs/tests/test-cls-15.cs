// Compiler options: -warnaserror

using System;
[assembly:CLSCompliant (true)]

public class CLSAttribute_1: Attribute {
       public CLSAttribute_1(int[] array) {
       }
   
       public CLSAttribute_1(int array) {
       }
}

[CLSCompliant (false)]
public class CLSAttribute_2: Attribute {
       private CLSAttribute_2(int arg) {
       }   
}

internal class CLSAttribute_3: Attribute {
       public CLSAttribute_3(int[] array) {
       }
}

[CLSCompliant (false)]
public class CLSAttribute_4: Attribute {
       private CLSAttribute_4(int[] args) {
       }   
}

public class ClassMain {
        public static void Main () {}
}
