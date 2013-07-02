//
// In the current Beta compiler (as of August 2005)
// the following produces a warning ("a" and "A") as opposed
// to an error
//
using System;
[assembly:CLSCompliant(true)]
[CLSCompliant(true)]
public enum X {
       A,
       a
}

class xX {
       public static void Main () {}
}
