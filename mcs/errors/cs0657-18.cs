// cs0657-18.cs: `assembly' is not a valid attribute location for this declaration. Valid attribute locations for this declaration are `type'
// Line : 9

using System;

[assembly: CLSCompliant (false)]
public class C {}
    
[assembly: CLSCompliant (false)]
public class D {}
