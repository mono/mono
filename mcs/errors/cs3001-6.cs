// cs3001.cs: Argument type 'sbyte' is not CLS-compliant
// Line: 8

using System;
[assembly:CLSCompliant(true)]

public class CLSClass {
        static public implicit operator CLSClass(byte value) {
               return new CLSClass();
        }
    
        static public implicit operator CLSClass(sbyte value) {
               return new CLSClass();
        }
}