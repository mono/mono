// cs3015.cs: 'CLSAttribute' has no accessible constructors which use only CLS compliant types
// Line: 7

using System;
[assembly:CLSCompliant (true)]

public class CLSAttribute: Attribute {
       private CLSAttribute(int arg) {
       }
}
