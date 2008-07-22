// CS3015: `CLSAttribute' has no accessible constructors which use only CLS-compliant types
// Line: 8
// Compiler options: -warnaserror -warn:1

using System;
[assembly:CLSCompliant (true)]

public class CLSAttribute: Attribute {
   public CLSAttribute(string[] array) {
   }
}
