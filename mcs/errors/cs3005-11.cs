// cs3005.cs: Identifier 'clsInterface' differing only in case is not CLS-compliant
// Line: 10

using System;
[assembly:CLSCompliant (true)]

public interface CLSInterface {
}

public class clsInterface: CLSInterface {
}