// cs3019.cs: CLS compliance checking will not be performed on 'CLSClass' because it is private or internal
// Line: 8
// Compiler options: -warnaserror -warn:2

using System;
[assembly:CLSCompliant (true)]

[CLSCompliant (false)]
class CLSClass {
}