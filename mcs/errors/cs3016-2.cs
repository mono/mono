// cs3016.cs: Arrays as attribute arguments are not CLS-compliant
// Line: 5

using System;
[assembly:CLSAttribute (new bool [] {true, false})]
[assembly:CLSCompliant (true)]

public class CLSAttribute: Attribute {
        public CLSAttribute () {
        }
        
        public CLSAttribute(bool[] array) {
        }
}