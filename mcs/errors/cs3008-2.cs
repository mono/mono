// cs3008.cs: Identifier 'CLSClass._myEvent' is not CLS-compliant
// Line: 8

using System;
[assembly: CLSCompliant(true)]

public delegate void MyDelegate();

public class CLSClass {
        public event MyDelegate _myEvent;
}
