// cs3016.cs: Arrays as attribute arguments are not CLS-compliant
// Line: 7

using System;
[assembly: System.CLSCompliant (true)]

[CLSAttribute (new bool [] {true, false})]
public enum E {
}

public class CLSAttribute: System.Attribute {
        public CLSAttribute () {
        }
        
        public CLSAttribute(bool[] array) {
        }
}