// cs3016.cs: Arrays as attribute arguments are not CLS-compliant
// Line: 7

using System;
[assembly:CLSCompliant (true)]

[CLSAttribute (new bool [] {true, false})]
public class CLSAttribute: Attribute {
        public CLSAttribute () {
        }
        
        public CLSAttribute(bool[] array) {
        }
}