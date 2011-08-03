// CS0657: `assembly' is not a valid attribute location for this declaration. Valid attribute locations for this declaration are `type'. All attributes in this section will be ignored
// Line: 10
// Compiler options: -warnaserror

using System;

[assembly: CLSCompliant (false)]
public class C {}
    
[assembly: CLSCompliant (false)]
public class D {}
