// CS3017: You cannot specify the CLSCompliant attribute on a module that differs from the CLSCompliant attribute on the assembly
// Line: 7
// Compiler options: -warnaserror -warn:1

using System;

[module: CLSCompliant (true)]
[assembly: CLSCompliant (false)]

class Test
{
}
