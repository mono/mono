// CS3016: Arrays as attribute arguments are not CLS-compliant
// Line: 8
// Compiler options: -warnaserror -warn:1

using System;
[assembly:CLSCompliant (true)]

[CLSAttribute (new bool [] {true, false})]
public class CLSAttribute: Attribute {
        public CLSAttribute () {
        }
        
        public CLSAttribute(bool[] array) {
        }
}
