// cs3017.cs: You cannot specify the CLSCompliant attribute on a module that differs from the CLSCompliant attribute on the assembly
// Line: 6

using System;

[module: CLSCompliant (true)]
[assembly: CLSCompliant (false)]

class Test
{
}
